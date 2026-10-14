# Proposal: Infraestructura de Azure con Terraform

## Why

El producto no tiene forma de desplegarse: no existe ni un fichero de Terraform, ni un Dockerfile, ni
un pipeline en el repositorio, y el backend **no se ha ejecutado nunca contra una base de datos**
(`ConnectionStrings:MyCompany` sigue siendo la cadena de ejemplo inválida). Antes de seguir
construyendo funcionalidad hace falta un sitio donde desplegarla, y ese sitio debe declararse como
código para que sea reproducible en lugar de un conjunto de recursos creados a mano.

## What Changes

- **Nueva estructura de Terraform** en `devops/iac/`, separada en módulos y en un *root* por entorno.
  Solo se implementa **`develop`**; `qa` y `production` quedan como estructura a replicar.
- **Seis módulos**: `resource-groups`, `container-registry`, `container-environments`, `key-vault`,
  `mysql` y `container-apps`.
- **Dos aplicaciones** desplegadas en Container Apps: la **API** y **Keycloak**
  (`quay.io/phasetwo/phasetwo-keycloak:26.8.0.1790930320`). El módulo recibe una **lista**, no apps
  cableadas.
- **Una instancia de Azure Database for MySQL Flexible Server por entorno** con **dos bases**:
  `api` y `keycloak`.
- **Un Key Vault por entorno**. Los contenedores reciben los secretos como **variables de entorno que
  referencian secretos del vault**, a través de una identidad gestionada con el rol
  `Key Vault Secrets User`.
- **Estado remoto desde el principio**: un *storage account* con **un contenedor por entorno**. Ese
  *storage account* y su grupo de recursos se crean **a mano y documentados en el README**, porque
  Terraform no puede crear el sitio donde guarda su propio estado.
- **Un ACR compartido** entre entornos, con un repositorio por aplicación, y permiso `AcrPull` para la
  identidad gestionada del entorno de Container Apps.
- **Keycloak en modo producción** (`start`, no `start-dev`), con `KC_PROXY_HEADERS=xforwarded` y
  `KC_HOSTNAME` apuntando a su FQDN externo, y sus credenciales de administración en Key Vault.

## Capabilities

### New Capabilities

Ninguna. Este cambio **no modifica comportamiento observable del producto**: no añade endpoints, ni
reglas de negocio, ni nada que un usuario pueda notar. Es infraestructura, así que se declara
`skip_specs: true` en `.openspec.yaml` y **no** se crea delta de capacidad.

### Modified Capabilities

Ninguna.

## Impact

- **Nuevo**: toda la carpeta `devops/iac/` — seis módulos, el *root* de `develop` y un README con el
  arranque manual del estado y las convenciones de nombres.
- **Sin cambios** en `src/`, en los tests ni en la configuración de la aplicación: las apps reciben su
  configuración por variables de entorno, que ya es una fuente que el host lee
  (`AddEnvironmentVariables`, `Keycloak:Authority` / `Keycloak:Audience`, `ConnectionStrings:MyCompany`).
- **Servicios de Azure**: grupos de recursos, Azure Container Registry, Container Apps Environment,
  Log Analytics, Key Vault y Azure Database for MySQL Flexible Server.
- **Dependencias que quedan fuera** y que hacen que la API **no pueda desplegarse todavía**: (a) una
  **imagen**, es decir un Dockerfile y un pipeline que la publique en el ACR, y (b) hablar **MySQL**,
  que exige cambiar el proveedor de EF Core y regenerar la migración. Son cambios propios:
  `dockerfile + pipeline` y `switch-to-mysql`.
- **No hay despliegue efectivo**: el cambio deja la estructura escrita y validada con `fmt` y
  `validate`. El primer `plan`/`apply` es un paso del usuario y necesita credenciales, el estado
  remoto ya creado y una imagen publicada en el ACR.
