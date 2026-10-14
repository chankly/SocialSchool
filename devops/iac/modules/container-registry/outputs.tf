output "id" {
  description = "Identificador del registro, para asignar permisos sobre él."
  value       = azurerm_container_registry.acr.id
}

output "name" {
  description = "Nombre del registro."
  value       = azurerm_container_registry.acr.name
}

output "login_server" {
  description = "Servidor del que las aplicaciones descargan sus imágenes."
  value       = azurerm_container_registry.acr.login_server
}
