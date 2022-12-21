#!/usr/bin/env bash

KEY=""
TOKEN=$(curl -d "grant_type=client_credentials&client_id=$KEY" https://domain/token | \
		python -c "s = str(input()); print(eval(s)['access_token']);")

URL="https://domain/resource"
curl -H "Authorization: bearer $TOKEN" $URL > leas.xml

python xml_to_json.py leas.xml
