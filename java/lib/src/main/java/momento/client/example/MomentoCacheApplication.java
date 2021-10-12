package momento.client.example;

import momento.sdk.Cache;
import momento.sdk.Momento;

public class MomentoCacheApplication {

  private static final String MOMENTO_AUTH_TOKEN = System.getenv("MOMENTO_AUTH_TOKEN");

  public static void main(String[] args) {
    String cacheName = "cache";
    String key = "key";
    String value = "value";
    int itemTtlSeconds = 10;
    System.out.println("Running Momento Cache Application");

    try(Momento momento = Momento.builder(MOMENTO_AUTH_TOKEN).build()) {
      try(Cache cache = momento.createOrGetCache(cacheName)) {
        cache.set(key, value, itemTtlSeconds); // key -> value with 10 second TTL
        String resp = cache.get(key).asStringUtf8().get();
        assert resp.equals(resp);
      }
    }

  }
}
