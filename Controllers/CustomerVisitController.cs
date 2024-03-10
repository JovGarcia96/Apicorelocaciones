using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Apicorelocaciones.Models;
using System.Data;
using System.Data.SqlClient;
using Microsoft.SqlServer.Types;
using System.Data.SqlTypes;
using System.Net;

namespace Apicorelocaciones.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerVisitController : ControllerBase
    {
        private readonly string cadenaSql;

        public CustomerVisitController(IConfiguration config)
        {
            cadenaSql = config.GetConnectionString("CadenaSQL");
        }

        [HttpGet]
        [Route("lista")]
        public IActionResult Lista()
        {
            List<CustomerLocation> lista = new List<CustomerLocation>();
            try
            {
                using (var conexion = new SqlConnection(cadenaSql))
                {
                    conexion.Open();
                    var cmd = new SqlCommand("GetAllLocations", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            lista.Add(new CustomerLocation()
                            {
                                CustomerCode = Convert.ToInt32(rd["CTE_CODIGO_K"]),
                                LocationCode = Convert.ToInt32(rd["CTECAN_CODIGO_K"]),
                                ZoneCode = rd["ZEAM_DIS_CODIGO_K"].ToString(),
                                RouteCode = Convert.ToInt32(rd["ZEAM_RUT_CODIGO_K"]),
                                TypeCode = Convert.ToInt32(rd["RUTTPO_CODIGO_K"]),
                                MapX = rd["MAP_X"].ToString(),
                                MapY = rd["MAP_Y"].ToString(),
                                Frequency = rd["FRECUENCY"].ToString()
                            });
                        }
                    }
                }
                return StatusCode(StatusCodes.Status200OK, new { mensaje = "Ok", response = lista });
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message, response = lista });
            }
        }

        [HttpPost]
        [Route("InsertCustomerLocation")]
        public ActionResult InsertCustomerLocation(int customerCode, int locationCode, string zoneCode, int routeCode, int typeCode, string mapX, string mapY, string frequency, [FromBody] SqlGeography coordinates)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(cadenaSql))
                {
                    using (SqlCommand command = new SqlCommand("InsertCustomerLocation", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CTE_CODIGO_K", customerCode);
                        command.Parameters.AddWithValue("@CTECAN_CODIGO_K", locationCode);
                        command.Parameters.AddWithValue("@ZEAM_DIS_CODIGO_K", zoneCode);
                        command.Parameters.AddWithValue("@ZEAM_RUT_CODIGO_K", routeCode);
                        command.Parameters.AddWithValue("@RUTTPO_CODIGO_K", typeCode);
                        command.Parameters.AddWithValue("@MAP_X", mapX);
                        command.Parameters.AddWithValue("@MAP_Y", mapY);
                        command.Parameters.AddWithValue("@FRECUENCY", frequency);

                        // Añadir el parámetro para COORDINATES, permitiendo valores nulos
                        SqlParameter coordinatesParam = new SqlParameter("@COORDINATES", SqlDbType.Udt);
                        coordinatesParam.UdtTypeName = "GEOGRAPHY";

                        if (coordinates != null)
                        {
                            coordinatesParam.Value = coordinates;
                        }
                        else
                        {
                            coordinatesParam.Value = DBNull.Value;
                        }

                        command.Parameters.Add(coordinatesParam);
                        connection.Open();

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return StatusCode(StatusCodes.Status201Created, "Registro insertado correctamente.");
                        }
                        else
                        {
                            return StatusCode(StatusCodes.Status500InternalServerError, "Error al insertar el registro.");
                        }
                    }
                }
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, error.Message);
            }
        }

        [HttpGet]
        [Route("ObtnerDiasdeVisita")]
        public ActionResult ObtnerDiasdeVisita([FromQuery] string frequency)
        {
            try
            {
                string visitDays = "";
                string nonVisitDays = "";

                using (SqlConnection connection = new SqlConnection(cadenaSql))
                {
                    using (SqlCommand command = new SqlCommand("GetVisitDays", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@FRECUENCY", frequency);

                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                visitDays = reader["VisitDays"].ToString();
                                nonVisitDays = reader["NonVisitDays"].ToString();
                            }
                            else
                            {
                                return StatusCode(StatusCodes.Status404NotFound, "No hay visitas programadas.");
                            }
                        }
                    }
                }

                return StatusCode(StatusCodes.Status200OK, new { VisitDays = visitDays, NonVisitDays = nonVisitDays });
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, error.Message);
            }
        }

        [HttpGet]
        [Route("ShowHistory")]
        public ActionResult ShowHistory()
        {
            try
            {
                List<CustomerLocation> historyList = new List<CustomerLocation>();

                using (SqlConnection connection = new SqlConnection(cadenaSql))
                {
                    using (SqlCommand command = new SqlCommand("ShowHistory", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                historyList.Add(new CustomerLocation()
                                {
                                    CustomerCode = Convert.ToInt32(reader["CTE_CODIGO_K"]),
                                    Frequency = reader["FRECUENCY"].ToString(),
                                    VisitStatus = reader["VisitStatus"].ToString(),
                                    MapX = reader["MAP_X"].ToString(),
                                    MapY = reader["MAP_Y"].ToString(),                                  
                                });
                            }
                        }
                    }
                }

                return StatusCode(StatusCodes.Status200OK, new { mensaje = "Ok", response = historyList });
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message });
            }
        }

        [HttpPut]
        [Route("UpdateCustomerLocation")]
        public IActionResult UpdateCustomerLocation([FromBody] CustomerLocation objeto)
        {
            try
            {
                using (var conexion = new SqlConnection(cadenaSql))
                {
                    conexion.Open();
                    var cmd = new SqlCommand("UpdateCustomerLocation", conexion);
                    cmd.Parameters.AddWithValue("@CTE_CODIGO_K", objeto.CustomerCode == 0 ? DBNull.Value : objeto.CustomerCode);
                    cmd.Parameters.AddWithValue("@CTECAN_CODIGO_K", objeto.LocationCode == 0 ? DBNull.Value : objeto.LocationCode);
                    cmd.Parameters.AddWithValue("@ZEAM_DIS_CODIGO_K", objeto.ZoneCode is null ? DBNull.Value : objeto.ZoneCode);
                    cmd.Parameters.AddWithValue("@ZEAM_RUT_CODIGO_K", objeto.RouteCode == 0 ? DBNull.Value : objeto.RouteCode);
                    cmd.Parameters.AddWithValue("@RUTTPO_CODIGO_K", objeto.TypeCode == 0 ? DBNull.Value : objeto.TypeCode);
                    cmd.Parameters.AddWithValue("@MAP_X", objeto.MapX is null ? DBNull.Value : objeto.MapX);
                    cmd.Parameters.AddWithValue("@MAP_Y", objeto.MapY is null ? DBNull.Value : objeto.MapY);
                    cmd.Parameters.AddWithValue("@FRECUENCY", objeto.Frequency is null ? DBNull.Value : objeto.Frequency);

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.ExecuteNonQuery();
                }

                return StatusCode(StatusCodes.Status200OK, new { mensaje = "Registro actualizado correctamente." });
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message });
            }
        }

        [HttpDelete]
        [Route("DeleteCustomerLocation")]
        public ActionResult DeleteCustomerLocation(int customerCode, int locationCode, string zoneCode, int routeCode, int typeCode)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(cadenaSql))
                {
                    using (SqlCommand command = new SqlCommand("DeleteCustomerLocation", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CTE_CODIGO_K", customerCode);
                        command.Parameters.AddWithValue("@CTECAN_CODIGO_K", locationCode);
                        command.Parameters.AddWithValue("@ZEAM_DIS_CODIGO_K", zoneCode);
                        command.Parameters.AddWithValue("@ZEAM_RUT_CODIGO_K", routeCode);
                        command.Parameters.AddWithValue("@RUTTPO_CODIGO_K", typeCode);

                        connection.Open();

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return StatusCode(StatusCodes.Status200OK, "Registro eliminado correctamente.");
                        }
                        else
                        {
                            return StatusCode(StatusCodes.Status404NotFound, "No se encontró ningún registro para eliminar.");
                        }
                    }
                }
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, error.Message);
            }
        }
    }
}