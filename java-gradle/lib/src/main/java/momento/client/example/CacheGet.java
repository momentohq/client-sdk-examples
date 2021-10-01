package momento.client.example;

import momento.sdk.Cache;
import momento.sdk.exceptions.CacheServiceException;
import momento.sdk.messages.CacheGetResponse;
import momento.sdk.messages.MomentoCacheResult;

public class CacheGet {

  private CacheGet() {}

  public static CacheGetResponse get(Cache cache, String key) {
    try {
      CacheGetResponse response = cache.get(key);
      if (response.getResult() == MomentoCacheResult.Hit) {
        System.out.println(String.format("Cache Hit: found value: %s", response.asStringUtf8()));
      } else {
        System.out.println(String.format("No value found for key: %s", key));
      }
      return response;
    } catch (CacheServiceException e) {
      e.printStackTrace();
      System.exit(1);
    }
    return null;
  }
}
