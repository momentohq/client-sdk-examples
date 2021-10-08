# Java SDK Customer Onboarding

# Working with Momento Cache Service

## Setting up

### Prerequisites

To make requests to Momento using the Java SDK you need the following:

- **Momento Auth Token**

    Momento Auth Token is required to authenticate with the Momento Cache Service. This token uniquely identifies cache interactions. The token should be treated like a sensitive password and all essential care must be taken to ensure its secrecy. We recommend that you store this token in a secret vault like AWS Secrets Manager.


Send us an email at eng+onboarding@momentohq.com to request a Momento Auth Token.

## **Experiment with the Example**

Example Code is available at `java-gradle/lib/src/main/java/momento/client/example/MomentoCacheApplication.java`

Modify the program to replace `"<YOUR TEST AUTH TOKEN>"` with your Momento Auth Token.

```java
private static final String MOMENTO_AUTH_TOKEN = "<YOUR TEST AUTH TOKEN>";
```

To run:

```shell
cd java-gradle/
./gradlw run
```

If you wish to open this example in an IntelliJ IDE, while opening the projecting point your IDE at `java-gradle/settings.gradle.kts`

## Using the Java SDK in your project

### Gradle Configuration

Update your Gradle build to include the components

**build.gradle.kts**

```kotlin
repositories {
    maven("https://momento.jfrog.io/artifactory/maven-public")
}

dependencies {
    implementation("momento.sandbox:momento-sdk:0.10.0")
}
```

### Making Requests

**Momento Client**

All interactions with Momento Cache Service require a Momento Client.

`moment.sdk.Momento`

Momento Client can be created using a static builder

- `static Momento.MomentoBuilder builder(String authToken)`

    Creates a builder the can be used to create a `Momento` client.

    ```java
    	Momento momento = Momento.builder(authToken).build();
    ```

    **throws**

    - `SdkClientException` if the provided `authToken` is null or empty


- `CreateCacheResponse createCache(String cacheName)`

    Creates a Momento Cache with provided `cacheName` as an addressable name of the cache. Cache name is unique per `authToken`. `cacheName` is non-null, non-empty string with maximum length of 255 characters.

    **throws**

    - `SdkClientException` if the cacheName is null
    - `PermissionDeniedException` if the provided `authToken` is invalid
    - `CacheAlreadyExistsException` if a cache with the same name already exists
    - `InvalidArgumentException` if the cache name fails to meet the valid criteria
    - `InternalServerException` if Momento had an internal error while processing the create request.


- `Cache getCache(String cacheName)`

    Creates a `momento.sdk.Cache` client that can be used to interact with a Momento Cache

    **throws**

    - `SdkClientException` if provided cacheName is null
    - `PermissionDeniedException` if `authToken` is invalid
    - `CacheNotFoundException` if a Momento Cache with `cacheName` doesn't exist
    - `InternalServerException` if Momento had an internal error while processing the request.


**Interacting with Cache**

`momento.sdk.Cache` client is required to interact with a Momento Cache. This client is obtained by using `momento.sdk.Momento` client's `Cache getCache(String cacheName)` method.

- `CacheSetResponse set(String key, String value, int ttlSeconds)`

    Stores an item in the Momento Cache

    `key` The key used to store the item in the cache

    `value` The value corresponding to the specified key

    `ttlSeconds` The duration for which an item will be stored in the key before it gets deleted

    **throws**

    - `SdkClientException` if `key` or `value` is null or `ttlSeconds` is ≤ 0
    - `PermissionDeniedException` if `authToken` used for making the request is invalid
    - `CacheNotFoundException` if target cache on which sets are being made is not found. This may arise if the Cache was deleted after the `momento.sdk.Cache` was created

    **Other variations**

    `CacheSetResponse set(String key, ByteBuffer value, int ttlSeconds)`

    `CacheSetResponse set(byte[] key, byte[] value, int ttlSeconds)`

    **Related Async variations**

    `CompletableFuture<CacheSetResponse> setAsync(String key, String value, int ttlSeconds)`

    `CompletableFuture<CacheSetResponse> setAsync(String key, ByteBuffer value, int ttlSeconds)`

    `CompletableFuture<CacheSetResponse> setAsync(byte[] key, byte[] value, int ttlSeconds)`


- `CacheGetResponse get(String key)`

    Get the value stored in the Momento Cache for a given `key`

    **Returns**

    `momento.sdk.messages.CacheGetResponse`

    Response object that encapsulates the result of the get request.

    - `momento.sdk.messages.MomentoCacheResult result()`

        Encapsulates `Hit` and `Miss` to indicate wether the request made resulted in a cache hit or not.

    - Also exposes values as `Optional<>`, will `Optional<>.empty()` if there is no cache `Hit`
        - `Optional<byte[]> asByteArray()`
        - `Optional<ByteBuffer> asByteBuffer()`
        - `Optional<String> asStringUtf8()`
        - `Optional<String> asString(java.nio.charset.Charset charset)`

    **throws**

    - `SdkClientException` if `key` or `value` is null or `ttlSeconds` is ≤ 0
    - `PermissionDeniedException` if `authToken` used for making the request is invalid
    - `CacheNotFoundException` if target cache on which sets are being made is not found. This may arise if the Cache was deleted after the `momento.sdk.Cache` was created

    **Other variations**

    `CacheGetResponse get(byte[] key)`

    **Related Async variations**

    `CompletableFuture<CacheGetResponse> getAsync(String key)`

    `CompletableFuture<CacheGetResponse> getAsync(byte[] key)`
