namespace DICOMAnonymizer.Core.Rules;

public class ReplaceWithValueRule : AnonymizationRule
{
    private readonly string _replacementValue;

    public ReplaceWithValueRule(DICOMCoreTag tag, string replacementValue)
    {
        Tag = tag;
        _replacementValue = replacementValue;
    }

    public override string? Apply(DICOMElement element)
    {
        return _replacementValue;
    }
}