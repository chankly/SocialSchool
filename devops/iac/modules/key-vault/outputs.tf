output "id" {
  description = "Identificador del vault."
  value       = azurerm_key_vault.key_vault.id
}

output "uri" {
  description = "URI del vault."
  value       = azurerm_key_vault.key_vault.vault_uri
}

# Se exponen los identificadores SIN versión para que las aplicaciones sigan leyendo el secreto cuando
# rote, en vez de quedar clavadas a la versión de hoy.
output "secret_ids" {
  description = "Identificadores sin versión de los secretos, indexados por nombre de secreto."
  value = {
    mysql_connection_string      = azurerm_key_vault_secret.mysql_connection_string.versionless_id
    mysql_admin_password         = azurerm_key_vault_secret.mysql_admin_password.versionless_id
    keycloak_admin_password      = azurerm_key_vault_secret.keycloak_admin_password.versionless_id
    keycloak_admin_client_id     = azurerm_key_vault_secret.keycloak_admin_client_id.versionless_id
    keycloak_admin_client_secret = azurerm_key_vault_secret.keycloak_admin_client_secret.versionless_id
  }
}

output "secret_names" {
  description = "Nombres de los secretos creados, para que ninguna otra capa repita los literales."
  value = {
    mysql_connection_string      = local.mysql_connection_string_secret
    mysql_admin_password         = local.mysql_admin_password_secret
    keycloak_admin_password      = local.keycloak_admin_password_secret
    keycloak_admin_client_id     = local.keycloak_admin_client_id_name
    keycloak_admin_client_secret = local.keycloak_admin_client_secret_name
  }
}
