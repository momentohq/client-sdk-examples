package momento.client.example;

import org.momento.scs.ClientGetResponse;
import org.momento.scs.MomentoResult;
import org.momento.scs.ScsClient;

import java.io.IOException;
import java.nio.ByteBuffer;
import java.nio.charset.StandardCharsets;
import java.util.Optional;

public class Library {

    // Should be stored carefully along with other secrets
    public static final String API_KEY = "<YOUR API KEY>";

    public boolean someLibraryMethod() {
        ScsClient client = new ScsClient("FOO", "CACHE_ID");
        // TODO: flesh out example!
        return true;
    }

    private static void runExample() throws IOException {
        // Actual code once ready
        //
        // MomentoClient momentoClient = new MomentoClient(API_KEY, Momento.Regions.US_WEST_2);
        //
        // Create a Cache
        // CreateCacheResponse createResponse = momentoClient.createCache(cacheName);
        //
        //
        // ScsClient cacheClient = momentoClient.getCache(cacheName);

        // ------- Begin makeshift code --------
        // Hardcoded Cache ID Key
        String cacheId = "CacheId";
        // Hardcoded Endoint, will not be needed and should just come from getCache
        String endpoint = "beta.cacheservice.com";
        ScsClient cacheClient = new ScsClient(API_KEY, cacheId, Optional.empty(), endpoint);
        // ------- End makeshift code ---------

        // Get What you Set
        String setValue = "Cache me If you can";
        cacheClient.set("gk007", ByteBuffer.wrap(setValue.getBytes(StandardCharsets.UTF_8)), 900);
        ClientGetResponse<ByteBuffer> response = cacheClient.get("gk007");
        assert response.getResult() == MomentoResult.Hit;
        String getValue = new String(response.getBody().array(), StandardCharsets.UTF_8);
        assert getValue == setValue;

    }

    public static void main(String[] args) throws IOException {
        runExample();
    }
}
