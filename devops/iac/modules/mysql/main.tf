resource "azurerm_mysql_flexible_server" "server" {
  name                   = "socialschool-mysql-${var.environment}"
  resource_group_name    = var.resource_group_name
  location               = var.resource_group_location
  administrator_login    = var.administrator_login
  administrator_password = var.administrator_password
  sku_name               = var.sku_name
  version                = var.engine_version
  tags                   = var.tags
}

resource "azurerm_mysql_flexible_database" "database" {
  for_each = { for database in var.databases : database.name => database }

  name                = each.value.name
  resource_group_name = var.resource_group_name
  server_name         = azurerm_mysql_flexible_server.server.name
  charset             = each.value.charset
  collation           = each.value.collation
}

# Un entorno de Container Apps de consumo no tiene direcciones de salida fijas, así que el firewall no
# puede restringirse por IP. 0.0.0.0 -> 0.0.0.0 es la convención de Azure para "permitir servicios de
# Azure". Producción querrá red privada.
resource "azurerm_mysql_flexible_server_firewall_rule" "azure_services" {
  name                = "allow-azure-services"
  resource_group_name = var.resource_group_name
  server_name         = azurerm_mysql_flexible_server.server.name
  start_ip_address    = "0.0.0.0"
  end_ip_address      = "0.0.0.0"
}
