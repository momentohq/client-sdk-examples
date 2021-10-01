package momento.client.example;

import momento.sdk.Cache;
import momento.sdk.Momento;

public class GetCache {

  public static void main(String[] args) {
    Momento momento =
        MomentoBuilder.getMomento(
            MomentoConfiguration.getAuthToken(), MomentoConfiguration.getMomentoEndpoint());
    Cache cache = getCache(momento, MomentoConfiguration.getMomentoCacheName());
    System.out.println(
        String.format(
            "Created a client to interact with cache named: %s",
            MomentoConfiguration.getMomentoCacheName()));
  }

  public static Cache getCache(Momento momento, String cacheName) {
    return momento.getCache(cacheName);
  }
}
