variable "environment" {
  description = "Entorno al que pertenece el vault."
  type        = string
}

variable "resource_group_name" {
  description = "Grupo de recursos donde se crea el vault."
  type        = string
}

variable "resource_group_location" {
  description = "Región donde se crea el vault."
  type        = string
}

variable "tenant_id" {
  description = "Inquilino propietario del vault."
  type        = string
}

variable "deployer_object_id" {
  description = "Identificador del principal que ejecuta el despliegue y que, por tanto, necesita escribir los secretos."
  type        = string
}

variable "identity_principal_id" {
  description = "Identificador de la identidad gestionada que recibe permiso de lectura de secretos."
  type        = string
}

variable "sku_name" {
  description = "Nivel del vault (standard o premium)."
  type        = string
  default     = "standard"
}

variable "soft_delete_retention_days" {
  description = "Días que el nombre queda reservado tras borrar un secreto o el vault (7 a 90). Un valor bajo acorta la espera al recrear en entornos no productivos."
  type        = number
  default     = 7
}

variable "purge_protection_enabled" {
  description = "Protección de purga. Desactivada en entornos no productivos para poder recrear el vault con el mismo nombre; producción debería activarla."
  type        = bool
  default     = false
}

variable "mysql_connection_string" {
  description = "Cadena de conexión completa a la base de datos de la API, tal y como la leerá la aplicación."
  type        = string
  sensitive   = true
}

variable "mysql_admin_password" {
  description = "Contraseña del administrador del servidor MySQL, que consume Keycloak."
  type        = string
  sensitive   = true
}

variable "keycloak_admin_password" {
  description = "Contraseña del administrador de Keycloak."
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

variable "tags" {
  description = "Etiquetas que se aplican al vault."
  type        = map(string)
  default     = {}
}
