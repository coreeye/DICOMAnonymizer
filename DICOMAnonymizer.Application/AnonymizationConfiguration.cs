using DICOMAnonymizer.Core;
using DICOMAnonymizer.Core.Rules;

namespace DICOMAnonymizer.Application;

public class AnonymizationConfiguration
{
    public List<AnonymizationRule> Rules { get; set; } = [];

    public AnonymizationConfiguration(IRandomNumberGenerator randomNumberGenerator)
    {
        // A.1: Patient
        Rules.Add(new SubtractRandomNumberRule(new DICOMCoreTag("0010", "0030"), randomNumberGenerator)); // PatientBirthDate
        Rules.Add(new KeepOriginalValueRule(new DICOMCoreTag("0010", "0040"))); // PatientSex

        // A.2: Study
        Rules.Add(new SubtractRandomNumberRule(new DICOMCoreTag("0008", "0020"), randomNumberGenerator)); // StudyDate
        Rules.Add(new KeepOriginalValueRule(new DICOMCoreTag("0020", "000d"))); // StudyInstanceUID

        // A.3: Series
        Rules.Add(new KeepOriginalValueRule(new DICOMCoreTag("0008", "0060"))); // Modality

        // A.4: Equipment
        Rules.Add(new KeepOriginalValueRule(new DICOMCoreTag("0008", "0070"))); // Manufacturer
        Rules.Add(new KeepOriginalValueRule(new DICOMCoreTag("0008", "1090"))); // ManufacturerModelName
        Rules.Add(new KeepOriginalValueRule(new DICOMCoreTag("0018", "1020"))); // SoftwareVersion
        Rules.Add(new KeepOriginalValueRule(new DICOMCoreTag("0018", "1164"))); // ImagerPixelSpacing

        // A.5: Image
        Rules.Add(new KeepOriginalValueRule(new DICOMCoreTag("0028", "0010"))); // Rows
        Rules.Add(new KeepOriginalValueRule(new DICOMCoreTag("0028", "0011"))); // Columns
        Rules.Add(new KeepOriginalValueRule(new DICOMCoreTag("0028", "0030"))); // Pixel Spacing
        Rules.Add(new KeepOriginalValueRule(new DICOMCoreTag("0028", "0100"))); // Bits Allocated
        Rules.Add(new KeepOriginalValueRule(new DICOMCoreTag("0028", "0103"))); // Pixel Representation
        Rules.Add(new KeepOriginalValueRule(new DICOMCoreTag("0028", "0106"))); // Smallest Image Pixel Value
        Rules.Add(new KeepOriginalValueRule(new DICOMCoreTag("0028", "0107"))); // Largest Image Pixel Value
        Rules.Add(new KeepOriginalValueRule(new DICOMCoreTag("7FE0", "0010"))); // Pixel Data
        Rules.Add(new KeepOriginalValueRule(new DICOMCoreTag("0008", "0016"))); // SOP Class UID
        Rules.Add(new KeepOriginalValueRule(new DICOMCoreTag("0008", "0018"))); // SOP Instance UID

        // Other
        Rules.Add(new KeepOriginalValueRule(new DICOMCoreTag("0028", "0004"))); // Photometric Interpretation
        Rules.Add(new KeepOriginalValueRule(new DICOMCoreTag("0028", "0101"))); // Bits Stored
        Rules.Add(new KeepOriginalValueRule(new DICOMCoreTag("0028", "0102"))); // High Bit
        Rules.Add(new KeepOriginalValueRule(new DICOMCoreTag("0002", "0002"))); // Media Storage SOP Class UID
        Rules.Add(new KeepOriginalValueRule(new DICOMCoreTag("0002", "0003"))); // Media Storage SOP Instance UID
        Rules.Add(new KeepOriginalValueRule(new DICOMCoreTag("0002", "0010"))); // Transfer Syntax UID
    }
}