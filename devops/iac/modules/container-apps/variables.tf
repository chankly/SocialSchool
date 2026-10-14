variable "environment" {
  description = "Entorno sobre el que se despliegan las aplicaciones. Forma parte de sus nombres."
  type        = string
}

variable "resource_group_name" {
  description = "Grupo de recursos donde se despliegan."
  type        = string
}

variable "container_app_environment_id" {
  description = "Identificador del entorno de Container Apps."
  type        = string
}

variable "default_domain" {
  description = "Dominio por defecto del entorno, que compuesto con el nombre de cada aplicación da su FQDN."
  type        = string
}

variable "identity_id" {
  description = "Identidad gestionada que usan las aplicaciones para el registro y para Key Vault."
  type        = string
}

variable "registry_login_server" {
  description = "Servidor del registro del que se descargan las imágenes."
  type        = string
}

variable "mysql_fqdn" {
  description = "Nombre DNS del servidor MySQL, para componer la URL JDBC de Keycloak."
  type        = string
}

variable "mysql_administrator_login" {
  description = "Usuario administrador del servidor MySQL."
  type        = string
}

variable "key_vault_secret_ids" {
  description = "Identificadores sin versión de los secretos de Key Vault, indexados por nombre de secreto."
  type        = map(string)
}

variable "key_vault_secret_names" {
  description = "Nombres de los secretos de Key Vault, indexados por el mismo criterio."
  type        = map(string)
}

variable "keycloak_realm" {
  description = "Realm de Keycloak del que la API valida los tokens."
  type        = string
}

variable "keycloak_admin_username" {
  description = "Usuario administrador inicial de Keycloak."
  type        = string
}

variable "api_audience" {
  description = "Audiencia que la API espera en el token de Keycloak."
  type        = string
}
