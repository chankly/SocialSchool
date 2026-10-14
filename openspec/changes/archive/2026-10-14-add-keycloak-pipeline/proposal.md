# Proposal: Pipeline de Keycloak

## Why

La Container App de Keycloak se despliega hoy con la **imagen de ejemplo de Microsoft**, porque la
infraestructura se escribió antes de que hubiera una imagen real que desplegar. No existe ninguna
forma de que un developer apunte la aplicación a la imagen real de Keycloak: el repositorio **no tiene
ni un solo workflow** —no existe `.github/`— y hacerlo a mano con `az containerapp update` deja el
despliegue sin traza ni repetibilidad.

## What Changes

- **Nuevo workflow de GitHub Actions**, de **disparo manual** (`workflow_dispatch`), con **un único
  parámetro**: el entorno (`develop`, `qa` o `production`).
- El workflow **actualiza la Container App de Keycloak** del entorno elegido con la imagen
  `quay.io/phasetwo/phasetwo-keycloak:26.8.0.1790930320`, y fija además **comando y argumentos**
  (`/opt/keycloak/bin/kc.sh` + `start`), porque esa imagen arranca en **modo desarrollo** por defecto.
- **La imagen no pasa por nuestro registro**: se descarga de `quay.io`, que es público. No hay build,
  no hay push y **la pipeline no necesita permiso de escritura sobre el ACR**.
- **Autenticación con OIDC federado**: el workflow pide un token en cada ejecución y **no guarda ningún
  secreto** en el repositorio.
- **Un cambio en Terraform**: `command` y `args` se añaden a los `ignore_changes` del submódulo de
  Keycloak, para que la pipeline sea dueña de la imagen **y de cómo arranca**, y Terraform siga siendo
  dueño del resto (variables de entorno, secretos, ingress y escala). Sin esto, el siguiente `apply`
  borraría el comando y Keycloak volvería al modo desarrollo.
- **Documentación** del alta de la federación OIDC, que es un paso manual en Azure.

## Capabilities

### New Capabilities

Ninguna. Esto es **herramienta de despliegue**, no comportamiento del producto: no añade endpoints, ni
reglas de negocio, ni nada que un usuario del sistema pueda notar. Se declara `skip_specs: true` en
`.openspec.yaml` y **no** se crea delta de capacidad.

### Modified Capabilities

Ninguna.

## Impact

- **Nuevo**: `.github/workflows/keycloak-deploy.yml` (el repositorio no tiene workflows) y la
  documentación del alta de la federación.
- **Modificado**: `devops/iac/modules/container-apps/keycloak/main.tf` — `command` y `args` añadidos a
  `ignore_changes`.
- **Sin cambios** en `src/`, en los tests ni en la configuración de la aplicación.
- **Servicios de Azure**: el registro queda **al margen** de esta pipeline; solo se toca la Container
  App de Keycloak del entorno elegido. La identidad que ejecuta el workflow necesita permiso para
  actualizarla.
- **Cada ejecución crea una revisión nueva** de la Container App.
- **No sustituye a Terraform**: el workflow no toca variables de entorno, secretos, ingress ni escala.
- **Paso del usuario**: configurar la federación OIDC una vez; sin ella el workflow no puede
  autenticarse contra Azure.
