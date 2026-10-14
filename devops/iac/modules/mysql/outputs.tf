output "fqdn" {
  description = "Nombre DNS del servidor, que es lo que necesitan las cadenas de conexión."
  value       = azurerm_mysql_flexible_server.server.fqdn
}

output "server_name" {
  description = "Nombre del servidor."
  value       = azurerm_mysql_flexible_server.server.name
}

output "database_names" {
  description = "Nombres de las bases creadas."
  value       = [for database in azurerm_mysql_flexible_database.database : database.name]
}
