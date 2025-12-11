using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RangeValueObject.Test;

public abstract record  RangeBound<T> : IComparable<RangeBound<T>>
    where T : IComparable<T>
{
    public abstract int CompareTo(RangeBound<T>? other);
    public int CompareTo(T other) => CompareTo(new Concrete<T>(other));

}

public record Concrete<T>(T Value) : RangeBound<T>
    where T : IComparable<T>
{
    public override int CompareTo(RangeBound<T>? other) =>
        other switch
        {
            PositiveInfinity<T> => -1, 
            NegativeInfinity<T> => 1, 
            Concrete<T>{ Value : var otherVal} => Value.CompareTo(otherVal),
            _ => throw new NotImplementedException()
        };

}
public record  PositiveInfinity<T> : RangeBound<T>
where T : IComparable<T>
{
    public override int CompareTo(RangeBound<T>? other) =>
        other switch
        {
            PositiveInfinity<T> => 0,
            NegativeInfinity<T> or Concrete<T> => 1,
            _ => throw new NotImplementedException(),
        };
}

public record NegativeInfinity<T> : RangeBound<T>
    where T : IComparable<T>
{
    public override int CompareTo(RangeBound<T>? other) =>
        other switch
        {
            NegativeInfinity<T> => 0,
            PositiveInfinity<T> or Concrete<T> => -1,
            _ => throw new NotImplementedException(),
        };
    
}
public readonly record struct Range<T>
    where T : IComparable<T>
{
    public RangeBound<T> Start { get; }
    public RangeBound<T> End { get; }
    public Range(RangeBound<T> start, RangeBound<T> end)
    {
        if (start.CompareTo(end) > 0)
            throw new ArgumentException("Start must be less than or equal to End");
        Start = start;
        End = end;
    }

    public bool Contains(T value) =>
        Start.CompareTo(value) <= 0 && End.CompareTo(value) >= 0;

    public static Range<T> Create(T start, T end) =>
        new Range<T>(new Concrete<T>(start), new Concrete<T>(end));
    public static Range<T> OpenEndedRange(T start) =>
        new Range<T>(new Concrete<T>(start), new PositiveInfinity<T>());

    public bool Overlaps(Range<T> other) =>
            Start.CompareTo(other.End) <= 0 && End.CompareTo(other.Start) >= 0;

}