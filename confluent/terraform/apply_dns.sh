#Automation Template to push Carrier IP information to Route53
echo $ZONEID
echo $DOMAIN
export IP=$(aws ec2 describe-instances --filters "Name=tag-key,Values= aws:autoscaling:groupName" "Name=tag-value,Values= wavelength-wl-workers" --query 'Reservations[].Instances[].NetworkInterfaces[].Association.CarrierIp' --output text)
IP_ARRAY=(`echo $IP | tr ',' ' '`)
echo ${IP_ARRAY[0]}, ${IP_ARRAY[1]}, ${IP_ARRAY[2]}
export TTL=60
export TYPE=A
export RECORDSET=confluent.${DOMAIN}
echo $RECORDSET

#Push Carrier IPs to multi-value Route53 A record
aws route53 change-resource-record-sets --region us-east-1 --hosted-zone-id $ZONEID --change-batch '{"Comment":"Adding Confluent DNS records","Changes":[{"Action":"UPSERT","ResourceRecordSet":{"ResourceRecords":[{"Value":"'"${IP_ARRAY[0]}"'"},{"Value":"'"${IP_ARRAY[1]}"'"},{"Value":"'"${IP_ARRAY[2]}"'"}],"Name":"'"$RECORDSET"'","Type":"'"$TYPE"'","TTL":'$TTL'}}]}'
