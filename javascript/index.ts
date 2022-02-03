import {SimpleCacheClient, CacheGetStatus, AlreadyExistsError} from '@momento/sdk';

const cacheName = "cache"
const cacheKey = "key"
const cacheValue = "value"
const ttl = 60
const authToken = process.env.MOMENTO_AUTH_TOKEN
if (!authToken) {
    throw new Error("Missing required environment variable MOMENTO_AUTH_TOKEN")
}

const defaultTtl = 60;
const momento = new SimpleCacheClient(authToken, defaultTtl);

const main = async () => {
    try {
        await momento.createCache(cacheName);
    } catch(e) {
        if (e instanceof AlreadyExistsError) {
            console.log("cache already exists")
        } else {
            throw e
        }
    }

    console.log(`Storing key=${cacheKey}, value=${cacheValue}, ttl=${ttl}`)
    await momento.set(cacheName, cacheKey, cacheValue)
    const getResp = await momento.get(cacheName, cacheKey)

    if (getResp.status === CacheGetStatus.Hit) {
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
