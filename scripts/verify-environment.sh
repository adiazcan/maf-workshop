#!/bin/bash

# verify-environment.sh
# Script para verificar el ambiente de desarrollo antes del workshop
# Microsoft Agent Framework Workshop

set -e

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Contadores
PASSED=0
FAILED=0
WARNINGS=0

# Banner
echo ""
echo "╔═══════════════════════════════════════════════════════════╗"
echo "║  Microsoft Agent Framework Workshop                       ║"
echo "║  Verificación de Ambiente de Desarrollo                   ║"
echo "╚═══════════════════════════════════════════════════════════╝"
echo ""

# Función para imprimir resultado OK
print_ok() {
    echo -e "${GREEN}✓${NC} $1"
    ((PASSED++))
}

# Función para imprimir resultado FAIL
print_fail() {
    echo -e "${RED}✗${NC} $1"
    echo -e "  ${RED}→${NC} $2"
    ((FAILED++))
}

# Función para imprimir warning
print_warning() {
    echo -e "${YELLOW}⚠${NC} $1"
    echo -e "  ${YELLOW}→${NC} $2"
    ((WARNINGS++))
}

# Función para imprimir sección
print_section() {
    echo ""
    echo -e "${BLUE}═══ $1 ═══${NC}"
}

# ============================================================================
# 1. VERIFICAR .NET SDK
# ============================================================================

print_section "1. .NET SDK"

if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    DOTNET_MAJOR=$(echo $DOTNET_VERSION | cut -d. -f1)
    
    if [ "$DOTNET_MAJOR" -ge 10 ]; then
        print_ok ".NET SDK $DOTNET_VERSION instalado"
    elif [ "$DOTNET_MAJOR" -eq 8 ] || [ "$DOTNET_MAJOR" -eq 9 ]; then
        print_warning ".NET SDK $DOTNET_VERSION instalado" "Se recomienda .NET 10 para el workshop"
    else
        print_fail ".NET SDK $DOTNET_VERSION instalado" "Se requiere .NET 10. Descargar: https://dot.net/download"
    fi
else
    print_fail ".NET SDK no encontrado" "Instalar desde: https://dot.net/download"
fi

# ============================================================================
# 2. VERIFICAR VISUAL STUDIO CODE
# ============================================================================

print_section "2. Visual Studio Code"

if command -v code &> /dev/null; then
    CODE_VERSION=$(code --version | head -n 1)
    print_ok "VS Code $CODE_VERSION instalado"
else
    print_warning "VS Code no encontrado en PATH" "Si está instalado, agregarlo al PATH. Si no, descargar: https://code.visualstudio.com"
fi

# ============================================================================
# 3. VERIFICAR GIT
# ============================================================================

print_section "3. Git"

if command -v git &> /dev/null; then
    GIT_VERSION=$(git --version | awk '{print $3}')
    print_ok "Git $GIT_VERSION instalado"
    
    # Verificar configuración básica de Git
    if git config --global user.name &> /dev/null && git config --global user.email &> /dev/null; then
        print_ok "Git configurado con nombre y email"
    else
        print_warning "Git no tiene nombre/email configurado" "Ejecutar: git config --global user.name 'Tu Nombre' && git config --global user.email 'tu@email.com'"
    fi
else
    print_fail "Git no encontrado" "Instalar desde: https://git-scm.com/downloads"
fi

# ============================================================================
# 4. VERIFICAR AZURE CLI (OPCIONAL)
# ============================================================================

print_section "4. Azure CLI (Opcional)"

if command -v az &> /dev/null; then
    AZ_VERSION=$(az version --output tsv 2>/dev/null | grep "azure-cli" | awk '{print $2}')
    print_ok "Azure CLI $AZ_VERSION instalado"
    
    # Verificar si está autenticado
    if az account show &> /dev/null; then
        ACCOUNT_NAME=$(az account show --query "name" -o tsv 2>/dev/null)
        print_ok "Autenticado en Azure: $ACCOUNT_NAME"
    else
        print_warning "Azure CLI no autenticado" "Ejecutar: az login"
    fi
else
    print_warning "Azure CLI no encontrado" "Opcional pero útil. Instalar: https://learn.microsoft.com/cli/azure/install-azure-cli"
fi

# ============================================================================
# 5. VERIFICAR CONECTIVIDAD A AZURE OPENAI
# ============================================================================

print_section "5. Conectividad a Azure OpenAI"

# Verificar si hay configuración de Azure OpenAI en variables de entorno o user secrets
OPENAI_ENDPOINT=${AZURE_OPENAI_ENDPOINT:-""}
OPENAI_KEY=${AZURE_OPENAI_KEY:-""}

if [ -z "$OPENAI_ENDPOINT" ] || [ -z "$OPENAI_KEY" ]; then
    print_warning "Credenciales de Azure OpenAI no encontradas en variables de entorno" "Configurar AZURE_OPENAI_ENDPOINT y AZURE_OPENAI_KEY, o usar dotnet user-secrets"
else
    # Intentar hacer una request simple a Azure OpenAI
    echo -n "  Probando conectividad a $OPENAI_ENDPOINT... "
    
    RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" \
        -H "api-key: $OPENAI_KEY" \
        "$OPENAI_ENDPOINT/openai/deployments?api-version=2024-10-01-preview" \
        --connect-timeout 10 \
        --max-time 15 2>/dev/null)
    
    if [ "$RESPONSE" = "200" ]; then
        echo ""
        print_ok "Conexión exitosa a Azure OpenAI"
    elif [ "$RESPONSE" = "401" ]; then
        echo ""
        print_fail "Azure OpenAI responde con 401 Unauthorized" "Verificar API key"
    elif [ "$RESPONSE" = "000" ]; then
        echo ""
        print_fail "No se puede conectar a Azure OpenAI" "Verificar endpoint y conectividad de red"
    else
        echo ""
        print_warning "Azure OpenAI responde con código HTTP $RESPONSE" "Verificar configuración"
    fi
fi

# ============================================================================
# 6. VERIFICAR PAQUETES NUGET REQUERIDOS (si hay un proyecto)
# ============================================================================

print_section "6. Acceso a NuGet"

# Intentar hacer request a nuget.org
echo -n "  Probando conectividad a NuGet.org... "
NUGET_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" "https://api.nuget.org/v3/index.json" --connect-timeout 5 --max-time 10 2>/dev/null)

if [ "$NUGET_RESPONSE" = "200" ]; then
    echo ""
    print_ok "Conexión exitosa a NuGet.org"
else
    echo ""
    print_warning "No se puede conectar a NuGet.org" "Verificar firewall/proxy. Código HTTP: $NUGET_RESPONSE"
fi

# ============================================================================
# 7. VERIFICAR ESPACIO EN DISCO
# ============================================================================

print_section "7. Espacio en Disco"

if [ "$(uname)" == "Darwin" ] || [ "$(uname)" == "Linux" ]; then
    # macOS o Linux
    AVAILABLE_SPACE=$(df -h . | awk 'NR==2 {print $4}')
    AVAILABLE_GB=$(df -k . | awk 'NR==2 {print int($4/1024/1024)}')
    
    if [ "$AVAILABLE_GB" -ge 5 ]; then
        print_ok "Espacio disponible: $AVAILABLE_SPACE"
    elif [ "$AVAILABLE_GB" -ge 2 ]; then
        print_warning "Espacio disponible: $AVAILABLE_SPACE" "Se recomienda al menos 5 GB libres"
    else
        print_fail "Espacio disponible: $AVAILABLE_SPACE" "Se requieren al menos 2 GB libres. Liberar espacio."
    fi
else
    # Windows (si se ejecuta desde Git Bash o WSL)
    print_warning "Verificación de espacio en disco no implementada para Windows" "Verificar manualmente que tienes al menos 5 GB libres"
fi

# ============================================================================
# 8. VERIFICAR PUERTO 5000 DISPONIBLE (para ASP.NET)
# ============================================================================

print_section "8. Puertos Requeridos"

# Verificar si puerto 5000 está en uso
if command -v lsof &> /dev/null; then
    if lsof -Pi :5000 -sTCP:LISTEN -t >/dev/null 2>&1; then
        print_warning "Puerto 5000 en uso" "Módulo 5 (ASP.NET) puede usar puerto alternativo automáticamente"
    else
        print_ok "Puerto 5000 disponible"
    fi
elif command -v netstat &> /dev/null; then
    if netstat -an | grep ":5000 " | grep "LISTEN" >/dev/null 2>&1; then
        print_warning "Puerto 5000 en uso" "Módulo 5 (ASP.NET) puede usar puerto alternativo automáticamente"
    else
        print_ok "Puerto 5000 disponible"
    fi
else
    print_warning "No se puede verificar disponibilidad de puerto 5000" "Herramienta lsof/netstat no encontrada"
fi

# ============================================================================
# 9. VERIFICAR REPOSITORIO CLONADO (si aplica)
# ============================================================================

print_section "9. Repositorio del Workshop"

if [ -d ".git" ]; then
    REPO_URL=$(git config --get remote.origin.url 2>/dev/null)
    if [ ! -z "$REPO_URL" ]; then
        print_ok "Repositorio clonado desde: $REPO_URL"
        
        # Verificar si hay cambios pendientes
        if git diff --quiet 2>/dev/null; then
            print_ok "Repositorio limpio (sin cambios locales)"
        else
            print_warning "Hay cambios locales en el repositorio" "Considera hacer commit o stash antes del workshop"
        fi
    else
        print_warning "Directorio .git encontrado pero sin remote origin" "Esto puede ser normal si no has clonado aún"
    fi
else
    print_warning "No se detectó repositorio Git en este directorio" "Clonar repositorio con: git clone <url>"
fi

# ============================================================================
# RESUMEN
# ============================================================================

echo ""
echo "╔═══════════════════════════════════════════════════════════╗"
echo "║  Resumen de Verificación                                   ║"
echo "╚═══════════════════════════════════════════════════════════╝"
echo ""
echo -e "${GREEN}Verificaciones exitosas:${NC} $PASSED"
echo -e "${YELLOW}Advertencias:${NC}            $WARNINGS"
echo -e "${RED}Verificaciones fallidas:${NC} $FAILED"
echo ""

# Determinar estado general
if [ $FAILED -eq 0 ] && [ $WARNINGS -eq 0 ]; then
    echo -e "${GREEN}✓ ¡Ambiente completamente configurado!${NC}"
    echo -e "  Estás listo para el workshop."
    exit 0
elif [ $FAILED -eq 0 ]; then
    echo -e "${YELLOW}⚠ Ambiente mayormente configurado${NC}"
    echo -e "  Revisa las advertencias arriba. Puedes continuar pero algunas funcionalidades"
    echo -e "  pueden estar limitadas."
    exit 0
elif [ $FAILED -le 2 ]; then
    echo -e "${YELLOW}⚠ Ambiente parcialmente configurado${NC}"
    echo -e "  Resuelve los errores marcados en rojo antes del workshop."
    exit 1
else
    echo -e "${RED}✗ Ambiente no configurado correctamente${NC}"
    echo -e "  Por favor resuelve todos los errores antes del workshop."
    echo -e "  Si necesitas ayuda, contacta al instructor."
    exit 1
fi
