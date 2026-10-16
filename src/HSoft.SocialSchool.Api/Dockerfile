# Imagen de la API de SocialSchool.
#
# Vive dentro del proyecto, como cuando Visual Studio agrega compatibilidad con Docker, y el contexto
# de build es la carpeta src/ —que es la que el propio tooling de contenedores usa por defecto
# (DockerDefaultContext = ..)—: el proyecto referencia a Domain, Application y Persistence, y esas tres
# carpetas están justo ahí, al lado. Así F5 con el perfil de Docker y la orden de abajo construyen lo
# mismo.
#
#   docker build --file src/HSoft.SocialSchool.Api/Dockerfile --tag socialschool-api:local src
#
# En el despliegue la construye la pipeline, que además le pone la etiqueta del commit
# (.github/workflows/api-deploy.yml).

# ---------------------------------------------------------------- build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Primero solo los proyectos: mientras no cambien los paquetes, esta capa se reutiliza tal cual y el
# restore no se repite en cada build.
COPY HSoft.SocialSchool.Domain/HSoft.SocialSchool.Domain.csproj HSoft.SocialSchool.Domain/
COPY HSoft.SocialSchool.Application/HSoft.SocialSchool.Application.csproj HSoft.SocialSchool.Application/
COPY HSoft.SocialSchool.Persistence/HSoft.SocialSchool.Persistence.csproj HSoft.SocialSchool.Persistence/
COPY HSoft.SocialSchool.Api/HSoft.SocialSchool.Api.csproj HSoft.SocialSchool.Api/
RUN dotnet restore HSoft.SocialSchool.Api/HSoft.SocialSchool.Api.csproj

COPY . .
RUN dotnet publish HSoft.SocialSchool.Api/HSoft.SocialSchool.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app

# ---------------------------------------------------------------- final
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .

# El ingress de Container Apps apunta al 8080, y Terraform inyecta este mismo valor como variable de
# entorno. Ponerlo aquí además como valor por defecto hace que la imagen funcione igual con un
# `docker run` suelto, sin quedarse en el 5000 que trae ASP.NET Core por defecto.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# La imagen de runtime trae un usuario sin privilegios y publica su identificador en APP_UID.
USER $APP_UID

ENTRYPOINT ["dotnet", "HSoft.SocialSchool.Api.dll"]
