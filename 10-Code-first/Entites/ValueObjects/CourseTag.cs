namespace _10_Code_first.Entites.ValueObjects;

/// <summary>
/// Value object representing a course difficulty tag.
/// Stored as a single string column via HasConversion (value converter).
/// </summary>
public class CourseTag : IEquatable<CourseTag>
{
    public string Value { get; }

    // Well-known tags (static factory members)
    public static readonly CourseTag Beginner = new("BEGINNER");
    public static readonly CourseTag Intermediate = new("INTERMEDIATE");
    public static readonly CourseTag Advanced = new("ADVANCED");

    public CourseTag(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Tag value cannot be empty.", nameof(value));

        Value = value.ToUpperInvariant();
    }

    public bool Equals(CourseTag? other) =>
        other is not null && Value == other.Value;

    public override bool Equals(object? obj) =>
        obj is CourseTag other && Equals(other);

    public override int GetHashCode() =>
        Value.GetHashCode();

    public override string ToString() => Value;

    public static bool operator ==(CourseTag? left, CourseTag? right) =>
        Equals(left, right);

    public static bool operator !=(CourseTag? left, CourseTag? right) =>
        !Equals(left, right);
}
