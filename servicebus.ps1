. .\vars.ps1

$namespace = "kedaacans9876"
$queue = "kedaqueue"

# create the service bus namespace
az servicebus namespace create -g $group -n $namespace --location $region

# create the service bus queue
az servicebus queue create -g $group --namespace-name $namespace -n $queue

# create the service bus connection string
$connectionString = az servicebus namespace authorization-rule keys list `
    -g $group `
    --namespace-name $namespace `
    -n RootManageSharedAccessKey `
    --query "primaryConnectionString" `
    -o tsv

"Connection string: $connectionString"