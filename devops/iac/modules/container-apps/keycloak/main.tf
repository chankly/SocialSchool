locals {
  app_name = "socialschool-keycloak-${var.environment}"

  # El FQDN se compone, no se lee del recurso: KC_HOSTNAME no puede depender de un atributo que solo
  # existe después de crear la aplicación, porque sería una dependencia circular.
  fqdn = "${local.app_name}.${var.default_domain}"
}

resource "azurerm_container_app" "keycloak" {
  name                         = local.app_name
  resource_group_name          = var.resource_group_name
  container_app_environment_id = var.container_app_environment_id
  revision_mode                = "Single"

  identity {
    type         = "UserAssigned"
    identity_ids = [var.identity_id]
  }

  registry {
    server   = var.registry_login_server
    identity = var.identity_id
  }

  secret {
    name                = var.key_vault_secret_names.mysql_admin_password
    key_vault_secret_id = var.key_vault_secret_ids.mysql_admin_password
    identity            = var.identity_id
  }

  secret {
    name                = var.key_vault_secret_names.keycloak_admin_password
    key_vault_secret_id = var.key_vault_secret_ids.keycloak_admin_password
    identity            = var.identity_id
  }

  template {
    # Keycloak guarda sesiones y su caché; se deja una única réplica, sin escalado.
    min_replicas = 1
    max_replicas = 1

    container {
      name    = "keycloak"
      image   = var.image
      cpu     = 1
      memory  = "2Gi"
      command = var.command
      args    = var.args

      env {
        name  = "KC_DB"
        value = "mysql"
      }

      env {
        name  = "KC_DB_URL"
        value = "jdbc:mysql://${var.mysql_fqdn}:3306/keycloak"
      }

      env {
        name  = "KC_DB_USERNAME"
        value = var.mysql_administrator_login
      }

      env {
        name  = "KC_HOSTNAME"
        value = "https://${local.fqdn}"
      }

      # El proxy de Container Apps termina el TLS y reenvía por HTTP.
      env {
        name  = "KC_PROXY_HEADERS"
        value = "xforwarded"
      }

      env {
        name  = "KC_HTTP_ENABLED"
        value = "true"
      }

      env {
        name  = "KC_BOOTSTRAP_ADMIN_USERNAME"
        value = var.keycloak_admin_username
      }

      env {
        name        = "KC_DB_PASSWORD"
        secret_name = var.key_vault_secret_names.mysql_admin_password
      }

      env {
        name        = "KC_BOOTSTRAP_ADMIN_PASSWORD"
        secret_name = var.key_vault_secret_names.keycloak_admin_password
      }
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
    ignore_changes = [template[0].container[0].image]
  }
}
