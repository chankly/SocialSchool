# Tasks

> Las decisiones abiertas del diseño (un tercer modo para desplegar una imagen ya publicada, separar los
> modos en el `concurrency`) no cambian lo que hay que construir aquí, así que no bloquean estas tareas.

## 1. Workflow

- [x] 1.1 Añadir al disparo manual el input `mode`, de tipo `choice`, con los valores `Build` y `BuildAndDeploy` **tal cual** (los `choice` distinguen mayúsculas) y **`Build` por defecto**, y renombrar el job para que se lea qué modo se ha lanzado. Verificación: sigue habiendo exactamente dos inputs y ninguno de más.
- [x] 1.2 Condicionar el despliegue: el paso que actualiza la Container App lleva `if: inputs.mode == 'BuildAndDeploy'`, mientras que construir y publicar **no** llevan condición, porque se hacen en los dos modos. Verificación: la condición está en el paso del despliegue, no en el job entero, y `docker build` / `docker push` quedan incondicionales.
- [x] 1.3 Separar el resumen en dos pasos condicionales: en `BuildAndDeploy` se lee de vuelta la imagen que ha quedado en la aplicación y en `Build` se informa de la imagen publicada y de que la aplicación no se ha tocado. Verificación: cada resumen lleva su `if` y ninguno puede imprimir que se ha desplegado cuando el modo es `Build`.
- [x] 1.4 Documentar en la cabecera del workflow los dos modos, que el valor por defecto no despliega y que el `AcrPush` hace falta en los dos. Verificación: la cabecera explica qué hace cada valor sin necesidad de leer los pasos.

## 2. Documentación

- [x] 2.1 Actualizar `devops/README.md`: el parámetro nuevo en la columna de parámetros de la tabla de pipelines y los dos modos en la sección de la API. Verificación: los valores escritos coinciden con los del workflow, incluido el valor por defecto.
- [x] 2.2 Anotar en `devops/iac/README.md` que publicar una imagen a mano ya no hace falta ni para eso, porque la pipeline puede publicar sin desplegar. Verificación: la frase no deja la impresión de que haya que publicar a mano.

## 3. Verificación

- [x] 3.1 Comprobar la estructura del workflow con un script temporal sin dependencias (no hay analizador de YAML), con las dos aserciones que importan: que el despliegue está condicionado a `BuildAndDeploy` y que construir y publicar no lo está. **No** se afirma que la sintaxis sea válida, ni que la condición se evalúe como se espera: eso solo se ve ejecutando la pipeline en GitHub.
- [x] 3.2 Revisar el diff: se limita al workflow y a los dos README, sin tocar `src/` ni Terraform, y el diff del workflow es pequeño (una adición, no una reescritura).
