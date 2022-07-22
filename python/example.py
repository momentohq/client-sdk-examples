import os
import momento.simple_cache_client as scc
import momento.errors as errors
import logging

from momento.cache_operation_types import CacheSetResponse, CacheGetResponse, CacheGetStatus

from example_utils.example_logging import initialize_logging

_MOMENTO_AUTH_TOKEN = os.getenv("MOMENTO_AUTH_TOKEN")
_CACHE_NAME = "cache"
_ITEM_DEFAULT_TTL_SECONDS = 60
_KEY = "MyKey"
_VALUE = "MyValue"

_logger = logging.getLogger("momento-example")


def _print_start_banner() -> None:
    _logger.info("******************************************************************")
    _logger.info("*                      Momento Example Start                     *")
    _logger.info("******************************************************************")


def _print_end_banner() -> None:
    _logger.info("******************************************************************")
    _logger.info("*                       Momento Example End                      *")
    _logger.info("******************************************************************")


def _create_cache(cache_client: scc.SimpleCacheClient, cache_name: str) -> None:
    try:
        cache_client.create_cache(cache_name)
    except errors.AlreadyExistsError:
        _logger.info(f"Cache with name: {cache_name!r} already exists.")


def _list_caches(cache_client: scc.SimpleCacheClient) -> None:
    _logger.info("Listing caches:")
    list_cache_result = cache_client.list_caches()
    while True:
        for cache_info in list_cache_result.caches():
            _logger.info(f"- {cache_info.name()!r}")
        next_token = list_cache_result.next_token()
        if next_token is None:
            break
        list_cache_result = cache_client.list_caches(next_token)
    _logger.info("")


if __name__ == "__main__":
    initialize_logging()
    _print_start_banner()
    with scc.init(
        _MOMENTO_AUTH_TOKEN, _ITEM_DEFAULT_TTL_SECONDS
    ) as cache_client:
        _create_cache(cache_client, _CACHE_NAME)
        _list_caches(cache_client)

        _logger.info(f"Setting Key: {_KEY!r} Value: {_VALUE!r}")
        set_resp = cache_client.set(_CACHE_NAME, _KEY, _VALUE)
        if set_resp.status == CacheSetStatus.ERROR:
            _logger.warning(f"cache set for key {_KEY} resulted in {set_resp}")

        _logger.info(f"Getting Key: {_KEY!r}")
        get_resp = cache_client.get(_CACHE_NAME, _KEY)
        if get_resp.status == CacheGetStatus.HIT:
            _logger.info(f"Looked up Value: {str(get_resp.value())!r}")
        elif get_resp.status == CacheGetStatus.MISS:
            _logger.info(f"cache miss for key {_KEY}: {get_resp}")
        elif get_resp.status == CacheGetStatus.ERROR:
            _logger.warning(f"cache get for key {_KEY} resulted in {get_resp}")
    _print_end_banner()
