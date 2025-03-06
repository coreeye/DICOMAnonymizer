
using System.Globalization;

namespace DICOMAnonymizer.Core.Rules;

public class SubtractRandomNumberRule : AnonymizationRule
{
    private readonly IRandomNumberGenerator _randomNumberGenerator;

    public SubtractRandomNumberRule(DICOMCoreTag tag, IRandomNumberGenerator randomNumberGenerator)
    {
        Tag = tag;
        _randomNumberGenerator = randomNumberGenerator;
    }

    public override string? Apply(DICOMElement element)
    {
        if (DateTime.TryParseExact(element.Value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
        {
            var randomDays = _randomNumberGenerator.Next(1, 365);
            var newDate = date.AddDays(-randomDays);
            return newDate.ToString("yyyyMMdd");
        }
        return element.Value;
    }
}