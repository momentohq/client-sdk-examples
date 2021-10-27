## Running the Example

- Node version 10.13 or higher is required

```bash
# Set your npm registry
npm config set @momento:registry https://momento.jfrog.io/artifactory/api/npm/npm-public/
cd typescript
npm install
npm run build

# Run example code
MOMENTO_AUTH_TOKEN=<YOUR AUTH TOKEN> npm run example
```

Example Code: [index.ts](index.ts)
- Send us an email at [support@momentohq.com](mailto:support@momentohq.com) to request a Momento Auth Token. It is required to authenticate with the Momento Cache Service. This token uniquely identifies cache interactions. The token should be treated like a sensitive password and all essential care must be taken to ensure its secrecy. We recommend that you store this token in a secret vault like AWS Secrets Manager.

## Using the sdk in your projects

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
await cache.setBytes(key, value, 50)
await cache.getBytes(key)
```

Handling cache misses
```typescript
const res = await cache.get("non-existent key")
if (res.result === MomentoCacheResult.Miss) {
    console.log("cache miss")
}
```
