import os
import momento.simple_cache_client as scc
import momento.errors as errors
import logging

_MOMENTO_AUTH_TOKEN = os.getenv("MOMENTO_AUTH_TOKEN")
_CACHE_NAME = "cache"
_ITEM_DEFAULT_TTL_SECONDS = 60
_KEY = "MyKey"
_VALUE = "MyValue"
_DEBUG_MODE = os.getenv("DEBUG")

if _DEBUG_MODE == "true":
    logger = logging.getLogger("momentosdk")
    logger.setLevel(logging.DEBUG)

    consoleHandler = logging.StreamHandler()
    consoleHandler.setLevel(logging.DEBUG)
    logger.addHandler(consoleHandler)

    formatter = logging.Formatter("%(asctime)s  %(name)s  %(levelname)s: %(message)s")
    consoleHandler.setFormatter(formatter)


def _print_start_banner() -> None:
    print("******************************************************************")
    print("*                      Momento Example Start                     *")
    print("******************************************************************")


def _print_end_banner() -> None:
    print("******************************************************************")
    print("*                       Momento Example End                      *")
    print("******************************************************************")


def _create_cache(cache_client: scc.SimpleCacheClient, cache_name: str) -> None:
    try:
        cache_client.create_cache(cache_name)
    except errors.AlreadyExistsError:
        print(f"Cache with name: {cache_name!r} already exists.")


def _list_caches(cache_client: scc.SimpleCacheClient) -> None:
    print("Listing caches:")
    list_cache_result = cache_client.list_caches()
    while True:
        for cache_info in list_cache_result.caches():
            print(f"- {cache_info.name()!r}")
        next_token = list_cache_result.next_token()
        if next_token is None:
            break
        list_cache_result = cache_client.list_caches(next_token)
    print()


if __name__ == "__main__":
    _print_start_banner()
    with scc.init(
        _MOMENTO_AUTH_TOKEN, _ITEM_DEFAULT_TTL_SECONDS
    ) as cache_client:
        _create_cache(cache_client, _CACHE_NAME)
        _list_caches(cache_client)

        print(f"Setting Key: {_KEY!r} Value: {_VALUE!r}")
        cache_client.set(_CACHE_NAME, _KEY, _VALUE)

        print(f"Getting Key: {_KEY!r}")
        get_resp = cache_client.get(_CACHE_NAME, _KEY)
        print(f"Look up resulted in a : {str(get_resp.status())}")
        print(f"Looked up Value: {str(get_resp.value())!r}")
    _print_end_banner()
