namespace DICOMAnonymizer.Core;

public class DefaultRandomNumberGenerator : IRandomNumberGenerator
{
    private static readonly Random _random = new();

    public int Next(int minValue, int maxValue)
    {
        return _random.Next(minValue, maxValue);
    }
}