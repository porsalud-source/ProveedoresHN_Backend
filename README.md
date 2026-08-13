# ProveedoresHN Backend

API ASP.NET Framework 4.8 para HELP Proveedores.

## Configuracion local

1. Restaurar los paquetes NuGet declarados en `WSRestApiProveedoresHelp/packages.config`.
2. Configurar en `WSRestApiProveedoresHelp/Web.config` los valores marcados como `CONFIGURAR_EN_SERVIDOR`.
3. Configurar las cadenas `help_prvEntities` y `helpEntities` con el servidor SQL autorizado.
4. No confirmar credenciales, archivos de publicacion privados ni copias locales de configuracion.

## Datos sensibles

El repositorio no incluye claves JWT, claves REST de OneSignal, contrasenas SMTP ni credenciales de base de datos. Estos valores deben administrarse fuera de Git y configurarse por ambiente.
