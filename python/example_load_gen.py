import asyncio
import logging
import os
from dataclasses import dataclass
from enum import Enum
from time import perf_counter_ns
from typing import Optional, TypeVar, Callable, Coroutine, Tuple

import colorlog
import psutil
import uvloop
from hdrh.histogram import HdrHistogram
import momento.errors
from momento.aio import simple_cache_client
from momento.cache_operation_types import CacheSetResponse, CacheGetResponse, CacheGetStatus
from momento.logs import initialize_momento_logging


def initialize_logging(level: int) -> None:
    initialize_momento_logging()
    root_logger = logging.getLogger()
    root_logger.setLevel(level)

    handler = colorlog.StreamHandler()
    handler.setFormatter(colorlog.ColoredFormatter(
        "%(asctime)s %(log_color)s%(levelname)-8s%(reset)s %(thin_cyan)s%(name)s%(reset)s %(message)s"
    ))
    handler.setLevel(level)
    root_logger.addHandler(handler)


class AsyncSetGetResult(Enum):
    SUCCESS = 'SUCCESS',
    UNAVAILABLE = 'UNAVAILABLE',
    DEADLINE_EXCEEDED = 'DEADLINE_EXCEEDED',


@dataclass
class LoadGenDataPoint:
    elapsed_millis: float
    num_workers: int
    num_clients: int
    asyncio_engine: str
    request_count: int
    tps: float
    p50: int
    p999: int


@dataclass
class BasicPythonLoadGenContext:
    start_time: float
    num_workers: int
    num_clients: int
    asyncio_engine: str
    get_latencies: HdrHistogram
    set_latencies: HdrHistogram
    # TODO: these could be generalized into a map structure that
    #  would make it possible to deal with a broader range of
    #  failure types more succinctly.
    global_request_count: int
    global_success_count: int
    global_unavailable_count: int
    global_deadline_exceeded_count: int
    last_load_gen_data_point: Optional[LoadGenDataPoint]


class BasicPythonLoadGen:
    cache_name = 'python-loadgen'
    # print_summary_every_n_requests = 10_000
    print_summary_every_n_requests = 1_000

    def __init__(self,
                 asyncio_engine: str,
                 request_timeout_ms: int,
                 cache_item_payload_bytes: int,
                 number_of_concurrent_requests: int,
                 total_number_of_operations_to_execute: int,
                 num_clients: int,
                 ):
        self.logger = logging.getLogger('load-gen')
        self.asyncio_engine = asyncio_engine
        self.auth_token = os.getenv('MOMENTO_AUTH_TOKEN')
        if not self.auth_token:
            raise ValueError('Missing required environment variable MOMENTO_AUTH_TOKEN')
        self.request_timeout_ms = request_timeout_ms
        self.number_of_concurrent_requests = number_of_concurrent_requests
        self.total_number_of_operations_to_execute = total_number_of_operations_to_execute
        self.cache_value = 'x' * cache_item_payload_bytes
        self.num_clients = num_clients

    async def run(self) -> None:
        cache_item_ttl_seconds = 60
        async with simple_cache_client.init(
                self.auth_token, cache_item_ttl_seconds, self.request_timeout_ms
        ) as cache_client:
            try:
                await cache_client.create_cache(BasicPythonLoadGen.cache_name)
            except momento.errors.AlreadyExistsError:
                self.logger.info(f"Cache with name: {BasicPythonLoadGen.cache_name} already exists.")

            num_operations_per_worker = round(self.total_number_of_operations_to_execute /
                                              self.number_of_concurrent_requests)
            load_gen_context = BasicPythonLoadGenContext(
                start_time=perf_counter_ns(),
                num_workers=self.number_of_concurrent_requests,
                num_clients=self.num_clients,
                asyncio_engine=self.asyncio_engine,
                get_latencies=HdrHistogram(1, 1000 * 60, 1),
                set_latencies=HdrHistogram(1, 1000 * 60, 1),
                global_request_count=0,
                global_success_count=0,
                global_unavailable_count=0,
                global_deadline_exceeded_count=0,
                last_load_gen_data_point=None
            )

            async_get_set_results = (
                self.launch_and_run_worker(cache_client, load_gen_context, worker_id + 1, num_operations_per_worker)
                for worker_id in range(self.number_of_concurrent_requests))
            await asyncio.gather(*async_get_set_results)
            self.logger.info('DONE!')

    async def launch_and_run_worker(
            self,
            client: simple_cache_client.SimpleCacheClient,
            context: BasicPythonLoadGenContext,
            worker_id: int,
            num_operations: int
    ) -> None:
        for i in range(num_operations):
            await self.issue_async_set_get(client, context, worker_id, i + 1)

            if context.global_request_count % BasicPythonLoadGen.print_summary_every_n_requests == 0:
                self.logger.info(f"""
current cpu usage: {psutil.cpu_percent() * psutil.cpu_count()}                

cumulative stats:
       total requests: {context.global_request_count} ({self.tps(context, context.global_request_count)} tps)
              success: {context.global_success_count} ({self.percent_requests(context, context.global_success_count)}%) ({self.tps(context, context.global_success_count)} tps)
          unavailable: {context.global_unavailable_count} ({self.percent_requests(context, context.global_unavailable_count)}%)
    deadline exceeded: {context.global_deadline_exceeded_count} ({self.percent_requests(context, context.global_deadline_exceeded_count)}%)
    
cumulative set latencies:
{self.output_histogram_summary(context.set_latencies)}

cumulative get latencies:
{self.output_histogram_summary(context.get_latencies)}
""")
                context.last_load_gen_data_point = self.generate_and_output_latest_data_point(context)
                context.get_latencies.reset()
                context.set_latencies.reset()

    async def issue_async_set_get(
            self,
            client: simple_cache_client.SimpleCacheClient,
            context: BasicPythonLoadGenContext,
            worker_id: int,
            operation_id: int
    ) -> None:
        cache_key = f"worker{worker_id}operation{operation_id}"
        set_start_time = perf_counter_ns()
        result: Optional[CacheSetResponse] = await self.execute_request_and_update_context_counts(
            context,
            lambda: client.set(self.cache_name, cache_key, self.cache_value)
        )
        if result:
            set_duration = self.get_elapsed_millis(set_start_time)
            context.set_latencies.record_value(set_duration)

        get_start_time = perf_counter_ns()
        get_result: Optional[CacheGetResponse] = await self.execute_request_and_update_context_counts(
            context,
            lambda: client.get(self.cache_name, cache_key)
        )
        if get_result:
            get_duration = self.get_elapsed_millis(get_start_time)
            context.get_latencies.record_value(get_duration)
            if get_result.status() == CacheGetStatus.HIT:
                value = get_result.value()
                value_string = f"{value[0:10]}... (len: {len(value)})"
            else:
                value_string = 'n/a'

            if context.global_request_count % BasicPythonLoadGen.print_summary_every_n_requests == 0:
                self.logger.info(
                    f"worker: {worker_id}, worker request: {operation_id}, global request: {context.global_request_count}, status: {get_result.status()}, val: {value_string}"
                )

    T = TypeVar("T")

    async def execute_request_and_update_context_counts(
            self,
            context: BasicPythonLoadGenContext,
            block: Callable[[], Coroutine[None, None, T]]
    ) -> Optional[T]:
        result, response = await self.execute_request(block)
        self.update_context_counts_for_request(context, result)
        return response

    async def execute_request(
            self,
            block: Callable[[], Coroutine[None, None, T]]
    ) -> Tuple[AsyncSetGetResult, Optional[T]]:
        try:
            result = await block()
            return AsyncSetGetResult.SUCCESS, result
        except momento.errors.InternalServerError as e:
            # TODO verify exception type
            self.logger.error(f"Caught InternalServerError: {e}")
            return AsyncSetGetResult.UNAVAILABLE, None
        except momento.errors.TimeoutError as e:
            # TODO need to verify exception type
            self.logger.error(f"Caught TimeoutError: {e}")
            return AsyncSetGetResult.DEADLINE_EXCEEDED, None

    @staticmethod
    def update_context_counts_for_request(
            context: BasicPythonLoadGenContext,
            result: AsyncSetGetResult
    ):
        context.global_request_count += 1
        if result == AsyncSetGetResult.SUCCESS:
            context.global_success_count += 1
        elif result == AsyncSetGetResult.UNAVAILABLE:
            context.global_unavailable_count += 1
        elif result == AsyncSetGetResult.DEADLINE_EXCEEDED:
            context.global_deadline_exceeded_count += 1
        else:
            raise ValueError(f"Unsupported result type: {result}")

    def tps(self, context: BasicPythonLoadGenContext, request_count: int) -> int:
        return round((request_count * 1000) / self.get_elapsed_millis(context.start_time))

    @staticmethod
    def percent_requests(context: BasicPythonLoadGenContext, count: int) -> float:
        # multiply the ratio by 100 to get a percentage.  round to the nearest 0.1.
        return round((count / context.global_request_count) * 100, 1)

    @staticmethod
    def output_histogram_summary(
            histogram: HdrHistogram
    ) -> str:
        return f"""
    count: {histogram.total_count}
      min: {histogram.min_value}
      p50: {histogram.get_value_at_percentile(50)}
      p90: {histogram.get_value_at_percentile(90)}
      p99: {histogram.get_value_at_percentile(99)}
    p99.9: {histogram.get_value_at_percentile(99.9)}
      max: {histogram.max_value}         
"""

    def generate_and_output_latest_data_point(self, context: BasicPythonLoadGenContext) -> LoadGenDataPoint:
        elapsed_millis = self.get_elapsed_millis(context.start_time)
        if context.last_load_gen_data_point:
            elapsed_millis_since_last_data_point = elapsed_millis - context.last_load_gen_data_point.elapsed_millis
            requests_since_last_data_point = context.global_request_count - context.last_load_gen_data_point.request_count
        else:
            elapsed_millis_since_last_data_point = elapsed_millis
            requests_since_last_data_point = context.global_request_count

        new_data_point = LoadGenDataPoint(
            elapsed_millis=elapsed_millis,
            num_workers=context.num_workers,
            num_clients=context.num_clients,
            asyncio_engine=context.asyncio_engine,
            request_count=context.global_request_count,
            tps=((requests_since_last_data_point * 1000) / elapsed_millis_since_last_data_point),
            p50=context.get_latencies.get_value_at_percentile(50),
            p999=context.get_latencies.get_value_at_percentile(99.9)
        )
        self.logger.info(f"Load gen data point: {new_data_point.elapsed_millis}\t{new_data_point.num_workers}\t{new_data_point.num_clients}\t{new_data_point.asyncio_engine}\t{new_data_point.request_count}\t{new_data_point.tps}\t{new_data_point.p50}\t{new_data_point.p999}")
        return new_data_point

    @staticmethod
    def get_elapsed_millis(start_time: float) -> int:
        end_time = perf_counter_ns()
        result = round((end_time - start_time) / 1e6)
        return result


PERFORMANCE_INFORMATION_MESSAGE = """
Thanks for trying out our basic python load generator!  This tool is
included to allow you to experiment with performance in your environment
based on different configurations.  It's very simplistic, and only intended
to give you a quick way to explore the performance of the Momento client
running on a single python process.

Note that because python has a global interpreter lock, user code runs on
a single thread and cannot take advantage of multiple CPU cores.  Thus, the
limiting factor in request throughput will often be CPU.  Keep an eye on your CPU
consumption while running the load generator, and if you reach 100%
of a CPU core then you most likely won't be able to improve throughput further
without running additional python processes.

CPU will also impact your client-side latency; as you increase the number of
concurrent requests, if they are competing for CPU time then the observed
latency will increase.

Also, since performance will be impacted by network latency, you'll get the best
results if you run on a cloud VM in the same region as your Momento cache.

Check out the configuration settings at the bottom of the 'example_load_gen.py' to
see how different configurations impact performance.

If you have questions or need help experimenting further, please reach out to us!
"""


async def main(
        log_level: int,
        asyncio_engine: str,
        request_timeout_ms: int,
        cache_item_payload_bytes: int,
        number_of_concurrent_requests: int,
        total_number_of_operations_to_execute: int,
        num_clients: int,
) -> None:
    initialize_logging(log_level)
    load_generator = BasicPythonLoadGen(
        asyncio_engine=asyncio_engine,
        request_timeout_ms=request_timeout_ms,
        cache_item_payload_bytes=cache_item_payload_bytes,
        number_of_concurrent_requests=number_of_concurrent_requests,
        total_number_of_operations_to_execute=total_number_of_operations_to_execute,
        num_clients=num_clients
    )
    await load_generator.run()
    print(PERFORMANCE_INFORMATION_MESSAGE)


load_generator_options = dict(
    asyncio_engine='uvloop',
    # asyncio_engine='default',

    #
    # This setting allows you to control the verbosity of the log output during
    # the load generator run. Available log levels are TRACE, DEBUG, INFO, WARN,
    # and ERROR.  DEBUG is a reasonable choice for this load generator program.
    #
    log_level=logging.DEBUG,
    #
    # Configures the Momento client to timeout if a request exceeds this limit.
    # Momento client default is 5 seconds.
    #
    request_timeout_ms=5 * 1_000,
    #
    # Controls the size of the payload that will be used for the cache items in
    # the load test.  Smaller payloads will generally provide lower latencies than
    # larger payloads.
    #
    cache_item_payload_bytes=2500,
    #
    # Controls the number of concurrent requests that will be made (via asynchronous
    # function calls) by the load test.  Increasing this number may improve throughput,
    # but it will also increase CPU consumption.  As CPU usage increases and there
    # is more contention between the concurrent function calls, client-side latencies
    # may increase.
    #
    # number_of_concurrent_requests=5_000,
    # number_of_concurrent_requests=500,
    # number_of_concurrent_requests=200,
    # number_of_concurrent_requests=100,
    # number_of_concurrent_requests=50,
    number_of_concurrent_requests=20,
    # number_of_concurrent_requests=10,
    # number_of_concurrent_requests=1,
    #
    # Controls how long the load test will run.  We will execute this many operations
    # (1 cache 'set' followed immediately by 1 'get') across all of our concurrent
    # workers before exiting.  Statistics will be logged every 1000 operations.
    #
    total_number_of_operations_to_execute=800_000,
    # total_number_of_operations_to_execute=50_000,

    num_clients=1
)


if __name__ == "__main__":
    # asyncio.set_event_loop_policy(uvloop.EventLoopPolicy())
    if load_generator_options['asyncio_engine'] == 'uvloop':
        uvloop.install()
    asyncio.run(main(**load_generator_options))
