
public class NumberUtils
{
    public static bool WithinRangeInclusive(int value, int r1, int r2)
    {
        int min = Math.Min(r1, r2);
        int max = Math.Max(r1, r2);
        return min <= value && value <= max;
    }

    public static bool WithinRangeExclusive(int value, int r1, int r2)
    {
        int min = Math.Min(r1, r2);
        int max = Math.Max(r1, r2);
        return min < value && value < max;
    }
}
