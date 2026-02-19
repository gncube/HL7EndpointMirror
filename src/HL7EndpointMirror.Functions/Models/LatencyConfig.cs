using System.ComponentModel.DataAnnotations;

namespace HL7EndpointMirror.Functions.Models;

public class LatencyConfig
{
    public const string SectionName = "LatencySimulation";
    public bool IsEnabled { get; set; } = false;

    [Range(0, 30000, ErrorMessage = "DelayMs must be between 0 and 30000.")]
    public int DelayMs { get; set; } = 0;
}