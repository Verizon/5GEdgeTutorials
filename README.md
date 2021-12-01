# Wavelength Testing

This is a CLI tool for deploying test nodes in AWS Wavelength and EC2, and for later tearing them down. The nodes run [`iperf3`](https://iperf.fr/) to act as test servers.

## Installation

After cloning this repository, run `pip install -r requirements.txt`.

## Usage

There are two commands: `deploy` and `teardown`. Each takes a `--help` option listing their arguments and options, most of which have usable defaults.

### Deploy

The `deploy` command requires one argument, which is the CIDR notation of which IPs are allowed to connect to the bastion host in EC2. Other options include the `iperf` port, the IPs allowed to connect to the Wavelength instance, and renaming most deployed components.

When finished, the `deploy` command will show an example `ssh` command for acccessing the bastion host, and an example `iperf3` command for testing Wavelength. (Note that the Wavelength node will only be accessible from a device on the Verion cellular network, like a phone or a laptop connected to a hotspot.)

### Teardown

The only option for the `teardown` command is the name of the VPC to destroy. If you used the default VPC name during `deploy`, the default here is the same. Note that this command assumes this VPC only contains the items created by `deploy` and will find and remove them. This includes returning the carrier IP that was assigned.

## Author

[Jim Pfleger](https://github.com/codemonkeyjim)
