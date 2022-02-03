## Running the Example

- Node version 10.13 or higher is required
- A Momento Auth Token is required, you can generate one using the [Momento CLI](https://github.com/momentohq/momento-cli)

```bash
# Set your npm registry
npm config set @momento:registry https://momento.jfrog.io/artifactory/api/npm/npm-public/
cd javascript
npm install

# Run example code
MOMENTO_AUTH_TOKEN=<YOUR AUTH TOKEN> npm run example
```

Example Code: [index.ts](index.ts)

## Using the SDK in your projects

### Installation

```bash
npm config set @momento:registry https://momento.jfrog.io/artifactory/api/npm/npm-public/
npm install @momento/sdk
```

### Usage

```typescript
import {SimpleCacheClient, CacheGetStatus} from '@momento/sdk';

// your authentication token for momento
const authToken = process.env.MOMENTO_AUTH_TOKEN;

// initializing momento
const DEFAULT_TTL = 60 // 60 seconds for default ttl
const momento = new SimpleCacheClient(authToken, DEFAULT_TTL);

// creating a cache named "myCache", and subsequently returning it
const CACHE_NAME = "myCache";
const cache = await momento.createCache(CACHE_NAME);

// sets key with default ttl
await cache.set(CACHE_NAME, "key", "value");
const res = await cache.get(CACHE_NAME, "key");
console.log("result: ", res.text());

// sets key with ttl of 5 seconds
await cache.set(CACHE_NAME, "key2", "value2", 5);

// permanently deletes cache
await momento.deleteCache(CACHE_NAME);
```

Momento also supports storing pure bytes,

```typescript
const key = new Uint8Array([109, 111, 109, 101, 110, 116, 111]);
const value = new Uint8Array([
  109, 111, 109, 101, 110, 116, 111, 32, 105, 115, 32, 97, 119, 101, 115, 111,
  109, 101, 33, 33, 33,
]);
await cache.set("cache", key, value, 50);
await cache.get("cache", key);
```

Handling cache misses

```typescript
const res = await cache.get("cache", "non-existent key");
if (res.status === CacheGetStatus.Miss) {
    console.log("cache miss");
}
```

Storing Files

```typescript
const buffer = fs.readFileSync("./file.txt");
const filebytes = Uint8Array.from(buffer);
const cacheKey = "key";
const cacheName = "my example cache";

// store file in cache
await cache.set(cacheName, cacheKey, filebytes);

// retrieve file from cache
const getResp = await cache.get(cacheName, cacheKey);

// write file to disk
fs.writeFileSync("./file-from-cache.txt", Buffer.from(getResp.bytes()));
```
