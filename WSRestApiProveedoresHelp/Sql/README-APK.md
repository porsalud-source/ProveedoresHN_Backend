# Descarga de Proveedores HN

GET o HEAD /api/Proveedor/DescargarProveedoresApk permite descargar la APK sin iniciar sesion.

Colocar la APK publicada en App_Data/Downloads/ProveedoresHN.apk dentro de la aplicacion IIS. El archivo se administra fuera de Git y no se incluye automaticamente en la publicacion del proyecto. Dar permiso de lectura a la identidad del pool de IIS.

Opcionalmente configurar PROVEEDORES_APK_FILE_PATH en appSettings del servidor con una ruta absoluta o virtual. El cliente no puede elegir la ruta. No se requiere modificar Web.config para usar la ubicacion predeterminada.

Respuestas: 200 con application/vnd.android.package-archive; 404 si falta el archivo o esta vacio; 503 si no puede leerse. HEAD devuelve los encabezados sin contenido. La descarga usa streaming y evita cache para servir la version vigente.

Actualizar el enlace de descarga del frontend a la base publica de esta API seguida de /api/Proveedor/DescargarProveedoresApk. Probar con la APK firmada antes de distribuirla.

El primer despliegue GPS requiere ejecutar TrackingPoints.sql en help_prv. Las columnas CapturedAt y ReceivedAt conservan UTC; CapturedAtHonduras y ReceivedAtHonduras son columnas calculadas UTC-6 para consulta.
