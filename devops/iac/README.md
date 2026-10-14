# Infraestructura de Azure con Terraform

Declara en código la infraestructura de SocialSchool en Azure. **Un solo *root*** y **el entorno como
variable**: se despliega todo para un entorno pasando `-var environment=<entorno>`.

```
devops/iac/
  main.tf         root: backend, provider y las llamadas a los seis módulos
  variables.tf    la variable environment y los valores sensibles
  modules/
    resource-groups/            crea "socialschool-rg-<entorno>" y publica name + location
    container-registry/         un registro por entorno
    container-environment-apps/ log workspace + entorno de Container Apps + identidad gestionada
    mysql/                      servidor MySQL con las bases api y keycloak
    key-vault/                  secretos del entorno y permisos
    container-apps/
      main.tf                   un submódulo por aplicación
      api/                      la API
      keycloak/                 Keycloak
```

## Requisitos

- **Terraform `~> 1.16`** disponible como `terraform`.
- **Azure CLI** autenticado (`az login`) sobre la suscripción de destino.
- **Permiso para crear asignaciones de rol** (Owner o User Access Administrator), porque la identidad
  de las aplicaciones recibe `AcrPull` sobre el registro y `Key Vault Secrets User` sobre el vault.
- La identidad que despliega necesita además **`Key Vault Secrets Officer`**: con RBAC activado, ser
  propietario de la suscripción **no** da acceso al plano de datos. El propio Terraform crea esa
  asignación, pero RBAC tarda unos minutos en propagarse, así que **si el primer `apply` falla al
  crear los secretos, se vuelve a ejecutar**.

## 1. Crear el almacenamiento del estado (una sola vez por suscripción)

Terraform no puede crear el sitio donde guarda su propio estado, así que este paso es manual. Hay un
contenedor **por entorno**.

```bash
az group create --name socialschool-rg-tfstate --location westeurope

az storage account create \
  --name socialschooltfstate \
  --resource-group socialschool-rg-tfstate \
  --location westeurope \
  --sku Standard_LRS \
  --min-tls-version TLS1_2 \
  --allow-blob-public-access false

for entorno in develop qa production; do
  az storage container create \
    --name "socialschool-deploy-${entorno}" \
    --account-name socialschooltfstate \
    --auth-mode login
done
```

## 2. Inicializar

El bloque `backend` está **vacío a propósito**: Terraform no admite variables ahí, y el contenedor
depende del entorno, así que los valores entran por `-backend-config`.

```bash
cd devops/iac
terraform init -input=false \
  -backend-config="resource_group_name=socialschool-rg-tfstate" \
  -backend-config="storage_account_name=socialschooltfstate" \
  -backend-config="container_name=socialschool-deploy-develop" \
  -backend-config="key=terraform.tfstate"
```

Para trabajar **sin Azure y sin estado** (lo que se usa para revisar sintaxis):

```bash
terraform init -backend=false
terraform validate
```

## 3. Aplicar

Las contraseñas **no están en ningún fichero**: entran por variables de entorno.

```bash
# cmd.exe: set TF_VAR_mysql_administrator_password=...
export TF_VAR_mysql_administrator_password="<contraseña>"
export TF_VAR_keycloak_admin_password="<contraseña>"

terraform apply -var environment=develop
```

## 4. Aplicar primero el registro (para publicar imágenes antes que el resto)

```bash
terraform apply -var environment=develop -target=module.container_registry
az acr login --name socialschoolacrdevelop
docker build -t socialschoolacrdevelop.azurecr.io/socialschool-api:develop .
docker push socialschoolacrdevelop.azurecr.io/socialschool-api:develop
terraform apply -var environment=develop
```

Los repositorios del registro **no** son un recurso de Azure: aparecen al empujar la primera imagen.

## 5. Convención de nombres

| Recurso | Patrón | Ejemplo (`develop`) |
|---|---|---|
| Grupo de recursos del estado | `socialschool-rg-tfstate` | igual |
| Storage account del estado | `socialschooltfstate` | igual |
| Contenedor del estado | `socialschool-deploy-<entorno>` | `socialschool-deploy-develop` |
| Grupo de recursos del entorno | `socialschool-rg-<entorno>` | `socialschool-rg-develop` |
| Registro | `socialschoolacr<entorno>` | `socialschoolacrdevelop` |
| Key Vault | `socialschool-kv-<entorno>` | `socialschool-kv-develop` |
| Servidor MySQL | `socialschool-mysql-<entorno>` | `socialschool-mysql-develop` |
| Espacio de logs | `socialschool-log-<entorno>` | `socialschool-log-develop` |
| Entorno de Container Apps | `socialschool-cae-<entorno>` | `socialschool-cae-develop` |
| Identidad gestionada | `socialschool-id-<entorno>` | `socialschool-id-develop` |
| Aplicaciones | `socialschool-<app>-<entorno>` | `socialschool-api-develop` |

No hay sufijo de unicidad: los nombres que son únicos a nivel global (registro, Key Vault, MySQL) se
resuelven solo con el entorno. Si uno está cogido, el `apply` falla y hay que decidir un sufijo.
Aviso de longitud: `socialschool-keycloak-production` son **32 caracteres, justo el límite** de
Container Apps; si el prefijo creciera, esa aplicación tendría que acortar el nombre.

## 6. Estado actual de las aplicaciones

- **Las dos aplicaciones se despliegan con la imagen de ejemplo de Microsoft**
  (`mcr.microsoft.com/azuredocs/containerapps-helloworld:latest`), que respeta
  `ASPNETCORE_URLS=http://*:8080`. La pipeline de despliegue sustituye la imagen y Terraform **no la
  revierte**, porque el recurso la ignora (`ignore_changes`).
- **Keycloak, mientras use la imagen de ejemplo, no lleva `command` ni `args`**: forzar
  `kc.sh start` sobre una imagen que no tiene ese binario daría un contenedor en bucle. El submódulo
  expone esas dos variables para el día en que entre la imagen real, que habrá que fijar a
  `["/opt/keycloak/bin/kc.sh"]` y `["start"]` respectivamente, porque el valor por defecto de la
  imagen es el **modo desarrollo**.
- Las variables `KC_*` de Keycloak ya están puestas (base de datos, proxy, hostname). Son inofensivas
  en la imagen de ejemplo.
- **La API todavía no puede hablar con la base de datos**: su capa de Persistencia usa SQL Server. Eso
  llega con el cambio pendiente `switch-to-mysql`.

## 7. Avisos que conviene no redescubrir

- **El FQDN de Keycloak es inmutable en la práctica**: forma parte del emisor (`iss`) de los tokens.
  Recrear el entorno de Container Apps cambia el dominio por defecto e **invalida los tokens ya
  emitidos**.
- **Key Vault retiene el nombre tras borrarlo** (borrado suave): un `destroy` seguido de `apply` puede
  fallar por el nombre reservado; hay que purgar el vault o esperar los 7 días de retención.
- **El firewall de MySQL permite los servicios de Azure**, porque un entorno de Container Apps de
  consumo no tiene direcciones de salida fijas. Producción querrá red privada.
- **La colación de las bases es insensible a mayúsculas** (`utf8mb4_0900_ai_ci`): `Peter` y `peter`
  colisionan. Afecta a la unicidad del nick y hay que confirmarlo en `switch-to-mysql`.
- **Keycloak usa el administrador del servidor MySQL**, no un usuario por base. Funciona, pero crear
  usuarios por base sería lo correcto.
