# Script to retrieve AWS metadata to feed to Verizon edge discovery service

# Step 1: based on tagging, extract Realm ASG instances' Wavelength Zone ID
wlzs=$(aws ec2 describe-instances \
--filters "Name=tag:app,Values=wavelength-mongodb-realm" \
--query 'Reservations[].Instances[].Placement.AvailabilityZone' \
--output text)

# Step 2: based on tagging, extract Realm ASG instances' Carrier IP address
carrier_ips=$(aws ec2 describe-instances \
--filters "Name=tag:app,Values=wavelength-mongodb-realm" \
--query 'Reservations[].Instances[].NetworkInterfaces[].Association.CarrierIp' \
--output text)

# Step 3: Feed data to the edge discovery service
python3 ./edge_discovery/edsSetup.py --carrier_ips "$carrier_ips" --wlzs "$wlzs"

