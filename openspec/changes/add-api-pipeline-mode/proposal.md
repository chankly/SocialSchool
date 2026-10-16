# Proposal: Dos modos en la pipeline de la API

## Why

La pipeline de la API, tal como quedó, hace **siempre** las tres cosas: construye la imagen, la publica
en el registro del entorno y apunta la Container App a ella. No hay manera de publicar una imagen sin
cambiar lo que está corriendo, y eso hace falta en dos situaciones concretas: dejar la imagen construida
y publicada antes de un despliegue que se decide después, y comprobar que la imagen se construye y se
publica en el registro sin tocar el entorno. Además, hoy cualquier pulsación de *Run workflow* termina
desplegando: el paso que cambia lo que ve la gente es justo el que no se puede evitar.

## What Changes

- **Nuevo parámetro `mode`** en el disparo manual, con dos valores: **`Build`** (construye la imagen y la
  publica en el registro del entorno) y **`BuildAndDeploy`** (además apunta la Container App a esa
  imagen). Trae **`Build`** por defecto, así que *Run workflow* sin tocar nada **no despliega**.
- **El despliegue pasa a ser condicional**: el paso que actualiza la Container App solo se ejecuta en
  `BuildAndDeploy`. Construir y publicar se hace en los dos modos.
- **Un resumen distinto por modo**: en `BuildAndDeploy` se muestra la imagen que ha quedado en la
  aplicación; en `Build` se dice que la imagen está publicada y que la aplicación **no** se ha tocado,
  para que nunca se anuncie un despliegue que no ha ocurrido.
- **Documentación**: `devops/README.md` (el parámetro nuevo en la tabla y los dos modos en la sección de
  la API) y una línea en `devops/iac/README.md`, donde publicar una imagen a mano ya no hace falta ni
  para eso.

## Capabilities

### New Capabilities

Ninguna. Esto es **construcción y despliegue**, no comportamiento del producto: no añade endpoints, ni
reglas de negocio, ni nada que un usuario del sistema pueda notar. Se declara `skip_specs: true` en
`.openspec.yaml` y **no** se crea delta de capacidad.

### Modified Capabilities

Ninguna.

## Impact

- **Modificado**: `.github/workflows/api-deploy.yml`, `devops/README.md` y `devops/iac/README.md`.
  **Nada** en `src/`, ni en el `Dockerfile`, ni en Terraform: el despliegue sigue siendo la misma orden
  de `az containerapp update`, solo que ahora es opcional.
- **Cambia el comportamiento por defecto**: antes, una ejecución sin tocar nada desplegaba; ahora
  construye y publica. Para desplegar hay que elegir `BuildAndDeploy` a propósito.
- **Permisos**: sin cambios. `Build` también entra en Azure, porque necesita `az acr login` para
  publicar, así que el `AcrPush` sobre el registro sigue haciendo falta en los dos modos.
- **Ni la etiqueta ni la convención de nombres cambian**: la imagen se sigue publicando con el sha corto
  del commit, en el registro del entorno.
