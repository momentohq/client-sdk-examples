package momento.client.example;

import momento.sdk.Cache;
import momento.sdk.Momento;

public class GetCache {

  public static Cache getCache(Momento momento, String cacheName) {
    return momento.getCache(cacheName);
  }
}
