# Una aplicación, un submódulo: cada una tiene su fichero y sus variables, en vez de una lista genérica.
module "keycloak" {
  source = "./keycloak"

  environment                  = var.environment
  resource_group_name          = var.resource_group_name
  container_app_environment_id = var.container_app_environment_id
  default_domain               = var.default_domain
  identity_id                  = var.identity_id
  registry_login_server        = var.registry_login_server
  mysql_fqdn                   = var.mysql_fqdn
  mysql_administrator_login    = var.mysql_administrator_login
  keycloak_admin_username      = var.keycloak_admin_username
  key_vault_secret_ids         = var.key_vault_secret_ids
  key_vault_secret_names       = var.key_vault_secret_names
}

module "api" {
  source = "./api"

  environment                  = var.environment
  resource_group_name          = var.resource_group_name
  container_app_environment_id = var.container_app_environment_id
  identity_id                  = var.identity_id
  registry_login_server        = var.registry_login_server
  key_vault_secret_ids         = var.key_vault_secret_ids
  key_vault_secret_names       = var.key_vault_secret_names

  # La autoridad sale del FQDN de Keycloak, que ya es conocido aquí porque su submódulo se despliega
  # antes. No hay dependencia circular: Keycloak no necesita nada de la API.
  keycloak_authority = "https://${module.keycloak.fqdn}/realms/${var.keycloak_realm}"
  api_audience       = var.api_audience
}
