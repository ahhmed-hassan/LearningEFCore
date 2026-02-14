namespace _10_Code_first.Entites.ValueObjects;

/// <summary>
/// Value object representing a date range.
/// StartDate and EndDate are persisted in the database.
/// TotalDays, Contains(), and IsActive are computed — NOT mapped to columns.
/// </summary>
public class DateRange
{
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    /// <summary>
    /// Computed property — NOT persisted. Querying this in LINQ will fail.
    /// </summary>
    public int TotalDays => (EndDate - StartDate).Days;

    /// <summary>
    /// Computed property — NOT persisted. Depends on the current time.
    /// </summary>
    public bool IsActive => StartDate <= DateTime.Now && DateTime.Now <= EndDate;

    // EF Core needs a parameterless constructor
    private DateRange() { }

    public DateRange(DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate)
            throw new ArgumentException("EndDate must be after StartDate.");

        StartDate = startDate;
        EndDate = endDate;
    }

    /// <summary>
    /// Checks if a given date falls within this range.
    /// Works in C# code, but NOT translatable inside a LINQ-to-SQL query.
    /// </summary>
    public bool Contains(DateTime date) => StartDate <= date && date <= EndDate;

    public bool Overlaps(DateRange other) =>
        StartDate < other.EndDate && other.StartDate < EndDate;

    public override bool Equals(object? obj) =>
        obj is DateRange other &&
        StartDate == other.StartDate &&
        EndDate == other.EndDate;

    public override int GetHashCode() =>
        HashCode.Combine(StartDate, EndDate);
}
