variable "environment" {
  description = "Entorno al que pertenece la aplicación."
  type        = string
}

variable "resource_group_name" {
  description = "Grupo de recursos donde se despliega."
  type        = string
}

variable "container_app_environment_id" {
  description = "Identificador del entorno de Container Apps."
  type        = string
}

variable "default_domain" {
  description = "Dominio por defecto del entorno, con el que se compone el FQDN de la aplicación."
  type        = string
}

variable "identity_id" {
  description = "Identidad gestionada para descargar la imagen y leer los secretos."
  type        = string
}

variable "registry_login_server" {
  description = "Servidor del registro del que se descarga la imagen."
  type        = string
}

variable "mysql_fqdn" {
  description = "Nombre DNS del servidor MySQL, con el que se compone la URL JDBC."
  type        = string
}

variable "mysql_administrator_login" {
  description = "Usuario administrador del servidor MySQL."
  type        = string
}

variable "key_vault_secret_ids" {
  description = "Identificadores sin versión de los secretos de Key Vault."
  type        = map(string)
}

variable "key_vault_secret_names" {
  description = "Nombres de los secretos de Key Vault."
  type        = map(string)
}

variable "keycloak_admin_username" {
  description = "Usuario administrador inicial de Keycloak."
  type        = string
}

variable "image" {
  description = "Imagen del contenedor. Por defecto la de ejemplo de Microsoft, porque la definitiva todavía no existe; la pipeline la reemplaza después y Terraform no la revierte."
  type        = string
  default     = "mcr.microsoft.com/azuredocs/containerapps-helloworld:latest"
}

variable "command" {
  description = "Sobrescribe el entrypoint del contenedor. Debe quedar vacío mientras se use la imagen de ejemplo; con la imagen real de Keycloak hay que fijarlo en [\"/opt/keycloak/bin/kc.sh\"] para poder arrancar en modo producción."
  type        = list(string)
  default     = null
}

variable "args" {
  description = "Argumentos del contenedor. Debe quedar vacío con la imagen de ejemplo; con la imagen real de Keycloak hay que fijarlo en [\"start\"], porque su valor por defecto es el modo desarrollo."
  type        = list(string)
  default     = null
}
