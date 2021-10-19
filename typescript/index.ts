import {Momento} from '@momento/sdk';

const cacheName = "cache2"
const cacheKey = "key"
const cacheValue = "value"
const ttl = 5000
const authToken = process.env.MOMENTO_AUTH_TOKEN
if (!authToken) {
    throw new Error("Missing required environment variable MOMENTO_AUTH_TOKEN")
}

const momento = new Momento(authToken);

const main = async () => {
    const cache = await momento.createOrGetCache(cacheName, {
        defaultTtlSeconds: 100
    });
    console.log(`Storing key=${cacheKey}, value=${cacheValue}, ttl=${ttl}`)
    await cache.set(cacheKey, cacheValue, ttl)
    const getResp = await cache.get(cacheKey)
    console.log(`result: ${getResp.text()}`)
}

main()
