. .\vars.ps1

$containerEnv = "aca-env-keda"
$containerWeb = "aca-web-keda"
$registry = "binarydadkeda"
$port = 8080

# create resource group
az group create -n $group -l $region

# create an ACR
az acr create -n $registry -g $group -l $region --sku Basic

# create the ACA environment
az containerapp env create -n $containerEnv -g $group -l $region

# create tha ACA web app
az containerapp create -n $containerWeb -g $group `
    --min-replicas 1 `
    --environment $containerEnv `
    --image "$registry.azurecr.io/webkeda:latest" `
    --registry-server "$registry.azurecr.io" `
    --target-port $port `
    --ingress 'external'

# update the ingress to use port 8080

# get the fqdn from the external web app
$webFqdn = az containerapp show -n $containerWeb -g $group --query "properties.configuration.ingress.fqdn" -o tsv

# dumps
"Web URL: https://$webFqdn"
