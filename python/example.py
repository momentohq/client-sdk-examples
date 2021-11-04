import os
import momento.momento as momento

_MOMENTO_AUTH_TOKEN = os.getenv('MOMENTO_AUTH_TOKEN')
_CACHE_NAME = 'cache'
_KEY = 'MyKey'
_VALUE = 'MyValue'

if __name__ == "__main__":
    with momento.init(_MOMENTO_AUTH_TOKEN) as momento_client:
        with momento_client.get_cache(_CACHE_NAME, ttl_seconds=60, create_if_absent=True) as cache_client:
            print('Setting Key: ' + _KEY + ' Value: ' + _VALUE)
            cache_client.set(_KEY, _VALUE)
            print('Getting Key: ' + _KEY)
            get_resp = cache_client.get('MyKey')
            print('Look up resulted in a : ' + str(get_resp.result()))
            print('Looked up Value: ' + get_resp.str_utf8())
