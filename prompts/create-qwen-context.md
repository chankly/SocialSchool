# Generate QWEN.md

Analiza completamente este repositorio y genera el archivo `QWEN.md` en la raíz del proyecto.

## Contexto del proyecto

Este proyecto ha sido creado a partir de una plantilla arquitectónica.

La plantilla contiene código de ejemplo que sirve para demostrar la arquitectura, patrones y convenciones de desarrollo. Las entidades, casos de uso, comandos, queries, servicios y demás elementos de dominio que existan actualmente en el código pueden ser únicamente ejemplos de la plantilla.

Por tanto:

* NO asumas que las entidades actuales representan el dominio real del proyecto.
* NO conviertas los ejemplos existentes en requisitos funcionales.
* NO inventes reglas de negocio que no puedan deducirse claramente del código o de documentación existente.
* Distingue siempre entre decisiones arquitectónicas reales y código de ejemplo procedente de la plantilla.

El proyecto evolucionará progresivamente y el código de ejemplo podrá ser reemplazado por la implementación real.

## Objetivo de QWEN.md

`QWEN.md` debe convertirse en el contexto técnico permanente que Qwen utilizará para trabajar en este repositorio.

Debe explicar:

1. Cómo está estructurado técnicamente el proyecto.
2. Qué arquitectura utiliza.
3. Qué responsabilidades tiene cada proyecto, capa, carpeta y componente importante.
4. Qué dependencias existen entre las capas.
5. Qué patrones arquitectónicos y de diseño se utilizan.
6. Qué convenciones debe seguir Qwen al modificar o crear código.
7. Cómo deben implementarse nuevas funcionalidades siguiendo la arquitectura existente.
8. Cómo deben implementarse Commands, Queries, Handlers, Validators, Domain entities, repositories, servicios, etc., cuando estos patrones existan en el proyecto.
9. Cómo se realiza la persistencia.
10. Cómo se configura la inyección de dependencias.
11. Cómo se construyen y organizan los tests.
12. Qué convenciones de nombres y organización del código existen.
13. Qué restricciones arquitectónicas deben respetarse.
14. Qué decisiones técnicas son importantes y no deben modificarse arbitrariamente.
15. Qué partes del código actual son ejemplos de la plantilla y no deben interpretarse como requisitos de negocio.

## Analiza antes de escribir

Antes de generar `QWEN.md`, inspecciona el repositorio completo.

Presta especial atención a:

* `.slnx` / `.sln`
* archivos `.csproj`
* estructura de proyectos
* `Program.cs`
* configuración y Dependency Injection
* Domain
* Application
* Infrastructure
* API
* tests
* entidades
* value objects
* aggregates
* commands
* queries
* handlers
* validators
* repositories
* servicios
* DTOs
* mappings
* persistencia
* EF Core
* middleware
* excepciones
* configuración
* logging
* autenticación/autorización
* cualquier otro patrón utilizado por la plantilla

No te limites a listar carpetas. Deduce las responsabilidades y relaciones arquitectónicas a partir del código existente.

## Contenido esperado de QWEN.md

Organiza el documento aproximadamente con esta estructura:

# Project Context

## Project Type

Explica que este repositorio es un proyecto real creado a partir de una plantilla arquitectónica y que el código existente puede contener ejemplos heredados de dicha plantilla.

## Architecture

Describe la arquitectura general del sistema.

## Solution Structure

Describe cada proyecto de la solución y su responsabilidad.

## Layer Responsibilities

Explica qué responsabilidad tiene cada capa.

## Dependency Rules

Documenta qué capas pueden depender de otras y qué dependencias están prohibidas.

## Domain

Describe cómo debe modelarse el dominio desde el punto de vista técnico.

No documentes aquí las entidades concretas como requisitos funcionales si proceden de la plantilla.

## Application

Describe cómo se organizan los casos de uso y la lógica de aplicación.

## Infrastructure

Describe persistencia, repositorios, servicios externos y demás infraestructura.

## API

Describe cómo deben implementarse los endpoints y cómo interactúan con Application.

## Dependency Injection

Describe las reglas y convenciones utilizadas para registrar servicios.

## Validation

Describe cómo se realizan las validaciones.

## Error Handling

Describe las excepciones y el tratamiento de errores.

## Persistence

Describe EF Core, DbContext, repositories, migrations y cualquier convención relevante.

## Testing

Describe cómo están organizados los tests y qué patrones deben seguir.

## Coding Conventions

Documenta las convenciones observadas en el código.

## Implementation Guidelines

Proporciona instrucciones concretas para que un agente de IA implemente nuevas funcionalidades respetando la arquitectura existente.

## Architectural Constraints

Enumera las reglas que Qwen NO debe romper.

## Template Example Code

Identifica las partes del código que parecen proceder de la plantilla y explica explícitamente que son ejemplos arquitectónicos y no necesariamente requisitos del dominio real.

## Important Notes

Incluye cualquier otra información técnica que sea importante para trabajar correctamente en este repositorio.

## Reglas importantes

* No modifiques ningún archivo del proyecto excepto `QWEN.md`.
* No implementes código.
* No crees nuevas clases.
* No modifiques la solución.
* No modifiques `openspec/`.
* No crees documentación adicional.
* No inventes información que no pueda justificarse mediante el repositorio.
* Cuando una decisión no pueda determinarse con certeza, indícalo explícitamente.
* Prioriza las convenciones realmente utilizadas en el código frente a convenciones genéricas.
* El documento debe ser útil para un agente de IA que vaya a implementar nuevas funcionalidades posteriormente.
* Evita documentación excesivamente genérica.
* Incluye ejemplos de patrones únicamente cuando ayuden a explicar una regla arquitectónica.
* No copies grandes cantidades de código fuente dentro de `QWEN.md`.

Finalmente, crea o actualiza únicamente:

`QWEN.md`

El resultado debe ser un documento conciso pero suficientemente completo para que otro agente de IA pueda entender la arquitectura y trabajar en el proyecto sin tener que redescubrir continuamente estas reglas.
