package momento.client.example;

import momento.sdk.Cache;
import momento.sdk.Momento;
import momento.sdk.exceptions.CacheAlreadyExistsException;
import momento.sdk.messages.CacheGetResponse;
import momento.sdk.messages.MomentoCacheResult;

public class MomentoCacheApplication {

  private static final String MOMENTO_AUTH_TOKEN = System.getenv("MOMENTO_AUTH_TOKEN");
  private static final String CACHE_NAME = "myCache";

  public static void main(String[] args) throws InterruptedException {
    System.out.println("Running Momento Cache Application");
    run();
  }

  private static void run() throws InterruptedException {
    try (Cache cache = createOrGetCache(CACHE_NAME)) {
      // Get what you set
      String getWhatYouSetKey = "MyFirstKey";
      String getWhatYouSetValue = "CacheMePlease";
      int ttl = 10; // second
      cache.set(getWhatYouSetKey, getWhatYouSetValue, ttl);

      // Read the value and it must be a HIT!
      CacheGetResponse getWhatYouSet = cache.get(getWhatYouSetKey);
      assert getWhatYouSet.result() == MomentoCacheResult.Hit : "Expect Cache Hit, But was a Miss";
      System.out.println("Cache Hit!!");
      assert getWhatYouSet.asStringUtf8().get().equals(getWhatYouSetValue)
          : String.format(
              "Expected value `%s` but found `%s`",
              getWhatYouSetValue, getWhatYouSet.asStringUtf8());
      System.out.println(
          String.format(
              "Expected value `%s` ==  Looked up value `%s`",
              getWhatYouSetValue, getWhatYouSet.asStringUtf8().get()));

      // Test TTL Enforcement
      System.out.println("");
      System.out.println("Checking TTL Enforcement");
      System.out.println("Sleeping for " + ttl + " seconds!");
      Thread.sleep(ttl * 1000); // millisecond
      // Read the value and it must be a MISS!
      CacheGetResponse ttlResponse = cache.get(getWhatYouSetKey);
      assert ttlResponse.result() == MomentoCacheResult.Miss : "Item found, but it is past the TTL";
      assert ttlResponse.asStringUtf8().isPresent() == false
          : "Item value found, but it shouldn't be present";
      System.out.println("Item dropped from Cache after expiry!");
    }
  }

  private static Cache createOrGetCache(String cacheName) {
    try (Momento momento = Momento.builder(MOMENTO_AUTH_TOKEN).build()) {
      // Create or Get Cache
      System.out.println("Creating Cache with name: " + cacheName);
      try {
        momento.createCache(cacheName);
        System.out.println("Successfully created Cache with name: " + cacheName);
      } catch (CacheAlreadyExistsException e) {
        // Cache already exists, so use it.
        System.out.println("Found a cache with name: " + cacheName);
      }
      System.out.println("Initializing Cache Client for cache: " + cacheName);
      return momento.getCache(cacheName);
    }
  }
}
