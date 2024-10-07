using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntraGraphAPI.Models
{
    public class LogAttribute
    {
        [Key]
        [Column("log_attr_id")]
        public int LogAttrId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("createdDateTime")]
        public DateTime CreatedDateTime { get; set; }

        [Column("appId")]
        [MaxLength(255)]
        public string AppId { get; set; }

        [Column("appDisplayName")]
        [MaxLength(255)]
        public string AppDisplayName { get; set; }

        [Column("ipAddress")]
        [MaxLength(255)]
        public string IpAddress { get; set; }

        [Column("clientAppUsed")]
        [MaxLength(50)]
        public string ClientAppUsed { get; set; }

        [Column("conditionalAccessStatus")]
        [MaxLength(50)]
        public string ConditionalAccessStatus { get; set; }

        [Column("riskLevelAggregated")]
        [MaxLength(50)]
        public string RiskLevelAggregated { get; set; }

        [Column("riskState")]
        [MaxLength(50)]
        public string RiskState { get; set; }

        [Column("resourceId")]
        [MaxLength(255)]
        public string ResourceId { get; set; }

        [Column("resourceDisplayName")]
        [MaxLength(255)]
        public string ResourceDisplayName { get; set; }

        // Device Details
        public DeviceDetail DeviceDetail { get; set; }

        // Location
        public Location Location { get; set; }
    }

    public class DeviceDetail
    {
        [Column("deviceId")]
        [MaxLength(255)]
        public string DeviceId { get; set; }

        [Column("deviceDisplayName")]
        [MaxLength(255)]
        public string DisplayName { get; set; }

        [Column("operatingSystem")]
        [MaxLength(255)]
        public string OperatingSystem { get; set; }

        [Column("browser")]
        [MaxLength(255)]
        public string Browser { get; set; }

        [Column("isCompliant")]
        public bool IsCompliant { get; set; }

        [Column("isManaged")]
        public bool IsManaged { get; set; }

        [Column("trustType")]
        [MaxLength(50)]
        public string TrustType { get; set; }
    }

    public class Location
    {
        [Column("city")]
        [MaxLength(255)]
        public string City { get; set; }

        [Column("state")]
        [MaxLength(255)]
        public string State { get; set; }

        [Column("countryOrRegion")]
        [MaxLength(50)]
        public string CountryOrRegion { get; set; }

        [Column("latitude")]
        public double? Latitude { get; set; }

        [Column("longitude")]
        public double? Longitude { get; set; }

        [Column("altitude")]
        public double? Altitude { get; set; }
    }
}
