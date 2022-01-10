# Redis <> 5G Edge Integration
Infrastructure templates for robust in-memory databases at the network edge. 

## Installation
Use the package manager [pip](https://pip.pypa.io/en/stable/) to install Redis module for testing endpoint connectivity.

```bash
pip3 install redis
```

## Usage
For private clusters, run the `private-redis-cluster.yaml` file in CloudFormation.

```bash
aws cloudformation create-stack --stack-name Redis_5GEdge_Private_Cluster --template-body file://private-redis-cluster.yaml  --parameters ParameterKey=EnvironmentName,ParameterValue=Verizon5GEdge --capabilities CAPABILITY_IAM
```

Next, from a Wavelength Zone, utilize Sentinel to connect to an available node. From your client, ensure that pip3 is installed (`yum install python3-pip`) and that the redis client is installed as well (`pip3 install redis`).


```python
# In this example, the database was named edgeDB on creation
from redis.sentinel import Sentinel
sentinel = Sentinel([('localhost', 8001)], socket_timeout=0.1)
print(sentinel.discover_master('edgeDB'))

master = sentinel.master_for('edgeDB', socket_timeout=0.1)
print(master.set('foo', 'bar'))
print(master.get('foo'))
```
