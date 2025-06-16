# Azure Container Apps - KEDA

## Scale Triggers

### HTTP Requests

Create an HTTP scaler. When the number of concurrent HTTP requests exceeds this value, more replicas are added.

```powershell
# add HTTP scale trigger to container app
az containerapp update `
    -n aca-web-keda -g rg-aca-keda `
    --min-replicas 1 `
    --max-replicas 5 `
    --scale-rule-name azure-http-rule `
    --scale-rule-type http `
    --scale-rule-http-concurrency 3
```

To test the scaling behavior, invoke the [curl.ps1](./curl.ps1) file to invoke parallel/concurrent requests to the endpoint. This will cause the amount of replicas to increase from 1 to 5. 

### Service Bus

If you don't have an existing queue, create one below.

```powershell
# invoke setup to create queue
.\servicequeue.ps1
```

Next, create a secret and service queue scaler (referencing the secret). When the amount of messages in a queue exceeds a threshold, more replicas are added.

```powershell
# create a secret
az containerapp secret set `
  -n aca-web-keda `
  -g rg-aca-keda `
  --secrets "queueconnection=MY_SERVICE_BUS_CONNECTION_STRING"

# add service queue scale trigger to container app
az containerapp update `
    -n aca-web-keda -g rg-aca-keda `
    --min-replicas 1 `
    --max-replicas 5 `
    --set-env-vars ConnectionStrings__ServiceBus=secretref:queueconnection `
    --scale-rule-name azure-service-bus-rule `
    --scale-rule-type azure-servicebus `
    --scale-rule-metadata "queueName=kedaqueue" `
                        "namespace=kedaacansrjp" `
                        "queueLength=5" `
    --scale-rule-auth "connection=queueconnection"
```

### MSSQL

Create a MSSQL database (either in a container app or using Azure SQL) along with a sample SQL table to be used as a databae query for a KEDA trigger.

```powershell
# add MSSQL scale trigger to container app
az containerapp update `
    -n aca-web-keda -g rg-aca-keda `
    --min-replicas 1 `
    --max-replicas 5 `
    --set-env-vars MSSQL_PASSWORD=secretref:mssqlpassword `
    --scale-rule-name mssql-rule `
    --scale-rule-type mssql `
    --scale-rule-metadata "host=kedaacademo.database.windows.net" `
                        "username=ryan" `
                        "passwordFromEnv=MSSQL_PASSWORD" `
                        "port=1433" `
                        "database=clientdb" `
                        "query=select count(*) from orders" `
                        "targetValue=2"
```



## Handy Commands

```powershell
# restart a revision
az containerapp revision restart --revision aca-web-keda--0000005 -n aca-web-keda -g rg-aca-keda

# get the container logs
az containerapp logs show --name aca-web-keda -g rg-aca-keda

# exec into an app replica
az containerapp exec -n aca-web-keda -g rg-aca-keda --replica aca-web-keda--0000012-84675f98fb-nxnbx
```

## Notes/Observations

- Creating a new scale rule creates a new revision of your application
- If you define more than one scale rule, the container app begins to scale once the first condition of any rules is met.

## Links

- [Set scaling rules in Azure Container Apps](https://learn.microsoft.com/en-us/azure/container-apps/scale-app?pivots=azure-cli)