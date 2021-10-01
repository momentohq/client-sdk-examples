package momento.client.example;

import momento.sdk.Momento;

/** Sample class to make a Momento Client */
public final class MomentoClient {

  private MomentoClient() {}

  public static Momento getMomentoClient(String authToken, String endpointOverride) {
    return Momento.builder().authToken(authToken).endpointOverride(endpointOverride).build();
  }
}
