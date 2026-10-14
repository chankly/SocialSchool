variable "environment" {
  description = "Entorno al que pertenece el servidor."
  type        = string
}

variable "resource_group_name" {
  description = "Grupo de recursos donde se crea el servidor."
  type        = string
}

variable "resource_group_location" {
  description = "Región donde se crea el servidor."
  type        = string
}

variable "administrator_login" {
  description = "Usuario administrador del servidor."
  type        = string
}

variable "administrator_password" {
  description = "Contraseña del administrador. Se recibe desde el root para que sea la misma que se guarda en Key Vault."
  type        = string
  sensitive   = true
}

variable "sku_name" {
  description = "Nivel del servidor. B_Standard_B1s es el burstable más barato y basta para desarrollo."
  type        = string
  default     = "B_Standard_B1ms"
}

variable "engine_version" {
  description = "Versión del motor MySQL. Se llama engine_version y no version porque `version` está reservado en los bloques module."
  type        = string
  default     = "8.0.21"
}

variable "databases" {
  description = <<-DESCRIPTION
    Bases que se crean en el servidor, con su charset y su colación explícitos. La colación es la de
    MySQL 8 para utf8mb4 y es INSENSIBLE A MAYÚSCULAS: `Peter` y `peter` colisionarían al compartir
    índice único, lo que afecta a la unicidad del nick del dominio. El cambio `switch-to-mysql` tiene
    que confirmar que es la que espera EF Core.
  DESCRIPTION
  type = list(object({
    name      = string
    charset   = string
    collation = string
  }))
  default = [
    { name = "api", charset = "utf8mb4", collation = "utf8mb4_0900_ai_ci" },
    { name = "keycloak", charset = "utf8mb4", collation = "utf8mb4_0900_ai_ci" },
  ]
}

variable "tags" {
  description = "Etiquetas que se aplican al servidor."
  type        = map(string)
  default     = {}
}
