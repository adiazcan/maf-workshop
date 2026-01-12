# Email de Preparación Pre-Workshop

**Para**: Participantes del Workshop Microsoft Agent Framework  
**De**: [Instructor]  
**Asunto**: 🚀 Preparación para el Workshop de Microsoft Agent Framework - Acción Requerida

---

Hola [Nombre],

¡Gracias por inscribirte al **Workshop de Introducción a Microsoft Agent Framework**! Para aprovechar al máximo nuestra sesión práctica, necesitarás preparar tu ambiente de desarrollo **antes del día del workshop**.

**Fecha del workshop**: [Fecha]  
**Hora**: [Hora de inicio] - [Hora de fin]  
**Ubicación**: [Lugar o enlace virtual]

---

## ✅ Tareas de Preparación (Completar antes del workshop)

### 1. Instalar Software Requerido

#### .NET 10 SDK
- **Descargar**: https://dot.net/download
- **Versión mínima**: 10.0.0
- **Verificar instalación**:
  ```bash
  dotnet --version
  ```
  Debe mostrar `10.0.x`

#### Visual Studio Code
- **Descargar**: https://code.visualstudio.com
- **Extensiones recomendadas** (opcional):
  - C# Dev Kit
  - C# (Microsoft)

#### Git
- **Descargar**: https://git-scm.com/downloads
- **Verificar instalación**:
  ```bash
  git --version
  ```

---

### 2. Clonar Repositorio del Workshop

```bash
git clone https://github.com/[org]/maf-workshop.git
cd maf-workshop
```

---

### 3. Configurar Azure OpenAI (IMPORTANTE)

Para poder ejecutar los labs, necesitarás acceso a Azure OpenAI Service:

#### Opción A: Usar Azure OpenAI provisto por el workshop

Si el workshop proporciona credenciales compartidas, te las enviaremos **24 horas antes** del evento.

#### Opción B: Usar tu propia suscripción de Azure

Si tienes una suscripción de Azure:

1. **Crear recurso de Azure OpenAI**:
   - Portal: https://portal.azure.com
   - Región recomendada: **East US 2** o **Sweden Central**

2. **Desplegar modelo gpt-5.2**:
   - Desde el recurso, ir a "Model deployments"
   - Desplegar modelo **gpt-5.2** (deployment name: `gpt-5.2`)

3. **Obtener credenciales**:
   - **Endpoint**: `https://[tu-recurso].openai.azure.com/`
   - **API Key**: Desde "Keys and Endpoint"

4. **Configurar en tu máquina** (usando user secrets):
   ```bash
   cd maf-workshop/docs/modulo-01-fundamentos/labs/01-hello-agent
   dotnet user-secrets set "AzureOpenAI:Endpoint" "https://[tu-recurso].openai.azure.com/"
   dotnet user-secrets set "AzureOpenAI:ApiKey" "[tu-api-key]"
   dotnet user-secrets set "AzureOpenAI:DeploymentName" "gpt-5.2"
   ```

**Nota**: Si no tienes suscripción de Azure, contáctanos para obtener acceso temporal.

---

### 4. Verificar tu Ambiente

Ejecuta nuestro script de verificación automática:

```bash
cd maf-workshop
chmod +x scripts/verify-environment.sh
./scripts/verify-environment.sh
```

**Output esperado**:
```
✓ .NET SDK 10.0.x instalado
✓ VS Code instalado
✓ Git instalado
✓ Conexión exitosa a Azure OpenAI
✓ Ambiente completamente configurado!
```

**Si ves errores**:
- Revisa la sección de troubleshooting más abajo
- Contáctanos con el output del script

---

## 📋 Checklist de Verificación

Por favor confirma que has completado todos estos pasos:

- [ ] .NET 10 SDK instalado y verificado
- [ ] Visual Studio Code instalado
- [ ] Git instalado y configurado
- [ ] Repositorio `maf-workshop` clonado
- [ ] Credenciales de Azure OpenAI configuradas
- [ ] Script de verificación ejecutado sin errores críticos

---

## 🚨 Troubleshooting Común

### "dotnet: command not found"

**Solución**:
1. Verificar que .NET 10 SDK se instaló correctamente
2. Reiniciar terminal/consola
3. En macOS/Linux: verificar PATH con `echo $PATH`
4. En Windows: reiniciar VS Code

### "401 Unauthorized" al probar Azure OpenAI

**Solución**:
1. Verificar que API key es correcta (sin espacios extra)
2. Verificar que endpoint termina en `openai.azure.com/`
3. Verificar que recurso de Azure OpenAI está activo en portal.azure.com

### "git clone" falla con error de red

**Solución**:
1. Verificar conexión a internet
2. Si estás detrás de firewall corporativo, contacta a IT
3. Como alternativa, descargar ZIP del repositorio

### Script de verificación muestra warnings

**Solución**:
- Warnings (⚠) son aceptables - puedes continuar
- Errores (✗) deben ser resueltos antes del workshop

---

## 💡 Recomendaciones Adicionales

### Antes del Workshop

- **Actualizar VS Code** a la última versión
- **Probar ejecutar** el primer lab (Módulo 1, Hello Agent)
- **Descargar packages de NuGet** con anticipación:
  ```bash
  cd docs/modulo-01-fundamentos/labs/01-hello-agent
  dotnet restore
  ```
- **Cargar tu laptop** completamente

### Durante el Workshop

- **Trae cable de corriente** (no dependas solo de batería)
- **Auriculares** (si es virtual o híbrido)
- **Segunda pantalla** (opcional pero útil)

---

## 📚 Pre-Lectura Opcional

Si quieres adelantarte, aquí hay recursos útiles:

- [¿Qué es Microsoft Agent Framework?](https://learn.microsoft.com/microsoft-agent-framework)
- [Azure OpenAI Service Quickstart](https://learn.microsoft.com/azure/ai-services/openai/quickstart)
- [Introducción a Agentes de IA](https://learn.microsoft.com/ai/agents)

**Nota**: No es necesario leer estos recursos antes del workshop - cubriremos todo desde cero.

---

## 🆘 ¿Necesitas Ayuda?

Si tienes problemas con la configuración:

- **Email**: [email del instructor]
- **Slack/Teams**: [canal de soporte]
- **Horario de consultas**: [horario disponible antes del workshop]

**No esperes hasta el día del workshop para resolver problemas técnicos** - contáctanos con anticipación.

---

## 📅 ¿Qué Aprenderás?

En este workshop de 7 horas, construirás:

✅ Tu primer agente conversacional  
✅ Agentes con function tools personalizadas  
✅ Workflows multi-agente (secuencial, paralelo, group chat)  
✅ Integración con observabilidad (OpenTelemetry + Azure Monitor)  
✅ Aplicación web multi-agente con ASP.NET y .NET Aspire  
✅ Debugging con DevUI  
✅ Introducción a Model Context Protocol (MCP)

---

## 🎯 Agenda del Workshop

| Hora | Actividad |
|------|-----------|
| 09:00-09:30 | Bienvenida + Setup Check |
| 09:30-10:30 | Módulo 1: Fundamentos |
| 10:30-10:45 | BREAK |
| 10:45-12:00 | Módulo 2: Function Tools |
| 12:00-13:00 | LUNCH |
| 13:00-14:30 | Módulo 3: Workflows |
| 14:30-14:45 | BREAK |
| 14:45-15:45 | Módulo 4: Observabilidad |
| 15:45-16:00 | BREAK |
| 16:00-17:00 | Módulo 5: ASP.NET + Aspire |
| 17:00-17:15 | Módulo 6: DevUI |
| 17:15-17:30 | Módulo 7: MCP + Cierre |

---

## ✅ Confirmación de Preparación

Por favor responde a este email con:

**"✅ Ambiente configurado"** cuando hayas completado todos los pasos de preparación.

Si tienes problemas, responde con:

**"🚨 Necesito ayuda con: [descripción del problema]"**

---

¡Nos vemos en el workshop! 🚀

Saludos,  
[Nombre del Instructor]  
[Título/Organización]

---

**P.D.**: Si conoces a alguien interesado en aprender Microsoft Agent Framework, comparte esta información. Aún tenemos [X] cupos disponibles.

---

## 📎 Enlaces Rápidos

- Repositorio: https://github.com/[org]/maf-workshop
- Documentación: https://learn.microsoft.com/microsoft-agent-framework
- Portal Azure: https://portal.azure.com
- Descarga .NET: https://dot.net/download
- Descarga VS Code: https://code.visualstudio.com
- Descarga Git: https://git-scm.com/downloads

---

**Este email fue enviado a**: [email del participante]  
**Workshop ID**: [ID único]  
**Fecha de envío**: [fecha]
