package momento.client.example.advanced;

import java.util.*;
import momento.sdk.Cache;
import momento.sdk.Momento;
import momento.sdk.messages.CacheGetResponse;

/**
 * Example application that shows how to handle Cache lookups and cache misses.
 *
 * <p>An in-memory hashmap based database stores items(item_id, item_name). The example looks to
 * fetch a given item from cache first. If it is hit, it returns the data. On a cache miss, attempts
 * to read data from Database and then add the data to cache. Any future lookups within the bounds
 * set by TTL will result in a cache hit.
 */
public class MomentoCacheWithDatabase {

  private static final String MOMENTO_AUTH_TOKEN = "<YOUR TOKEN HERE>";
  private static final String CACHE_NAME = "cache";
  private static final String ITEM_NOT_FOUND_MESSAGE = "Not Found in Cache or Database";
  private static final List<String> itemIds = Arrays.asList("1", "20");

  public static void main(String[] args) {
    Database database = new DatabaseImpl();
    try (Momento momento = Momento.builder(MOMENTO_AUTH_TOKEN).build()) {
      try (Cache cache = momento.getOrCreateCache(CACHE_NAME)) {
        runExample(cache, database);
      }
    }
  }

  private static void runExample(Cache cache, Database database) {
    for (String itemId : itemIds) {
      System.out.println(String.format("Initiating Lookup for item id: %s", itemId));
      String result = lookup(itemId, cache, database).orElse(ITEM_NOT_FOUND_MESSAGE);
      System.out.println(String.format("Item id: %s Item: %s", itemId, result));

      // Item was found in Database or Cache the second look up should be a cache hit.
      if (!result.equals(ITEM_NOT_FOUND_MESSAGE)) {
        System.out.println(String.format("\n \nLookup Item id: %s again.", itemId));

        String secondLookup =
                lookup(itemId, cache, database)
                        .orElseThrow(
                                () -> new AssertionError("Item should be present."));
        System.out.println(String.format("Look up for Item id: %s Item: %s", itemId, secondLookup));
      }

      System.out.println(String.format("Done looking up item id: %s \n \n", itemId));
    }
  }

  // Handle cache lookups and fallback to database when item isn't found
  private static Optional<String> lookup(String itemId, Cache cache, Database database) {
    CacheGetResponse response = cache.get(itemId);
    writeCacheLog(response, itemId);
    return response.string().or(() -> handleCacheMiss(itemId, cache, database));
  }

  // Handle Cache Miss
  // Lookup the item in database and if found, add the item to cache.
  private static Optional<String> handleCacheMiss(String itemId, Cache cache, Database database) {
    Optional<String> item = database.getItem(itemId);
    if (item.isPresent()) {
      cache.set(itemId, item.get(), 60);
      System.out.println(
          String.format(
              "Item stored with key: %s and value: %s stored in Cache", itemId, item.get()));
    }
    return item;
  }

  private static void writeCacheLog(CacheGetResponse response, String key) {
    System.out.println(
        String.format("Cache lookup up for item id: %s resulted in : %s", key, response.result()));
  }
}

/** Simple in-memory database */
class DatabaseImpl implements Database {
  private final Map<String, String> data;

  DatabaseImpl() {
    data = buildDatabase();
  }

  @Override
  public Optional<String> getItem(String itemId) {
    if (data.containsKey(itemId)) {
      System.out.println(String.format("Item with id: %s found in database", itemId));
      return Optional.ofNullable(data.get(itemId));
    }
    System.out.println(String.format("Item with id: %s not found in database", itemId));
    return Optional.empty();
  }

  private static Map<String, String> buildDatabase() {
    Map<String, String> data = new HashMap<>();
    data.put("1", "Bananas");
    data.put("2", "Apples");
    data.put("3", "Mangoes");
    data.put("4", "Watermelon");
    return data;
  }
}

interface Database {
  Optional<String> getItem(String itemId);
}
