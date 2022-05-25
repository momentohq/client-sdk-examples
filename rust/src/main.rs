use momento::response::cache_get_response::MomentoGetStatus;
use momento::simple_cache_client::SimpleCacheClient;
use std::env;
use std::num::NonZeroU64;
use std::process;

#[tokio::main]
async fn main() {
    // Initializing Momento
    let auth_token =
        env::var("MOMENTO_AUTH_TOKEN").expect("env var MOMENTO_AUTH_TOKEN must be set");
    let item_default_ttl_seconds = 60;
    let mut cache_client = match SimpleCacheClient::new(
        auth_token,
        NonZeroU64::new(item_default_ttl_seconds).unwrap(),
    )
    .await
    {
        Ok(c) => c,
        Err(err) => {
            eprintln!("{}", err);
            process::exit(1);
        }
    };

    // Creating a cache named "cache"
    let cache_name = String::from("cache");
    match cache_client.create_cache(&cache_name).await {
        Ok(_) => {}
        Err(err) => {
            eprintln!("{}", err);
        }
    }

    // List the caches
    println!("Listing caches:");
    match cache_client.list_caches(None).await {
        Ok(list_cache_result) => {
            for listed_cache in list_cache_result.caches {
                println!("{}", listed_cache.cache_name);
            }
            println!();
        }
        Err(err) => {
            eprintln!("{}", err);
        }
    }

    // Sets key with default TTL and get value with that key
    let key = String::from("my_key");
    let value = String::from("my_value");
    println!("Setting key: {}, value: {}", key, value);
    match cache_client
        .set(&cache_name, key.clone(), value.clone(), None)
        .await
    {
        Ok(_) => {}
        Err(err) => {
            eprintln!("{}", err);
        }
    };
    match cache_client.get(&cache_name, key.clone()).await {
        Ok(r) => match r.result {
            MomentoGetStatus::HIT => println!("cache hit!"),
            MomentoGetStatus::MISS => println!("cache miss"),
            _ => println!("error occurred"),
        },
        Err(err) => {
            eprintln!("{}", err);
        }
    };
    // Permanently deletes cache
    match cache_client.delete_cache(&cache_name).await {
        Ok(_) => {
            println!("Permanently deleted cache named, {}", cache_name);
        }
        Err(err) => {
            eprintln!("{}", err);
        }
    };
}
