# Parse secrets
sed  's/ //g' < secret.tfvars > tmp
source ./tmp
cat tmp
echo $atlas_public_key

# Authenticate to REST API
TOKEN=$(curl --request POST --header 'Content-Type: application/json' --header 'Accept: application/json' --data '{"username": "'${atlas_public_key}'", "apiKey": "'${atlas_private_key}'"}' https://realm.mongodb.com/api/admin/v3.0/auth/providers/mongodb-cloud/login | jq ".access_token")
BEARER=$(echo $TOKEN | tr -d '"')
echo "Your Bearer Token:" $BEARER

#Install MongoDB Realm CLI
npm install -g mongodb-realm-cli

#Authenticate to Realm CLI
realm-cli login --api-key="$atlas_public_key" --private-api-key="$atlas_private_key"

# # Push Configuration
wget https://wavelengthtutorials.s3.amazonaws.com/RealmTestApp.zip
mkdir RealmConfig
unzip RealmTestApp.zip -d RealmConfig
realm-cli push --local ./RealmConfig --remote -d LOCAL -y

# Get your Realm Apps
URL="https://realm.mongodb.com/api/admin/v3.0/groups/${group_id}/apps"
ID=$(curl --request GET --header 'Content-Type: application/json' --header "Authorization: Bearer ${BEARER}" $URL | jq -r '.[0]._id')
echo $ID
echo "Completed Realm App ID retrieval...."

# Get Key Pair
prefix='https://realm.mongodb.com/api/admin/v3.0/groups/'
URL="${prefix}${group_id}/apps/${ID}/api_keys"
KEY=$(curl --request POST --header 'Content-Type: application/json' --header "Authorization: Bearer ${BEARER}" --data '{"name": "wavelength-test"}' $URL | jq '.key')
echo "Completed Key Pair retrieval...."

# Describe Realm App
REALMAPPID=$(realm-cli apps describe -a "RealmTestApp" | tail -n +2 | jq ".client_app_id")
echo $REALMAPPID

# Take Realm data and add to db_sync_setup.sh
echo "echo 'REALMAPPID=${REALMAPPID}' >> .env" >> ./db_sync_setup.sh
echo "echo 'APIKEY=${KEY}' >> .env" >> ./db_sync_setup.sh
echo "./build.sh" >> ./db_sync_setup.sh
rm tmp