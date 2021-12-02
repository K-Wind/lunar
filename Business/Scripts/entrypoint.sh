#!/bin/sh

echo "startup script is running"
echo "$(cat /etc/resolv.conf)"

#chmod +x ./wait-for-it.sh

#./wait-for-it.sh rabbitmq -t 120

dotnet Business.dll