terraform {
  required_version = "~> 1.16"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }

  backend "azurerm" {
    resource_group_name   = "hsoft-rg"
    storage_account_name  = "hsoftcontainer"
    // -backend-config="container_name=socialschool-terraform-dv"
    // container_name        = "socialschool-terraform-${var.environment}"
    key                   = "terraform.tfstate"
  }
}

provider "azurerm" {
  features {}
}

data "azurerm_client_config" "current" {}

locals {
  # La API lee su cadena de conexión de Key Vault; aquí se compone una sola vez.
  mysql_connection_string = join(";", [
    "Server=${module.mysql.fqdn}",
    "Port=3306",
    "Database=api",
    "User Id=${var.mysql_administrator_login}",
    "Password=${var.mysql_administrator_password}",
  ])
}

module "resource_groups" {
  source = "./modules/resource-groups"
  environment = var.environment
}

module "container_registry" {
  source = "./modules/container-registry"

  environment             = var.environment
  resource_group_name     = module.resource_groups.name
  resource_group_location = module.resource_groups.location
}

module "container_app_environment" {
  source = "./modules/container-environment-apps"

  environment             = var.environment
  resource_group_name     = module.resource_groups.name
  resource_group_location = module.resource_groups.location
  container_registry_id   = module.container_registry.id
}

module "mysql" {
  source = "./modules/mysql"

  environment             = var.environment
  resource_group_name     = module.resource_groups.name
  resource_group_location = module.resource_groups.location
  administrator_login     = var.mysql_administrator_login
  administrator_password  = var.mysql_administrator_password
}

module "key_vault" {
  source = "./modules/key-vault"

  environment             = var.environment
  resource_group_name     = module.resource_groups.name
  resource_group_location = module.resource_groups.location
  tenant_id               = data.azurerm_client_config.current.tenant_id
  deployer_object_id      = data.azurerm_client_config.current.object_id
  identity_principal_id   = module.container_app_environment.identity_principal_id
  mysql_connection_string = local.mysql_connection_string
  mysql_admin_password    = var.mysql_administrator_password
  keycloak_admin_password = var.keycloak_admin_password
}

module "container_apps" {
  source = "./modules/container-apps"

  environment                  = var.environment
  resource_group_name          = module.resource_groups.name
  container_app_environment_id = module.container_app_environment.id
  default_domain               = module.container_app_environment.default_domain
  identity_id                  = module.container_app_environment.identity_id
  registry_login_server        = module.container_registry.login_server
  mysql_fqdn                   = module.mysql.fqdn
  mysql_administrator_login    = var.mysql_administrator_login
  key_vault_secret_ids         = module.key_vault.secret_ids
  key_vault_secret_names       = module.key_vault.secret_names
  keycloak_realm               = var.keycloak_realm
  keycloak_admin_username      = var.keycloak_admin_username
  api_audience                 = var.api_audience
}
