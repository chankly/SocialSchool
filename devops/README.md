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
| **Keycloak** | `.github/workflows/keycloak-deploy.yml` | Apunta la Container App de Keycloak del entorno elegido a la imagen pública de Phase Two y la arranca en modo producción. | `environment` (`develop` \| `qa` \| `production`) |

Todas se lanzan **a mano** desde GitHub: `Actions` → la pipeline → `Run workflow`.

Falta la pipeline de la API: necesita su `Dockerfile`, sí pasará por nuestro Azure Container Registry
y todavía no existe.

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

# 3. Permiso para actualizar la Container App: Contributor sobre el grupo de recursos del entorno.
#    Repite este paso para cada entorno que vayas a desplegar.
az role assignment create \
  --assignee "$APP_ID" \
  --role Contributor \
  --scope "/subscriptions/<subscription-id>/resourceGroups/socialschool-rg-develop"
```

Después, en el repositorio de GitHub (`Settings` → `Secrets and variables` → `Actions` → **Variables**),
define estas tres, que son identificadores y no credenciales:

| Variable | Valor |
|---|---|
| `AZURE_CLIENT_ID` | el `appId` de la identidad (`$APP_ID`) |
| `AZURE_TENANT_ID` | el inquilino de Azure |
| `AZURE_SUBSCRIPTION_ID` | la suscripción donde viven los entornos |

## Ejecutar una pipeline es un paso manual

Nada de esto se ejecuta desde el repositorio: ni `terraform apply` ni los workflows. Necesitan
credenciales contra Azure y, en el caso de Keycloak, la federación de arriba ya configurada. Lo que sí
se puede comprobar sin Azure es el formato y la validación de la infraestructura
(`terraform fmt -check -recursive`, `init -backend=false` y `validate`), como explica
`iac/README.md`.
