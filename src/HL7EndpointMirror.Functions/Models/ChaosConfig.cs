using System.ComponentModel.DataAnnotations;

namespace HL7EndpointMirror.Functions.Models;

public class ChaosConfig
{
    public const string SectionName = "ChaosConfig";
    public bool IsEnabled { get; set; } = false;

    [Range(0, 100, ErrorMessage = "FailureRatePercent must be between 0 and 100.")]
    public int FailureRatePercent { get; set; } = 10;

    [Required]
    public string DefaultErrorType { get; set; } = "500";

    public LatencyConfig LatencySimulation { get; set; } = new();
}
