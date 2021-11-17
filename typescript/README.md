## Running the Example

- Node version 10.13 or higher is required
- A Momento Auth Token is required, you can generate one using the [Momento CLI](https://github.com/momentohq/momento-cli)

```bash
# Set your npm registry
npm config set @momento:registry https://momento.jfrog.io/artifactory/api/npm/npm-public/
cd typescript
npm install

# Run example code
MOMENTO_AUTH_TOKEN=<YOUR AUTH TOKEN> npm run example
```

Example Code: [index.ts](index.ts)

## Using the SDK in your projects

### Installation
```bash
npm config set @momento:registry https://momento.jfrog.io/artifactory/npm-public/
npm install @momento/sdk
```

### Usage

```typescript
import {Momento, MomentoCacheResult} from "@momento/sdk";

// your authentication token for momento
const authToken = process.env.MOMENTO_AUTH_TOKEN

// initializing momento
const momento = new Momento(authToken);

// creating a cache named "myCache", and subsequently returning it
const cache = await momento.createOrGetCache("myCache", {
    defaultTtlSeconds: 100
})

// sets key with default ttl
await cache.set("key", "value")
const res = await cache.get("key")
console.log("result: ", res.text())

// sets key with ttl of 5 seconds
await cache.set("key2", "value2", 5)

// permanently deletes cache
await momento.deleteCache("myCache")
```

Momento also supports storing pure bytes,
```typescript
const key = new Uint8Array([109,111,109,101,110,116,111])
const value = new Uint8Array([109,111,109,101,110,116,111,32,105,115,32,97,119,101,115,111,109,101,33,33,33])
await cache.set(key, value, 50)
await cache.get(key)
```

Handling cache misses
```typescript
const res = await cache.get("non-existent key")
if (res.result === MomentoCacheResult.Miss) {
    console.log("cache miss")
}
```

Storing Files
```typescript
const buffer = fs.readFileSync("./file.txt");
const filebytes = Uint8Array.from(buffer);
const cacheKey = "key";

// store file in cache
await cache.set(cacheKey, filebytes);

// retrieve file from cache
const getResp = await cache.get(cacheKey);

// write file to disk
fs.writeFileSync('./file-from-cache.txt', Buffer.from(getResp.bytes()));
```
