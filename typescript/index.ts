import {Momento, MomentoCacheResult} from '@momento/sdk';

const cacheName = "cache"
const cacheKey = "key"
const cacheValue = "value"
const ttl = 100
const authToken = process.env.MOMENTO_AUTH_TOKEN
if (!authToken) {
    throw new Error("Missing required environment variable MOMENTO_AUTH_TOKEN")
}

const momento = new Momento(authToken);

const main = async () => {
    const cache = await momento.createOrGetCache(cacheName, {
        defaultTtlSeconds: ttl
    });
    console.log(`Storing key=${cacheKey}, value=${cacheValue}, ttl=${ttl}`)

    await cache.set(cacheKey, cacheValue)
    const getResp = await cache.get(cacheKey)
    if (getResp.result === MomentoCacheResult.Hit) {
        console.log(`cache hit: ${getResp.text()}`)
    } else {
        console.log('cache miss')
    }
}

main().then(() => {
    console.log("success!!")
}).catch((e) => {
    console.error("failed to get from cache", e)
})
