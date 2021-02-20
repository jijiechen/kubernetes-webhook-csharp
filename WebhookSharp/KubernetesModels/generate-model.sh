#!/bin/bash

# cmd_nswag=$(command -v nswag || true)
# if [ "$cmd_nswag" == "" ]; then
#     npm install -g nswag
#     nswag version /runtime:NetCore31
# fi

curl --fail -o admission.swagger.json https://gist.githubusercontent.com/bergeron/70ca86cf31762e16f18b2be3c549a074/raw/77c67214eff1c9edf7b133947c0d0ff557dcdc6f/k8s.io.api.admission.v1.swagger.json
nswag openapi2csclient /input:admission.swagger.json  /classname:AdmissionReview /namespace:ImagePrinter /output:AdmissionReview.cs

sed -i 's/public RawExtension/public System.Text.Json.JsonElement/' AdmissionReview.cs