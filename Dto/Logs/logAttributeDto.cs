public class LogAttributeDTO
{
    public DateTime CreatedDateTime { get; set; }
    public string AppId { get; set; }
    public string AppDisplayName { get; set; }
    public string IpAddress { get; set; }
    public string ClientAppUsed { get; set; }
    public string ConditionalAccessStatus { get; set; }
    public string RiskLevelAggregated { get; set; }
    public string RiskState { get; set; }
    public string ResourceId { get; set; }
    public string ResourceDisplayName { get; set; }

    public DeviceDetailDTO DeviceDetail { get; set; }
    public LocationDTO Location { get; set; }
}

public class DeviceDetailDTO
{
    public string DeviceId { get; set; }
    public string DisplayName { get; set; }
    public string OperatingSystem { get; set; }
    public string Browser { get; set; }
    public bool IsCompliant { get; set; }
    public bool IsManaged { get; set; }
    public string TrustType { get; set; }
}

public class LocationDTO
{
    public string City { get; set; }
    public string State { get; set; }
    public string CountryOrRegion { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? Altitude { get; set; }
}
