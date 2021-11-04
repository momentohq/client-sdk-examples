## Running the Example

- [Python 3.6 or above is required](https://www.python.org/downloads/)
- [Setting up virtual environments is recommended](https://packaging.python.org/guides/installing-using-pip-and-virtual-environments/)


```bash
python3 -m venv momento-python-examples

source momento-python-examples/bin/activate

pip install -r requirements.txt --extra-index-url https://momento.jfrog.io/artifactory/api/pypi/pypi-public/simple

MOMENTO_AUTH_TOKEN=<YOUR_TOKEN> python example.py
```

## Using SDK in your project
`pip install momento===0.1.1 --extra-index-url https://momento.jfrog.io/artifactory/api/pypi/pypi-public/simple`
