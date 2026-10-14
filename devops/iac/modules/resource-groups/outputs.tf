output "name" {
  description = "Nombre del grupo de recursos, que el resto de módulos usa como destino."
  value       = azurerm_resource_group.rg.name
}

output "location" {
  description = "Región del grupo de recursos, para que ningún módulo tenga que repetirla."
  value       = azurerm_resource_group.rg.location
}

output "id" {
  description = "Identificador del grupo de recursos."
  value       = azurerm_resource_group.rg.id
}
