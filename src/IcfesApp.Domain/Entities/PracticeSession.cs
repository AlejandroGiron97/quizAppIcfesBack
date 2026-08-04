using IcfesApp.Domain.Common;
using IcfesApp.Domain.Enums;

namespace IcfesApp.Domain.Entities;

public class PracticeSession : BaseEntity
{
    public Guid StudentUserId { get; set; }

    public Guid SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    public PracticeSessionStatus Status { get; set; } = PracticeSessionStatus.InProgress;
    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FinishedAtUtc { get; set; }

    public ICollection<StudentAnswer> Answers { get; set; } = [];
}
