---
apiVersion: platform.confluent.io/v1beta1
kind: Zookeeper
metadata:
  name: zookeeper
  namespace: confluent
spec:
  dataVolumeCapacity: 10Gi
  image:
    application: confluentinc/cp-zookeeper:${cp_version}
    init: confluentinc/confluent-init-container:${cfk_version}
  logVolumeCapacity: 10Gi
  replicas: ${zookeeper_replicas}
---
apiVersion: platform.confluent.io/v1beta1
kind: Kafka
metadata:
  name: kafka
  namespace: confluent
spec:
  configOverrides:
    server:
    - confluent.cluster.link.enable=true
  dataVolumeCapacity: 50Gi
  image:
    application: confluentinc/cp-server:${cp_version}
    init: confluentinc/confluent-init-container:${cfk_version}
  listeners:
    custom:
    - externalAccess:
        nodePort:
          host: np.${domain}
          nodePortOffset: 31000
        type: nodePort
      name: np
      port: 9096
    external:
      externalAccess:
        loadBalancer:
          domain: kafka.${domain}
        type: loadBalancer
  metricReporter:
    enabled: true
  replicas: ${broker_replicas}
---
apiVersion: platform.confluent.io/v1beta1
kind: SchemaRegistry
metadata:
  name: schemaregistry
  namespace: confluent
spec:
  image:
    application: confluentinc/cp-schema-registry:${cp_version}
    init: confluentinc/confluent-init-container:${cfk_version}
  replicas: 1
---
apiVersion: platform.confluent.io/v1beta1
kind: Connect
metadata:
  name: connect
  namespace: confluent
spec:
  dependencies:
    kafka:
      bootstrapEndpoint: kafka:9071
  image:
    application: confluentinc/cp-server-connect:${cp_version}
    init: confluentinc/confluent-init-container:${cfk_version}
  replicas: 1
---
apiVersion: platform.confluent.io/v1beta1
kind: KsqlDB
metadata:
  name: ksqldb
  namespace: confluent
spec:
  dataVolumeCapacity: 10Gi
  image:
    application: confluentinc/cp-ksqldb-server:${cp_version}
    init: confluentinc/confluent-init-container:${cfk_version}
  replicas: 1
---
apiVersion: platform.confluent.io/v1beta1
kind: ControlCenter
metadata:
  name: controlcenter
  namespace: confluent
spec:
  dataVolumeCapacity: 10Gi
  dependencies:
    connect:
    - name: connect
      url: http://connect.confluent.svc.cluster.local:8083
    ksqldb:
    - name: ksqldb
      url: http://ksqldb.confluent.svc.cluster.local:8088
    schemaRegistry:
      url: http://schemaregistry.confluent.svc.cluster.local:8081
  image:
    application: confluentinc/cp-enterprise-control-center:${cp_version}
    init: confluentinc/confluent-init-container:${cfk_version}
  replicas: 1