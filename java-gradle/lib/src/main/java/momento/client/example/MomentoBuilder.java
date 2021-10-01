package momento.client.example;

import momento.sdk.Momento;

/** Sample class to make a Momento Client */
public final class MomentoBuilder {

  private MomentoBuilder() {}

  public static void main(String[] args) {
    Momento momento =
        getMomento(MomentoConfiguration.getAuthToken(), MomentoConfiguration.getMomentoEndpoint());
  }

  public static Momento getMomento(String authToken, String endpointOverride) {
    return Momento.builder().authToken(authToken).endpointOverride(endpointOverride).build();
  }
}
