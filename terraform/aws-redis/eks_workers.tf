# TODO: Move some of this to managed node groups, which support custom launch templates

locals {
  userdata = <<-EOT
    #!/bin/bash
    set -o xtrace
    /etc/eks/bootstrap.sh ${module.eks_cluster.cluster_id}
  EOT
}

# Create security group for edge resources
resource "aws_security_group_rule" "edge_confluent_1" {
  type              = "ingress"
  from_port         = 31000
  to_port           = 31003
  protocol          = "tcp"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = module.eks_cluster.worker_security_group_id
}

resource "aws_launch_template" "worker_launch_template" {
  name = "${var.cluster_name}-wl-workers"
  image_id      = lookup(var.worker_image_id, var.region)
  instance_type = var.worker_instance_type
  key_name      = var.worker_key_name

  network_interfaces {
    associate_carrier_ip_address = true
    security_groups              = [module.eks_cluster.worker_security_group_id]
  }

  block_device_mappings {
    device_name = "/dev/xvda"

    ebs {
      volume_size = var.worker_volume_size
    }
  }

  iam_instance_profile {
    arn = aws_iam_instance_profile.worker_role.arn
  }

  metadata_options {
    http_endpoint               = "enabled"
    http_put_response_hop_limit = 2
    http_tokens                 = "required"
  }

  user_data = base64encode(local.userdata)
}

# Create one ASG for each Wavelength subnet
resource "aws_autoscaling_group" "wavelength_workers" {
  for_each = var.wavelength_zones

  name             = "${var.cluster_name}-wl-workers-${each.key}"
  max_size         = 10
  min_size         = 0
  desired_capacity = each.value.worker_nodes
  # health_check_grace_period = 300
  # health_check_type         = "ELB"

  vpc_zone_identifier = [aws_subnet.wavelength_subnets[each.key].id]

  launch_template {
    id      = aws_launch_template.worker_launch_template.id
    version = "$Latest"
  }

  tag {
    key                 = "Name"
    value               = "${module.eks_cluster.cluster_id}-${var.worker_nodegroup_name}-Node-${each.key}"
    propagate_at_launch = true
  }

  tag {
    value               = "owned"
    key                 = "kubernetes.io/cluster/${module.eks_cluster.cluster_id}"
    propagate_at_launch = true
  }
}

resource "aws_launch_template" "region_launch_template" {
  name = "${var.cluster_name}-workers"
  image_id      = lookup(var.worker_image_id, var.region)
  instance_type = var.worker_instance_type
  key_name      = var.worker_key_name

  network_interfaces {
    associate_public_ip_address = true
    security_groups              = [module.eks_cluster.worker_security_group_id]
  }

  block_device_mappings {
    device_name = "/dev/xvda"

    ebs {
      volume_size = var.worker_volume_size
    }
  }

  iam_instance_profile {
    arn = aws_iam_instance_profile.worker_role.arn
  }

  metadata_options {
    http_endpoint               = "enabled"
    http_put_response_hop_limit = 2
    http_tokens                 = "required"
  }

  user_data = base64encode(local.userdata)
}

# Create one ASG for all parent region subnets
resource "aws_autoscaling_group" "region_workers" {

  name             = "${var.cluster_name}-region-workers"
  max_size         = 10
  min_size         = 0
  desired_capacity = 2
  # health_check_grace_period = 300
  # health_check_type         = "ELB"

  vpc_zone_identifier = [for subnet in aws_subnet.region_subnets: subnet.id]

  launch_template {
    id      = aws_launch_template.region_launch_template.id
    version = "$Latest"
  }

  tag {
    key                 = "Name"
    value               = "${module.eks_cluster.cluster_id}-Region-Node"
    propagate_at_launch = true
  }

  tag {
    value               = "owned"
    key                 = "kubernetes.io/cluster/${module.eks_cluster.cluster_id}"
    propagate_at_launch = true
  }
}


resource "null_resource" "init_redisDB" {
  provisioner "local-exec" {
    command = "bash ./redis.sh"
    # environment = {
    #   var = foo
    # }
  }
  depends_on = [ #Make sure that node groups are live
    aws_autoscaling_group.wavelength_workers
  ]
}
