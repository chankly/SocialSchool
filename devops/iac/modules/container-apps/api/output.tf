output "fqdn" {
  description = "FQDN público de la API."
  value       = azurerm_container_app.api.ingress[0].fqdn
}
