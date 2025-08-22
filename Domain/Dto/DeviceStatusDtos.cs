using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto;

public class CreateDeviceStatusDto
{
    public required string DeviceId { get; set; }  // Unique identifier for the device
    public required string Status { get; set; }  // e.g., "Available", "In Use", "Maintenance"
    public DateTime LastUpdated { get; set; } // Timestamp of the last status update
}

public class DeviceStatusResponseDto
{
    public Guid Id { get; set; }  // PK, UUID
    public required string DeviceId { get; set; }  // Unique identifier for the device
    public required string Status { get; set; }  // e.g., "Available", "In Use", "Maintenance"
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;  // Timestamp of the last status update
}