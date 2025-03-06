namespace DICOMAnonymizer.Core.Rules;

public class KeepOriginalValueRule : AnonymizationRule
{
    public KeepOriginalValueRule(DICOMCoreTag tag)
    {
        Tag = tag;
    }

    public override string? Apply(DICOMElement element)
    {
        return element.Value; // Keep the original value
    }
}
