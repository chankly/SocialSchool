# Proposal: Dockerfile de la API y su pipeline

## Why

La Container App de la API se despliega hoy con la **imagen de ejemplo de Microsoft**, porque la
infraestructura se escribió antes de que existiera una imagen real que desplegar. El módulo de la API
**ya ignora la imagen** (`ignore_changes`), es decir, la pipeline es la dueña de ella, pero esa
pipeline no existe: el repositorio **no tiene ningún `Dockerfile`** y no hay forma de construir la
imagen del backend en ningún sitio. Consecuencia: la única manera de desplegar la API es construirla y
empujarla a mano, sin traza ni repetibilidad, y mientras tanto la aplicación corre el contenedor de
ejemplo. Es exactamente la situación que tenía Keycloak antes de su pipeline, con tres diferencias: aquí
**sí** hay Dockerfile que escribir y **sí** pasa por nuestro registro, y además esta pipeline **nombra
los recursos del entorno**, así que depende de que su prefijo coincida con el que usa Terraform.

## What Changes

- **Nuevo `Dockerfile`** en la raíz del repositorio, multi-stage (`dotnet/sdk:10.0` → `dotnet/aspnet:10.0`),
  que publica `HSoft.SocialSchool.Api` y arranca el contenedor con un **usuario no root** en el
  **puerto 8080**, que es el que espera el ingress de Terraform. Más un `.dockerignore` para no mandar
  al contexto lo que no hace falta.
- **Nuevo workflow de GitHub Actions**, de **disparo manual** (`workflow_dispatch`), con **un único
  parámetro**: el entorno (`develop`, `qa` o `production`). Hace tres cosas: **construye** la imagen en
  el runner, la **publica** en el registro del entorno y **actualiza la Container App** de la API a esa
  imagen.
- **Etiqueta por commit**: la imagen se publica como `socialschool-api:<sha-corto>`. La cadena de imagen
  cambia en cada ejecución, así que Container Apps crea revisión nueva sin depender de nada más.
  **No** hay etiqueta móvil (`:develop`).
- **Sin tests en la pipeline**: decisión del usuario. La pipeline construye y publica; los tests de
  integración siguen siendo un paso manual.
- **Autenticación con OIDC federado**: el workflow pide un token en cada ejecución y **no guarda ningún
  secreto** en el repositorio.
- **Documentación de `ENVIRONMENT_PREFIX`**: el workflow compone los nombres de los recursos con esta
  variable, que hoy **no está documentada en ninguna parte** y sin la cual los nombres salen como
  `socialschool-rg-`. Es una **variable de entorno de GitHub**, una por *environment*, con el prefijo
  corto del entorno: **`dv`** para *develop*, **`qa`** para *qa* y **`pr`** para *production*.
- **Alineación de los nombres de los recursos a `dv`/`qa`/`pr`**: `devops/iac/variables.tf` ya declara
  esos valores y el workflow de Keycloak ya compone los nombres con el prefijo corto, pero los ejemplos
  de `devops/iac/README.md` y el ámbito del rol en `devops/README.md` usan el nombre completo del
  entorno (`socialschool-rg-develop`), que **no** es lo que crea Terraform.
- **Corrección del `docker build` manual**: el comando del README de `iac` usa hoy el contexto
  equivocado (`.` después de `cd devops/iac`, que es el directorio de Terraform y no el repositorio).

## Capabilities

### New Capabilities

Ninguna. Esto es **construcción y despliegue**, no comportamiento del producto: no añade endpoints, ni
reglas de negocio, ni nada que un usuario del sistema pueda notar. Se declara `skip_specs: true` en
`.openspec.yaml` y **no** se crea delta de capacidad.

### Modified Capabilities

Ninguna.

## Impact

- **Nuevo**: `Dockerfile`, `.dockerignore` y `.github/workflows/api-deploy.yml`.
- **Modificado**: `devops/README.md` (la fila y la sección de la pipeline, la variable
  `ENVIRONMENT_PREFIX` con sus tres valores, y el rol `AcrPush`) y `devops/iac/README.md` (el contexto
  del `docker build` manual y los nombres de recurso de todos sus ejemplos). **Nada** en `src/`, en
  `tests/` ni en la configuración de la aplicación: el Dockerfile se construye *desde* el código, no lo
  cambia, y los nombres siguen siendo de Terraform.
- **Servicios de Azure**: se publica en el registro del entorno y se actualiza la Container App de la
  API. La identidad que ejecuta el workflow necesita, además de lo que ya tiene la de Keycloak,
  **`AcrPush` sobre el registro**: empujar una imagen es plano de datos, así que ser `Contributor` del
  grupo de recursos **no** basta. Es la diferencia principal con la pipeline de Keycloak.
- **El registro estrena repositorio**: `socialschool-api` no existe hasta el primer push.
- **Cada ejecución crea una revisión nueva** de la Container App y **deja una etiqueta más** en el
  registro.
- **No sustituye a Terraform**: el workflow no toca variables de entorno, secretos, ingress ni escala.
- **Pasos del usuario**: tener el entorno aplicado (el registro y la aplicación existen), configurar la
  federación OIDC, añadir el rol `AcrPush` y definir `ENVIRONMENT_PREFIX` (`dv`, `qa`, `pr`) en cada
  *environment* de GitHub.
