package momento.client.example;

import momento.sdk.Cache;
import momento.sdk.Momento;
import momento.sdk.exceptions.CacheServiceException;
import momento.sdk.messages.CacheSetResponse;

public final class CacheSet {

  private CacheSet() {}

  public static Cache getCache(Momento momento, String cacheName) {
    return momento.getCache(cacheName);
  }

  public static CacheSetResponse set(Cache cache, String key, String value, int ttlSeconds) {
    try {
      CacheSetResponse response = cache.set(key, value, ttlSeconds);
      System.out.println("Set succeeded");
      return response;
    } catch (CacheServiceException e) {
      e.printStackTrace();
      System.exit(1);
    }
    return null;
  }
}
