output "id" {
  description = "Identificador del entorno de Container Apps, donde se despliegan las aplicaciones."
  value       = azurerm_container_app_environment.environment.id
}

output "default_domain" {
  description = "Dominio por defecto del entorno. Compuesto con el nombre de cada aplicación da su FQDN, y así puede fijarse KC_HOSTNAME sin dependencia circular."
  value       = azurerm_container_app_environment.environment.default_domain
}

output "identity_id" {
  description = "Identificador de la identidad gestionada que comparten las aplicaciones."
  value       = azurerm_user_assigned_identity.apps.id
}

output "identity_principal_id" {
  description = "Identificador del principal de esa identidad, que es lo que exige una asignación de rol."
  value       = azurerm_user_assigned_identity.apps.principal_id
}

output "log_analytics_workspace_id" {
  description = "Identificador del espacio de trabajo de logs."
  value       = azurerm_log_analytics_workspace.log_workspace.id
}
