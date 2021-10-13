package momento.client.example;

import momento.sdk.Cache;
import momento.sdk.Momento;

public class MomentoCacheApplication {

  private static final String MOMENTO_AUTH_TOKEN = System.getenv("MOMENTO_AUTH_TOKEN");
  private static final String CACHE_NAME = "cache";
  private static final String KEY = "key";
  private static final String VALUE = "value";
  private static final int ITEM_TTL_SECONDS = 10;

  public static void main(String[] args) {
    System.out.println("Running Momento Cache Demo Application");
    try (Momento momento = Momento.builder(MOMENTO_AUTH_TOKEN).build()) {
      try (Cache cache = momento.getOrCreateCache(CACHE_NAME)) {
        cache.set(KEY, VALUE, ITEM_TTL_SECONDS);
        String resp = cache.get(KEY).string().get();
        assert resp.equals(VALUE);
      }
    }
  }
}
