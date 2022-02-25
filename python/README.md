# Python Client SDK

_Read this in other languages_: [日本語](README.ja.md)

<br>

## Running the Example

- [Python 3.7 or above is required](https://www.python.org/downloads/)
- A Momento Auth Token is required, you can generate one using the [Momento CLI](https://github.com/momentohq/momento-cli)

```bash
python3 -m pip install -r requirements.txt
MOMENTO_AUTH_TOKEN=<YOUR_TOKEN> python3 example.py
```

To turn on SDK debug logs, run as follows:

```bash
DEBUG=true MOMENTO_AUTH_TOKEN=<YOUR_TOKEN> python3 example.py
```

## Using SDK in your project

Add `momento==0.9.1` to `requirements.txt` or any other dependency management framework used by your project.

To install directly to your system:

```bash
pip install momento==0.9.1
```
