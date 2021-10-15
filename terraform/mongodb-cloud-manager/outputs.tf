output "nodes_private_IPs" {
  value = aws_instance.AgentNode.*.private_ip
  description = "The Private IP addresses of your MongoDB database nodes."
}

output "database_nodes" {
  value = aws_eip.mongodb_node_eip.*.carrier_ip
  description = "The Carrier IP addresses of your MongoDB database nodes."
}

output "node_hostnames" {
  value = aws_instance.AgentNode.*.private_dns
  description = "The hostnames (Private DNS names) of your MongoDB database nodes."
}

