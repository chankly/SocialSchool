# devops

Herramienta de despliegue del proyecto. Hoy convive con la infraestructura, que vive en `iac/`.

```
devops/
  iac/            infraestructura de Azure con Terraform (ver su propio README)
  README.md       este fichero
```

## Pipelines

| Pipeline | Fichero | Qué hace | Parámetros |
|---|---|---|---|
| **API** | `.github/workflows/api-deploy.yml` | Construye la imagen de la API con el `Dockerfile` de la raíz, la publica en el registro del entorno y apunta la Container App de la API a esa imagen. | `environment` (`develop` \| `qa` \| `production`) |
| **Keycloak** | `.github/workflows/keycloak-deploy.yml` | Apunta la Container App de Keycloak del entorno elegido a la imagen pública de Phase Two y la arranca en modo producción. | `environment` (`develop` \| `qa` \| `production`) |

Todas se lanzan **a mano** desde GitHub: `Actions` → la pipeline → `Run workflow`.

### La pipeline de la API en detalle

- **Sí construye y publica imágenes.** Es la que llena el registro: cada ejecución publica
  `socialschool-api` con la etiqueta del **sha corto del commit**.
- **Necesita `AcrPush` además de `Contributor`.** Publicar una imagen es plano de datos, así que
  `Contributor` sobre el grupo de recursos permite actualizar la Container App pero **no** empujar la
  imagen. Es la diferencia principal con la pipeline de Keycloak, que no necesita nada de eso porque su
  imagen es pública y la descarga la propia aplicación.
- **Solo cambia la imagen.** No toca variables de entorno, secretos, ingress ni escala: eso lo gobierna
  Terraform, que por eso ignora (`ignore_changes`) la imagen de esa aplicación.
- **Garantiza revisión nueva.** Con una etiqueta fija por entorno la cadena de imagen podría repetirse
  entre dos ejecuciones y Container Apps podría no crear revisión, dejando la anterior sin que nada lo
  delate; con la etiqueta por commit eso no puede pasar.
- **No ejecuta los tests.** Los tests de integración son un paso manual.
- **El `Dockerfile` vive dentro del proyecto** (`src/HSoft.SocialSchool.Api/Dockerfile`), como cuando
  Visual Studio agrega compatibilidad con Docker, y el contexto de build es **`src/`**: es lo que usa el
  tooling de contenedores (`DockerDefaultContext = ..`) y lo que el build necesita, porque la API
  referencia a los otros tres proyectos. El `.dockerignore` está en `src/`, la raíz del contexto, y
  sirve para todos los proyectos que se construyan desde ahí.

### La pipeline de Keycloak en detalle

- **No construye ni publica imágenes.** La imagen viene de `quay.io`, que es público, y la descarga la
  propia Container App. Por eso no necesita permiso de escritura sobre el registro.
- **Solo cambia tres cosas**: la imagen, el comando y los argumentos. El comando es necesario porque la
  imagen de Phase Two arranca en **modo desarrollo** por defecto.
- **No toca** variables de entorno, secretos, ingress ni escala: eso lo gobierna Terraform. Por eso
  `devops/iac` ignora (`ignore_changes`) la imagen, el comando y los argumentos de esa aplicación.
- Cada ejecución **crea una revisión nueva** de la Container App.

## Alta de la federación OIDC (una sola vez)

El workflow no guarda ningún secreto: pide un token a Azure en cada ejecución, y para eso hay que
crear una identidad federada con este repositorio.

```bash
# 1. Identidad y su service principal
APP_ID=$(az ad app create --display-name "socialschool-github-pipeline" --query appId -o tsv)
az ad sp create --id "$APP_ID"

# 2. Credencial federada: GitHub pide el token en nombre de este repositorio.
#    Sustituye <owner>/<repo> y la rama por las tuyas. El `subject` debe corresponder a la referencia
#    desde la que se dispara; si vas a lanzarlo desde varias ramas, necesitas una credencial por rama.
cat > federated-credential.json <<'JSON'
{
  "name": "github-socialschool",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:<owner>/<repo>:ref:refs/heads/main",
  "audiences": ["api://AzureADTokenExchange"]
}
JSON
az ad app federated-credential create --id "$APP_ID" --parameters federated-credential.json

# 3. Permisos sobre el entorno. Repite los dos para cada entorno que vayas a desplegar, cambiando el
#    prefijo (dv, qa o pr) en el ámbito.
#
#    3.a Contributor sobre el grupo de recursos: actualizar las Container Apps. Lo necesitan las dos
#        pipelines.
az role assignment create \
  --assignee "$APP_ID" \
  --role Contributor \
  --scope "/subscriptions/<subscription-id>/resourceGroups/socialschool-rg-dv"

#    3.b AcrPush sobre el registro: publicar imágenes. Solo lo necesita la pipeline de la API.
az role assignment create \
  --assignee "$APP_ID" \
  --role AcrPush \
  --scope "/subscriptions/<subscription-id>/resourceGroups/socialschool-rg-dv/providers/Microsoft.ContainerRegistry/registries/socialschoolacrdv"
```

Después, en el repositorio de GitHub (`Settings` → `Secrets and variables` → `Actions` → **Variables**),
define estas tres, que son identificadores y no credenciales:

| Variable | Valor |
|---|---|
| `AZURE_CLIENT_ID` | el `appId` de la identidad (`$APP_ID`) |
| `AZURE_TENANT_ID` | el inquilino de Azure |
| `AZURE_SUBSCRIPTION_ID` | la suscripción donde viven los entornos |

### `ENVIRONMENT_PREFIX`: el prefijo del entorno

Las pipelines **nombran recursos de Azure**, y el nombre lleva el prefijo corto del entorno. Ese valor
no es un identificador de Azure ni un secreto: es cómo se llama cada entorno, así que se define
**dentro de cada *environment* de GitHub** (`Settings` → `Environments` → el entorno → `Environment
variables`), no en las variables del repositorio, para que cada entorno lleve el suyo.

| *Environment* de GitHub | `ENVIRONMENT_PREFIX` | Grupo de recursos resultante |
|---|---|---|
| `develop` | `dv` | `socialschool-rg-dv` |
| `qa` | `qa` | `socialschool-rg-qa` |
| `production` | `pr` | `socialschool-rg-pr` |

El valor tiene que ser **el mismo con el que se aplicó Terraform** (`-var environment=dv`), porque es
lo que forma los nombres de los recursos a los que apuntan las pipelines. La de la API lo comprueba con
una guarda: si el *environment* se queda sin la variable, **para antes de tocar Azure** con un mensaje
que dice qué falta, en lugar de fallar más tarde con un error de `az` difícil de leer.

## Ejecutar una pipeline es un paso manual

Nada de esto se ejecuta desde el repositorio: ni `terraform apply` ni los workflows. Necesitan
credenciales contra Azure y, en el caso de Keycloak, la federación de arriba ya configurada. Lo que sí
se puede comprobar sin Azure es el formato y la validación de la infraestructura
(`terraform fmt -check -recursive`, `init -backend=false` y `validate`), como explica
`iac/README.md`.
