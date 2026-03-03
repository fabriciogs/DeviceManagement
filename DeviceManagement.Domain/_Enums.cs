using System.ComponentModel;

namespace DeviceManagement.Domain;

/// <summary>Domain: Device State</summary>
public enum DeviceState
{
    /// <summary>Device is available.</summary>
    [Description("Available")]
    Available = 1,

    /// <summary>Device is currently in use.</summary>
    [Description("In Use")]
    InUse = 2,

    /// <summary>Device is inactive.</summary>
    [Description("Inactive")]
    Inactive = 3
}