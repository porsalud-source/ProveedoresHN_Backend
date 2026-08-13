using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using WSRestApiProveedoresHelp.Models;

namespace WSRestApiProveedoresHelp.Controllers
{

    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [Authorize]
    [RoutePrefix("api/SeleccionMultiple")]
    public class SeleccionMultipleController : ApiController
    {

        private help_prvEntities db = new help_prvEntities();

        public List<object> Data = new List<object>();

        [HttpGet]
        [Route("TiposDeSiniestroFicohsa")]
        public IHttpActionResult TiposDeSiniestroFicohsa(int IdPais)
        {
            try
            {
                var lista = db.help_prv_F_Listado_Siniestros_Ficohsa(IdPais).ToList();
                return Ok(lista);
            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }

        }

        [HttpGet]
        [Route("TiposDeVehiculosFicohsa")]
        public IHttpActionResult TiposDeVehiculosFicohsa(int IdPais)
        {
            try
            {
                var lista = db.help_prv_F_Listado_Tipo_Vehiculo_Ficohsa(IdPais).ToList();
                return Ok(lista);
            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }

        }

        [HttpGet]
        [Route("TiposDeParentescoFicohsa")]
        public IHttpActionResult TiposDeParentescoFicohsa(int IdPais)
        {
            try
            {
                var lista = db.help_prv_F_Listado_Parentesco_Ficohsa(IdPais).ToList();
                return Ok(lista);
            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }

        }

        [HttpGet]
        [Route("TipoDeConductor")]
        public IHttpActionResult TipoDeConductor()
        {
            try
            {

                var lista = db.help_prv_F_Listado_TipoConductor().ToList();
                lista.ForEach(item => {
                    Data.Add(new
                    {
                        Id = item.Id,
                        TipoConductor = item.TipoConductor
                    });
                });
                return Ok(Data);
            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }

        [HttpGet]
        [Route("TipoDeGenero")]
        public IHttpActionResult TipoDeGenero()
        {

            try
            {
                var lista = db.help_prv_F_Listado_Genero().ToList();

                lista.ForEach(item => {
                    Data.Add(new
                    {
                        Id = item.Id,
                        Genero = item.Genero
                    });
                });
                return Ok(Data);
            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }

        [HttpGet]
        [Route("TipoDeAudiencia")]
        public IHttpActionResult TipoDeAudiencia()
        {
            try
            {
                var lista = db.help_prv_F_Listado_TipoAudiencia().ToList();

                lista.ForEach(item => {
                    Data.Add(new
                    {
                        Id = item.Id,
                        TipoAudiencia = item.TipoAudiencia
                    });
                });
                return Ok(Data);
            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }

        [HttpGet]
        [Route("TipoDeCombustible")]
        public IHttpActionResult TipoDeCombustible()
        {
            try
            {
                var lista = db.help_prv_F_Listado_TipoCombustible().ToList();

                lista.ForEach(item => {
                    Data.Add(new
                    {
                        Id = item.Id,
                        TipoCombustible = item.TipoCombustible
                    });
                });
                return Ok(Data);
            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }

        [HttpGet]
        [Route("TipoDeFotografia")]
        public IHttpActionResult TipoDeFotografia()
        {
            try
            {
                var lista = db.help_prv_F_Listado_TipoFotografia(1).ToList();

                lista.ForEach(item => {
                    Data.Add(new
                    {
                        Id = item.Id,
                        TipoFotografia = item.TipoFotografia
                    });
                });
                return Ok(Data);
            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }

        [HttpGet]
        [Route("TipoDeFirma")]
        public IHttpActionResult TipoDeFirma()
        {
            try
            {
                var lista = db.help_prv_F_Listado_TipoFotografia(2).ToList();//db.help_TipoFotografia.Where(x => x.Tipo == 2).ToList();

                lista.ForEach(item => {
                    Data.Add(new
                    {
                        Id = item.Id,
                        TipoFirma = item.TipoFotografia
                    });
                });
                return Ok(Data);
            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }

        [HttpGet]
        [Route("TipoDeLicencia")]
        public IHttpActionResult TipoDeLicencia(int IdPais)
        {
            try
            {
                var lista = db.help_prv_F_Listado_TipoLicencia(IdPais).ToList(); //help_TipoLicencia.Where(x => x.RefPaisId == IdPais).ToList();

                lista.ForEach(item => {
                    Data.Add(new
                    {
                        Id = item.Id,
                        TipoLicencia = item.TipoLicencia
                    });
                });
                return Ok(Data);
            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }  

        [HttpGet]
        [Route("TipoDePersonaSiniestro")]
        public IHttpActionResult TipoDePersonaSiniestro()
        {
            try
            {
                var lista = db.help_prv_F_Listado_TipoPersonaSiniestro().ToList();

                lista.ForEach(item => {
                    Data.Add(new
                    {
                        Id = item.Id,
                        TipoPersonaSiniestro = item.TipoPersonaSiniestro
                    });
                });
                return Ok(Data);
            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }

        [HttpGet]
        [Route("TipoDeSolicitanteInformeAjuste")]
        public IHttpActionResult TipoDeSolicitanteInformeAjuste()
        {
            try
            {
                var lista = db.help_prv_F_Listado_TipoSolicitanteInformeAjuste().ToList();

                lista.ForEach(item => {
                    Data.Add(new
                    {
                        Id = item.Id,
                        TipoSolicitante = item.TipoSolicitante
                    });
                });
                return Ok(Data);
            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }

        [HttpGet]
        [Route("TipoDeEntidadComunicativa")]
        public IHttpActionResult TipoDeEntidadComunicativa()
        {
            try
            {
                var lista = db.help_prv_F_Listado_EntidadComunicativa().ToList();

                lista.ForEach(item => {
                    Data.Add(new
                    {
                        Id = item.Id,
                        Entidad = item.Entidad
                    });
                });
                return Ok(Data);
            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }


        [HttpGet]
        [Route("ListadoTalleresFicohsa")]
        public IHttpActionResult ListadoTalleresFicohsa()
        {
            try
            {
                var lista = db.help_prv_F_Listado_TallerMecanico().ToList();

                lista.ForEach(item => {
                    Data.Add(new
                    {
                        Id = item.Id,
                        NombreTaller = item.TallerNombre,
                        Categoria = item.Categoria
                    });
                });
                return Ok(Data);
            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }

        [HttpGet]
        [Route("ListadoDanioAlVehiculoFicohsa")]
        public IHttpActionResult ListadoDanioAlVehiculoFicohsa()
        {
            try
            {
                var lista = db.help_prv_F_Listado_Danio_Vehiculo_Ficohsa().ToList();

                lista.ForEach(item => {
                    Data.Add(new
                    {
                        Id = item.Id,
                        Codigo = item.CODIGO,
                        Descripcion = item.DESCRIPCION
                    });
                });
                return Ok(Data);
            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }


    }
}