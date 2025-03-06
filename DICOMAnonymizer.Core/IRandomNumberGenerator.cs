namespace DICOMAnonymizer.Core;

public interface IRandomNumberGenerator
{
    int Next(int minValue, int maxValue);
}
