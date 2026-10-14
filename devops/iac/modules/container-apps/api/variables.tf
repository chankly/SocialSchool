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

variable "identity_id" {
  description = "Identidad gestionada para descargar la imagen y leer los secretos."
  type        = string
}

variable "registry_login_server" {
  description = "Servidor del registro del que se descarga la imagen."
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

variable "keycloak_authority" {
  description = "Autoridad de Keycloak contra la que la API valida los tokens."
  type        = string
}

variable "api_audience" {
  description = "Audiencia que la API espera en el token."
  type        = string
}

variable "image" {
  description = "Imagen del contenedor. Por defecto la de ejemplo de Microsoft, porque la definitiva todavía no existe; la pipeline la reemplaza después y Terraform no la revierte."
  type        = string
  default     = "mcr.microsoft.com/azuredocs/containerapps-helloworld:latest"
}

variable "min_replicas" {
  description = "Réplicas mínimas. Cero permite apagar la aplicación cuando nadie la usa."
  type        = number
  default     = 0
}

variable "max_replicas" {
  description = "Réplicas máximas."
  type        = number
  default     = 5
}
