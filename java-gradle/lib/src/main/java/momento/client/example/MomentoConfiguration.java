package momento.client.example;

public final class MomentoConfiguration {

  private static final String MOMENTO_AUTH_TOKEN = "<YOUR AUTH TOKEN HERE>";
  private static final String MOMENTO_ENDPOINT = null;
  private static final String MOMENTO_CACHE_NAME = "myCache";

  public static final String getAuthToken() {
    return MOMENTO_AUTH_TOKEN;
  }

  public static final String getMomentoEndpoint() {
    return MOMENTO_ENDPOINT;
  }

  public static final String getMomentoCacheName() {
    return MOMENTO_CACHE_NAME;
  }
}
