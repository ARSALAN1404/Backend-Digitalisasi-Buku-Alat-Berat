using astratech_apps_backend.Models;
using System.Collections.Generic;

namespace astratech_apps_backend.DTOs
{
    // 1. Tambahkan Role di Request Login
    public class FailureLoginRequest
    {
        public string Nim { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Ditambahkan agar sinkron dengan Controller
    }

    public class FailureLoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public FailureUserDto User { get; set; } = new FailureUserDto();
    }

    // 2. Sesuaikan UserDto: Ganti Kelas menjadi Role
    public class FailureUserDto
    {
        public string Nim { get; set; } = string.Empty;
        public string Nama { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Sebelumnya 'Kelas'
    }

    // 3. Pastikan property ini sesuai dengan kolom di tabel dbo.failure_code kamu
    public class FailureCodeDetailResponse
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? UserCode { get; set; }
        public string? Trouble { get; set; } // Pastikan di DB namanya 'description' atau 'trouble'
        public string? ProblemAppears { get; set; }
        public string? ActionOfController { get; set; }
        public string? ComponentInCharge { get; set; }
        public string? Category { get; set; }
        public string? ReferenceFile { get; set; }
        public string? ReferencePage { get; set; }
        
        // Relasi untuk Troubleshooting
        public List<FailurePossibleCause>? PossibleCauses { get; set; }
        public List<FailureDiagnosticTool>? Tools { get; set; }
    }

    public class FailureRecommendationRequest
    {
        public List<int> CheckedCauses { get; set; } = new List<int>();
    }

    public class FailureRecommendationResponse
    {
        public int CauseNumber { get; set; }
        public string CauseDescription { get; set; } = string.Empty;
        public string? RemedyText { get; set; }
        public string? RemedyDetail { get; set; }
    }

    // 4. DTO untuk menyimpan hasil diagnosa ke database
    public class FailureSaveResultRequest
    {
        public string SessionId { get; set; } = string.Empty;
        public string FailureCode { get; set; } = string.Empty;
        public string MechanicNim { get; set; } = string.Empty;
        public int CauseFound { get; set; }
        public string? Notes { get; set; }
    }
}