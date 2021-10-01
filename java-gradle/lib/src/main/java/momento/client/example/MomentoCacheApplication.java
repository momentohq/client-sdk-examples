package momento.client.example;

import momento.sdk.Cache;
import momento.sdk.Momento;
import momento.sdk.exceptions.CacheAlreadyExistsException;
import momento.sdk.messages.CacheGetResponse;
import momento.sdk.messages.MomentoCacheResult;

public class MomentoCacheApplication {

  private static final String MOMENTO_AUTH_TOKEN = "<TEST_AUTH_TOKEN>";
  private static final String MOMENTO_ENDPOINT =
      null; // Fill this in, only if provided by the Momento Team
  private static final String CACHE_NAME = "dummy";

  public static void main(String[] args) throws InterruptedException {
    run();
  }

  private static void run() throws InterruptedException {
    Momento momento =
        Momento.builder().authToken(MOMENTO_AUTH_TOKEN).endpointOverride(MOMENTO_ENDPOINT).build();

    // Create or Get Cache
    Cache cache = null;
    try {
      cache = momento.createCache(CACHE_NAME);
      System.out.println("Created a new Cache with name: " + CACHE_NAME);
    } catch (CacheAlreadyExistsException e) {
      // Cache already exists, so use it.
      System.out.println("Found a cache with name: " + CACHE_NAME);
      cache = momento.getCache(CACHE_NAME);
    }

    // Get what you set
    String getWhatYouSetKey = "MyFirstKey";
    String getWhatYouSetValue = "CacheMePlease";
    cache.set(getWhatYouSetKey, getWhatYouSetValue, 10);
    // Read the value and it must be a HIT!
    CacheGetResponse getWhatYouSet = cache.get(getWhatYouSetKey);
    assert getWhatYouSet.getResult() == MomentoCacheResult.Hit : "Expect Cache Hit, But was a Miss";
    System.out.println("Cache Hit!!");
    assert getWhatYouSet.asStringUtf8().equals(getWhatYouSetValue)
        : String.format(
            "Expected value `%s` but found `%s`", getWhatYouSetValue, getWhatYouSet.asStringUtf8());
    System.out.println(
        String.format(
            "Expected value `%s` ==  Looked up value `%s`",
            getWhatYouSetValue, getWhatYouSet.asStringUtf8()));

    // Test TTL Enforcement
    String enforceTTLKey = "EraseMeWhenItsDoneKey";
    cache.set(enforceTTLKey, "I am Ephemeral", 1);
    Thread.sleep(1000);
    CacheGetResponse ttlResponse = cache.get(enforceTTLKey);
    assert ttlResponse.getResult() == MomentoCacheResult.Miss
        : "Item found, but it is past the TTL";
    System.out.println("Item dropped from Cache after expiry!");
  }
}
