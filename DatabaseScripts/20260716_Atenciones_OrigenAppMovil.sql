USE [help_prv];
GO

ALTER FUNCTION [dbo].[help_prv_F_Lista_Mi_Atencion_FICOHSA_APP_Ajustadores]
(
    @IdAtencion INT
)
RETURNS TABLE
AS
RETURN
(
    SELECT A.[Id]
        ,A.[Poliza]
        ,A.[Plan]
        ,A.[Certificado]
        ,A.[RefIdentificacionTipo]
        ,A.[Identificacion]
        ,A.[TelefonoOrigen]
        ,A.[esAppMovil]
        ,A.[OrigenAtencion]
        ,A.[Cliente]
        ,A.[RefTipoServicioId]
        ,A.[FechaInicio]
        ,A.[FechaFin]
        ,A.[RefAgenteId]
        ,A.[RefProveedorId]
        ,A.[RefClienteId]
        ,A.[RefEstadoId]
        ,A.[RefCiudadId]
        ,A.[RefColoniaId]
        ,A.[FechaPagoCliente]
        ,A.[IdPoliza]
        ,A.[Direccion]
        ,A.[Observaciones]
        ,A.[LatitudCliente]
        ,A.[LongitudCliente]
        ,A.[RefGestorId]
        ,S.[TiempoDeEsperaMaximo] TiempoMaximoEspera
        ,E.Id IdEstadoAtencion
        ,ISNULL(E.EstadoAtencion, '') LbEstado
        ,ISNULL(E.Color, '') ColorEstado
        ,[TipoAtencion] LbTipoServicio
        ,[RefCategoriaServiciosId] IdCategoriaServicio
        ,CAT.Id IdCategoria
        ,ISNULL(CAT.NombreCategoria, '') LbCategoriaServicio
        ,CAT.[IconClassName]
        ,U.Nombre UsuarioRegistroNombre
        ,ISNULL(A.NombreConvenio,'N/D') LbConvenio
        ,ISNULL(A.[Plan],'N/D') LbPlan
        ,ISNULL((SELECT pr.NombreProveedor FROM help..help_Proveedor pr WHERE pr.Id = ATS.[RefProveedorId]),'N/D') NombreProveedor
        ,Pai.Pais
        ,A.RefPaisId
        ,A.Zona
        ,A.DireccionDestino
        ,EAS.Estado EstadoServicio
        ,ISNULL((SELECT Ciudad FROM sophia..Ciudad C WHERE C.Id = A.RefCiudadId), 'N/D') Ciudad
        ,ISNULL((SELECT Colonia FROM sophia..Colonia Col WHERE Col.Id = A.RefColoniaId), 'N/D') Colonia
    FROM [help].[dbo].[help_Atenciones] A WITH(NOLOCK)
    INNER JOIN [Sophia].[dbo].[Usuario] U WITH(NOLOCK) ON U.Id = A.RefGestorId
    INNER JOIN [help].[dbo].[help_Servicios] S WITH(NOLOCK) ON S.Id = RefTipoServicioId
    INNER JOIN [help].[dbo].[help_RefServiciosCategoria] CAT WITH(NOLOCK) ON CAT.Id = S.RefCategoriaServiciosId
    INNER JOIN [Sophia].[dbo].[Pais] Pai WITH(NOLOCK) ON Pai.Id = A.RefPaisId
    INNER JOIN [help].[dbo].[help_RefEstadoAtencion] E WITH(NOLOCK) ON E.id = A.RefEstadoId
    INNER JOIN [help].[dbo].[help_Atenciones_Servicios] ATS WITH(NOLOCK) ON ATS.RefAtencionId = A.Id
    INNER JOIN [help].[dbo].[help_RefEstadoAtencionServicio] EAS WITH(NOLOCK) ON EAS.Id = ATS.RefIdEstado
    WHERE A.Id = @IdAtencion
);
GO

ALTER PROCEDURE [dbo].[help_prv_P_Api_Datos_Expediente_Siniestro]
    @IdAtencion INT
AS
BEGIN
    SELECT TOP 1 A.Id
        ,SA.Id ServicioAtencionId
        ,A.Cliente
        ,A.Direccion
        ,A.DireccionDestino
        ,Pai.Id IdPais
        ,Pai.Pais
        ,C.Id IdCiudad
        ,C.RefDepartamentoId
        ,C.Ciudad
        ,ISNULL(Co.Colonia,'NO DEFINIDA') Colonia
        ,A.NombreAdicional
        ,A.NombreCorredor
        ,A.RefPlanId
        ,A.PolizaExterna
        ,A.NombreConvenio
        ,A.[Plan]
        ,A.Zona
        ,A.Taller
        ,A.TelefonoAdicional
        ,A.TelefonoOrigen
        ,A.esAppMovil
        ,A.OrigenAtencion
        ,A.TelefonoReportante
        ,A.Observaciones
        ,A.RefGestorId
        ,U.Nombre
        ,SA.RefProveedorId
        ,P.NombreProveedor
        ,PA.NombreAgente
        ,PA.TypoAgenteString TipoAgente
        ,SA.FechaRegistro
        ,S.TipoAtencion Servicio
        ,SC.NombreCategoria
        ,'' IdAjusteAudiencia
        ,'' AgendarAudiencia
        ,'' AseguradoUsoPoliza
        ,'' Blindado
        ,'' ColorVehiculo
        ,'' ConductorAfiliado
        ,'' ConductorDetenido
        ,'' DanioFrontal
        ,'' DanioLataralIzquierdo
        ,'' DanioLateralDerecho
        ,'' DanioTrasero
        ,'' VersionAsegurado
        ,'' DescripcionAudiencia
        ,'' AnalisisAsesorEmergencia
        ,'' DescripcionTercerosHeridos
        ,'' DescripcionTercerosMuertos
        ,'' [DañosVehiculo]
        ,'' FechaHora
        ,'' FechaRegistroAjuste
        ,'' IdentidaConductor
        ,'' LesionadosSinAudiencia
        ,'' LugarAudiencia
        ,'' MarcaVehiculo
        ,'' ModeloVehiculo
        ,'' ChasisVehiculo
        ,'' PlacaVehiculo
        ,'' AnioVehiculo
        ,'' NombreAsegurado
        ,'' NombreConductor
        ,ISNULL(A.Poliza,A.PolizaExterna) AS Poliza
        ,'' ObservacionTaller
        ,'' RefProveedorAgenteAbogadoId
        ,'' RefProveedorAgenteId
        ,'' TerceroResponsable
        ,'' TercerosMuertos
        ,'' VehiculoDetenido
        ,'' VehiculoVolcado
        ,'' IdAjustador
        ,AU.[Year]
        ,ISNULL(AU.Modelo,'FALTA ASIGNACION') Modelo
        ,ISNULL(AU.NumeroPlaca,'FALTA ASIGNACION') NumeroPlaca
        ,ISNULL(AU.Color,'FALTA ASIGNACION') Color
        ,ISNULL(AU.Chasis,'FALTA ASIGNACION') Chasis
        ,ISNULL(AU.Motor,'FALTA ASIGNACION') Motor
        ,ISNULL(AU.Marca,'FALTA ASIGNACION') Marca
        ,ISNULL(AU.Tipo_Vehiculo,'FALTA ASIGNACION') Tipo_Vehiculo
        ,AU.Ramo
        ,AU.Seccion1
        ,AU.Seccion2
        ,AU.Seccion3
        ,A.Certificado
        ,A.TipoSiniestro
        ,A.TipoVehiculo
        ,A.Parentesco
        ,A.Deducible
        ,A.ValorReserva
        ,A.Moneda
        ,A.LatitudCliente
        ,A.LongitudCliente
    FROM help..help_Atenciones A WITH(NOLOCK)
    INNER JOIN help..help_Atenciones_Servicios SA WITH(NOLOCK) ON A.Id = SA.RefAtencionId
    INNER JOIN help..help_Servicios S WITH(NOLOCK) ON SA.RefTipoServicioId = S.Id
    INNER JOIN help..help_RefServiciosCategoria SC WITH(NOLOCK) ON S.RefCategoriaServiciosId = SC.Id
    INNER JOIN help..help_ProveedorAgentes PA WITH(NOLOCK) ON A.RefProveedorId = PA.Id
    INNER JOIN help..help_Proveedor P WITH(NOLOCK) ON PA.RefProveedorId = P.Id
    INNER JOIN Sophia..Usuario U WITH(NOLOCK) ON SA.RefAgenteId = U.Id
    INNER JOIN Sophia..Pais Pai WITH(NOLOCK) ON A.RefPaisId = Pai.Id
    INNER JOIN Sophia..Ciudad C WITH(NOLOCK) ON A.RefCiudadId = C.Id
    LEFT JOIN Sophia..Colonia Co WITH(NOLOCK) ON A.RefColoniaId = Co.Id
    LEFT JOIN help..help_Atenciones_Automovil ATA WITH(NOLOCK) ON A.Id = ATA.RefAtencionId
    LEFT JOIN help..help_Automovil AU WITH(NOLOCK) ON ATA.RefAutomovilId = AU.Id
    WHERE A.Id = @IdAtencion;
END
GO
