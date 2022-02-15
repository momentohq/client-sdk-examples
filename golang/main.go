package main

import (
	"log"
	"os"
	"strings"

	"github.com/google/uuid"
	"github.com/momentohq/client-sdk-go/momento"
)

func main() {
	var AuthToken = os.Getenv("MOMENTO_AUTH_TOKEN")
	const (
		CacheName             = "momentocache"
		ItemDefaultTtlSeconds = 60
	)

	if AuthToken == "" {
		log.Fatal("Missing required environment variable MOMENTO_AUTH_TOKEN")
	}

	// Initializes Momento
	client, err := momento.SimpleCacheClient(&momento.SimpleCacheClientRequest{
		AuthToken:         AuthToken,
		DefaultTtlSeconds: ItemDefaultTtlSeconds,
	})
	if err != nil {
		panic(err)
	} else {
		// Create Cache and check if CacheName exists
		err := client.CreateCache(&momento.CreateCacheRequest{
			CacheName: CacheName,
		})
		if err != nil && !strings.Contains(err.Error(), "AlreadyExists") {
			panic(err)
		}
	}

	// Sets key with default TTL and gets value with that key
	key := []byte(uuid.NewString())
	value := []byte(uuid.NewString())
	log.Printf("Setting key: %s, value: %s\n", key, value)
	_, err = client.Set(&momento.CacheSetRequest{
		CacheName: CacheName,
		Key:       key,
		Value:     value,
	})
	if err != nil {
		panic(err)
	}
	log.Printf("Getting key: %s\n", key)
	resp, err := client.Get(&momento.CacheGetRequest{
		CacheName: CacheName,
		Key:       key,
	})
	if err != nil {
		panic(err)
	} else {
		log.Printf("Lookup resulted in a : %s\n", resp.Result())
		log.Printf("Looked up value: %s\n", resp.StringValue())
	}

	// Permanently delete the cache
	err = client.DeleteCache(&momento.DeleteCacheRequest{CacheName: CacheName})
	if err != nil {
		panic(err)
	}
}
