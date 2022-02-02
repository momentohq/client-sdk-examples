import asyncio
import logging
import os

import momento.aio.simple_cache_client as simple_cache_client
import momento.errors as errors

_MOMENTO_AUTH_TOKEN = os.getenv('MOMENTO_AUTH_TOKEN')
_CACHE_NAME = 'cache'
_ITEM_DEFAULT_TTL_SECONDS = 60
_KEY = 'MyKey'
_VALUE = 'MyValue'
_DEBUG_MODE = os.getenv('DEBUG')

if _DEBUG_MODE == 'true':
    logger = logging.getLogger('momentosdk')
    logger.setLevel(logging.DEBUG)

    consoleHandler = logging.StreamHandler()
    consoleHandler.setLevel(logging.DEBUG)
    logger.addHandler(consoleHandler)

    formatter = logging.Formatter(
        '%(asctime)s  %(name)s  %(levelname)s: %(message)s'
    )
    consoleHandler.setFormatter(formatter)


def _print_start_banner():
    print("******************************************************************")
    print("*                      Momento Example Start                     *")
    print("******************************************************************")


def _print_end_banner():
    print("******************************************************************")
    print("*                       Momento Example End                      *")
    print("******************************************************************")


async def _create_cache(simple_cache_client, cache_name):
    try:
        await simple_cache_client.create_cache(cache_name)
    except errors.AlreadyExistsError:
        print(f'Cache with name: `{cache_name}` already exists.')


async def main():
    _print_start_banner()
    async with simple_cache_client.init(
        _MOMENTO_AUTH_TOKEN, _ITEM_DEFAULT_TTL_SECONDS
    ) as cache_client:
        await _create_cache(cache_client, _CACHE_NAME)
        print(f'Setting Key: {_KEY} Value: {_VALUE}')
        await cache_client.set(_CACHE_NAME, _KEY, _VALUE)
        print(f'Getting Key: {_KEY}')
        get_resp = await cache_client.get(_CACHE_NAME, _KEY)
        print(f'Look up resulted in a : {get_resp.status()}')
        print(f'Looked up Value: {get_resp.value()}')
    _print_end_banner()


if __name__ == "__main__":
    asyncio.run(main())
