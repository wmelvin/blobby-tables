# ----------------------------------------------------------------------
# PowerShell script with steps to create resources using the Azure CLI.
#
# ----------------------------------------------------------------------

# az login

# az account set -s $SUBSCRIPTION


# # ----------------------------------------------------------------------
# # -- Get key variables from file in local encrypted folder.
 
# $keysFile = "$env:UserProfile\KeepLocal\blobby-settings"

# # -- Source the file to set the vars.
# . $keysFile

# function CheckVarSet ([string] $varName) {
#     $val = Get-Variable -Name $varName -ValueOnly -ErrorAction:Ignore
#     if (0 -eq $val.Length) {
#       Write-Host "ERROR: '$varName' not set in '$keysFile'."
#       Exit 1
#     }
# }

# CheckVarSet "AdminUser"


# ----------------------------------------------------------------------
# -- Assign vars for script.

$baseName = "demo17"
$rgName = "${baseName}-rg"
$location = "eastus"
$storageAcctName = "${baseName}storage"

# -- Create the Resource Group.
az group create -n $rgName -l $location

# -- Create the Storage Account.
#    https://docs.microsoft.com/en-us/cli/azure/storage/account?view=azure-cli-latest#az-storage-account-create

az storage account create `
    -n $storageAcctName `
    -l $location `
    -g $rgName `
    --sku Standard_LRS


# -- Get the storage account key.
#    Example found in Microsoft Docs: "Mount a file share to a Python function app - Azure CLI"
#    https://docs.microsoft.com/en-us/azure/azure-functions/scripts/functions-cli-mount-files-storage-linux

# $storageKey = $(az storage account keys list -g $rgName -n $storageAcctName --query '[0].value' -o tsv)


# -- Create storage containers.
#    https://docs.microsoft.com/en-us/cli/azure/storage/container?view=azure-cli-latest#az-storage-container-create

# az storage container create --account-key $storageKey --account-name $storageAcctName -n "blobby1"

