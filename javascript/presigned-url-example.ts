import {MomentoSigner, CacheOperation} from '@gomomento/sdk';
import fetch, {Response} from 'node-fetch';

const signingKey = requireEnvVar('MOMENTO_SIGNING_KEY');
const signingEndpoint = requireEnvVar('MOMENTO_ENDPOINT');
const cacheName = getCacheName();
const cacheKey = 'MyCacheKey';
const cacheValue = 'MyCacheValue';
const expiryEpochSeconds = Math.floor(Date.now() / 1000) + 10 * 60; // 10 minutes from now
const objectTtlSeconds = 180;

const main = async () => {
  const signer = await MomentoSigner.init(signingKey);

  console.log('\n*** Running presigned url example ***');
  await runPresignedUrlExample(signer);

  console.log('\n*** Running signed token example ***');
  await runSignedTokenExample(signer);
};

async function runPresignedUrlExample(signer: MomentoSigner) {
  // create presigned URL
  const setUrl = await signer.createPresignedUrl(signingEndpoint, {
    cacheName: cacheName,
    cacheKey: cacheKey,
    cacheOperation: CacheOperation.Set,
    ttlSeconds: objectTtlSeconds,
    expiryEpochSeconds: expiryEpochSeconds,
  });
  const getUrl = await signer.createPresignedUrl(signingEndpoint, {
    cacheName: cacheName,
    cacheKey: cacheKey,
    cacheOperation: CacheOperation.Get,
    expiryEpochSeconds: expiryEpochSeconds,
  });
  console.log(
    `Created signed urls with claims: exp = ${expiryEpochSeconds}, cache = ${cacheName}, key = ${cacheKey}, ttl (for set) = ${objectTtlSeconds}`
  );

  console.log(`Posting value with signed URL: '${cacheValue}'`);
  const setResponse = await fetch(setUrl, {method: 'POST', body: cacheValue});
  await ensureSuccessfulResponse(setResponse);
  const getResponse = await fetch(getUrl);
  await ensureSuccessfulResponse(getResponse);

  const data = await getResponse.text();
  console.log(`Retrieved value with signed URL: ${JSON.stringify(data)}`);
  console.log('Presigned url example finished successfully!\n');
}

async function runSignedTokenExample(signer: MomentoSigner) {
  // create signed token for this cache
  const accessToken = await signer.signAccessToken({
    cacheName: cacheName,
    expiryEpochSeconds: expiryEpochSeconds,
  });
  console.log(
    `Created signed access token for cache '${cacheName}' with expiry ${expiryEpochSeconds}`
  );

  // build urls
  const cacheKey = 'someKey';
  const setUrl = buildSetUrl(
    signingEndpoint,
    cacheName,
    cacheKey,
    accessToken,
    objectTtlSeconds
  );
  const getUrl = buildGetUrl(signingEndpoint, cacheName, cacheKey, accessToken);

  const cacheValue = 'somedata';
  console.log(
    `Posting value via URL with signed token for key '${cacheKey}': '${cacheValue}'`
  );
  const setResponse = await fetch(setUrl, {method: 'POST', body: cacheValue});
  await ensureSuccessfulResponse(setResponse);
  const getResponse = await fetch(getUrl);
  await ensureSuccessfulResponse(getResponse);

  const data = await getResponse.text();
  console.log(
    `Retrieved value via URL with signed token: ${JSON.stringify(data)}`
  );
  console.log('Signed token example finished successfully!\n');
}

function requireEnvVar(envVarName: string): string {
  const result = process.env[envVarName];
  if (!result) {
    throw new Error(`Missing required environment variable ${envVarName}`);
  }
  return result;
}

function getCacheName(): string {
  if (process.env.CACHE_NAME) {
    return process.env.CACHE_NAME;
  } else {
    const defaultCacheName = 'default-cache';
    console.log(
      `Environment variable CACHE_NAME not set. Using cache name '${defaultCacheName}'.`
    );
    return defaultCacheName;
  }
}

function buildSetUrl(
  endpoint: string,
  cacheName: string,
  cacheKey: string,
  token: string,
  ttlSeconds: number
): string {
  return `https://rest.${endpoint}/cache/set/${cacheName}/${cacheKey}?token=${token}&ttl_milliseconds=${
    ttlSeconds * 1000
  }`;
}

function buildGetUrl(
  endpoint: string,
  cacheName: string,
  cacheKey: string,
  token: string
): string {
  return `https://rest.${endpoint}/cache/get/${cacheName}/${cacheKey}?token=${token}`;
}

async function ensureSuccessfulResponse(response: Response) {
  if (!response.ok) {
    const message = await response.text();
    throw new Error(
      `Request failed for ${response.url}:\n${response.status} ${response.statusText} ${message}`
    );
  }
}

main()
  .then(() => {
    console.log('success!!');
  })
  .catch(e => {
    console.error('failure :(\n', e);
  });
