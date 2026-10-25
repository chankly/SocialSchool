resource "azurerm_log_analytics_workspace" "log_workspace" {
  name                = "socialschool-log-${var.environment}"
  resource_group_name = var.resource_group_name
  location            = var.resource_group_location
  sku                 = var.log_analytics_sku
  retention_in_days   = var.log_retention_days
  tags                = var.tags
}

resource "azurerm_container_app_environment" "environment" {
  name                       = "socialschool-cappenv-${var.environment}"
  resource_group_name        = var.resource_group_name
  location                   = var.resource_group_location
  log_analytics_workspace_id = azurerm_log_analytics_workspace.log_workspace.id
  tags                       = var.tags
}

# Identidad que comparten las aplicaciones: con ella descargan del registro y leen los secretos.
resource "azurerm_user_assigned_identity" "apps" {
  name                = "socialschool-id-${var.environment}"
  resource_group_name = var.resource_group_name
  location            = var.resource_group_location
  tags                = var.tags
}

resource "azurerm_role_assignment" "acr_pull" {
  scope                = var.container_registry_id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_user_assigned_identity.apps.principal_id
}
