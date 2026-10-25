# SocialSchool — Definición del producto

> **Estado:** fuente de verdad de la definición del producto.
> **Origen:** entrevista de requisitos con el responsable del producto (septiembre de 2026).
> **Ámbito:** este documento describe **el backend**, que es el núcleo de la solución. La aplicación
> móvil y la aplicación web son clientes consumidores y quedan fuera de este documento.
>
> **Advertencia sobre la plantilla:** el repositorio nace de una plantilla arquitectónica cuyo
> dominio de ejemplo es un *Referee Evaluator* (partidos, árbitros, equipos). **Ese dominio NO
> representa el negocio de SocialSchool** y debe descartarse. Ninguna entidad, caso de uso ni regla
> del ejemplo (`Match`, `MatchReferee`, `TeamInfo`, `MatchStatus`, `RefereeRole`…) es un requisito.
> Lo único que se conserva de la plantilla son las **decisiones arquitectónicas** (Clean Architecture,
> Mediator, Result pattern, validación en dos capas, eventos de dominio…), que no son objeto de este
> documento.

---

## 1. Visión del producto

SocialSchool es una solución **híbrida entre red social y aplicación para escuelas de deporte**.
Pone en contacto a **deportistas** y **escuelas de deporte** en torno a tres ideas:

1. Que la **trayectoria** de un deportista sea visible y contrastable cuando llega a una escuela nueva.
2. Que su **nivel técnico** pueda comprobarse a partir de las puntuaciones que escuelas y otros
   deportistas dan a las maniobras que domina.
3. Que deportistas y escuelas mantengan el **contacto** y puedan **ver el progreso** unos de otros.

La solución se compone de:

| Componente | Papel |
|---|---|
| **Backend (este proyecto)** | Núcleo de la solución. Ofrece toda la funcionalidad. |
| Aplicación móvil | Cliente consumidor. |
| Aplicación web | Cliente consumidor. |

---

## 2. Problema y meta

**Problema que resuelve.** Un deportista que cambia de escuela llega como un desconocido: no hay
forma de demostrar dónde ha entrenado, con quién ni qué nivel técnico tiene. Y no existe un lugar
común donde deportistas y escuelas mantengan el contacto y vean la evolución del deportista.

**Meta.** Tener una red social en la que:

- los deportistas sigan a sus amigos deportistas y a las escuelas de deporte,
- una escuela, al recibir a un deportista nuevo, pueda ver **por qué otras escuelas ha pasado** y
  **su nivel técnico** según las puntuaciones de escuelas y de otros deportistas,
- los deportistas estén en contacto entre sí y sigan sus progresos.

---

## 3. Usuarios y tipos de cuenta

Existen **dos tipos de cuenta**, y **cada cuenta tiene exactamente un tipo**:

| Tipo | Descripción |
|---|---|
| `DEPORTISTA` | Usuario mayoritario. Practica uno o varios deportes, declara maniobras y recibe puntuaciones. |
| `ESCUELA` | Escuela de deporte. Su perfil está asociado a **un único deporte**. Valida deportistas y los puntúa. |

Reglas de cuenta:

- El tipo de cuenta **se elige al crear la cuenta y es inmutable**: una cuenta de deportista nunca
  puede convertirse en escuela, ni al contrario.
- **Nick**: obligatorio en ambas cuentas, **único**, **inmutable** y formado **solo por letras y
  números**.
- Para registrarse hay que ser **mayor de edad**.

**El "técnico" no es un usuario del sistema.** Es un perfil técnico con acceso a la base de datos y
al conjunto del sistema. Hoy realiza manualmente tres tareas que ningún rol de la aplicación cubre:

1. **Validar** las cuentas de escuela pendientes.
2. **Resolver denuncias** (borrar el contenido denunciado).
3. **Restaurar** cuentas eliminadas.

Está previsto estudiarlo en el futuro como un tipo de usuario propio con panel de administración;
**hoy no lo es y no forma parte del alcance**.

---

## 4. Cuenta, registro y sesión

- **Autenticación: mediante Keycloak**, que federa Google dentro de Keycloak. La API solo acepta
  **access tokens JWT emitidos por Keycloak** (valida autoridad, firma, expiración y audiencia
  `SocialSchool-Api`) y exige usuario autenticado por defecto; no hay credenciales propias (ni
  contraseña, ni verificación de email, ni recuperación de contraseña por parte de este sistema).
- **Audience mapper:** el cliente de la API en Keycloak necesita un mapper que incluya
  `SocialSchool-Api` en el claim `aud`; sin él, la API rechaza todos los tokens.
- **Registro: autentica Keycloak, la cuenta la crea el backend.** Keycloak federa el proveedor de
  identidad y emite el token; al registrarse **lanza un evento de integración** y el backend materializa
  la cuenta de negocio (`User`) con la identidad y el **tipo de cuenta** elegido. Los **datos del
  perfil** —nombre, fecha de nacimiento, deportes, contacto…, lo que distingue a un deportista o a una
  escuela— los completa el usuario **en nuestra aplicación** la primera vez, guiado por su tipo de cuenta.
- **El perfil vive en nuestro dominio.** Keycloak solo aporta la identidad; los datos del perfil no se
  capturan allí. `GET /api/user/profile/me` responde `204` mientras el perfil no exista, y es lo que usa
  la web para mostrar el formulario de alta.
- **Consecuencia:** un usuario puede estar **autenticado sin cuenta de negocio** mientras el evento no
  se haya procesado. La API debe aceptarlo igualmente: autenticarse no exige tener cuenta.
- **Campos del deportista:** nombre, apellidos, email, **fecha de nacimiento** (la edad se muestra
  calculada a partir de ella), país, ciudad, foto de perfil, pequeña biografía y **deportes que
  practica** (selección múltiple —se marcan los deseados, a modo de *checkbox*— sobre el catálogo).
- **Campos de la escuela:** nombre, logo, **un único deporte**, descripción, ubicación y datos de
  contacto (email, teléfono y web).
- **Alta de escuela:** queda en estado **pendiente de validación** y el técnico la valida en base de
  datos. *(Qué puede hacer una escuela en estado pendiente queda como decisión abierta; ver §15.)*
- **Eliminación de cuenta: borrado suave.** Se oculta **todo** a los demás y se conserva **todo**
  internamente (publicaciones, imágenes, puntuaciones, vínculos, comentarios). La restauración la hace
  hoy el técnico; en el futuro se estudiará facilitarla al propio usuario. **No** se soporta descarga
  ni exportación de datos.

---

## 5. Relaciones entre usuarios

Existen **tres mecanismos** de relación, y ninguno de ellos es un "seguir" asimétrico: **no existe la
posibilidad de seguir a alguien sin su aceptación**.

### 5.1. Amistad

- Se crea por **solicitud + aceptación** y es **simétrica**: al aceptarse, ambos pasan a ser amigos.
- Solo puede darse entre **deportista↔deportista** o **deportista↔escuela**. **Nunca entre dos
  escuelas.**
- El **rechazo es silencioso**: el solicitante no recibe aviso del rechazo, simplemente ve que puede
  volver a enviar la solicitud.
- Cualquiera de las dos partes puede **deshacer** la amistad en cualquier momento.
- Es requisito para ver el perfil completo de un deportista y para puntuar sus maniobras.

### 5.2. Vínculo de asistencia (escuela ↔ deportista)

Es el mecanismo que sostiene la propuesta de valor del producto y es **independiente de la amistad**.

- Lo puede **iniciar cualquiera de las dos partes**: una escuela puede añadir a un deportista a su
  lista de asistencia, y un deportista puede declarar que ha asistido a una escuela.
- La otra parte **acepta o rechaza**. El **rechazo es silencioso** (quien lo propuso ve que puede
  volver a solicitarlo).
- El deportista puede declarar asistencia a una escuela **aunque no practique el deporte de esa
  escuela**.
- Mientras el vínculo está **aceptado**, la escuela **ve automáticamente el perfil del deportista
  como si fueran amigos** y puede puntuarle sus maniobras.
- Cualquiera de las dos partes puede **retirar** el vínculo en cualquier momento; al retirarlo, la
  asistencia desaparece y **se borran las puntuaciones** que esa escuela había dado a ese deportista.
- **Amigos y escuelas asistidas son dos listas distintas**, aunque pueden solaparse.

### 5.3. Bloqueo

- Es **mutuo en efecto**: ninguno de los dos vuelve a ver al otro.
- Es **permanente**: no existe desbloqueo, porque a partir del bloqueo dejan de verse mutuamente.
- **Rompe la amistad** si existía, y hace desaparecer al otro de **las búsquedas** y de **las listas
  de asistentes**.
- Un usuario bloqueado **no puede enviar solicitudes** al que lo bloqueó.
- Si hay un vínculo de asistencia entre ambos, el vínculo **desaparece**.

---

## 6. Visibilidad y privacidad

| Elemento | Quién lo ve |
|---|---|
| Perfil de **deportista** | Solo sus **amigos** y las **escuelas con vínculo de asistencia aceptado**. Cualquier otro usuario ve únicamente **foto, nombre y un botón "seguir"**. |
| Perfil de **escuela** | **Público** para usuarios autenticados: perfil completo y servicios. |
| **Lista de asistentes** de una escuela | Usuarios autenticados, con **nombre y foto** de cada deportista. Para navegar al perfil del deportista hace falta ser su **amigo**. La lista **excluye** a quien haya bloqueado (o haya sido bloqueado por) el usuario que consulta. |
| **Escuelas a las que ha asistido** un deportista | Sus amigos. Además, una escuela **con vínculo de asistencia** las ve aunque no sean amigos. |
| Asistencias **no aceptadas** | Solo el propio deportista, marcadas como **"pendiente de validación"**. El resto de usuarios solo ve las **aceptadas**. |

Todas las consultas requieren un usuario **autenticado**. Las cuentas eliminadas y los usuarios
bloqueados quedan excluidos de búsquedas y listas.

---

## 7. Funcionalidades

### 7.1. Catálogo de deportes y maniobras

- Lo **mantiene el técnico** y es ampliable con el tiempo.
- Una **maniobra** pertenece siempre a **un deporte**. *(El término "habilidad" se ha usado en la
  entrevista como sinónimo de "maniobra"; en este documento se usa siempre **maniobra**.)*
- Un deportista **solo puede declarar maniobras de los deportes que practica**.

### 7.2. Maniobras del deportista

- El deportista **selecciona** del catálogo las maniobras que domina.
- Una maniobra declarada puede **deshabilitarse** (quitarla de su lista). **No se borran sus
  puntuaciones**: la maniobra queda deshabilitada y, si se vuelve a habilitar, aparece con las
  puntuaciones anteriores.

### 7.3. Puntuaciones

- **Solo se puntúan maniobras declaradas.** No existe puntuación de un nivel global.
- Escala: **1 a 5 estrellas**.
- **Autopuntuación permitida**: un usuario puede puntuar sus propias maniobras.
- Un usuario puede puntuar **varias o todas** las maniobras de otro usuario.
- Quien puntúa puede **editar** su puntuación e incluso **borrarla**.
- El usuario puntuado **no puede ocultar ni borrar** las puntuaciones ajenas.
- Lo que se muestra es la **media** de todas las puntuaciones recibidas; al abrir el detalle de la
  maniobra se ve **qué puntuación ha dado cada usuario** que la ha puntuado.
- **La puntuación de una escuela vale lo mismo que la de un deportista.**
- Una maniobra **sin puntuaciones** se muestra con las **5 estrellas vacías**.

**Quién puede puntuar:**

- **Deportista → deportista**, siempre que sean **amigos**.
- **Escuela → deportista**, siempre que exista **vínculo de asistencia aceptado**. (Las escuelas no
  declaran maniobras, por lo que nadie las puntúa a ellas.)

**Borrado en cascada (consecuencia de las reglas de relación):**

- Al **deshacer una amistad**, se borran las puntuaciones emitidas entre ambos.
- Al **retirarse un vínculo de asistencia**, se borran las puntuaciones que esa escuela había dado a
  ese deportista.
- Al **bloquear**, se rompe la amistad y se borran las puntuaciones entre ambos.

### 7.4. Publicaciones

- Una publicación lleva **un único elemento multimedia: una foto O un vídeo** (nunca ambos, nunca
  varios) acompañado de un comentario, y se asocia a **un deporte**.
- Una publicación **no se vincula a una maniobra**, solo al deporte.
- Las publicaciones de una **escuela** se asocian **siempre a su propio deporte** (solo tiene uno).
- El autor puede **borrar** su publicación; **no puede editarla**.
- En el perfil de un deportista se ven **sus últimas publicaciones mezclando todos sus deportes**; el
  visitante puede **filtrar por el deporte** que le interese. La sección de maniobras se organiza en
  **una pestaña por deporte**.

### 7.5. Comentarios y "me gusta"

- **Perfil de deportista:** solo pueden comentar y dar "me gusta" los **deportistas amigos** y las
  **escuelas con vínculo de asistencia**.
- **Perfil de escuela (público):** **cualquier usuario autenticado** puede comentar y dar "me gusta".
- **Un autor solo puede borrar su propio comentario.** Nadie puede borrar comentarios ajenos, ni
  siquiera en su propia publicación. En su lugar, se **denuncia** el comentario y el técnico puede
  borrarlo.

### 7.6. Muro

- Es un flujo **cronológico**, sin algoritmos ni recomendaciones.
- Un **deportista** ve en su muro las publicaciones de **sus amigos** y de **las escuelas a las que ha
  asistido**.
- Para ver una escuela con la que no tiene ninguna relación, debe **buscarla** y entrar en su perfil
  explícitamente.
- Las **novedades y ofertas** de una escuela son **publicaciones normales** y van **mezcladas** en el
  mismo flujo.

### 7.7. Buscador

- Búsqueda por **texto libre** sobre el **nick**, el **nombre** y los **apellidos**.
- **No** hay filtro por deporte.
- Los resultados se presentan **igual** para escuelas y deportistas: **imagen y nombre**.
- Desde el resultado se puede acceder a la **escuela**; si el resultado es un **deportista que no es
  amigo**, al entrar en su perfil solo se ve **foto, nombre y el botón "seguir"**.
- Se excluyen las cuentas eliminadas y los usuarios bloqueados.

### 7.8. Denuncias y moderación

- Se pueden denunciar **comentarios y publicaciones**.
- La denuncia lleva **categoría** y **texto libre**. Categorías: **Obsceno**, **Racista**,
  **Discriminatorio**, **Otros**.
- Mientras se revisa, **el contenido denunciado deja de mostrarse**.
- La resolución la hace el **técnico**, que recibe una **notificación**.

### 7.9. Servicios de la escuela

- Sección del perfil de escuela con una **lista de servicios**, cada uno con **nombre, descripción,
  precio y moneda**.
- La escuela elige la **moneda** que desea usar.
- Los servicios se **editan libremente** y **no hay histórico**.
- **No se puede contratar, reservar ni pagar** desde la aplicación.
- Las **ofertas** quedan fuera de esta sección: si una escuela quiere anunciar una oferta, publica un
  **post** con una foto con la información.

### 7.10. Notificaciones

**Eventos que generan notificación:**

1. Solicitud de amistad **recibida**.
2. Solicitud de amistad **aceptada**.
3. Asistencia **solicitada**.
4. Asistencia **aceptada**.
5. Asistencia **rechazada**.
6. Nuevo **comentario**.
7. Nuevo **"me gusta"**.
8. Nueva **puntuación** recibida.
9. **Denuncia** dirigida al técnico.

**Canales:** aplicación, email y push. El usuario puede **desactivar las notificaciones por canal**
(las de aplicación, las de email o las de push por separado).

**Alcance actual:** en el MVP solo se implementan las notificaciones **dentro de la aplicación**. Las
de **email y push se abordan más adelante** (ver §15).

---

## 8. Conceptos del negocio (glosario)

| Concepto | Definición |
|---|---|
| **Deportista** | Usuario que practica uno o varios deportes, declara maniobras y recibe puntuaciones. |
| **Escuela** | Usuario de tipo escuela, asociado a **un único deporte**, que publica, ofrece servicios y puntúa a los deportistas que han asistido a ella. |
| **Técnico** | Persona con acceso a base de datos, no un usuario de la aplicación. Valida escuelas, modera denuncias y restaura cuentas. |
| **Deporte** | Elemento de catálogo mantenido por el técnico. Los deportistas seleccionan los que practican; una escuela tiene exactamente uno. |
| **Maniobra** | Elemento de catálogo, perteneciente a un deporte, que un deportista declara dominar y que otros puntúan. Sinónimo usado en la entrevista: "habilidad". |
| **Maniobra declarada** | Maniobra que un deportista ha seleccionado de su catálogo. Puede estar habilitada o deshabilitada. |
| **Puntuación** | Valoración de 1 a 5 estrellas sobre una maniobra declarada, emitida por un usuario autorizado. |
| **Amistad** | Relación simétrica y voluntaria entre deportistas, o entre deportista y escuela, creada por solicitud y aceptación. |
| **Asistencia** | Vínculo entre un deportista y una escuela, iniciado por cualquiera de las partes y aceptado por la otra. Da a la escuela acceso al perfil del deportista y derecho a puntuarle. |
| **Bloqueo** | Relación mutua y permanente por la que dos usuarios dejan de verse por completo. |
| **Publicación** | Aportación de un usuario compuesta por una foto **o** un vídeo, un comentario y un deporte. |
| **Muro** | Flujo cronológico con las publicaciones que le corresponden al usuario. |
| **Denuncia** | Aviso de contenido inapropiado (comentario o publicación) con categoría y texto libre. |
| **Nick** | Identificador público único, inmutable y alfanumérico de cada cuenta. |
| **Servicio** | Elemento de la sección comercial de una escuela: nombre, descripción, precio y moneda. |

---

## 9. Reglas transversales (invariantes del producto)

1. Una cuenta tiene **un solo tipo**, elegido al crearse, y es **inmutable**.
2. El **nick** es obligatorio, **único**, **inmutable** y **solo de letras y números**.
3. Solo pueden registrarse **mayores de edad**.
4. **No existe relación asimétrica de seguimiento**: toda relación se establece por aceptación.
5. La **amistad es simétrica** y no puede existir entre dos escuelas.
6. Los **rechazos son silenciosos** y permiten volver a solicitarlo.
7. Cualquiera de las partes puede **deshacer** una amistad o **retirar** una asistencia, sin permiso
   de la otra.
8. El **bloqueo es mutuo, permanente e irreversible**, y arrastra la amistad, las puntuaciones y la
   asistencia.
9. El perfil del **deportista es privado**; el de la **escuela es público** para autenticados.
10. Una **escuela con asistencia aceptada** ve al deportista **como si fuera amigo**.
11. Un deportista **solo declara maniobras de los deportes que practica**.
12. **Solo se puntúan maniobras**, nunca un nivel global, con **1 a 5 estrellas**.
13. La **autopuntuación está permitida**.
14. Una maniobra deshabilitada **conserva sus puntuaciones**.
15. **Perder la relación borra las puntuaciones** obtenidas a través de ella.
16. Una publicación lleva **un solo** elemento multimedia (foto **o** vídeo) y **un deporte**; las de
    escuela, siempre el suyo.
17. Las publicaciones **no se editan**; se **borran**.
18. Cada autor **solo** borra su propio comentario; los ajenos se **denuncian**.
19. El contenido **denunciado se oculta** hasta que el técnico lo resuelve.
20. El **muro es cronológico**, sin algoritmo.
21. El **buscador** es de texto libre por nick, nombre y apellidos, sin filtro de deporte.
22. **Una escuela = un deporte.**
23. Los **servicios no son contratables**: son informativos.
24. La **eliminación de cuenta es suave** y reversible por el técnico, conservando todo el contenido.
25. Ninguna operación de negocio está disponible para usuarios no autenticados.

---

## 10. Ciclos de vida y estados

| Elemento | Estados | Transiciones |
|---|---|---|
| **Cuenta** | `Activa` · `PendienteValidacion` (escuela) · `Eliminada` | Alta → Activa (deportista) o PendienteValidacion (escuela); PendienteValidacion → Activa (la valida el técnico); Activa → Eliminada (borrado suave del usuario); Eliminada → Activa (restauración por el técnico). |
| **Solicitud de amistad** | `Pendiente` · `Aceptada` · `Rechazada` · `Cancelada` | Cualquier usuario → Pendiente; Pendiente → Aceptada (crea la amistad) o Rechazada (silenciosa); el solicitante puede cancelarla. Tras un rechazo puede volver a emitirse. |
| **Asistencia** | `Pendiente` · `Aceptada` · `Rechazada` · `Retirada` | La inicia cualquiera de las partes → Pendiente; Aceptada (habilita visibilidad y puntuación) o Rechazada (silenciosa, reemisable); Aceptada → Retirada (la retira cualquiera de las partes y se borran las puntuaciones de esa escuela). **Una sola fila por par**: volver a solicitar devuelve esa misma fila a `Pendiente`, así que no se conserva histórico de los intentos anteriores. |
| **Maniobra declarada** | `Habilitada` · `Deshabilitada` | El deportista la habilita al seleccionarla del catálogo; puede deshabilitarla y volver a habilitarla conservando el histórico de puntuaciones. |
| **Denuncia** | `Pendiente` · `Resuelta` | Cualquier usuario denuncia → Pendiente (el contenido se oculta); el técnico la resuelve. |
| **Publicación** | `Publicada` · `Eliminada` | El autor publica; el autor borra (sin edición posible). |
| **Bloqueo** | `Activo` | Se crea una vez y **no se revierte**. |

---

## 11. Integraciones y sistemas externos

| Integración | Uso | Estado |
|---|---|---|
| **Keycloak** | Proveedor de identidad de la API: emite los access tokens que la API valida (autoridad y audiencia). | En alcance. |
| **Google** | Inicio de sesión social del usuario, **federado dentro de Keycloak**. | En alcance. |
| **Keycloak Events** | Evento de integración que avisa al backend cuando un usuario se registra, para materializar su cuenta. | Pendiente de diseñar (ver §15.13 y §15.14). |
| **Keycloak Admin API** | Consulta de los datos del usuario que el evento no transporte (atributos personalizados). | Pendiente de diseñar (ver §15.13). |
| **Azure Blob Storage** | Almacenamiento de imágenes y vídeos. | En alcance. |
| **Worker de medios** | Procesa imágenes y vídeos a distintos formatos. Es un **componente independiente**, disparado por un *trigger* al detectar que ha entrado una imagen. | **Fuera del alcance de este backend**; se documenta como dependencia. |
| **Notificaciones por email** | Canal de notificación. | **Pospuesto.** Proveedor sin decidir: Azure Communication Services o FCM/APNs. |
| **Notificaciones push** | Canal de notificación. | **Pospuesto.** Proveedor sin decidir: Azure Communication Services o FCM/APNs. |
| **Multi-idioma de interfaz** | Español e inglés. | En alcance. Afecta a los textos de la interfaz y a los catálogos; **no** al contenido escrito por los usuarios. |

---

## 12. Restricciones

**Funcionales**

- Solo **mayores de edad**.
- **Nick inmutable** y **solo letras y números**.
- **Una escuela = un deporte.**
- Una publicación = **una sola** foto o vídeo.
- **Sin mensajería privada**, **sin pagos/reservas/contratación** y **sin panel de administración**.
- La operación del técnico se realiza **fuera de la API** (acceso directo a base de datos).

**No funcionales**

- Objetivo de **carga media**: unos **1.000 usuarios**, con una arquitectura preparada para adaptarse
  a medida que crezca el consumo.
- El resto de requisitos no funcionales (volumen exacto, tiempos de respuesta, almacenamiento)
  **no están definidos**.

---

## 13. Alcance del MVP

1. Cuentas aprovisionadas desde el **registro en Keycloak** (Google federado dentro de Keycloak) por
   **evento de integración**: perfil de deportista y de escuela, **nick único**, foto, ubicación,
   biografía y deportes. **No** hay formulario de perfil en nuestra aplicación.
2. **Alta de escuela con validación pendiente** (manual, por el técnico).
3. **Catálogo** de deportes y maniobras (mantenido por el técnico).
4. **Maniobras declaradas** y **puntuaciones 1-5** con media y detalle por autor; autopuntuación;
   maniobras deshabilitables sin perder histórico.
5. **Amistades** con solicitud, aceptación y rechazo silencioso; borrado en cascada de puntuaciones al
   deshacerlas.
6. **Asistencia a escuelas** con doble dirección de solicitud, aceptación/rechazo, retirada por
   cualquiera de las partes, visibilidad especial de la escuela sobre el asistente y efecto sobre la
   lista de asistentes.
7. **Publicaciones** (una foto **o** un vídeo + comentario + deporte), **comentarios** y **"me gusta"**
   con las reglas de visibilidad definidas; borrado propio, sin edición.
8. **Muro** cronológico con publicaciones de amigos y de escuelas asistidas.
9. **Buscador** por texto (nick, nombre y apellidos).
10. **Bloqueo** mutuo y permanente.
11. **Denuncias** de comentarios y publicaciones (categoría + texto) con ocultación mientras se
    revisan.
12. **Notificaciones dentro de la aplicación** para los eventos definidos, desactivables por canal.
13. **Servicios de escuela** (nombre, descripción, precio y moneda).
14. **Multi-idioma de interfaz**: español e inglés.

---

## 14. Fuera de alcance (explícito)

- **Mensajería privada** entre usuarios (se abordará en el futuro).
- **Contratación, reserva o pago** de servicios de escuela.
- **Ofertas** como entidad propia (se sustituyen por publicaciones).
- **Relación entre escuelas** (escuela↔escuela).
- **Panel de administración**: el técnico opera directamente sobre la base de datos.
- **Worker de medios**: componente independiente, fuera de este backend.
- **Contenido en directo** y **vídeo en streaming**.
- **Versión web de administración**.
- **Notificaciones por email y push** (pospuestas).
- **Descarga o exportación de datos** del usuario.
- La **aplicación móvil** y la **aplicación web** (son clientes consumidores, no este backend).
- El **dominio de ejemplo de la plantilla** (partidos y árbitros) y sus casos de uso asociados.

---

## 15. Decisiones abiertas

Puntos **no definidos** durante la entrevista. Se documentan para no inventarlos; ningún requisito de
este documento depende de una decisión inventada.

1. **Escuela pendiente de validación:** qué puede hacer mientras está pendiente (¿existe y es
   invisible para todos?, ¿puede iniciar sesión y completar su perfil?, ¿puede publicar?).
2. **Muro de la escuela:** si la cuenta de escuela tiene un muro propio y con publicaciones de quién.
   Derivación coherente sin confirmar: las de sus deportistas amigos y las de sus deportistas con
   vínculo de asistencia (porque el vínculo se comporta "como si fueran amigos").
3. **Revisión de una solicitud de asistencia:** si la escuela ve el historial de escuelas del
   solicitante también **antes** de aceptar (hoy está definido para el vínculo aceptado).
4. **Proveedor de email y push** (Azure Communication Services frente a FCM/APNs) y momento de
   implementación.
5. **Deshabilitar una maniobra:** confirmar que, además de ocultarla del perfil, no participa en
   ningún agregado mostrado. Las puntuaciones se conservan en todo caso.
6. **Contenido denunciado:** si la ocultación afecta también **a su autor** mientras está pendiente.
7. **Vinculación de identidades:** qué ocurre si un mismo email accede por Google y por Facebook
   (¿una sola cuenta o dos?).
8. **Verificación de edad:** mecanismo concreto (autodeclarada u otro) y cómo aplica a las cuentas de
   escuela.
9. **Edición de comentarios:** la entrevista definió borrado y denuncia, pero no si un comentario
   puede editarse.
10. **Deporte de la escuela:** se ha definido que una escuela tiene **un** deporte, pero no si ese
    deporte puede **cambiarse** una vez creada la cuenta (y qué ocurriría con sus publicaciones, que
    se asocian a su deporte).
11. **Facebook:** la última decisión sobre identidad menciona **solo Google** federado en Keycloak
    (§4 y §11). Queda por confirmar si Facebook sigue en el alcance o se descarta.
12. **Identidad frente a las cuentas de negocio:** la cuenta de negocio (`Users`) se materializa a
    partir del evento de registro de Keycloak, pero no se ha decidido qué pasa si un mismo `sub`
    cambia de correo, ni cómo se vincula si en el futuro conviven varios proveedores federados.
13. **Contenido del evento de registro:** el payload por defecto de Keycloak **no** incluye los
    atributos personalizados del perfil. Hay que decidir entre un *Event Listener SPI* propio que lo
    enriquezca, un proveedor de webhooks de terceros, o consultar la Admin API al recibirlo. Depende
    de la versión y de la configuración del realm, así que debe comprobarse antes de diseñar nada.
14. **Quién valida el nick:** la unicidad del nick y la existencia del deporte dependen de nuestra
    base de datos, y Keycloak no puede comprobarlas en su formulario. Un registro puede triunfar en
    Keycloak y **fallar al materializar la cuenta**. Falta decidir qué ocurre entonces (reintento,
    aviso al usuario, bloqueo) y cómo se le pide otro nick.
15. **Fuente de verdad del perfil:** el perfil queda duplicado en Keycloak y en nuestro dominio.
    Falta decidir si una edición posterior en Keycloak pisa el dato local, o si nuestro dominio manda
    y Keycloak solo captura en el alta.
16. **Atributos personalizados en Keycloak:** hay que declararlos (User Profile), añadirlos al flujo
    de registro y, si deben viajar en el token, a los mappers. Es trabajo de configuración en
    Keycloak, no en el backend.
