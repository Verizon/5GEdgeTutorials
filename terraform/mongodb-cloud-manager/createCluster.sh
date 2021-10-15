#Authenticate to REST API
TOKEN=$(curl --request POST --header 'Content-Type: application/json' --header 'Accept: application/json' --data '{"username": "'${atlas_public_key}'", "apiKey": "'${atlas_private_key}'"}' https://realm.mongodb.com/api/admin/v3.0/auth/providers/mongodb-cloud/login | jq ".access_token")
BEARER=$(echo $TOKEN | tr -d '"')
echo "Your Bearer Token:" $BEARER
echo $node_a
echo $node_b
echo $node_c

sed -i "s/node_a/${node_a}/" data.json
sed -i "s/node_b/${node_b}/" data.json
sed -i "s/node_c/${node_c}/" data.json

curl --user "${atlas_public_key}:${atlas_private_key}" --digest \
     --header 'Content-Type: application/json' \
     --request PUT "https://cloud.mongodb.com/api/public/v1.0/groups/${mmsGroupId}/automationConfig?pretty=true" \
     --data "@data.json"
     
     
sed -i "s/${node_a}/node_a/" data.json
sed -i "s/${node_b}/node_b/" data.json
sed -i "s/${node_c}/node_c/" data.json