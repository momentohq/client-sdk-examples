package momento.client.example;

import momento.sdk.Cache;
import momento.sdk.Momento;

public class MomentoCacheApplication {

  private static final String MOMENTO_AUTH_TOKEN = System.getenv("MOMENTO_AUTH_TOKEN");
  private static final String CACHE_NAME = "cache";
  private static final String KEY = "key";
  private static final String VALUE = "value";
  private static final int ITEM_TTL_SECONDS = 60;

  public static void main(String[] args) {
    System.out.println("Running Momento Cache Demo Application");
    try (Momento momento = Momento.builder(MOMENTO_AUTH_TOKEN).build()) {
      try (Cache cache =
          momento.cacheBuilder(CACHE_NAME, ITEM_TTL_SECONDS).createCacheIfDoesntExist().build()) {
        System.out.println(
            String.format("Storing key=%s value=%s w/ ttl=%ds", KEY, VALUE, ITEM_TTL_SECONDS));
        cache.set(KEY, VALUE);
        System.out.println(String.format("Looking up item for key=%s ", KEY));
        String resp = cache.get(KEY).string().get();
        assert resp.equals(VALUE);
        System.out.println(
            String.format("storedValue=%s is equal to lookedUpValue=%s ", VALUE, resp));
      }
    }
    System.out.println("Momento Cache Demo Application Done.");
  }
}
