# API de Medición de Niveles de Agua

## Descripción
API REST desarrollada en .NET Core MVC para la gestión y monitoreo de niveles de agua en estanques. Permite registrar mediciones desde dispositivos IoT y consultar el histórico de mediciones.

## Especificaciones Técnicas
- Framework: .NET Core 8.0
- Arquitectura: REST API / MVC
- ORM: Entity Framework Core
- Base de datos: Oracle 19c
- Swagger/OpenAPI: v3.0

## Requisitos Previos
- .NET Core SDK 8.0 o superior
- Oracle Database 19c o superior
- Entity Framework Core Tools (solo para desarrollo local)
- Docker (opcional)
- Visual Studio 2022 o VS Code con extensiones C#

## Guía de Instalación y Ejecución

### 1. Instalación Local

1. Clonar el repositorio
```bash
# Clonar el repositorio
git clone https://github.com/cristobalrios2001/EsvalTK

# Navegar al directorio del proyecto
cd EsvalTK
```

2. Restaurar dependencias
```bash
dotnet restore
```

3. Configurar variables de entorno
```bash
# Linux/macOS
export DB_USER=
export DB_PASSWORD=
export DB_SOURCE="(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521)))(CONNECT_DATA=(SERVICE_NAME=esvaltk)))"

# Windows (PowerShell)
$env:DB_USER=""
$env:DB_PASSWORD=""
$env:DB_SOURCE="(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521)))(CONNECT_DATA=(SERVICE_NAME=esvaltk)))"
```

4. Configurar base de datos (Solo Desarrollo Local)
> **NOTA IMPORTANTE**: Las migraciones son utilizadas solo en el ambiente de desarrollo local. 
> El ambiente de producción y Docker utilizan una base de datos pre-configurada.

```bash
# Instalar herramienta EF Core
dotnet tool install --global dotnet-ef

# Crear migración inicial
dotnet ef migrations add InitialMigration

# Aplicar migración
dotnet ef database update
```

5. Iniciar la aplicación
```bash
# Modo desarrollo
dotnet run
```

La API estará disponible en:
- HTTP: http://localhost:7121
- Swagger UI: http://localhost:7121/docs

### 2. Despliegue con Docker

1. Construir la imagen
```bash
docker build -t mediciones-api .
```

2. Ejecutar el contenedor
```bash
docker run -d \
  -p 8080:7121 \
  --name mediciones-container \
  -e DB_USER=esval_user \
  -e DB_PASSWORD=Esval_tk1 \
  -e DB_SOURCE="[CONNECTION_STRING]" \
  mediciones-api
```
La API estará disponible en:
- HTTP: http://localhost:8080
- Swagger UI: http://localhost:8080/docs

## API Endpoints

### 1. Registrar Medición de Nivel de Agua
Endpoint utilizado por el dispositivo receptor (antena receptora de señales LoRa) para registrar nuevas mediciones de nivel de agua.

- **URL**: `/api/Mediciones/RegisterWaterLevelMeasurement`
- **Método**: `POST`
- **Content-Type**: `application/json`

#### Request Body (MedicionRequest)
```json
{
    "idDispositivo": "string",
    "nivelAgua": 0.0
}
```

#### Respuestas
**Éxito (200 OK)**
```json
{
    "message": "Medición registrada con éxito.",
    "idDispositivo": "string",
    "nivel": 0,
    "fechaRegistro": "2024-12-21T10:30:00"
}
```

**Error de Validación (400 Bad Request)**
```json
{
    "errors": {
        "propertyName": [
            "mensaje de error de validación"
        ]
    }
}
```

**Dispositivo No Encontrado (404 Not Found)**
```json
{
    "message": "No se encontró un dispositivo activo con el ID proporcionado."
}
```

### 2. Obtener Últimas Mediciones por Dispositivo
Endpoint para consultar las mediciones más recientes de todos los dispositivos activos.

- **URL**: `/api/Mediciones/GetLatestMeasurementByDevice`
- **Método**: `GET`

#### Respuestas
**Éxito (200 OK)**
```json
[
    {
        "idRelacion": "string",
        "numeroEstanque": "string",
        "nivel": 0,
        "fecha": "2024-12-21",
        "hora": "10:30:00"
    }
]
```

**Sin Datos (404 Not Found)**
```json
{
    "message": "No se encontraron mediciones."
}
```

## Testing

### Paquetes Utilizados
- xUnit (v2.9.2) - Framework principal de testing
- xunit.runner.visualstudio (v2.8.2) - Runner para Visual Studio
- FluentAssertions (v6.12.2) - Aserciones más legibles
- Moq (v4.20.72) - Framework de mocking
- coverlet.collector (v6.0.0) - Cobertura de código
- Microsoft.EntityFrameworkCore.InMemory (v9.0.0) - Base de datos en memoria para testing

### Ejecutar Pruebas
```bash
# Ejecutar todas las pruebas
dotnet test

# Ejecutar pruebas con cobertura
dotnet test /p:CollectCoverage=true
```

## Versionamiento
- Versión actual: 1.0.0

## Soporte y Contacto
- Equipo responsable: Equipo Capstone UCN 2024
  - Diego Morales
  - Cristóbal Ríos
  - Francisco Saavedra

