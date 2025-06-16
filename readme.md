# Azure Container Apps - KEDA

Azure Container Apps (ACA) comes with KEDA (Kubernetes Event-Driven Autoscaler) built in, providing seamless, event-driven scaling for containerized applications. Users simply define scaling rules in their ACA configuration, and KEDA automatically monitors event sources—such as HTTP traffic, queue messages, or Azure services—and adjusts the number of running containers accordingly. This integration is fully managed by Azure, so there’s no need to install or maintain KEDA or Kubernetes clusters yourself.

## Benefits of Using KEDA in Azure Container Apps

- **Event-Driven Scaling:** KEDA enables ACA to react to real-time events, scaling your containers up or down based on demand, not just on CPU or memory usage.
- **Scale to Zero:** ACA can automatically scale your containers down to zero when idle, reducing costs, and quickly scale back up when new events arrive.
- **Wide Range of Triggers:** With KEDA, ACA supports scaling on many triggers, including Azure Service Bus, Event Hubs, HTTP requests, and more, making it ideal for microservices and event-driven architectures.
- **Simplicity:** Developers focus on application code and scaling rules, while Azure handles the underlying infrastructure, cluster management, and KEDA configuration.
- **Cost Efficiency:** You only pay for active resources, making ACA with KEDA cost-effective for workloads with variable or unpredictable demand.

## Differences from Other Deployment Types (e.g., Kubernetes)

| Feature                     | Azure Container Apps (with KEDA)         | Kubernetes (e.g., AKS with HPA/KEDA)         |
|-----------------------------|------------------------------------------|----------------------------------------------|
| **Management**              | Fully managed, no cluster management     | User manages Kubernetes clusters             |
| **Scaling Triggers**        | Event-driven via KEDA, built-in support  | HPA: CPU/memory by default; KEDA optional    |
| **Scale to Zero**           | Supported for most triggers              | Supported only if KEDA is installed          |
| **Ease of Use**             | Declarative, simple scaling rules        | Requires YAML, CRDs, and cluster expertise   |
| **Cost Model**              | Consumption-based, pay for active usage  | Pay for cluster nodes and running resources  |
| **Integration**             | Seamless with Azure services             | Manual setup for Azure integration           |

In Kubernetes environments like AKS, you must install and configure KEDA yourself if you want event-driven scaling, and you are responsible for managing the cluster. ACA, by contrast, abstracts away the infrastructure and provides a serverless experience with KEDA scaling out of the box, making it much easier to use and maintain.

KEDA’s integration with Azure Container Apps makes it easy to build scalable, event-driven applications without the complexity of managing Kubernetes infrastructure. ACA leverages KEDA to deliver flexible, efficient scaling based on real-world events, with features like scale-to-zero, simplified operations, and cost savings—offering a more streamlined and accessible alternative to traditional Kubernetes deployments.

## KEDA Scaler Types

### HTTP Requests

The HTTP Requests scaler in KEDA automatically adjusts the number of pods for a deployment based on incoming HTTP traffic, supporting scaling to and from zero. For example, you can set up an `HTTPScaledObject` to scale a deployment named `api-deployment` when the request rate to `www.api.com` exceeds five requests per second. This scaler uses an interceptor to queue and forward HTTP requests, ensuring smooth scaling even when no pods are running.

Create an HTTP scaler below. For this scaler, when the number of concurrent HTTP requests exceeds this value, more replicas are added.

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

To test the scaling behavior, invoke the [test-http.ps1](./test-http.ps1) file to invoke parallel/concurrent requests to the endpoint. This will cause the amount of replicas to increase from 1 to 5. 

### Service Bus

The Azure Service Bus Queue scaler enables KEDA to scale workloads according to the number of messages in an Azure Service Bus queue. For instance, you can configure KEDA to increase pod count when the queue length for `orders-queue` surpasses 100 messages, ensuring timely processing of queued tasks. The configuration typically includes the queue name and connection string.

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

To test the scaling behavior, invoke the [test-queue.ps1](./test-queue.ps1) file to create a high-volume of queue messages, triggering a scale-up of the replicas.

### MSSQL

The MSSQL scaler allows KEDA to scale deployments based on the result of a custom SQL query against a Microsoft SQL Server database. For example, you might scale up a worker deployment if a query like `SELECT COUNT(*) FROM jobs WHERE status='pending'` returns a value above 50. This approach is useful when application workload is directly tied to database state.

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

### CRON

The Cron scaler lets you define time-based scaling schedules for your workloads. For example, you can set a rule to scale up a deployment to five replicas every weekday at 8:00 AM and scale it back down at 6:00 PM. This is ideal for predictable workloads that fluctuate based on business hours or scheduled tasks.

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
# list replicas
az containerapp replica list -n aca-web-keda -g rg-aca-keda -o table

# list revisions
az containerapp revision list -n aca-web-keda -g rg-aca-keda -o table

# restart a revision
az containerapp revision restart --revision aca-web-keda--0000012 -n aca-web-keda -g rg-aca-keda

# get the container logs
az containerapp logs show --name aca-web-keda -g rg-aca-keda

# invoke queue url
curl.exe http://localhost:5233/queuemessage

# debug the app
az containerapp debug -n aca-web-keda -g rg-aca-keda

# exec into a replica
az containerapp exec -n aca-web-keda -g rg-aca-keda --replica aca-web-keda--0000010-79ccf6db4d-vhks7
```

## Notes/Observations

- Creating a new scale rule creates a new revision of your application
- If you define more than one scale rule, the container app begins to scale once the first condition of any rules is met.

## Links

- [Set scaling rules in Azure Container Apps](https://learn.microsoft.com/en-us/azure/container-apps/scale-app?pivots=azure-cli)