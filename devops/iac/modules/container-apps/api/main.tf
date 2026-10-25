resource "azurerm_container_app" "api" {
  name                         = "socialschool-api-${var.environment}"
  resource_group_name          = var.resource_group_name
  container_app_environment_id = var.container_app_environment_id
  revision_mode                = "Single"

  identity {
    type         = "UserAssigned"
    identity_ids = [var.identity_id]
  }

  # La descarga de la imagen va con la identidad gestionada, no con credenciales del registro.
  registry {
    server   = var.registry_login_server
    identity = var.identity_id
  }

  # Referencia a Key Vault: aquí no viaja el valor, solo el identificador del secreto.
  secret {
    name                = var.key_vault_secret_names.mysql_connection_string
    key_vault_secret_id = var.key_vault_secret_ids.mysql_connection_string
    identity            = var.identity_id
  }

  # Credenciales del cliente con el que la API consulta la Admin API de Keycloak al recibir un registro.
  secret {
    name                = var.key_vault_secret_names.keycloak_admin_client_id
    key_vault_secret_id = var.key_vault_secret_ids.keycloak_admin_client_id
    identity            = var.identity_id
  }

  secret {
    name                = var.key_vault_secret_names.keycloak_admin_client_secret
    key_vault_secret_id = var.key_vault_secret_ids.keycloak_admin_client_secret
    identity            = var.identity_id
  }

  template {
    min_replicas = var.min_replicas
    max_replicas = var.max_replicas

    container {
      name   = "api"
      image  = var.image
      cpu    = 0.5
      memory = "1Gi"

      env {
        name  = "ASPNETCORE_URLS"
        value = "http://*:8080"
      }

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }

      env {
        name  = "Keycloak__Authority"
        value = var.keycloak_authority
      }

      env {
        name  = "Keycloak__Audience"
        value = var.api_audience
      }

      env {
        name        = "Keycloak__Admin__ClientId"
        secret_name = var.key_vault_secret_names.keycloak_admin_client_id
      }

      env {
        name        = "Keycloak__Admin__ClientSecret"
        secret_name = var.key_vault_secret_names.keycloak_admin_client_secret
      }

      env {
        name        = "ConnectionStrings__MyCompany"
        secret_name = var.key_vault_secret_names.mysql_connection_string
      }
    }

    http_scale_rule {
      name                = "http-scale-api"
      concurrent_requests = 10
    }
  }

  ingress {
    external_enabled = true
    target_port      = 8080
    transport        = "http"

    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }

  lifecycle {
    # La pipeline de despliegue reemplaza la imagen; así Terraform no la revierte en el siguiente apply.
    ignore_changes = [template[0].container[0].image]
  }
}
