namespace SampleTech.Api.Authorization;

/// <summary>
/// Named authorization policy constants.
/// Register these with AddAuthorization() in Program.cs.
/// </summary>
public static class Policies
{
    /// <summary>Platform-level backend administrators only.</summary>
    public const string AdminOnly = "AdminOnly";

    /// <summary>Admins and underwriters: submission review, quote approval.</summary>
    public const string UnderwriterOrAbove = "UnderwriterOrAbove";

    /// <summary>Admins, agents, and brokers: client management, quote submission.</summary>
    public const string AgentOrAbove = "AgentOrAbove";

    /// <summary>Admins and brokers: portfolio/multi-agency views.</summary>
    public const string BrokerOrAbove = "BrokerOrAbove";

    /// <summary>All authenticated users (all five roles).</summary>
    public const string AnyRole = "AnyRole";

    /// <summary>All roles except Client: internal staff only.</summary>
    public const string StaffOnly = "StaffOnly";
}
