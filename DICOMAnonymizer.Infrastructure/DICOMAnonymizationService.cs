using DICOMAnonymizer.Application;
using DICOMAnonymizer.Core;
using FellowOakDicom;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace DICOMAnonymizer.Infrastructure;

public class DICOMAnonymizationService(ILogger<DICOMAnonymizationService> logger, AnonymizationConfiguration config, DICOMFileService dcmFileService) : IAnonymizationService
{
    private readonly ILogger<DICOMAnonymizationService> _logger = logger;
    private readonly AnonymizationConfiguration _config = config;
    private readonly DICOMFileService _dcmFileService = dcmFileService;

    public void AnonymizeFolder(string inputFolderPath, string outputFolderPath)
    {
        // 1. Get all DICOM files in the input folder
        try
        {
            var dicomFiles = Directory.GetFiles(inputFolderPath, "*.dcm");

            // Create the output directory if it doesn't exist
            Directory.CreateDirectory(outputFolderPath);

            // 2. Iterate through each DICOM file
            foreach (var dicomFile in dicomFiles)
            {
                try
                {
                    // 3. Load the DICOM file
                    var dcmFile = _dcmFileService.LoadDicomFile(dicomFile);

                    // 4. Anonymize the DICOM file
                    AnonymizeFile(dcmFile);

                    // 5. Save the anonymized DICOM file to the output folder
                    string outputFilePath = Path.Combine(outputFolderPath, Path.GetFileName(dicomFile));
                    _dcmFileService.SaveDicomFile(dcmFile, outputFilePath);
                    _logger.LogInformation("Successfully anonymized DICOM file: {InputFile} and saved to {OutputFile}", dicomFile, outputFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing file: {DicomFile}", dicomFile);
                }
            }
        }
        catch (DirectoryNotFoundException)
        {
            _logger.LogError("Input directory not found: {InputFolderPath}", inputFolderPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during folder processing. Input folder: {InputFolderPath}, Output folder: {OutputFolderPath}", inputFolderPath, outputFolderPath);
        }
    }

    private void AnonymizeFile(DICOMFile file)
    {
        try
        {
            DicomDataset dataset = ((DICOMDatasetWrapper)file.Dataset).DicomDataset;

            // Step 1: Apply configured anonymization rules
            foreach (var rule in _config.Rules)
            {
                try
                {
                    var dcmTag = new DicomTag(Convert.ToUInt16(rule.Tag.Group, 16), Convert.ToUInt16(rule.Tag.Element, 16));

                    if (dataset.Contains(dcmTag))
                    {
                        DicomItem item = dataset.GetDicomItem<DicomItem>(dcmTag);

                        // Skip tags with VRs that cannot be handled as strings
                        if (item.ValueRepresentation == DicomVR.OB || item.ValueRepresentation == DicomVR.OW || item.ValueRepresentation == DicomVR.SQ)
                        {
                            _logger.LogInformation("Skipping anonymization for tag {Tag} with VR {VR}", dcmTag, item.ValueRepresentation);
                            continue;
                        }

                        if (item is DicomElement element)
                        {
                            if (element.ValueRepresentation == DicomVR.DS || element.ValueRepresentation == DicomVR.IS)
                            {
                                var originalValues = element.Get<string[]>() ?? [];
                                var anonymizedValues = originalValues
                                    .Select(value => rule.Apply(new DICOMElement(rule.Tag, value)) ?? value)
                                    .ToArray();

                                dataset.AddOrUpdate(dcmTag, anonymizedValues);
                            }
                            else
                            {
                                if (element.Count > 0)
                                {
                                    string? originalValue = element.Get<string>(0);
                                    string? anonymizedValue = rule.Apply(new DICOMElement(rule.Tag, originalValue));

                                    if (anonymizedValue is not null)
                                    {
                                        dataset.AddOrUpdate(dcmTag, anonymizedValue);
                                    }
                                }
                                else
                                {
                                    // Try to add a default value if the tag is empty
                                    try
                                    {
                                        dataset.AddOrUpdate(dcmTag, "DEFAULT_VALUE");
                                    }
                                    catch (Exception)
                                    {
                                        _logger.LogWarning("Could not add default value for tag {Tag}", dcmTag);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error applying rule to DICOM file for tag {Tag}", rule.Tag);
                }
            }

            // Step 2: Process unmapped tags
            RemoveUnmappedTags(dataset);

            // Step 3:  Ensure required attributes are present (Post-processing)
            EnsureRequiredAttributes(dataset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while anonymizing DICOM file.");
        }
    }

    private void RemoveUnmappedTags(DicomDataset dataset)
    {
        var essentialTags = new List<string>
        {
            "00080016", // SOP Class UID
            "00080018", // SOP Instance UID
            "00020010", // Transfer Syntax UID
            "00020002", // Media Storage SOP Class UID
            "00020003",  // Media Storage SOP Instance UID
            "00080060", // Modality - Example Required Tag
            "00280004"  // Photometric Interpretation - Example Required Tag
        };

        var allowedTags = new List<string>
        {
            // Add all tags that are required/allowed for the SOP Class here
            // You need to consult the DICOM standard for the specific SOP Class
            "00280010", // Rows
            "00280011", // Columns
            "00280100", // Bits Allocated
            "00280101", // Bits Stored
            "00280102", // High Bit
            "00280103", // Pixel Representation
            "7FE00010"  // Pixel Data
        };

        var unmappedTags = dataset
            .Select(x => new DICOMCoreTag(x.Tag.Group.ToString("X4", CultureInfo.InvariantCulture),
                                          x.Tag.Element.ToString("X4", CultureInfo.InvariantCulture)))
            .ToList();

        foreach (var tag in unmappedTags)
        {
            var tagId = tag.Group + tag.Element;

            // Skip essential tags
            if (essentialTags.Contains(tagId.ToLowerInvariant()))
            {
                continue;
            }

            if (allowedTags.Contains(tagId.ToLowerInvariant()))
            {
                continue;
            }

            var dcmTag = new DicomTag(Convert.ToUInt16(tag.Group, 16), Convert.ToUInt16(tag.Element, 16));

            // Set Patient Name to "DEFAULT NAME"
            if (tag.Group == "0010" && tag.Element == "0010")
            {
                dataset.AddOrUpdate(dcmTag, "DEFAULT NAME");
            }
            else if (tag.Group != "7FE0") // Do not remove Pixel Data
            {
                dataset.Remove(dcmTag);
            }
        }
    }

    private void EnsureRequiredAttributes(DicomDataset dataset)
    {
        if (!dataset.Contains(DicomTag.Modality))
        {
            dataset.AddOrUpdate(DicomTag.Modality, "CR"); // Or "DX", etc.
        }

        if (!dataset.Contains(DicomTag.Manufacturer))
        {
            dataset.AddOrUpdate(DicomTag.Manufacturer, "Anonymized");
        }

        if (!dataset.Contains(DicomTag.PatientID))
        {
            dataset.AddOrUpdate(DicomTag.PatientID, "AnonymizedPatientID");
        }
    }
}