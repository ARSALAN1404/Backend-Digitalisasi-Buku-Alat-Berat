using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace astratech_apps_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FailureCodeController : ControllerBase
    {
        private readonly string _connectionString;

        public FailureCodeController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("LocalEquipmentDb") ?? "";
        }

        // Fungsi pembantu untuk cek apakah kolom ada di Reader
        private bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        [HttpGet("{kode}")]
        public IActionResult GetFailureDetail(string kode)
        {
            if (string.IsNullOrEmpty(kode)) 
                return BadRequest(new { message = "Kode tidak boleh kosong" });

            try
            {
                // Membuat Base URL untuk akses gambar di folder wwwroot
                string baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/images/troubleshooting/";

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    // --- TAHAP 1: AMBIL DATA UTAMA ---
                    SqlCommand cmd1 = new SqlCommand("sp_GetFailureCodeByCode", conn);
                    cmd1.CommandType = CommandType.StoredProcedure;
                    cmd1.Parameters.AddWithValue("@code", kode.Trim().ToUpper());

                    object finalResult = null;

                    using (SqlDataReader reader1 = cmd1.ExecuteReader())
                    {
                        if (reader1.Read())
                        {
                            finalResult = new {
                                id = reader1["id"]?.ToString() ?? "",
                                code = reader1["code"]?.ToString()?.Trim() ?? "",
                                user_code = reader1["user_code"]?.ToString()?.Trim() ?? "",
                                description = reader1["description"]?.ToString() ?? "",
                                problem_appears = reader1["problem_appears"]?.ToString() ?? "",
                                action_of_controller = reader1["action_of_controller"]?.ToString() ?? "",
                                component_in_charge = reader1["component_in_charge"]?.ToString() ?? "",
                                category = reader1["category"]?.ToString() ?? "",
                                contents_of_trouble = reader1["contents_of_trouble"]?.ToString() ?? "",
                                related_information = reader1["related_information"]?.ToString() ?? "",
                                causes = new List<object>() // Tempat nampung causes nanti
                            };
                        }
                        else
                        {
                            return NotFound(new { message = "Kode tidak ditemukan." });
                        }
                    }

                    // --- TAHAP 2: AMBIL DATA PENYEBAB (POSSIBLE CAUSES) ---
                    List<object> causesList = new List<object>();
                    SqlCommand cmd2 = new SqlCommand("sp_GetPossibleCauseByCode", conn);
                    cmd2.CommandType = CommandType.StoredProcedure;
                    cmd2.Parameters.AddWithValue("@code", kode.Trim().ToUpper());

                    using (SqlDataReader reader2 = cmd2.ExecuteReader())
                    {
                        while (reader2.Read())
                        {
                            causesList.Add(new {
                                cause_description = reader2["cause_description"]?.ToString() ?? "",
                                check_method = reader2["check_method"]?.ToString() ?? "",
                                standard_condition = reader2["standard_condition"]?.ToString() ?? "",
                                
                                // Cek dulu kolom standard_unit ada atau enggak biar gak crash
                                standard_unit = HasColumn(reader2, "standard_unit") ? (reader2["standard_unit"]?.ToString() ?? "") : "",
                                
                                special_method = reader2["special_method"]?.ToString()?.Trim().ToLower() ?? "tidak",

                                // TAMBAHAN: Ambil data gambar dari SP dan buat URL lengkapnya
                                image_url = (HasColumn(reader2, "image_filename") && reader2["image_filename"] != DBNull.Value) ? 
                                            baseUrl + reader2["image_filename"].ToString() : null,

                                standard_image_url = (HasColumn(reader2, "standard_image_filename") && reader2["standard_image_filename"] != DBNull.Value) ? 
                                                     baseUrl + reader2["standard_image_filename"].ToString() : null
                            });
                        }
                    }

                    // Masukkan list causes ke objek hasil akhir
                    var response = (dynamic)finalResult;
                    
                    return Ok(new {
                        id = response.id,
                        code = response.code,
                        user_code = response.user_code,
                        description = response.description,
                        problem_appears = response.problem_appears,
                        action_of_controller = response.action_of_controller,
                        component_in_charge = response.component_in_charge,
                        category = response.category,
                        contents_of_trouble = response.contents_of_trouble,
                        related_information = response.related_information,
                        causes = causesList 
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}