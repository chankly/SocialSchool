output "fqdn_keycloak" {
  description = "FQDN público de Keycloak. Trátalo como inmutable: forma parte del emisor de los tokens."
  value       = module.keycloak.fqdn
}

output "fqdn_api" {
  description = "FQDN público de la API."
  value       = module.api.fqdn
}
