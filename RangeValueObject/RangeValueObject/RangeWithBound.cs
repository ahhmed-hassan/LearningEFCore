using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RangeValueObject;

public readonly record struct RangeBound<T> : IComparable<RangeBound<T>>
    where T : IComparable<T>
{
    private readonly T? _value;
    private readonly bool _isInfinity;

    private RangeBound(T? value, bool isInfinity) 
    {
        _value = value;
        _isInfinity = isInfinity;
    }
    public int CompareTo(RangeBound<T> other)
    {
        if (_isInfinity && other._isInfinity) return 0;
        if (_isInfinity) return 1;
        if (other._isInfinity) return -1;
        return _value!.CompareTo(other._value);
    }
    public int CompareTo(T other) => CompareTo(ValueOf(other));
    public static RangeBound<T> Infinity() => new RangeBound<T>(default, true);
    public static RangeBound<T> ValueOf(T value) => new RangeBound<T>(value, false);
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
        new Range<T>(RangeBound<T>.ValueOf(start), RangeBound<T>.ValueOf(end));
    public static Range<T> OpenEndedRange(T start) =>
        new Range<T>(RangeBound<T>.ValueOf(start), RangeBound<T>.Infinity());

    public bool Overlaps(Range<T> other) =>
            Start.CompareTo(other.End) <= 0 && End.CompareTo(other.Start) >= 0;

}
