# Tasks

> Reescrito tras adoptar la estructura del proyecto de referencia: **un solo *root*** con el entorno
> como variable y **un submódulo por aplicación**, en lugar de carpetas por entorno y una lista
> genérica de aplicaciones.

## 1. Preparación

- [x] 1.1 Comprobar que `terraform` está disponible y con una versión compatible (`~> 1.16`); verificado con `terraform version` sobre la 1.16.5.
- [x] 1.2 Escribir `devops/iac/README.md` con el arranque manual del estado (grupo, cuenta y un contenedor por entorno), la convención de nombres, cómo inicializar con `-backend-config` y cómo aplicar; verificado que los nombres del README coinciden con los que usa el `init` documentado.

## 2. Módulos base

- [x] 2.1 Módulo `resource-groups`: crea `socialschool-rg-<entorno>` y publica `name` y `location` para que ningún otro módulo repita la región.
- [x] 2.2 Módulo `container-registry`: **un registro por entorno** (`socialschoolacr<entorno>`), con el usuario administrador deshabilitado porque las aplicaciones descargan con identidad gestionada.
- [x] 2.3 Módulo `container-environment-apps`: espacio de trabajo de logs con retención explícita, entorno de Container Apps, identidad gestionada compartida y su permiso `AcrPull` sobre el registro.

## 3. Secretos

- [x] 3.1 Módulo `key-vault`: vault del entorno con **RBAC habilitado** (`rbac_authorization_enabled`, corregido durante la implementación: sin él las asignaciones de rol no servían de nada), los tres secretos (cadena de conexión, contraseña de MySQL y de Keycloak) y **dos** asignaciones: `Key Vault Secrets User` para la identidad de las aplicaciones y `Key Vault Secrets Officer` para la identidad que despliega.

## 4. Base de datos

- [x] 4.1 Módulo `mysql`: instancia Flexible Server, las bases `api` y `keycloak` con **charset y colación explícitos** (documentando que la colación es insensible a mayúsculas y que afecta a la unicidad del nick), y la regla de firewall que permite servicios de Azure; la variable de versión se llama `engine_version` porque `version` está reservado en los bloques `module`.

## 5. Aplicaciones

- [x] 5.1 Módulo `container-apps` como agregador con **un submódulo por aplicación** (`api` y `keycloak`), cada uno con su `main.tf`, `variables.tf` y `output.tf`.
- [x] 5.2 Submódulo `api`: imagen de ejemplo de Microsoft, `ASPNETCORE_URLS=http://*:8080`, escala por peticiones HTTP, ingress externo y la cadena de conexión recibida como **referencia a Key Vault**, nunca como valor.
- [x] 5.3 Submódulo `keycloak`: variables `KC_*` (MySQL, proxy `xforwarded`, `KC_HOSTNAME` calculado con el dominio del entorno para no crear dependencia circular), credenciales de administración por secretos, **sin `command`/`args`** mientras use la imagen de ejemplo (con la real hay que fijarlos para salir del modo desarrollo) y una única réplica.

## 6. Root

- [x] 6.1 `main.tf` con el bloque `backend` vacío (los valores entran por `-backend-config`, porque el contenedor depende del entorno y el backend no admite variables), el *provider*, el `data azurerm_client_config` y la llamada a los seis módulos con el orden de dependencia.
- [x] 6.2 `variables.tf` con `environment` como variable que gobierna el despliegue, más los dos valores sensibles y los cuatro ajustes con valor por defecto (audiencia, realm, administrador de Keycloak y login de MySQL).

## 7. Verificación

- [x] 7.1 `terraform fmt -check -recursive devops/iac` sin diferencias.
- [x] 7.2 `terraform init -backend=false` y `terraform validate` en `devops/iac` → `Success! The configuration is valid.`, lo que comprueba el cableado de los seis módulos y de los dos submódulos de aplicación.
- [x] 7.3 Revisar el diff: solo `devops/iac/` más los artefactos del propio cambio, sin tocar `src/`, `tests/`, la configuración de la aplicación ni `devops/keycloak-webhooks/`.
