# Design: Dockerfile de la API y su pipeline

## Context

Ver `proposal.md` — Why. Estado verificado que condiciona el enfoque:

- La Container App de la API se despliega hoy con `helloworld` y su módulo ya ignora la imagen
  (`ignore_changes = [template[0].container[0].image]`), en `revision_mode = "Single"`, con ingress a
  `target_port = 8080` y `ASPNETCORE_URLS=http://*:8080` entre las variables de entorno.
- El registro **es uno por entorno** (`socialschoolacr<entorno>`, con `login_server` publicado por el
  módulo) y la aplicación lo lee con la **identidad gestionada**, no con credenciales.
- El repositorio **no tenía ningún `Dockerfile` ni `.dockerignore`**, y su único workflow era
  `keycloak-deploy.yml`, que marca la forma a imitar: disparo manual, un `choice` de entorno, OIDC y
  `concurrency`.
- **El tooling de contenedores de Visual Studio manda en la colocación de los ficheros**: la API ya
  referencia `Microsoft.VisualStudio.Azure.Containers.Tools.Targets` 1.19.5, que define
  `DockerDefaultDockerfile = Dockerfile` y, **para .NET Core, `DockerDefaultContext = ..`**, y que
  publica el `.dockerignore` desde `$(DockerDefaultContext)\.dockerignore`. Con el `Dockerfile` dentro
  del proyecto, el contexto por defecto es **`src/`**: justo lo que el build necesita, porque los tres
  proyectos referenciados están ahí. Se leyó en `C:\Users\hevia\.nuget\packages\...\1.19.5\build\Container.targets`.
- **No hay `global.json`, `Directory.Build.props`, `Directory.Packages.props` ni `nuget.config`**: la
  versión del SDK en la imagen la fija la **etiqueta de la imagen base**, no un fichero del repo.
- La solución es `HSoft.SocialSchool.slnx`, en la raíz, con `src/` (Domain, Application, Persistence,
  Api) y `tests/`. El proyecto de tests es de integración sobre `WebApplicationFactory` y **no toca base
  de datos**.
- **Docker 28.1.1 está disponible en esta máquina**, así que la imagen se puede construir y arrancar
  aquí; lo que no se puede probar aquí es el push ni la actualización de la Container App.
- **Los nombres de los recursos los gobierna el prefijo corto**: `devops/iac/variables.tf` declara
  `environment` como *"Entorno sobre el que se despliega todo (dv, qa o pr)"*, `main.tf` ya comenta
  `container_name = "socialschool-terraform-dv"` y el workflow de Keycloak compone los nombres con
  `vars.ENVIRONMENT_PREFIX`. Los ejemplos de `devops/iac/README.md` son los que estaban desalineados.
- **Terraform no valida esa variable** (no hay bloque `validation`), así que `-var environment=develop`
  se acepta sin quejarse y crea recursos con el nombre largo: la desalineación no da error, da nombres
  distintos de los que la pipeline busca. Como `qa` coincide en los dos esquemas, solo se notaría en
  *develop* y *production*.

## Goals / Non-Goals

**Goals:**

- Una imagen de la API **reproducible** que arranque en el puerto que Terraform espera.
- Que un developer, desde GitHub y a mano, publique esa imagen y la deje desplegada en cualquier
  entorno.
- Que el despliegue **siempre** coja la imagen recién construida, sin depender de que Container Apps
  detecte un cambio.
- Que el prefijo con el que el workflow nombra los recursos sea **el mismo** con el que los nombra
  Terraform, y que eso quede escrito una sola vez y sin ambigüedad.
- Que la colocación del `Dockerfile` sea la que usa Visual Studio, para que el siguiente microservicio
  se construya igual sin inventar nada.
- **Sin secretos guardados** en el repositorio.

**Non-Goals:**

- Ejecutar los tests en la pipeline: decisión explícita del usuario, queda como paso manual.
- Tocar variables de entorno, secretos, ingress o escala: son de Terraform.
- La pipeline de Keycloak, que ya existe y no necesita registro.
- Cambiar el esquema de nombres: se **alinean los documentos a lo que ya hace Terraform**, no al revés.
- Convertir el perfil `Docker` en el perfil de arranque por defecto: se **añade** a
  `launchSettings.json`, pero para F5 se siguen usando `http`/`https`, como pidió el usuario.

## Decisions

**1. El `Dockerfile` vive dentro del proyecto y el contexto de build es `src/`.** Se coloca en
`src/HSoft.SocialSchool.Api/Dockerfile` y se declara en el `.csproj` del proyecto
(`<None Include="Dockerfile" />`), que es como queda cuando Visual Studio agrega compatibilidad con
Docker, de modo que la pauta se pueda copiar tal cual al siguiente microservicio. El contexto `src/`
no es arbitrario: es el `DockerDefaultContext = ..` que el propio tooling de contenedores usa por
defecto para .NET Core, así que F5 con el perfil de Docker y la orden del workflow construyen **lo
mismo** (`docker build --file src/HSoft.SocialSchool.Api/Dockerfile --tag <imagen> src`). Ventajas
añadidas: el contexto lleva solo las cuatro carpetas de proyecto —nada de `devops/`, `tests/` ni
`openspec/`— y **un único `.dockerignore` en `src/` sirve para todos los `Dockerfile` que se construyan
desde ahí**. *Alternativa descartada*: el `Dockerfile` en la raíz con el contexto en el repositorio
entero, que fue la primera versión de este cambio; se descarta a petición del usuario, que quiere la
colocación de Visual Studio precisamente para que los microservicios siguientes la sigan.

**2. Multi-stage, publicando solo el proyecto de la API.** `sdk:10.0` para compilar y `aspnet:10.0`
para ejecutar. Se restauran las referencias copiando primero los cuatro `.csproj`, de modo que la capa
de restore se cachea mientras no cambien los paquetes; después se copia el contexto entero y se publica
en `Release`. Todas las rutas del `Dockerfile` son **relativas a `src/`**. El proyecto de tests no entra
ni en el contexto. Como no hay `global.json`, **la etiqueta de la imagen base es la única que fija el
SDK**: subir de versión es cambiar dos líneas.

**3. Usuario no root y puerto 8080 en el propio Dockerfile.** La imagen `aspnet` trae un usuario `app`
y la variable `APP_UID`, y el contenedor corre con él. Además se declara
`ENV ASPNETCORE_URLS=http://+:8080` como valor por defecto de la imagen: es **el mismo** que inyecta
Terraform, así que la imagen funciona igual en Azure y con un `docker run` suelto, sin que la
aplicación se quede escuchando en el 5000 por defecto.

**4. La imagen se construye en el runner de GitHub.** `docker build` + `docker push` tras `az acr
login`. *Alternativa descartada*: `az acr build` (ACR Tasks), que no necesita Docker en el runner y
construye dentro de Azure; se descarta para mantener **una sola forma de construir** —la que ya
documenta `devops/iac/README.md` y la que cualquiera puede reproducir en su máquina— y porque los logs
del build quedan en el workflow.

**5. Etiqueta por commit, sin etiqueta móvil.** `socialschool-api:<sha-corto>`. Es el punto donde este
diseño se separa del README, que hablaba de `:develop`: con una etiqueta fija, la cadena de imagen
puede ser idéntica entre dos ejecuciones y Container Apps **puede no crear revisión nueva**, con lo que
el despliegue parecería correcto y seguiría corriendo la imagen anterior. Con el sha corto el cambio es
explícito y la revisión está garantizada. El entorno ya queda claro en el nombre de la imagen porque
**cada entorno tiene su propio registro**, así que repetirlo en la etiqueta sería redundante.
*Trade-off aceptado*: las etiquetas son inmutables y el registro acumula una por commit.

**6. La pipeline hace las tres cosas, con `az containerapp update --image`.** Construir, publicar y
apuntar la aplicación. No se usa un `--yaml` completo, que declararía también variables de entorno,
recursos y escala y **pelearía con Terraform**, que es quien los gobierna.

**7. Sin tests en la pipeline.** Decisión del usuario. Se deja escrito el precio: **nada comprueba el
código antes de publicar la imagen**, de modo que un fallo de comportamiento no lo detecta el
despliegue. Un fallo de compilación sí, porque el build ocurre antes del push.

**8. `ENVIRONMENT_PREFIX` es una variable de entorno de GitHub, con el prefijo corto del entorno.**
El workflow compone los nombres igual que el de Keycloak (`socialschool-rg-${{ vars.ENVIRONMENT_PREFIX }}`),
y cada *environment* de GitHub define su valor: **`dv`** en *develop*, **`qa`** en *qa* y **`pr`** en
*production*. Así el nombre del *environment* de GitHub queda desacoplado del sufijo con el que Azure
nombra los recursos. *Alternativa descartada*: que el workflow dedujera el prefijo del entorno elegido
con una tabla propia, que no exigiría configurar nada pero cablearía la traducción en el YAML y
divergiría de la pipeline de Keycloak. *Precio de la opción elegida*: son tres altas manuales en GitHub
y, si un *environment* se queda sin la variable, los nombres salen como `socialschool-rg-` y el fallo
aparece como un error de Azure difícil de leer; por eso el workflow lleva un paso de guarda que
comprueba que no está vacía y **para con un mensaje que dice qué falta y dónde se define, antes de
tocar Azure**. Documentarla con sus tres valores es parte de este cambio.

**9. `AcrPush` es un permiso nuevo que hay que dar de alta.** Publicar en el registro es plano de datos:
`Contributor` sobre el grupo de recursos permite actualizar la Container App pero **no** empujar la
imagen. La de Keycloak no lo necesitaba porque su imagen es pública. Queda documentado como una
asignación de rol más.

**10. Los documentos se alinean a `dv`/`qa`/`pr`, que es lo que ya hacen `variables.tf` y la pipeline de
Keycloak.** Se corrigen los ejemplos de `devops/iac/README.md` (`-var environment=dv`,
`socialschool-rg-dv`, `socialschoolacrdv`, `socialschool-terraform-dv` y la tabla de convención de
nombres) y el ámbito del rol en `devops/README.md`. Se elige alinear los documentos y no el código
porque el prefijo corto es lo que Terraform ya declara y lo que las dos pipelines ya asumen: cambiar el
esquema obligaría a tocar `variables.tf`, el workflow de Keycloak y los recursos ya creados. Efecto
colateral que también se corrige: el aviso de que `socialschool-keycloak-production` son "32 caracteres,
justo el límite" de Container Apps deja de aplicar, porque con `pr` el nombre se queda en 22.

**11. El perfil `Docker` de `launchSettings.json` completa la colocación de Visual Studio.** Se añade
junto a `http` y `https`, sin tocar ninguno de los dos: `commandName: Docker`, `launchUrl` a `/swagger`,
`ASPNETCORE_URLS=http://+:8080` (el mismo puerto que el `EXPOSE` del Dockerfile y que el ingress de
Terraform), `publishAllPorts: true` y `useSSL: false`, porque la API sirve HTTP. Se deja **sin**
`ASPNETCORE_ENVIRONMENT`, para que el contenedor se comporte como el desplegado (*Production*); con
`Development` aparecerían los extras de desarrollo —el `debugInfo` del `ProblemDetails` y sin
redirección a HTTPS— y es una línea cambiarlo si se prefiere depurar así. *Trade-off*: el perfil no se
puede ejercitar desde aquí, porque quien lo ejecuta es Visual Studio.

## Risks / Trade-offs

- **El push y la actualización de la aplicación no se pueden probar desde aquí**: necesitan Azure y la
  federación OIDC → son paso del usuario. Lo verificable en esta máquina es construir la imagen,
  arrancarla y responder por HTTP, más la comprobación estructural del workflow.
- **Los nombres reales de los recursos no se pueden comprobar desde aquí**, pero el usuario confirmó el
  **2026-10-16** que aplicó Terraform con `-var environment=dv`, así que el grupo de recursos de
  *develop* es `socialschool-rg-dv` y el prefijo del workflow coincide sin ajustes. Queda como aviso
  para *qa* y *production*, que todavía no están aplicados: hay que desplegarlos con
  `-var environment=qa` y `-var environment=pr` para que sus nombres sigan coincidiendo con el prefijo.
- **El perfil `Docker` de `launchSettings.json` no se puede probar aquí**: lo ejecuta Visual Studio.
  Lo único verificable desde el repositorio es que el JSON siga siendo válido y que no altere los otros
  perfiles.
- **El registro acumula una etiqueta por commit** y no hay política de limpieza → decisión abierta.
- **Sin tests, la pipeline no valida comportamiento**, solo que el código compila.
- **Divergencia entre repositorio y nube**: el `image` de Terraform dice `helloworld` mientras la
  realidad es otra. Recrear la aplicación **la devuelve a `helloworld`**. Es el precio aceptado de "la
  pipeline manda sobre la imagen", el mismo que ya se aceptó para Keycloak.
- **La verificación de Terraform es offline** (`fmt` y `validate`): el `plan`/`apply` es del usuario.
- **Si el nombre del repositorio o de la aplicación cambiara**, hay tres sitios que lo dicen (el
  workflow, la cabecera del workflow y el README) → por eso la convención queda escrita una vez en cada
  sitio y el `Dockerfile` y el contexto son variables del job.
- **El contexto de build es `src/`, no el repositorio**: si algún día un `Dockerfile` necesita ficheros
  de fuera de `src/`, habrá que cambiar el contexto del job (y el `.dockerignore` con él).

## Migration Plan

1. Tener el entorno aplicado con Terraform (el registro y la Container App existen): *develop* ya lo
   está, con `-var environment=dv`, así que el grupo de recursos es `socialschool-rg-dv`, que es el
   nombre que buscará la pipeline. *qa* y *production* tendrán que aplicarse con `qa` y `pr`.
2. Configurar la federación OIDC, añadir el rol `AcrPush` y definir `ENVIRONMENT_PREFIX` en los tres
   *environments* de GitHub (pasos manuales, documentados en `devops/README.md`).
3. Ejecutar el workflow con `environment=develop` y comprobar que la revisión nueva arranca con la
   imagen de la API y responde.
4. **Reversión**: apuntar la aplicación otra vez a
   `mcr.microsoft.com/azuredocs/containerapps-helloworld:latest`, o recrearla con Terraform.

## Open Questions

- Poner la misma guarda de `ENVIRONMENT_PREFIX` en el workflow de Keycloak, que hoy fallaría igual de
  silenciosamente.
- Política de limpieza de etiquetas en el registro.
- Si algún día la pipeline debe ejecutar los tests (y, con ellos, levantar un MySQL de servicio).
- Si el `image` por defecto de la API en Terraform debe dejar de ser `helloworld` ahora que existe una
  imagen propia que construir.
- Si el perfil `Docker` debería llevar `ASPNETCORE_ENVIRONMENT=Development` para depurar con los extras
  de desarrollo dentro del contenedor.

## Verificación (2026-10-16)

Lo que se ha comprobado aquí, con el resultado real:

- **La imagen se construye**: `docker build --file src/HSoft.SocialSchool.Api/Dockerfile --tag
  socialschool-api:local src`, con los `bin/` y `obj/` de un build local presentes, así que se confirma
  que el `.dockerignore` de `src/` impide que entren restos de Windows en el contexto. Resultado:
  **384 MB**.
- **El `.csproj` admite el ítem del Dockerfile**: `dotnet build` de la solución pasa con
  `<None Include="Dockerfile" />` y **sin** aviso de ítem duplicado (`NETSDK1022`), que era el riesgo
  real de escribirlo a mano con el glob implícito del SDK. Los 34 avisos que salen son todos
  preexistentes (CS1591 de documentación XML y nulabilidad).
- **El contenedor arranca y sirve**: `docker run` publicando el 8080, con la aplicación en
  `Environment: Production`, escuchando en `http://+:8080` (el `ASPNETCORE_URLS` del Dockerfile, que
  además evita el aviso de puerto sin determinar). Respuestas reales:
  - `GET /swagger/index.html` → **200**
  - `GET /swagger/v1/swagger.json` → **200**
  - `GET /swagger` → **301** (redirección de Swagger UI a `index.html`)
  - `GET /` → **401** (no hay endpoint y actúa la política de denegación por defecto)
- **Aviso que costó un desvío**: `HEAD /swagger/index.html` (por ejemplo `curl -I`) devuelve **401**,
  porque el middleware de Swagger UI no atiende `HEAD` y la petición llega hasta el middleware de
  autorización. No es que la imagen esté rota: con `GET` responde 200. Para probar el contenedor hay
  que usar `GET`.
- **`launchSettings.json` sigue siendo válido** tras añadir el perfil `Docker`, y el diff es una
  adición limpia de **10 líneas** (`git diff --numstat`): los perfiles `http` y `https` quedan intactos
  y no hay cambios de formato. Dos detalles comprobados: el fichero **empieza por BOM** (`EF BB BF`),
  que ya estaba en `HEAD` —lo escribe Visual Studio— y que hace que `python -m json.tool` **falle**
  aunque el JSON sea correcto (hay que leerlo como `utf-8-sig` para parsearlo); y los finales de línea
  del árbol de trabajo son CRLF, que es lo normal con `core.autocrlf = true`, así que el diff no
  arrastra ese ruido. El perfil en sí solo lo ejercita Visual Studio: desde aquí **no** se puede
  afirmar que F5 levante el contenedor, eso es prueba del usuario.
- **El workflow pasa la comprobación estructural**: 50 aserciones sobre estructura, contenido y
  ausencias (ningún entorno cableado, ningún secreto, ningún nombre de recurso compuesto con otra cosa
  que `vars.ENVIRONMENT_PREFIX`, el `Dockerfile` y el contexto del proyecto, nada de tests, la etiqueta
  derivada del commit). **No** valida la sintaxis YAML: eso lo hace GitHub al subir el workflow, porque
  aquí no hay analizador.
- **Lo que no se puede comprobar desde aquí**: el `az acr login`, el push al registro, el
  `az containerapp update` y la federación OIDC, que necesitan Azure. Son pasos del usuario.
