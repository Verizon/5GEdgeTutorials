Content-Type: multipart/mixed; boundary="//"
MIME-Version: 1.0

--//
Content-Type: text/cloud-config; charset="us-ascii"
MIME-Version: 1.0
Content-Transfer-Encoding: 7bit
Content-Disposition: attachment; filename="cloud-config.txt"

#cloud-config
cloud_final_modules:
- [scripts-user, always]

--//
Content-Type: text/x-shellscript; charset="us-ascii"
MIME-Version: 1.0
Content-Transfer-Encoding: 7bit
Content-Disposition: attachment; filename="userdata.txt"

#!/bin/sh
curl -OL https://cloud.mongodb.com/download/agent/automation/mongodb-mms-automation-agent-manager-11.6.0.7119-1.x86_64.rpm
sudo rpm -U mongodb-mms-automation-agent-manager-11.6.0.7119-1.x86_64.rpm
sudo rm /etc/mongodb-mms/automation-agent.config
sudo mv /home/ec2-user/automation-agent.config /etc/mongodb-mms/automation-agent.config
sudo yum -y install cyrus-sasl cyrus-sasl-gssapi cyrus-sasl-plain krb5-libs libcurl net-snmp net-snmp-libs openldap openssl xz-libs
sudo mkdir -p /data
sudo chown mongod:mongod /data


#Move MongoDB environment variables to /etc/mongodb-mms/automation-agent.config
echo > /etc/mongodb-mms/automation-agent.config
echo "mmsGroupId=${mmsGroupId}" >> /etc/mongodb-mms/automation-agent.config
# echo "mmsGroupId=616448bb01c9f50859f569a7" >> /etc/mongodb-mms/automation-agent.config
echo "mmsApiKey=${mmsApiKey}" >> /etc/mongodb-mms/automation-agent.config
# echo "mmsApiKey=616448cde33d36737520d00b55f5799fc0b24a5514006c62692d501f" >> /etc/mongodb-mms/automation-agent.config
echo "mmsBaseUrl=https://cloud.mongodb.com" >> /etc/mongodb-mms/automation-agent.config
echo "logFile=/var/log/mongodb-mms-automation/automation-agent.log" >> /etc/mongodb-mms/automation-agent.config
echo "mmsConfigBackup=/var/lib/mongodb-mms-automation/mms-cluster-config-backup.json" >> /etc/mongodb-mms/automation-agent.config
echo "logLevel=INFO" >> /etc/mongodb-mms/automation-agent.config
echo "maxLogFiles=10" >> /etc/mongodb-mms/automation-agent.config
echo "maxLogFileSize=268435456" >> /etc/mongodb-mms/automation-agent.config

#Start CloudManager Agent
sudo systemctl start mongodb-mms-automation-agent.service
sudo systemctl enable mongodb-mms-automation-agent.service