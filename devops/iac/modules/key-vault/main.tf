locals {
  mysql_connection_string_secret    = "mysql-connection-string"
  mysql_admin_password_secret       = "mysql-admin-password"
  keycloak_admin_password_secret    = "keycloak-admin-password"
  keycloak_admin_client_id_name     = "keycloak-admin-client-id"
  keycloak_admin_client_secret_name = "keycloak-admin-client-secret"
}

resource "azurerm_key_vault" "key_vault" {
  name                       = "socialschool-kv-${var.environment}"
  resource_group_name        = var.resource_group_name
  location                   = var.resource_group_location
  tenant_id                  = var.tenant_id
  sku_name                   = var.sku_name
  soft_delete_retention_days = var.soft_delete_retention_days
  purge_protection_enabled   = var.purge_protection_enabled
  tags                       = var.tags

  # Sin esto el vault se queda en el modelo de directivas de acceso y las asignaciones de rol de
  # abajo no servirían de nada: las aplicaciones recibirían 403 al leer sus secretos.
  rbac_authorization_enabled = true
}

# La identidad de las aplicaciones necesita LEER los secretos.
resource "azurerm_role_assignment" "secrets_user" {
  scope                = azurerm_key_vault.key_vault.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = var.identity_principal_id
}

# Y la identidad que despliega necesita ESCRIBIRLOS. Con RBAC activado, ser propietario de la
# suscripción no da acceso al plano de datos: hay que conceder el rol explícitamente. Como los roles
# tardan unos minutos en propagarse, es posible que el primer apply falle al crear los secretos; en
# ese caso se vuelve a ejecutar.
resource "azurerm_role_assignment" "secrets_officer" {
  scope                = azurerm_key_vault.key_vault.id
  role_definition_name = "Key Vault Secrets Officer"
  principal_id         = var.deployer_object_id
}

resource "azurerm_key_vault_secret" "mysql_connection_string" {
  name         = local.mysql_connection_string_secret
  value        = var.mysql_connection_string
  key_vault_id = azurerm_key_vault.key_vault.id
}

resource "azurerm_key_vault_secret" "mysql_admin_password" {
  name         = local.mysql_admin_password_secret
  value        = var.mysql_admin_password
  key_vault_id = azurerm_key_vault.key_vault.id
}

resource "azurerm_key_vault_secret" "keycloak_admin_password" {
  name         = local.keycloak_admin_password_secret
  value        = var.keycloak_admin_password
  key_vault_id = azurerm_key_vault.key_vault.id
}

# Cliente confidencial con el que la API consulta la Admin API de Keycloak cuando llega un registro.
# El cliente se crea a mano en el realm (service account con el rol view-users); ver devops/README.md.
resource "azurerm_key_vault_secret" "keycloak_admin_client_id" {
  name         = local.keycloak_admin_client_id_name
  value        = var.keycloak_admin_client_id
  key_vault_id = azurerm_key_vault.key_vault.id
}

resource "azurerm_key_vault_secret" "keycloak_admin_client_secret" {
  name         = local.keycloak_admin_client_secret_name
  value        = var.keycloak_admin_client_secret
  key_vault_id = azurerm_key_vault.key_vault.id
}
