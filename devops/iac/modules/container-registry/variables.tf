variable "environment" {
  description = "Entorno al que pertenece el registro."
  type        = string
}

variable "resource_group_name" {
  description = "Grupo de recursos donde se crea el registro."
  type        = string
}

variable "resource_group_location" {
  description = "Región donde se crea el registro."
  type        = string
}

variable "sku" {
  description = "Nivel del registro (Basic, Standard o Premium)."
  type        = string
  default     = "Basic"
}

variable "tags" {
  description = "Etiquetas que se aplican al registro."
  type        = map(string)
  default     = {}
}
