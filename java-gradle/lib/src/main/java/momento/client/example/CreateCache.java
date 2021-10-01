package momento.client.example;

import momento.sdk.Cache;
import momento.sdk.Momento;
import momento.sdk.exceptions.CacheServiceException;

public final class CreateCache {

  private CreateCache() {}

  public static void main(String[] args) {
    Momento momento =
        MomentoBuilder.getMomento(
            MomentoConfiguration.getAuthToken(), MomentoConfiguration.getMomentoEndpoint());
    createCache(momento, MomentoConfiguration.getMomentoCacheName());
    System.out.println(
        String.format(
            "CacheName: %s created successfully", MomentoConfiguration.getMomentoCacheName()));
  }

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
