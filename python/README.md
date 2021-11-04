## Running the Example

- [Python 3.6 or above is required](https://www.python.org/downloads/)

```bash

python3 -m pip install -r requirements.txt --extra-index-url https://momento.jfrog.io/artifactory/api/pypi/pypi-public/simple

MOMENTO_AUTH_TOKEN=<YOUR_TOKEN> python example.py

```

## Using SDK in your project
Add the following to requirements.txt or any other dependency management framework used by your project
`momento==0.1.1`

The SDK is available at `https://momento.jfrog.io/artifactory/api/pypi/pypi-public/simple`, this can be configured using `--extra-index-url` option

e.g.
`pip install momento==0.1.1 --extra-index-url https://momento.jfrog.io/artifactory/api/pypi/pypi-public/simple`
