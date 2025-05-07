# NeuroViz Python API

[![PyPI version](https://badge.fury.io/py/neuroviz.svg)](https://badge.fury.io/py/neuroviz)

## Installation

```bash
pip install neuroviz
```

## Quick Start

```python
from time import sleep
from neuroviz import NeuroViz

# Create a NeuroViz instance (starts an HTTP server)
neuro = NeuroViz(port=9001, use_secret=False)

# Modify parameters and update visualization in real-time
neuro.set_live_parameters({
    "transparency": 0.5,
    "glow": 0.2,
    "smoothness": 0.8,
    "emission": 0.3,
    "light_intensity": 1.0,
    "light_temperature": 6500.0
})

sleep(100) # Keep the server running for 100 seconds
```

## Usage Examples

- Combinations: [examples/combinations.py](examples/combinations.py)
- Usage in jupyter notebook: [examples/notebook.ipynb](examples/notebook.ipynb)
