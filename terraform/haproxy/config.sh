set -a
. ./secret.tfvars
set +a

echo $aws_access_key_id
echo $aws_secret_access_key

sed -i "s/<your-secret-key>/${aws_secret_access_key}/" hapee-config.sh
sed -i "s/<your-access-key>/${aws_access_key_id}/" hapee-config.sh