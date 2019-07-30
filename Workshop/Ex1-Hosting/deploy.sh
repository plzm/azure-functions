#!/bin/bash

# Login first
az login

# #####
# Variables

# Azure
azure_subscription_id="$(az account show -o tsv --query "id")"
azure_aad_tenant_id="$(az account show -o tsv --query "tenantId")"

azure_region="eastus"  # PROVIDE
azure_external_ips_allowed="75.68.47.183"  # PROVIDE

# General
prefix="azws"   # This is just a naming prefix used to create other variable values, like resource group names and such

# Resource group
resource_group_name="$prefix"

# Storage
storage_acct_name="$prefix""pzsa"
storage_acct_key="DO NOT SET EXPLICITLY"
storage_container_input="input"
storage_container_output="output"

# App Insights
app_insights_name="$prefix""ai"
app_insights_key="DO NOT SET EXPLICITLY"

# App Service Plan
app_service_plan_name="$prefix""asp"
app_service_plan_sku="S1"

# Function App
functionapp_name="$prefix""fn"
functionapp_msi_role="Contributor"
functionapp_msi_scope="DO NOT SET EXPLICITLY"
functionapp_msi_principal_id="DO NOT SET EXPLICITLY"

# Assemble RG-level MSI scope (this makes individual resource scope assignments superfluous)
functionapp_msi_scope="/subscriptions/""$azure_subscription_id""/resourceGroups/""$resource_group_name"

# Function App MSI scope and role specific to storage
functionapp_msi_scope_storage="/subscriptions/""$azure_subscription_id""/resourceGroups/""$resource_group_name""/providers/Microsoft.Storage/storageAccounts/""$storage_acct_name"
functionapp_msi_role_storage="Storage Blob Data Contributor"

# Key Vault
key_vault_name="$prefix""kv"

# #####

# #####
# Operations

# https://docs.microsoft.com/en-us/cli/azure/group
# Create new resource group
echo "Create Resource Group"
az group create -l $azure_region -n $resource_group_name

# https://docs.microsoft.com/en-us/cli/azure/storage/account
# Create storage account
echo "Create Storage Account"
az storage account create -l $azure_region -g $resource_group_name -n $storage_acct_name --kind StorageV2 --sku Standard_LRS

# List storage account keys (need a key for container create)
# az storage account keys list -n $storage_acct_name -g $resource_group_name
echo "Get Storage Account key"
storage_acct_key="$(az storage account keys list -g "$resource_group_name" -n "$storage_acct_name" -o tsv --query "[0].value")"

# https://docs.microsoft.com/en-us/cli/azure/storage/container
# Create containers in storage account
echo "Create Storage Containers"
az storage container create -n $storage_container_input --account-name $storage_acct_name --account-key $storage_acct_key
az storage container create -n $storage_container_output --account-name $storage_acct_name --account-key $storage_acct_key

# https://docs.microsoft.com/en-us/cli/azure/appservice/plan
# Create app service plan
echo "Create App Service Plan"
az appservice plan create -l $azure_region -g $resource_group_name -n $app_service_plan_name --sku $app_service_plan_sku

# https://docs.microsoft.com/en-us/cli/azure/group/deployment
# Create application insights instance and get instrumentation key
echo "Create Application Insights and get Instrumentation Key"
app_insights_key="$(az group deployment create -g $resource_group_name -n $app_insights_name --template-file "app_insights.template.json" \
	-o tsv --query "properties.outputs.app_insights_instrumentation_key.value" \
	--parameters location="$azure_region" instance_name="$app_insights_name")"

# https://docs.microsoft.com/en-us/cli/azure/functionapp
# Create function app with plan and app insights created above
# Using Windows at this point because MSI on Linux still in preview
echo "Create Function App and link to App Service Plan and App Insights instance created above"
az functionapp create -g $resource_group_name -n $functionapp_name --storage-account $storage_acct_name \
	--app-insights $app_insights_name --app-insights-key $app_insights_key \
	--plan $app_service_plan_name --os-type Windows --runtime dotnet

# https://docs.microsoft.com/en-us/cli/azure/functionapp/identity
# Assign managed identity to function app
# Omit scope assignment for least privilege, assign explicit access below for storage, key vault, SQL
#  --scope $functionapp_msi_scope
echo "Assign managed identity to function app"
functionapp_msi_principal_id="$(az functionapp identity assign -g $resource_group_name -n $functionapp_name --role $functionapp_msi_role -o tsv --query "principalId")"
echo $functionapp_msi_principal_id

# echo "Sleep to allow MSI identity to finish provisioning"
sleep 120s

# Get managed identity principal and tenant ID
# az functionapp identity show -g $resource_group_name -n $functionapp_name
echo "Get Function App identity Principal ID and Display Name"
# functionapp_msi_principal_id="$(az functionapp identity show -g $resource_group_name -n $functionapp_name -o tsv --query "principalId")"
functionapp_msi_display_name="$(az ad sp show --id $functionapp_msi_principal_id -o tsv --query "displayName")"

# Assign Function App MSI rights to storage
echo "Assign service principal rights to read/write data to storage account (this is redundant with RG Contributor, but OK to do and needed if the storage acct is in another RG)"
az role assignment create --scope "$functionapp_msi_scope_storage" --assignee-object-id "$functionapp_msi_principal_id" --role "$functionapp_msi_role_storage"

# https://docs.microsoft.com/en-us/cli/azure/keyvault
# Create key vault
echo "Create Azure Key Vault"
az keyvault create -l $azure_region -g $resource_group_name -n $key_vault_name

echo "Assign the Function App MSI access to the key vault"
az keyvault set-policy -g $resource_group_name -n $key_vault_name --object-id $functionapp_msi_principal_id --secret-permissions get

# #####
