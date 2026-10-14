resource "azurerm_resource_group" "rg" {
  name     = "socialschool-rg-${var.environment}"
  location = var.location
  tags     = var.tags
}
