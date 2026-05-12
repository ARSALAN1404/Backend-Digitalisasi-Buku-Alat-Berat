using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using astratech_apps_backend.DTOs;
using astratech_apps_backend.Helpers;
using System.Data;

namespace astratech_apps_backend.Controllers
{
    [Route("api/failure-auth")]
    [ApiController]
    public class FailureAuthController : ControllerBase
    {
        private readonly JwtHelper _jwtHelper;
        private readonly string _connectionString;

        public FailureAuthController(JwtHelper jwtHelper, IConfiguration configuration)
        {
            _jwtHelper = jwtHelper;
            // Menghilangkan warning CS8601 & CS8618
            _connectionString = configuration.GetConnectionString("LocalEquipmentDb") ?? "";
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] FailureLoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Nim))
                return BadRequest(new { success = false, message = "NIM wajib diisi" });

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = @"SELECT id, nim, nama, role FROM users 
                                   WHERE nim = @nim AND role = @role AND is_active = 1";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@nim", request.Nim.Trim());
                    cmd.Parameters.AddWithValue("@role", request.Role.Trim());

                    await conn.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync()) 
                        {
                            var user = new FailureUserDto
                            {
                                Nim = reader["nim"]?.ToString()?.Trim() ?? string.Empty,
                                Nama = reader["nama"]?.ToString()?.Trim() ?? string.Empty,
                                Role = reader["role"]?.ToString()?.Trim() ?? string.Empty
                            };

                            var token = _jwtHelper.GenerateToken(user.Nim, user.Nama, user.Role);

                            return Ok(new {
                                success = true,
                                message = "Login berhasil",
                                data = new { Token = token, User = user }
                            });
                        }
                    }
                }
                return Unauthorized(new { success = false, message = "NIP/NIM atau Role tidak valid!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Database Error: " + ex.Message });
            }
        }
    }
}