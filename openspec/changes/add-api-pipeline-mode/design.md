# Design: Dos modos en la pipeline de la API

## Context

Ver `proposal.md` — Why. Estado verificado que condiciona el enfoque:

- El workflow de la API (`.github/workflows/api-deploy.yml`, del cambio `add-api-pipeline`, archivado el
  **2026-10-16**) tiene **un solo parámetro** (`environment`), construye en el runner, publica en el
  registro del entorno con la etiqueta del sha corto y **siempre** actualiza la Container App.
- El módulo de Terraform de la aplicación **ignora la imagen** (`ignore_changes`), así que quien la
  actualiza es la pipeline. Este cambio **no toca Terraform**: el despliegue seguirá siendo la misma
  orden, solo que condicional.
- Desplegar es la operación que cambia lo que ve la gente; construir y publicar no altera nada en
  ejecución. De ahí que el valor por defecto deba ser el inocuo.
- En esta máquina **no hay analizador de YAML**, así que la sintaxis la valida GitHub al subir el
  workflow, y la evaluación de las condiciones solo se ve ejecutándolo.

## Goals / Non-Goals

**Goals:**

- Poder publicar una imagen en el registro **sin tocar** la Container App.
- Que *Run workflow* sin cambiar nada **no despliegue**.
- Que los dos modos compartan todo lo demás: construir, etiquetar con el sha corto, publicar y los
  mismos permisos.

**Non-Goals:**

- Tocar Terraform, el `Dockerfile`, `launchSettings.json` o los tests.
- Añadir un tercer modo del tipo "desplegar una imagen ya publicada": el despliegue necesita una imagen
  recién construida en la misma ejecución.
- Automatizar el disparo: sigue siendo manual.

## Decisions

**1. El parámetro se llama `mode` y sus valores son `Build` y `BuildAndDeploy`, tal cual los escribió el
usuario.** Los `choice` de GitHub **distinguen mayúsculas**, así que se copian literalmente: `Build` es
lo que la interfaz envía y lo que hay que comparar en las condiciones. La descripción del parámetro va en
español, como la del entorno. Trae **`Build`** por defecto, para que la pulsación inocente no despliegue;
*alternativa descartada*: dejar `BuildAndDeploy` por defecto, que mantendría el comportamiento anterior
pero deja el despliegue a un despiste.

**2. La condición va en los pasos, con `if:`, no en un `if` de shell.** `if: inputs.mode == 'BuildAndDeploy'`
en el paso que actualiza la aplicación hace que GitHub muestre ese paso **omitido** cuando no aplica, así
que el propio log dice qué ha pasado. Un `if` de shell dentro de un `run` funcionaría igual, pero
escondería la decisión dentro del script y los pasos aparecerían como ejecutados.

**3. Construir y publicar **no** se condicionan.** Son el tronco común: en los dos modos la imagen se
construye y se publica con la etiqueta del commit. Condicionarlos obligaría a duplicar pasos.

**4. `Build` entra igualmente en Azure.** Publicar exige `az acr login`, que pide el token federado, así
que el paso de `azure/login` y el `AcrPush` sobre el registro siguen haciendo falta en los dos modos. No
se puede "ahorrar" el login en `Build`.

**5. Dos pasos de resumen, uno por modo, en lugar de un `if` de shell dentro de un solo paso.** Cada uno
dice lo que de verdad ha pasado: `BuildAndDeploy` lee de vuelta la imagen que ha quedado en la aplicación
(`az containerapp show`), y `Build` informa de la imagen publicada y de que la aplicación sigue como
estaba. Así es imposible imprimir "imagen desplegada" en una ejecución que no ha desplegado.

**6. El job se renombra para que se lea el modo** (`API en <entorno> (<modo>)`), porque el nombre
"Desplegar la API en <entorno>" mentiría en modo `Build`. El `concurrency` **no** cambia: sigue siendo por
entorno, de modo que dos ejecuciones sobre el mismo entorno se serializan aunque una sea `Build` y la otra
`BuildAndDeploy`.

## Risks / Trade-offs

- **La condición no se puede probar aquí**: las condiciones las evalúa GitHub al ejecutar el workflow.
  Lo verificable desde el repositorio es que el `if` esté en el paso correcto y con el valor correcto,
  más la comprobación estructural del resto del fichero. Hacer *click* en los dos modos es prueba del
  usuario.
- **Cambia el comportamiento por defecto**, y es lo que se busca: quien pulse *Run workflow* esperando
  desplegar como antes tendrá que elegir `BuildAndDeploy`. Queda escrito en la cabecera del workflow y en
  `devops/README.md`.
- **En modo `Build` no se comprueba nada de la aplicación** (ni que arranque, ni qué imagen tiene): es un
  modo de publicar, no de verificar.
- **La imagen publicada en modo `Build` puede quedarse sin desplegar**: es intencionado, y el registro
  acumula etiquetas que quizá nunca se usen (la limpieza ya estaba como decisión abierta).

## Migration Plan

1. El workflow pasa a tener dos parámetros; no hay migración de datos ni de infraestructura.
2. Para desplegar como hasta ahora: elegir entorno **y** `BuildAndDeploy`.
3. Para publicar sin tocar el entorno: elegir entorno y dejar `Build`.
4. **Reversión**: cambiar el valor por defecto a `BuildAndDeploy` en el workflow.

## Open Questions

- Si algún día hace falta un tercer modo para desplegar una imagen ya publicada (por ejemplo, volver a
  una etiqueta anterior sin reconstruir).
- Si el `concurrency` debería separar los modos, para que un `Build` no espere a un `BuildAndDeploy` del
  mismo entorno.
