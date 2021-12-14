---
apiVersion: v1
kind: Namespace
metadata:
  name: ${namespace}
---
apiVersion: platform.confluent.io/v1beta1
kind: Zookeeper
metadata:
  name: zookeeper
  namespace: ${namespace}
spec:
  dataVolumeCapacity: 10Gi
  image:
    application: confluentinc/cp-zookeeper:${cp_version}
    init: confluentinc/confluent-init-container:${cfk_version}
  logVolumeCapacity: 10Gi
  replicas: ${zookeeper_replicas}
  podTemplate:
    tolerations:
    - key: "confluent.io/location"
      operator: "Equal"
      value: "wavelength"
      effect: "NoSchedule"
    affinity:
      nodeAffinity:
        requiredDuringSchedulingIgnoredDuringExecution:
          nodeSelectorTerms:
          - matchExpressions:
            - key: "topology.kubernetes.io/zone"
              operator: In
              values: ["${zone}"]
---
apiVersion: platform.confluent.io/v1beta1
kind: Kafka
metadata:
  name: kafka
  namespace: ${namespace}
spec:
  configOverrides:
    server:
    - confluent.cluster.link.enable=true
  dataVolumeCapacity: 40Gi
  image:
    application: confluentinc/cp-server:${cp_version}
    init: confluentinc/confluent-init-container:${cfk_version}
  listeners:
    custom:
    - externalAccess:
        nodePort:
          host: np.${domain}
          nodePortOffset: ${nodeport_offset}
        type: nodePort
      name: np
      port: 9096
  metricReporter:
    enabled: true
  replicas: ${broker_replicas}
  podTemplate:
    tolerations:
    - key: "confluent.io/location"
      operator: "Equal"
      value: "wavelength"
      effect: "NoSchedule"
    affinity:
      nodeAffinity:
        requiredDuringSchedulingIgnoredDuringExecution:
          nodeSelectorTerms:
          - matchExpressions:
            - key: "topology.kubernetes.io/zone"
              operator: In
              values: ["${zone}"]
---
apiVersion: platform.confluent.io/v1beta1
kind: SchemaRegistry
metadata:
  name: schemaregistry
  namespace: ${namespace}
spec:
  image:
    application: confluentinc/cp-schema-registry:${cp_version}
    init: confluentinc/confluent-init-container:${cfk_version}
  replicas: 1
  podTemplate:
    tolerations:
    - key: "confluent.io/location"
      operator: "Equal"
      value: "wavelength"
      effect: "NoSchedule"
    affinity:
      nodeAffinity:
        requiredDuringSchedulingIgnoredDuringExecution:
          nodeSelectorTerms:
          - matchExpressions:
            - key: "topology.kubernetes.io/zone"
              operator: In
              values: ["${zone}"]