package main

import (
	"fmt"
	"os"

	"github.com/google/uuid"
	"github.com/momentohq/client-sdk-go/momento"
)

func main() {
	var AuthToken = os.Getenv("MOMENTO_AUTH_TOKEN")
	const (
		CacheName             = "momentocache"
		ItemDefaultTtlSeconds = 60
	)

	// Initializes Momento
	client, err := momento.SimpleCacheClient(&momento.SimpleCacheClientRequest{
		AuthToken:         AuthToken,
		DefaultTtlSeconds: ItemDefaultTtlSeconds,
	})
	if err != nil {
		fmt.Println(err.Error())
	} else {
		// Create Cache and check if CacheName exists
		err := client.CreateCache(&momento.CreateCacheRequest{
			CacheName: CacheName,
		})
		if err != nil {
			fmt.Println(err.Error())
			os.Exit(1)
		}
	}

	// Sets key with default TTL and gets value with that key
	key := []byte(uuid.NewString())
	value := []byte(uuid.NewString())
	fmt.Printf("Setting key: %s, value: %s\n", key, value)
	_, err = client.Set(&momento.CacheSetRequest{
		CacheName: CacheName,
		Key:       key,
		Value:     value,
	})
	if err != nil {
		fmt.Println(err.Error())
		os.Exit(1)
	}
	fmt.Printf("Getting key: %s", key)
	resp, err := client.Get(&momento.CacheGetRequest{
		CacheName: CacheName,
		Key:       key,
	})
	if err != nil {
		fmt.Println(err.Error())
		os.Exit(1)
	} else {
		fmt.Printf("Lookup resulted in a : %s\n", resp.Result())
		fmt.Printf("Looked up value: %s\n", resp.StringValue())
	}

	// Permanently delete the cache
	client.DeleteCache(&momento.DeleteCacheRequest{CacheName: CacheName})
}
