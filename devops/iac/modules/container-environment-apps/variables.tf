variable "environment" {
  description = "Entorno al que pertenece el entorno de aplicaciones."
  type        = string
}

variable "resource_group_name" {
  description = "Grupo de recursos donde se crean el entorno, el espacio de logs y la identidad."
  type        = string
}

variable "resource_group_location" {
  description = "Región donde se crean los recursos."
  type        = string
}

variable "container_registry_id" {
  description = "Identificador del registro sobre el que la identidad recibe permiso de descarga."
  type        = string
}

variable "log_analytics_sku" {
  description = "Nivel del espacio de trabajo de Log Analytics."
  type        = string
  default     = "PerGB2018"
}

variable "log_retention_days" {
  description = "Días de retención de los logs, explícitos para no depender del valor por defecto."
  type        = number
  default     = 30
}

variable "tags" {
  description = "Etiquetas que se aplican a los recursos del módulo."
  type        = map(string)
  default     = {}
}
