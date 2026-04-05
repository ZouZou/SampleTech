namespace SampleTech.Api.Models;

public class PolicyDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PolicyId { get; set; }
    public Policy Policy { get; set; } = null!;

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public Guid UploadedByUserId { get; set; }
    public User UploadedByUser { get; set; } = null!;

    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;

    /// <summary>Blob storage path or URL. Local path in dev, S3 URL in production.</summary>
    public string BlobUrl { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
