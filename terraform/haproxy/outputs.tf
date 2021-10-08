output "nodes_private_IPs" {
  value = aws_instance.hapee_node.*.private_ip
}

output "node1_interface_allocs_id" {
  value = aws_eip.hapee_node_eip1.*.id
}

output "node1_interface_public_IPs" {
  value = aws_eip.hapee_node_eip1.*.carrier_ip
}

output "node_primary_interface_IDs" {
  value = aws_instance.hapee_node.*.primary_network_interface_id
}

