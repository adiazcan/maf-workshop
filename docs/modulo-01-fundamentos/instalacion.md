# Guía de Instalación: Módulo 1 - Fundamentos

**Duración estimada**: 20-30 minutos  
**Última actualización**: 2026-01-12

## Requisitos Previos

Antes de comenzar el workshop, debes tener instalado:

1. **.NET 10 SDK**
2. **Visual Studio Code** (recomendado) o Visual Studio 2025
3. **Git** (para clonar repositorios de ejemplo)
4. **Cuenta de Azure** con acceso a Azure OpenAI Service

---

## Paso 1: Instalar .NET 10 SDK

### Windows

1. Descarga el instalador desde: https://dotnet.microsoft.com/download/dotnet/10.0
2. Ejecuta el instalador `.exe`
3. Sigue las instrucciones del asistente
4. Reinicia tu terminal

### macOS

**Opción 1: Instalador oficial**
```bash
# Descarga desde https://dotnet.microsoft.com/download/dotnet/10.0
# Ejecuta el archivo .pkg descargado
```

**Opción 2: Homebrew**
```bash
brew install dotnet@10
```

### Linux (Ubuntu/Debian)

```bash
# Agregar el repositorio de paquetes de Microsoft
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Instalar .NET 10 SDK
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0
```

### Verificar Instalación

```bash
dotnet --version
```

**Salida esperada**: `10.0.x` (donde x es la versión de patch)

**Verificar que puedes crear proyectos**:
```bash
dotnet new console -n TestApp
cd TestApp
dotnet run
```

Deberías ver: `Hello, World!`

**Limpiar proyecto de prueba**:
```bash
cd ..
rm -rf TestApp
```

---

## Paso 2: Instalar Visual Studio Code

### Windows / macOS / Linux

1. Descarga desde: https://code.visualstudio.com/
2. Instala el editor
3. Abre Visual Studio Code

### Extensiones Recomendadas

Instala estas extensiones para una mejor experiencia:

1. **C# Dev Kit** (Microsoft)
   - ID: `ms-dotnettools.csdevkit`
   - Proporciona IntelliSense, debugging, y herramientas de .NET

2. **C#** (Microsoft)
   - ID: `ms-dotnettools.csharp`
   - Soporte de lenguaje para C#

3. **REST Client** (opcional, útil para probar APIs)
   - ID: `humao.rest-client`

**Instalar desde la terminal de VS Code**:
```bash
code --install-extension ms-dotnettools.csdevkit
code --install-extension ms-dotnettools.csharp
```

---

## Paso 3: Instalar Git

### Windows

Descarga desde: https://git-scm.com/download/win

### macOS

```bash
# Git suele venir preinstalado. Si no:
brew install git
```

### Linux

```bash
sudo apt-get install git
```

### Verificar Instalación

```bash
git --version
```

---

## Paso 4: Configurar Azure OpenAI Service

### 4.1 Crear Recurso de Azure OpenAI

1. Inicia sesión en [Azure Portal](https://portal.azure.com)

2. Haz clic en **"Crear un recurso"**

3. Busca **"Azure OpenAI"**

4. Configura el recurso:
   - **Subscripción**: Tu subscripción de Azure
   - **Grupo de recursos**: Crear nuevo o usar existente (e.g., `rg-maf-workshop`)
   - **Región**: `East US 2` o `Sweden Central` (recomendado para disponibilidad de modelos)
   - **Nombre**: `maf-workshop-openai` (debe ser globalmente único)
   - **Nivel de precios**: `Standard S0`

5. Haz clic en **"Revisar y crear"** → **"Crear"**

6. Espera 2-3 minutos hasta que el despliegue se complete

### 4.2 Desplegar Modelo gpt-5.2

1. Ve a tu recurso de Azure OpenAI en el portal

2. En el menú lateral, selecciona **"Deployments"** (bajo "Resource Management")

3. Haz clic en **"Create new deployment"**

4. Configura el deployment:
   - **Modelo**: `gpt-5.2`
   - **Versión del modelo**: `2026-01-01` (la más reciente disponible)
   - **Nombre del deployment**: `gpt-5.2` (usaremos este nombre en el código)
   - **Capacidad (TPM)**: 50K (suficiente para el workshop)

5. Haz clic en **"Create"**

6. Espera hasta que el estado cambie a **"Succeeded"**

### 4.3 Obtener Credenciales

1. En tu recurso de Azure OpenAI, ve a **"Keys and Endpoint"**

2. Copia y guarda de forma segura:
   - **Endpoint**: `https://tu-recurso-nombre.openai.azure.com/`
   - **Key 1**: Tu API key (e.g., `abc123...`)

**⚠️ IMPORTANTE**: Nunca compartas tu API key públicamente ni la incluyas en código fuente versionado.

### 4.4 Configurar User Secrets (Desarrollo Local)

Los **user secrets** permiten guardar credenciales localmente sin incluirlas en tu código.

**Desde la carpeta de tu proyecto**:

```bash
# Inicializar user secrets para tu proyecto
dotnet user-secrets init

# Guardar el API Key
dotnet user-secrets set "AzureOpenAI:ApiKey" "TU-API-KEY-AQUI"

# Verificar que se guardó correctamente
dotnet user-secrets list
```

**Salida esperada**:
```
AzureOpenAI:ApiKey = TU-API-KEY-AQUI
```

---

## Paso 5: Verificar Configuración Completa

Ejecuta este script de verificación para confirmar que todo está instalado correctamente:

### Crear Script de Verificación

Crea un archivo `verify-setup.sh` (Linux/macOS) o `verify-setup.ps1` (Windows):

**Linux/macOS** (`verify-setup.sh`):
```bash
#!/bin/bash

echo "=== Verificación de Setup para MAF Workshop ==="
echo ""

# .NET SDK
echo "1. Verificando .NET SDK..."
dotnet --version || echo "❌ .NET SDK no encontrado"
echo ""

# VS Code
echo "2. Verificando Visual Studio Code..."
code --version || echo "❌ VS Code no encontrado"
echo ""

# Git
echo "3. Verificando Git..."
git --version || echo "❌ Git no encontrado"
echo ""

# Azure CLI (opcional)
echo "4. Verificando Azure CLI (opcional)..."
az --version || echo "⚠️ Azure CLI no encontrado (opcional)"
echo ""

echo "=== Verificación Completa ==="
```

**Windows PowerShell** (`verify-setup.ps1`):
```powershell
Write-Host "=== Verificación de Setup para MAF Workshop ===" -ForegroundColor Cyan
Write-Host ""

# .NET SDK
Write-Host "1. Verificando .NET SDK..." -ForegroundColor Yellow
dotnet --version
Write-Host ""

# VS Code
Write-Host "2. Verificando Visual Studio Code..." -ForegroundColor Yellow
code --version
Write-Host ""

# Git
Write-Host "3. Verificando Git..." -ForegroundColor Yellow
git --version
Write-Host ""

# Azure CLI (opcional)
Write-Host "4. Verificando Azure CLI (opcional)..." -ForegroundColor Yellow
az --version
Write-Host ""

Write-Host "=== Verificación Completa ===" -ForegroundColor Green
```

### Ejecutar Script

**Linux/macOS**:
```bash
chmod +x verify-setup.sh
./verify-setup.sh
```

**Windows**:
```powershell
.\verify-setup.ps1
```

---

## Solución de Problemas Comunes

### Problema: "dotnet: command not found"

**Causa**: .NET SDK no está en el PATH del sistema.

**Solución**:
1. Reinicia tu terminal después de instalar .NET
2. En Windows, reinicia tu computadora
3. Verifica la instalación manualmente en:
   - Windows: `C:\Program Files\dotnet\`
   - macOS: `/usr/local/share/dotnet/`
   - Linux: `/usr/share/dotnet/`

### Problema: "code: command not found"

**Causa**: VS Code no está en el PATH.

**Solución**:
1. Abre VS Code manualmente
2. Abre la paleta de comandos (`Ctrl+Shift+P` o `Cmd+Shift+P`)
3. Busca "Shell Command: Install 'code' command in PATH"
4. Reinicia tu terminal

### Problema: "No puedo crear recurso de Azure OpenAI"

**Causa**: Acceso limitado a Azure OpenAI en tu región/subscripción.

**Solución**:
1. Solicita acceso a Azure OpenAI: https://aka.ms/oai/access
2. Cambia de región (intenta `East US 2`, `Sweden Central`, o `West Europe`)
3. Verifica que tu subscripción tiene cuota disponible

### Problema: "403 Forbidden al desplegar modelo"

**Causa**: Tu subscripción no tiene permiso para ese modelo.

**Solución**:
1. Usa un modelo alternativo disponible (verifica en el portal)
2. Contacta al administrador de tu subscripción de Azure
3. Intenta con `gpt-4` o `gpt-4-turbo` si `gpt-5.2` no está disponible

### Problema: "User secrets no funcionan"

**Causa**: El proyecto no tiene inicializado user secrets.

**Solución**:
```bash
cd tu-proyecto
dotnet user-secrets init
dotnet user-secrets set "AzureOpenAI:ApiKey" "tu-key"
```

---

## Checklist Final

Antes de comenzar el workshop, verifica que tienes:

- [ ] ✅ .NET 10 SDK instalado (`dotnet --version`)
- [ ] ✅ Visual Studio Code instalado
- [ ] ✅ Extensión C# Dev Kit instalada en VS Code
- [ ] ✅ Git instalado (`git --version`)
- [ ] ✅ Recurso de Azure OpenAI creado
- [ ] ✅ Modelo `gpt-5.2` desplegado en Azure
- [ ] ✅ Endpoint y API Key guardados de forma segura
- [ ] ✅ User secrets configurados correctamente

---

## Próximos Pasos

Una vez completada la instalación, estás listo para:

1. Regresar al [README del Módulo 1](README.md) para entender los conceptos teóricos
2. Comenzar con [Lab 01: Hello Agent](labs/01-hello-agent/) para crear tu primer agente

---

## Recursos Adicionales

- [Documentación oficial de .NET](https://docs.microsoft.com/dotnet/)
- [Azure OpenAI Quickstart](https://learn.microsoft.com/azure/ai-services/openai/quickstart)
- [VS Code C# Documentation](https://code.visualstudio.com/docs/languages/csharp)
- [Git Basics](https://git-scm.com/book/en/v2/Getting-Started-Git-Basics)

---

**Tiempo total estimado de instalación**: 20-30 minutos  
**Soporte**: Si encuentras problemas, consulta la [guía de troubleshooting](../troubleshooting.md)
