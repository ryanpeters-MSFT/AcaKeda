# Azure Container Apps - KEDA

## Handy Commands

```powershell
# get the app information
az containerapp show -n aca-web-keda -g rg-aca-keda | code -

# redeploy the app
```

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

Next, create a service queue scaler. When the amount of messages in a queue exceeds a threshold, more replicas are added.

```powershell
# add service queue scale trigger to container app
az containerapp update `
    -n aca-web-keda -g rg-aca-keda `
    --min-replicas 1 `
    --max-replicas 5 `
    --scale-rule-name azure-service-bus-rule `
    --scale-rule-type azure-servicebus `
    --scale-rule-metadata "queueName=kedaqueue" `
                        "namespace=kedaacansrjp" `
                        "queueLength=5" `
    --scale-rule-auth "connection=queueconnection"
```

## Notes/Observations

- Creating a new scale rule creates a new revision of your application
- If you define more than one scale rule, the container app begins to scale once the first condition of any rules is met.

## Links

- [Set scaling rules in Azure Container Apps](https://learn.microsoft.com/en-us/azure/container-apps/scale-app?pivots=azure-cli)