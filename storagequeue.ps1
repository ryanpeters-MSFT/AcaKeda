. .\vars.ps1

$storageAccount = "kedastorage1234"
$queue = "kedastoragequeue"

# create a storage account
az storage account create -n $storageAccount -g $group

# create a storage queue
az storage queue create `
  --account-name $storageAccount `
  --name $queue