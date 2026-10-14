output "fqdn" {
  description = "FQDN público de Keycloak, calculado a partir del nombre y del dominio del entorno."
  value       = local.fqdn
}

output "id" {
  description = "Identificador de la aplicación."
  value       = azurerm_container_app.keycloak.id
}
