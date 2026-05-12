using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using astractech_backend.DTOs; // Pastikan namespace DTO benar

namespace astratech_apps_backend.Controllers 
{
    [Route("api/[controller]")]
    [ApiController]
    public class FailureDiagnosisController : ControllerBase
    {
        private readonly string _connectionString;

        public FailureDiagnosisController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("LocalEquipmentDb") ?? "";
        }

        // --- Endpoint untuk Ambil Riwayat ---
        [HttpGet("history/{nim}")]
        public IActionResult GetHistory(string nim)
        {
            List<FailureHistoryResponse> historyList = new List<FailureHistoryResponse>();
            try 
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_GetDiagnosisHistory", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@user_nim", nim);
                    
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            historyList.Add(new FailureHistoryResponse {
                                FailureCode = reader["failure_code"].ToString() ?? "",
                                DiagnosisTitle = reader["diagnosis_title"].ToString() ?? "",
                                DateDisplay = reader["date_display"].ToString() ?? "",
                                TotalSteps = Convert.ToInt32(reader["total_steps"]),
                                StepsDisplay = reader["steps_display"].ToString() ?? "",
                                SolutionText = reader["solution_text"] == DBNull.Value ? null : reader["solution_text"].ToString(),
                                Notes = reader["notes"].ToString(),
                                MechanicName = reader["mechanic_name"].ToString() ?? "",
                                MechanicNim = reader["mechanic_nim"].ToString() ?? "",
                                SessionId = reader["session_id"].ToString() ?? ""
                            });
                        }
                    }
                }
                return Ok(historyList);
            }
            catch (Exception ex) {
                return BadRequest(new { message = ex.Message });
            }
        }

        // --- Endpoint untuk Simpan Riwayat (Dipanggil pas beres diagnosa) ---
        [HttpPost("history/save")]
        public IActionResult SaveHistory([FromBody] FailureSaveHistoryRequest req)
        {
            try {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_SaveDiagnosisHistory", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@failure_code", req.FailureCode);
                    cmd.Parameters.AddWithValue("@user_nim", req.UserNim);
                    cmd.Parameters.AddWithValue("@diagnosis_title", req.DiagnosisTitle ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@total_steps", req.TotalSteps);
                    cmd.Parameters.AddWithValue("@solution_text", req.SolutionText ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@notes", req.Notes ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@session_id", req.SessionId ?? (object)DBNull.Value);

                    conn.Open();
                    var newId = cmd.ExecuteScalar();
                    return Ok(new { message = "Success", session_id = newId });
                }
            }
            catch (Exception ex) {
                return BadRequest(new { message = ex.Message });
            }
        }

        // --- Endpoint untuk Hapus Riwayat ---
        [HttpDelete("history/{sessionId}")]
        public IActionResult DeleteHistory(string sessionId)
        {
            try {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_DeleteDiagnosisHistory", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@session_id", sessionId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                return Ok(new { message = "Data berhasil dihapus" });
            }
            catch (Exception ex) {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Kode GetDiagnostics yang lama tetap di bawah sini...
    }
}