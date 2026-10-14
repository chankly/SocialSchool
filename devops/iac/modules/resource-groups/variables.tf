variable "environment" {
  description = "Entorno al que pertenece el grupo de recursos."
  type        = string
}

variable "location" {
  description = "Región de Azure del grupo de recursos."
  type        = string
  default     = "spaincentral"
}

variable "tags" {
  description = "Etiquetas que se aplican al grupo de recursos."
  type        = map(string)
  default     = {}
}
