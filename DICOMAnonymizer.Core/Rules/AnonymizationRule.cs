namespace DICOMAnonymizer.Core.Rules;

public abstract class AnonymizationRule
{
    public DICOMCoreTag Tag { get; set; } = null!;

    public abstract string? Apply(DICOMElement element);
}