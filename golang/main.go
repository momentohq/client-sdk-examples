package main

import (
	"log"
	"os"
	"strings"

	"github.com/google/uuid"
	"github.com/momentohq/client-sdk-go/momento"
)

func main() {
	var authToken = os.Getenv("MOMENTO_AUTH_TOKEN")
	const (
		cacheName             = "cache"
		itemDefaultTtlSeconds = 60
	)

	if authToken == "" {
		log.Fatal("Missing required environment variable MOMENTO_AUTH_TOKEN")
	}

	// Initializes Momento
	client, err := momento.NewSimpleCacheClient(authToken, itemDefaultTtlSeconds)
	if err != nil {
		panic(err)
	}

	// Create Cache and check if CacheName exists
	err = client.CreateCache(&momento.CreateCacheRequest{
		CacheName: cacheName,
	})
	if err != nil && !strings.Contains(err.Error(), momento.AlreadyExists) {
		panic(err)
	}
	log.Printf("Cache named %s is created\n", cacheName)

	// Sets key with default TTL and gets value with that key
	key := []byte(uuid.NewString())
	value := []byte(uuid.NewString())
	log.Printf("Setting key: %s, value: %s\n", key, value)
	_, err = client.Set(&momento.CacheSetRequest{
		CacheName: cacheName,
		Key:       key,
		Value:     value,
	})
	if err != nil {
		panic(err)
	}
	log.Printf("Getting key: %s\n", key)
	resp, err := client.Get(&momento.CacheGetRequest{
		CacheName: cacheName,
		Key:       key,
	})
	if err != nil {
		panic(err)
	}
	log.Printf("Lookup resulted in a : %s\n", resp.Result())
	log.Printf("Looked up value: %s\n", resp.StringValue())

	// Permanently delete the cache
	err = client.DeleteCache(&momento.DeleteCacheRequest{CacheName: cacheName})
	if err != nil {
		panic(err)
	}
	log.Printf("Cache named %s is deleted\n", cacheName)
}
