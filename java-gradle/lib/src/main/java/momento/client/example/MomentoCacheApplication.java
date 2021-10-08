package momento.client.example;

import momento.sdk.Cache;
import momento.sdk.Momento;
import momento.sdk.exceptions.CacheAlreadyExistsException;
import momento.sdk.messages.CacheGetResponse;
import momento.sdk.messages.MomentoCacheResult;

public class MomentoCacheApplication {

  private static final String MOMENTO_AUTH_TOKEN = "<YOUR TEST AUTH TOKEN>";
  private static final String CACHE_NAME = "myCache";

  public static void main(String[] args) throws InterruptedException {
    System.out.println("Running Momento Cache Application");
    run();
  }

  private static void run() throws InterruptedException {
    Momento momento = Momento.builder(MOMENTO_AUTH_TOKEN).build();

    // Create or Get Cache
    System.out.println("Creating Cache with name: " + CACHE_NAME);
    try {
      momento.createCache(CACHE_NAME);
      System.out.println("Successfully created Cache with name: " + CACHE_NAME);
    } catch (CacheAlreadyExistsException e) {
      // Cache already exists, so use it.
      System.out.println("Found a cache with name: " + CACHE_NAME);
    }
    System.out.println("Initializing Cache Client for cache: " + CACHE_NAME);
    Cache cache = momento.getCache(CACHE_NAME);

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
            "Expected value `%s` but found `%s`", getWhatYouSetValue, getWhatYouSet.asStringUtf8());
    System.out.println(
        String.format(
            "Expected value `%s` ==  Looked up value `%s`",
            getWhatYouSetValue, getWhatYouSet.asStringUtf8().get()));

    // Test TTL Enforcement
    System.out.println("");
    System.out.println("Checking TTL Enforcement");
    Thread.sleep(ttl * 1000); // millisecond
    // Read the value and it must be a MISS!
    CacheGetResponse ttlResponse = cache.get(getWhatYouSetKey);
    assert ttlResponse.result() == MomentoCacheResult.Miss : "Item found, but it is past the TTL";
    assert ttlResponse.asStringUtf8().isPresent() == false
        : "Item value found, but it shouldn't be present";
    System.out.println("Item dropped from Cache after expiry!");
  }
}
