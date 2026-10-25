variable "environment" {
  description = "Entorno sobre el que se despliega todo (dv, qa o pr). Es la variable que gobierna el despliegue: de ella salen los nombres de los recursos."
  type        = string
}

variable "api_audience" {
  description = "Audiencia que la API espera en el token de Keycloak."
  type        = string
  default     = "0B28A13A-DF06-4840-958A-CC33605B3EB7"
}

variable "keycloak_realm" {
  description = "Realm de Keycloak del que la API valida los tokens."
  type        = string
  default     = "socialschool"
}

variable "keycloak_admin_username" {
  description = "Usuario administrador inicial de Keycloak. La contraseña va por variable de entorno, no por aquí."
  type        = string
  default     = "kcadmin"
}

variable "keycloak_admin_password" {
  description = "Contraseña del administrador de Keycloak. Se pasa por TF_VAR_keycloak_admin_password, nunca por un fichero versionado."
  type        = string
  sensitive   = true
}

variable "keycloak_admin_client_id" {
  description = "Identificador del cliente confidencial con el que la API consulta la Admin API de Keycloak."
  type        = string
}

variable "keycloak_admin_client_secret" {
  description = "Secreto de ese cliente. Se pasa por TF_VAR_keycloak_admin_client_secret, nunca por un fichero versionado."
  type        = string
  sensitive   = true
}

variable "mysql_administrator_login" {
  description = "Usuario administrador del servidor MySQL."
  type        = string
  default     = "mysqladmin"
}

variable "mysql_administrator_password" {
  description = "Contraseña del administrador de MySQL. Se pasa por TF_VAR_mysql_administrator_password, nunca por un fichero versionado."
  type        = string
  sensitive   = true
}
