#! /bin/bash
sudo yum update
sudo yum install -y git wget docker
sudo systemctl start docker
cd /home/ec2-user
git clone https://github.com/graboskyc/MQTTtoRealm.git
cd MQTTtoRealm
chmod +x build.sh
wget https://wavelengthtutorials.s3.amazonaws.com/RealmTestApp.zip
mkdir RealmTestApp
unzip RealmTestApp.zip -d RealmTestApp
sudo su
curl --silent --location https://rpm.nodesource.com/setup_14.x | bash -
yum -y install nodejs
npm install -g npm
npm install -g mongodb-realm-cli
