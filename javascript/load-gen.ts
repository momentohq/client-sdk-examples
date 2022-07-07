import {
  AlreadyExistsError,
  CacheGetStatus,
  GetResponse,
  LogFormat,
  LogLevel,
  getLogger,
  SimpleCacheClient,
  LoggerOptions,
  Logger,
  InternalServerError,
  TimeoutError,
} from '@gomomento/sdk';

const authToken = process.env.MOMENTO_AUTH_TOKEN;
if (!authToken) {
  throw new Error('Missing required environment variable MOMENTO_AUTH_TOKEN');
}

const defaultTtl = 60;

const loggerOptions: LoggerOptions = {
  level: LogLevel.DEBUG,
  // level: LogLevel.INFO,
  format: LogFormat.CONSOLE,
};

const momento = new SimpleCacheClient(authToken, defaultTtl, {
  requestTimeoutMs: 5 * 1000,
  loggerOptions: loggerOptions,
});
const logger: Logger = getLogger('load-gen', loggerOptions);
const cacheName = 'js-loadgen';

const main = async () => {
  try {
    await momento.createCache(cacheName);
  } catch (e) {
    if (e instanceof AlreadyExistsError) {
      logger.info('cache already exists');
    } else {
      throw e;
    }
  }

  const numAsyncWorkers = 1000;
  const numRequestsPerWorker = 50;
  // const numAsyncWorkers = 1;
  // const numRequestsPerWorker = 1000;

  const asyncGetSetResults = [...Array(numAsyncWorkers).keys()].map(workerId =>
    launchAndRunWorkers(workerId + 1, numRequestsPerWorker)
  );
  const allResultPromises = Promise.all(asyncGetSetResults);
  const allResults = await allResultPromises;
  logger.info(`DONE!  Num successes per worker: ${JSON.stringify(allResults)}`);
};

const cacheValue2500Bytes = 'x'.repeat(2500);
let globalRequestCount = 0;
let globalSuccessCount = 0;
let globalUnavailableCount = 0;
let globalDeadlineExceededCount = 0;

async function launchAndRunWorkers(
  workerId: number,
  numRequests: number
): Promise<number> {
  let successCount = 0;
  for (let i = 1; i <= numRequests; i++) {
    const result = await issueAsyncSetGet(workerId, i);
    globalRequestCount++;
    switch (result) {
      case AsyncSetGetResult.SUCCESS:
        successCount++;
        globalSuccessCount++;
        break;
      case AsyncSetGetResult.UNAVAILABLE:
        globalUnavailableCount++;
        break;
      case AsyncSetGetResult.DEADLINE_EXCEEDED:
        globalDeadlineExceededCount++;
        break;
    }
    if (globalRequestCount % 1000 === 0) {
      logger.info(`
stats:
              total: ${globalRequestCount}
            success: ${globalSuccessCount}
        unavailable: ${globalUnavailableCount}
  deadline exceeded: ${globalDeadlineExceededCount}
`);
    }
  }
  return successCount;
}

enum AsyncSetGetResult {
  SUCCESS = 'SUCCESS',
  UNAVAILABLE = 'UNAVAILABLE',
  DEADLINE_EXCEEDED = 'DEADLINE_EXCEEDED',
}

async function issueAsyncSetGet(
  workerId: number,
  requestId: number
): Promise<AsyncSetGetResult> {
  const cacheKey = `request${requestId}`;
  try {
    await momento.set(cacheName, cacheKey, cacheValue2500Bytes);
    return await momento
      .get(cacheName, cacheKey)
      .then((getResp: GetResponse) => {
        let valueString: string;
        if (getResp.status === CacheGetStatus.Hit) {
          const value = String(getResp.text());
          valueString = `${value.substring(0, 10)}... (len: ${value.length})`;
        } else {
          valueString = 'n/a';
        }
        if (globalRequestCount % 1000 === 0) {
          logger.info(
            `worker: ${workerId}, worker request: ${requestId}, global request: ${globalRequestCount}, status: ${getResp.status}, val: ${valueString}`
          );
        }
        return AsyncSetGetResult.SUCCESS;
      });
  } catch (e) {
    if (e instanceof InternalServerError) {
      if (e.message.includes('UNAVAILABLE')) {
        logger.error(
          `Caught UNAVAILABLE error; swallowing: ${e.name}, ${e.message}`
        );
        return AsyncSetGetResult.UNAVAILABLE;
      } else {
        throw e;
      }
    } else if (e instanceof TimeoutError) {
      if (e.message.includes('DEADLINE_EXCEEDED')) {
        logger.error(
          `Caught DEADLINE_EXCEEDED error; swallowing: ${e.name}, ${e.message}`
        );
        return AsyncSetGetResult.DEADLINE_EXCEEDED;
      } else {
        throw e;
      }
    } else {
      throw e;
    }
  }
}

function delay(ms: number) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

main()
  .then(() => {
    logger.info('success!!');
  })
  .catch(e => {
    console.error('Uncaught exception while running load gen', e);
    throw e;
  });
