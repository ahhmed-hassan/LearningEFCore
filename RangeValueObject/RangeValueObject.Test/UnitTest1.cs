namespace RangeValueObject.Test;
using Xunit;

// <summary>
// Unit tests for RangeWithBound. 
// Also tests the Range and RangeBound classes. The Range Bound classes should be tested 
// indirectly through the Range class.
/// </summary>
public class RangeWithBoundTests
{

    [Fact]
    public void Contains_ValueWithinRange_ReturnsTrue()
    {
        var range = Range<int>.Create(5, 10);
        Assert.True(range.Contains(7));
    }
    [Fact]
    public void Contains_ValueOutsideRange_ReturnsFalse()
    {
        var range = Range<int>.Create(5, 10);
        Assert.False(range.Contains(4));
        Assert.False(range.Contains(11));
    }

    [Fact]
    public void Overlaps_OverlappingRanges_ReturnsTrue()
    {
        var range1 = Range<int>.Create(5, 10);
        var range2 = Range<int>.Create(8, 12);
        Assert.True(range1.Overlaps(range2));
    }

    [Fact]
    public void Overlaps_NonOverlappingRanges_ReturnsFalse()
    {
        var range1 = Range<int>.Create(5, 10);
        var range2 = Range<int>.Create(11, 15);
        Assert.False(range1.Overlaps(range2));
    }

    [Fact]
    public void Overlaps_RangesTouchingAtBoundary_ReturnsTrue()
    {
        var range1 = Range<int>.Create(5, 10);
        var range2 = Range<int>.Create(10, 15);
        Assert.True(range1.Overlaps(range2));
    }

    [Fact]
    public void OpenEndedRange_ContainsValueGreaterThanStart_ReturnsTrue()
    {
        var range = Range<int>.OpenEndedRange(5);
        Assert.True(range.Contains(10));
    }
    
    [Fact]
    public void OpenEndedRange_ContainsValueLessThanStart_ReturnsFalse()
    {
        var range = Range<int>.OpenEndedRange(5);
        Assert.False(range.Contains(4));
    }

    
    

}
