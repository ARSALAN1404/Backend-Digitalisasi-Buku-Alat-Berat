namespace astractech_backend.DTOs
{
    // ... kode yang sudah ada tetap dipertahankan ...

    public class FailureHistoryResponse
    {
        public string FailureCode { get; set; } = string.Empty;
        public string DiagnosisTitle { get; set; } = string.Empty;
        public string DateDisplay { get; set; } = string.Empty;
        public int TotalSteps { get; set; }
        public string StepsDisplay { get; set; } = string.Empty;
        public string? SolutionText { get; set; }
        public string? Notes { get; set; }
        public string MechanicName { get; set; } = string.Empty;
        public string MechanicNim { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
    }

    public class FailureSaveHistoryRequest
    {
        public string FailureCode { get; set; } = string.Empty;
        public string UserNim { get; set; } = string.Empty;
        public string? DiagnosisTitle { get; set; }
        public int TotalSteps { get; set; }
        public string? SolutionText { get; set; }
        public string? Notes { get; set; }
        public string? SessionId { get; set; }
    }
}