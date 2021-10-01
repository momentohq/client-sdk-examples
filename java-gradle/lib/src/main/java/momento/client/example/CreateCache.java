package momento.client.example;

import momento.sdk.Cache;
import momento.sdk.Momento;
import momento.sdk.exceptions.CacheServiceException;

public final class CreateCache {

  private CreateCache() {}

  public static Cache createCache(Momento momento, String cacheName) {
    try {
      return momento.createCache(cacheName);
    } catch (CacheServiceException e) {
      e.printStackTrace();
      System.out.println("Cache Creation failed");
    }
    return null;
  }
}
