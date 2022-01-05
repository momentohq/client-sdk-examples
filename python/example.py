import os
import momento.momento as momento
import logging


_MOMENTO_AUTH_TOKEN = os.getenv('MOMENTO_AUTH_TOKEN')
_CACHE_NAME = 'dhashdjas'
_KEY = 'MyKey'
_VALUE = 'MyValue'
_DEBUG_MODE = os.getenv('DEBUG')


if (_DEBUG_MODE == 'true'):
    logger = logging.getLogger('momentosdk')
    logger.setLevel(logging.DEBUG)

    consoleHandler = logging.StreamHandler()
    consoleHandler.setLevel(logging.DEBUG)
    logger.addHandler(consoleHandler)

    formatter = logging.Formatter('%(asctime)s  %(name)s  %(levelname)s: %(message)s')
    consoleHandler.setFormatter(formatter)


def _print_start_banner():
    print('******************************************************************')
    print('*                      Momento Example Start                     *')
    print('******************************************************************')

def _print_end_banner():
    print('******************************************************************')
    print('*                       Momento Example End                      *')
    print('******************************************************************')


if __name__ == "__main__":
    _print_start_banner()
    with momento.init(_MOMENTO_AUTH_TOKEN) as momento_client:
        with momento_client.get_cache(_CACHE_NAME, ttl_seconds=60, create_if_absent=True) as cache_client:
            print('Setting Key: ' + _KEY + ' Value: ' + _VALUE)
            cache_client.set(_KEY, _VALUE)
            print('Getting Key: ' + _KEY)
            get_resp = cache_client.get('MyKey')
            print('Look up resulted in a : ' + str(get_resp.result()))
            print('Looked up Value: ' + get_resp.str_utf8())
    _print_end_banner()
