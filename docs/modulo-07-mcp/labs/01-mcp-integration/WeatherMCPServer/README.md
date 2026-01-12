# Weather MCP Server

Este es un servidor MCP (Model Context Protocol) personalizado que expone herramientas relacionadas con el clima.

## Arquitectura

```
WeatherMCPServer (Servidor MCP)
    ↓ stdio transport
MCPIntegration (Cliente MAF)
    ↓ Azure OpenAI
Agente con herramientas MCP
```

## Herramientas Expuestas

### 1. get_weather
Obtiene el clima actual para una ciudad específica.

**Parámetros:**
- `city` (string, requerido): Nombre de la ciudad

**Ejemplo de uso:**
```json
{
  "city": "Madrid"
}
```

### 2. get_forecast
Obtiene el pronóstico del clima para los próximos días.

**Parámetros:**
- `city` (string, requerido): Nombre de la ciudad
- `days` (integer, opcional): Número de días (1-7), default: 3

**Ejemplo de uso:**
```json
{
  "city": "Barcelona",
  "days": 5
}
```

### 3. convert_temperature
Convierte temperatura entre Celsius y Fahrenheit.

**Parámetros:**
- `value` (number, requerido): Valor de temperatura
- `from_unit` (string, opcional): "C" o "F", default: "C"

**Ejemplo de uso:**
```json
{
  "value": 25,
  "from_unit": "C"
}
```

## Recursos Expuestos

- `weather://madrid` - Datos del clima en Madrid
- `weather://barcelona` - Datos del clima en Barcelona

## Cómo Ejecutar

### Opción 1: Ejecutar directamente
```bash
cd WeatherMCPServer
dotnet run
```

El servidor esperará conexiones stdio (entrada/salida estándar).

### Opción 2: Desde el cliente
El cliente MCPIntegration automáticamente inicia el servidor cuando se ejecuta.

## Protocolo MCP

Este servidor implementa el protocolo MCP usando el SDK oficial `ModelContextProtocol` para .NET.

### Endpoints implementados:
- `tools/list` - Lista todas las herramientas disponibles
- `tools/call` - Ejecuta una herramienta específica
- `resources/list` - Lista todos los recursos disponibles
- `resources/read` - Lee un recurso específico

## Notas de Implementación

- **Transporte:** stdio (entrada/salida estándar)
- **Datos:** Simulados para fines educativos
- **Ciudades soportadas:** Madrid, Barcelona, Sevilla, Bilbao, Valencia, París, Londres, Nueva York
