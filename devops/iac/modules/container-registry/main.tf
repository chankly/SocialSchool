# Un registro por entorno: el nombre lleva el entorno porque los nombres de ACR son únicos a nivel
# global y solo admiten alfanuméricos.
#
# Los repositorios no son un recurso de Azure: aparecen al empujar la primera imagen, así que este
# módulo no los declara.
resource "azurerm_container_registry" "acr" {
  name                = "socialschoolacr${var.environment}"
  resource_group_name = var.resource_group_name
  location            = var.resource_group_location
  sku                 = var.sku

  # Las aplicaciones descargan con su identidad gestionada, no con usuario y contraseña.
  admin_enabled = false

  tags = var.tags
}
