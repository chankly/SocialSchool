# Tasks

> Las decisiones abiertas del diseño (retención de revisiones, segundo parámetro para la versión,
> pipeline de la API) no cambian lo que hay que construir aquí, así que no bloquean estas tareas.

## 1. Workflow

- [x] 1.1 Crear `.github/workflows/keycloak-deploy.yml` con `on: workflow_dispatch`, un `input` de tipo `choice` con `develop`, `qa` y `production` (por defecto `develop`), `permissions` mínimos (`id-token: write` para OIDC y `contents: read`) y un paso `azure/login` con credenciales federadas alimentadas por variables del repositorio. Verificación: **comprobación estructural mecánica** (estructura, profundidades, ausencia de tabuladores y de entradas de más). *Matiz*: en esta máquina **no hay analizador de YAML** (PyYAML no está instalado y PowerShell 7 no trae `ConvertFrom-Yaml`) y no se instalan herramientas, así que **la sintaxis la valida GitHub al subir el workflow**, no esta comprobación.
- [x] 1.2 Añadir el paso de despliegue con `az containerapp update -g socialschool-rg-<entorno> -n socialschool-keycloak-<entorno> --image quay.io/phasetwo/phasetwo-keycloak:26.8.0.1790930320 --command /opt/keycloak/bin/kc.sh --args start`, componiendo los nombres a partir del parámetro. Verificación mecánica: los nombres se componen con `${{ inputs.environment }}`, no hay ningún entorno cableado y no aparece el ACR, ni `--yaml`, ni variables de entorno, ni secretos, ni ingress, ni escala.
- [x] 1.3 Documentar en la cabecera del workflow el permiso que necesita la identidad, la convención de nombres, que el disparo es manual y qué **no** hace el workflow. Verificación: la cabecera cubre los cinco puntos y enlaza con `devops/README.md` para el alta de la federación.

## 2. Terraform

- [x] 2.1 Añadir `command` y `args` a los `ignore_changes` del submódulo `modules/container-apps/keycloak`, con el comentario que explica por qué la pipeline es dueña de esos dos valores y no solo de la imagen. Verificación: `terraform validate` pasa y el formato del módulo está limpio.

## 3. Documentación del alta

- [x] 3.1 Crear `devops/README.md` con el alta de la federación OIDC (identidad, *federated credential* hacia el repositorio y rol para actualizar la Container App), las variables del repositorio y cómo lanzar el workflow, dejando escrito que **la ejecución es paso del usuario**. Verificación: los comandos son copiables y los nombres de recursos coinciden con los del workflow.
- [x] 3.2 Añadir en `devops/iac/README.md` la nota de que la imagen de Keycloak la gobierna la pipeline, que el `image` de Terraform es solo el valor inicial y que recrear la aplicación la devuelve a la imagen de ejemplo. *Además*, y por indicación del usuario, se corrigieron los nombres del almacenamiento del estado del README para que coincidan con los de `main.tf` (`hsoft-rg`, `hsoftcontainer`, contenedor `socialschool-terraform-<entorno>`), que son los reales.

## 4. Verificación

- [x] 4.1 Ejecutar `terraform fmt -check -recursive devops\iac` y `terraform -chdir=devops/iac init -backend=false` + `validate`. La validación pasa (`Success! The configuration is valid.`) y el formato está limpio **salvo `devops/iac/main.tf`**, que la comprobación señala porque el usuario decidió mantener su alineación manual; el fichero se restauró a su versión y **no forma parte de este cambio**.
- [x] 4.2 Revisar el diff: solo se han añadido `.github/`, `devops/README.md` y los artefactos de este cambio, más la línea del submódulo y la corrección del README de `iac`; sin tocar `src/`, `tests/` ni la configuración de la aplicación.
