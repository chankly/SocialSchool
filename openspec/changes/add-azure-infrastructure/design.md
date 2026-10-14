# Design: Infraestructura de Azure con Terraform

## Context

Ver `proposal.md` — Why. Estado del repositorio que condiciona el enfoque (verificado):

- **No había nada de infraestructura**: cero ficheros `.tf`, cero `Dockerfile`, ni siquiera existe
  `.github/`.
- **La API habla SQL Server** (`Microsoft.EntityFrameworkCore.SqlServer`, `UseSqlServer`) mientras
  este diseño declara **MySQL**. La API no puede usarlo todavía: su cambio de proveedor es el cambio
  aparte `switch-to-mysql`.
- **La aplicación lee variables de entorno** (`AddEnvironmentVariables`), así que alimentarla con
  secretos y con la autoridad de Keycloak **no exige tocar código**.
- **Se adopta la estructura del proyecto de referencia** (`AzureSample/devops/iac`) como modelo: un
  solo *root* con el entorno como variable, y un submódulo por aplicación. Se descarta la estructura
  inicial de carpetas por entorno y la lista genérica de aplicaciones.
- `devops/keycloak-webhooks/compose.yaml` es un entorno de pruebas y **no** se toma como referencia.

## Goals / Non-Goals

**Goals:**

- Declarar en código la infraestructura de **cualquiera de los tres entornos** con un único juego de
  ficheros, cambiando solo `-var environment`.
- Dejar resueltos desde el primer día el **estado remoto** y los **secretos**.
- Poder verificar la estructura **sin Azure** (`fmt` y `validate`).
- Poder desplegar **por partes**: registro primero para publicar imágenes, resto después.

**Non-Goals:**

- Desplegar nada: `plan` y `apply` necesitan credenciales, el estado creado e imágenes publicadas.
- Cambiar la API (su paso a MySQL vive en otro cambio).
- Red privada, *private endpoints*, dominio propio y certificados.
- El realm, los clientes y el *audience mapper* de Keycloak, que siguen siendo manuales.
- Medios en Blob, observabilidad más allá del espacio de logs, el worker y la app web.
- Un *pipeline* de despliegue: existe en la referencia (`devops/pipelines/`) pero es otro cambio.

## Decisions

**1. Un solo *root* con el entorno como variable.** `devops/iac/main.tf` + `variables.tf`, y
`environment` como variable que gobierna los nombres de todo. *Alternativa descartada*: una carpeta
por entorno, cada una con su `backend.tf` (aísla el estado por construcción, pero duplica el código y
obliga a `cd` por entorno). Se adopta el modelo de la referencia.

**2. Estado remoto con un contenedor por entorno, y los valores por `-backend-config`.** El bloque
`backend` va **vacío**: Terraform **no admite variables** ahí y el contenedor depende del entorno. La
referencia escribe `${var.environment}` dentro del bloque, que es justo lo que Terraform no permite;
aquí se pasa en el `init`. El grupo, la cuenta y los contenedores se crean **a mano y documentados en
el README**, porque Terraform no puede crear el sitio donde guarda su propio estado.

**3. Una suscripción, un grupo de recursos por entorno.** Decisión del usuario.

**4. Un registro por entorno.** Se descarta el registro compartido que se había propuesto: con las
carpetas por entorno desaparecía el motivo, y el nombre lleva el entorno porque los nombres de ACR son
únicos a nivel global y solo admiten alfanuméricos. *Trade-off*: la misma imagen se construye o se
replica una vez por entorno, en lugar de promoverse entre ellos.

**5. Los módulos reciben el entorno y derivan sus propios nombres**, y `resource-groups` publica
`name` y `location` para que ningún otro módulo repita la región. Los nombres no llevan sufijo de
unicidad: si un nombre global está cogido, el `apply` falla y habrá que decidirlo.

**6. Los secretos llegan al contenedor como variables de entorno que referencian Key Vault**, con una
identidad gestionada compartida. *Alternativa descartada*: montarlos como fichero con el *CSI driver*
(añade driver, volumen y permisos) e inyectarlos en claro desde Terraform (el valor quedaría en la
definición del contenedor). La aplicación **no necesita cambios**.

**7. Key Vault del entorno con RBAC habilitado.** `rbac_authorization_enabled = true` es
imprescindible: sin él el vault se queda en el modelo de directivas de acceso y las asignaciones de
rol no sirven de nada (las aplicaciones recibirían 403). **Corrección aplicada durante la
implementación**: la primera versión usaba `enable_rbac_authorization` (deprecado) y, más grave, no
traía ningún mecanismo para que la identidad que despliega pudiera **escribir** los secretos: con
RBAC, ser propietario de la suscripción no da acceso al plano de datos. Se añade una segunda
asignación, `Key Vault Secrets Officer`, a quien despliega. Los roles tardan en propagarse, así que el
primer `apply` puede fallar al crear los secretos y se repite.

**8. MySQL Flexible Server por entorno con dos bases (`api`, `keycloak`)**, charset y colación
explícitos. La colación es insensible a mayúsculas, lo que afecta a la unicidad del nick del dominio;
queda escrito en el módulo y hay que confirmarlo en `switch-to-mysql`.

**9. Un submódulo por aplicación** (`container-apps/api` y `container-apps/keycloak`), y un agregador
que los invoca. *Alternativa descartada*: un módulo genérico que reciba una lista de aplicaciones
(menos ficheros, pero cada aplicación acaba teniendo ramas condicionales). Es el modelo de la
referencia y el que permite desplegar o mirar una aplicación aislada con `-target`.

**10. Keycloak detrás del proxy de Container Apps.** `KC_PROXY_HEADERS=xforwarded`,
`KC_HTTP_ENABLED=true` y `KC_HOSTNAME` con su URL HTTPS. El hostname se **compone** con el nombre de
la aplicación y el dominio por defecto del entorno, y no se lee del recurso: leerlo sería una
dependencia circular, porque el atributo solo existe después de crear la aplicación.

**11. Firewall de MySQL permitiendo servicios de Azure.** Un entorno de Container Apps de consumo no
tiene direcciones de salida fijas, así que no se puede restringir por IP. Producción querrá red
privada.

**12. Mientras no existan las imágenes reales, las aplicaciones usan la imagen de ejemplo de
Microsoft** (`containerapps-helloworld`), que respeta `ASPNETCORE_URLS=http://*:8080` y por eso
encaja con el ingress en el 8080. La pipeline la sustituirá, y el recurso la **ignora**
(`ignore_changes`) para que Terraform no la revierta en el siguiente `apply`. Consecuencia: un
despliegue correcto no significa que la aplicación real esté corriendo.

**13. `command` y `args` quedan vacíos hasta que entre la imagen real de Keycloak.** Forzar
`kc.sh start` sobre la imagen de ejemplo daría un contenedor en bucle, porque ese binario no existe.
El submódulo expone ambas variables para que, con la imagen real, se fijen a
`["/opt/keycloak/bin/kc.sh"]` y `["start"]`, ya que el valor por defecto de la imagen es el **modo
desarrollo**. La intención del diseño original (modo producción) sigue en pie, pero **no es
verificable** en esta fase.

## Risks / Trade-offs

- **Nombres únicos a nivel global sin sufijo** (registro, Key Vault, MySQL) → el primer `apply` los
  revela; si chocan, hay que decidir un sufijo y volver a planificar.
- **La propagación de RBAC puede hacer fallar el primer `apply`** al crear los secretos → se repite el
  `apply` unos minutos después. Está documentado en el README.
- **Un despliegue verde no significa que la aplicación real funcione**: se despliega la imagen de
  ejemplo. Los avisos del README lo dejan claro.
- **El `iss` de Keycloak depende de su FQDN**: recrear el entorno invalida los tokens emitidos y obliga
  a reconfigurar el realm. El paso definitivo sería un dominio propio (fuera de alcance).
- **Key Vault retiene el nombre tras borrarlo** → un `destroy` seguido de `apply` puede fallar; hay que
  purgar o esperar la retención.
- **`socialschool-keycloak-production` son 32 caracteres**, justo el límite de Container Apps → si el
  prefijo creciera, esa aplicación tendría que acortar el nombre.
- **Keycloak usa el administrador del servidor MySQL**, no un usuario por base → mejora pendiente.
- **`plan` y `apply` no son verificables desde el repositorio**, y el estado remoto tampoco se puede
  crear desde aquí → el cambio se cierra con `fmt` y `validate`, y el resto es paso del usuario.

## Migration Plan

1. Crear a mano el grupo, la cuenta y los contenedores del estado (README).
2. `terraform init` con los cuatro `-backend-config`.
3. `terraform apply -var environment=develop -target=module.container_registry` y publicar las
   imágenes en el registro.
4. `terraform apply -var environment=develop` para el resto.
5. **Reversión**: `terraform destroy -var environment=develop`. El almacenamiento del estado **no** se
   destruye. El Key Vault puede requerir `purge` antes de recrearlo con el mismo nombre.

## Open Questions

- Automatizar el realm, los clientes y el *audience mapper* de Keycloak con su *provider* de Terraform.
- Usuarios de MySQL por base en lugar del administrador del servidor.
- Red privada y *private endpoints* entre Container Apps y MySQL o Key Vault.
- Publicar las imágenes con un *pipeline* (la referencia usa Azure DevOps con `-target` por tipo de
  recurso) y sustituir las imágenes de ejemplo.
- Fijar `command` y `args` de Keycloak cuando entre la imagen real, para salir del modo desarrollo.
- Almacenamiento de medios en Blob (`project.md` §11), observabilidad más allá de los logs, y paridad
  del entorno local con MySQL.
