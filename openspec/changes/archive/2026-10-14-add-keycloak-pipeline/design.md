# Design: Pipeline de Keycloak

## Context

Ver `proposal.md` — Why. Estado que condiciona el enfoque:

- La Container App de Keycloak se despliega hoy con `helloworld` y su módulo **ya ignora la imagen**
  (`ignore_changes = [template[0].container[0].image]`), porque el 2026-10-14 se decidió que **la
  pipeline manda sobre la imagen**.
- El repositorio **no tiene ningún workflow**: no existe `.github/`. Este es el primero.
- Convención de nombres, tomada de `devops/iac/README.md`: grupo de recursos `socialschool-rg-<entorno>`
  y aplicación `socialschool-keycloak-<entorno>`.
- La pipeline por servicio del proyecto de referencia
  (`AzureSample/src/FirstApi/devops/pipeline.yml`) es **Azure DevOps** y tiene **dos** parámetros
  (`environment` + `Build`/`BuildAndDeploy`), además de construir la imagen, empujarla a su registro y
  desplegar con un `--yaml` completo. Se copia la **forma** (disparo manual, parámetro, nombres
  derivados del entorno) pero no la sintaxis, ni el segundo parámetro, ni el YAML.

## Goals / Non-Goals

**Goals:**

- Que un developer, desde GitHub y a mano, apunte Keycloak a su imagen real en cualquier entorno.
- Que el workflow sea la **única fuente de verdad** de la imagen y de cómo arranca el contenedor.
- **Sin secretos guardados** en el repositorio.

**Non-Goals:**

- Construir o publicar imágenes propias: no hay Dockerfile y, para esta pipeline, no hace falta.
- Publicar nada en nuestro ACR.
- Tocar variables de entorno, secretos, ingress o escala: son de Terraform.
- La pipeline de la API, que llegará con su Dockerfile y sí pasará por el registro.

## Decisions

**1. Un solo parámetro, el entorno.** Con la imagen tomada de `quay.io` no hay nada que compilar ni
que publicar, así que el segundo parámetro del encargo inicial —crear la imagen o crear y desplegar—
desaparece. Decisión del usuario, que además contradijo su propio encargo anterior: se queda con la
versión más simple.

**2. La imagen no entra en nuestro registro.** `quay.io` es público y las Container Apps la descargan
sin credenciales. Consecuencia: la pipeline **no** necesita `AcrPush` y **no** hay que reconfigurar las
credenciales del registro en la Container App. Esto la separa de la referencia, que lee el usuario y la
contraseña del ACR porque **su** registro tiene el administrador habilitado; el nuestro lo tiene
deshabilitado a propósito.

**3. La pipeline fija imagen, comando y argumentos.** La imagen de Keycloak arranca en **modo
desarrollo** por defecto; sin `kc.sh start` no hay modo producción. `az containerapp update` acepta
`--image`, `--command` y `--args` en la misma llamada, así que las tres cosas van juntas y en un solo
paso.

**4. Terraform ignora las tres cosas.** `command` y `args` se añaden al `ignore_changes` del submódulo
(la imagen ya estaba). **Sin esto el cambio sería un fallo silencioso**: el siguiente `apply` borraría
el comando, Keycloak volvería al modo desarrollo y nadie lo notaría hasta que algo fallara.
*Alternativa descartada*: poner el comando en la configuración de Terraform, que es imposible mientras
la imagen desplegada sea `helloworld`, porque ese binario no existe y el contenedor entraría en bucle.

**5. `az containerapp update --image`, no el `--yaml` completo de la referencia.** Un YAML declararía
también variables de entorno, recursos y escala, y eso **pelearía con Terraform**, que es quien los
gobierna.

**6. OIDC federado, sin secretos.** El workflow usa `azure/login` con *federated credentials*; la
identidad necesita permiso para actualizar la Container App (Contributor sobre ella o sobre el grupo de
recursos). *Alternativa descartada*: service principal con su secreto en los secretos del repositorio
(se monta antes, a cambio de un secreto de larga vida que hay que rotar).

**7. El workflow compone los nombres a partir del entorno**, igual que Terraform
(`socialschool-rg-<entorno>`, `socialschool-keycloak-<entorno>`), para no duplicar literales que se
desincronicen; la convención queda documentada en el propio workflow.

**8. La versión de Keycloak está escrita en el workflow.** El usuario lo pidió así: deja una traza
explícita de qué versión de Phase Two se despliega. Subir de versión es un cambio de una línea, y es
también el sitio donde añadir un segundo parámetro el día que haga falta.

## Risks / Trade-offs

- **El workflow no se puede probar desde el repositorio**: la ejecución necesita la federación OIDC
  configurada y credenciales contra Azure → es paso del usuario; aquí se revisa solo la sintaxis.
- **Cada ejecución crea una revisión nueva** y las revisiones se acumulan → queda como decisión abierta
  fijar la retención de revisiones.
- **Divergencia entre repositorio y nube**: el `image` de Terraform dice `helloworld` mientras la
  realidad es otra. Es el precio aceptado de "la pipeline manda".
- **Recrear la aplicación la devuelve a `helloworld`**, porque la imagen de la configuración sigue
  siendo la de ejemplo → conviene saberlo antes de recrearla.
- **La verificación de Terraform es offline** (`fmt` y `validate`): el `plan`/`apply` es del usuario.

## Migration Plan

1. Configurar la federación OIDC (paso manual, documentado en `devops/README.md`).
2. Ejecutar el workflow con `environment=develop` y comprobar que la revisión nueva arranca con la
   imagen de Keycloak.
3. **Reversión**: ejecutar el workflow con la imagen anterior, o recrear la aplicación con Terraform,
   que la devuelve a `helloworld`.

## Open Questions

- Retención de revisiones de la Container App, hoy sin límite explícito.
- Si la versión de Keycloak debe pasar a ser un segundo parámetro cuando se actualice Phase Two.
- La pipeline equivalente para la API, que sí necesitará Dockerfile y registro.
- Si algún día se quiere una imagen propia de Keycloak (temas, SPI, importación del realm), y con ella
  volver al registro.
