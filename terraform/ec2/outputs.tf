output "bastion_private_ip" {
  value = aws_instance.bastion_host_instance.*.private_ip
  description= "The Private IP address of the bastion host in the parent region"
}

output "bastion_public_ip" {
  value = aws_instance.bastion_host_instance.*.public_ip
  description= "The Public IP address of the bastion host in the parent region"
}

output "wavelength_private_ip" {
  value = aws_instance.edge_instance.private_ip
  description= "The Private IP address of the edge instance in the Wavelength Zone"
}

output "wavelength_carrier_ip" {
  value = aws_eip.tf-wavelength-ip.carrier_ip
  description= "The Carrier IP address of the edge instance in the Wavelength Zone"
}

