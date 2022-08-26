import {
  AlreadyExistsError,
  CacheGetStatus,
  LogLevel,
  LogFormat,
  SimpleCacheClient,
} from '@gomomento/sdk';

const cacheName = 'cache';
const cacheKey = 'key';
const cacheValue = 'value';
const authToken = process.env.MOMENTO_AUTH_TOKEN;
if (!authToken) {
  throw new Error('Missing required environment variable MOMENTO_AUTH_TOKEN');
}

const defaultTtl = 60;
const momento = new SimpleCacheClient(authToken, defaultTtl, {
  numChannels: 6,
  channelOptions: {
    // default value for max session memory is 10mb.  Under high load, it is easy to exceed this,
    // after which point all requests will fail with a client-side RESOURCE_EXHAUSTED exception.
    // This needs to be tunable: https://github.com/momentohq/dev-eco-issue-tracker/issues/85
    'grpc-node.max_session_memory': 256,
    // This flag controls whether channels use a shared global pool of subchannels, or whether
    // each channel gets its own subchannel pool.  The default value is 0, meaning a single global
    // pool.  Setting it to 1 provides significant performance improvements when we instantiate more
    // than one grpc client.
    'grpc.use_local_subchannel_pool': 1,
  },
  loggerOptions: {
    level: LogLevel.DEBUG,
    format: LogFormat.CONSOLE,
  },
});

const main = async () => {
  try {
    await momento.createCache(cacheName);
  } catch (e) {
    if (e instanceof AlreadyExistsError) {
      console.log('cache already exists');
    } else {
      throw e;
    }
  }

  console.log('Listing caches:');
  let token;
  do {
    const listResp = await momento.listCaches();
    listResp.getCaches().forEach(cacheInfo => {
      console.log(`${cacheInfo.getName()}`);
    });
    token = listResp.getNextToken();
  } while (token !== null);

  const exampleTtlSeconds = 10;
  console.log(
    `Storing key=${cacheKey}, value=${cacheValue}, ttl=${exampleTtlSeconds}`
  );
  await momento.set(cacheName, cacheKey, cacheValue, exampleTtlSeconds);
  const getResp = await momento.get(cacheName, cacheKey);

  if (getResp.status === CacheGetStatus.Hit) {
    console.log(`cache hit: ${String(getResp.text())}`);
  } else {
    console.log('cache miss');
  }
};

main()
  .then(() => {
    console.log('success!!');
  })
  .catch(e => {
    console.error('failed to get from cache', e);
  });
