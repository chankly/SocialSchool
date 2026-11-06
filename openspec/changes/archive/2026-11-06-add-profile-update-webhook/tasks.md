# Tasks

> Las preguntas abiertas del diseño (materializar la cuenta desde una actualización, guardar el correo
> anterior) no cambian lo que hay que construir aquí.

## 1. Dominio

- [x] 1.1 Añadir `ChangeEmail` a `User`, con la misma validación que el alta (no vacío) y documentado. Verificación: no toca el tipo de cuenta ni el resto de campos, y `ChangePreferredLanguage` se queda como está.

## 2. Application

- [x] 2.1 Crear `SyncUserProfileCommand` (identidad externa, correo y idioma opcionales) con su handler: busca la cuenta, aplica solo lo que llega y solo persiste si algo cambia; si no hay cuenta, termina bien sin crear nada. Verificación: el handler no crea cuentas ni toca `AccountType`.
- [x] 2.2 Añadir el validador del mensaje en `Validators/Commands`: la identidad externa no vacía y con formato GUID. *Corrección respecto al enunciado*: **no** se exige que llegue alguno de los dos campos. Un evento que solo cambia el nombre es legítimo, no un mensaje mal formado; si el validador lo rechazara, `ValidationBehavior` lo convertiría en un 500. Ese caso lo resuelve el handler sin escribir nada.

## 3. Api

- [x] 3.1 Ampliar `KeycloakEvent` con el `details` del evento (`updated_email`, `updated_locale`) en un modelo propio (`KeycloakEventChange`), sin modelar el resto del cuerpo. Verificación: el segundo sample, que solo trae nombre y apellidos, deja los dos campos vacíos y el flujo lo acusa sin escribir.
- [x] 3.2 Repartir en `WebHooksController` por tipo de evento: alta, actualización y acuse. Verificación: el controlador no decide qué campos importan, solo traduce al comando y mapea el resultado.

## 4. Spec y verificación

- [x] 4.1 Añadir a la spec de `registration` el requisito de sincronización con sus escenarios (idioma, correo, cambio que no nos afecta, tipo de cuenta intacto, cuenta desconocida).
- [x] 4.2 `dotnet build` y tests del handler con repositorio simulado: cambio de idioma, cambio de correo, nada que cambiar (no persiste), mismos valores (no persiste), cuenta desconocida (no crea) y que el tipo de cuenta no cambia. *(Resultado real: los seis pasan; la suite queda en 33/34 por el fallo preexistente de audiencia.)*
