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
                        var item = dataset.GetDicomItem<DicomItem>(dcmTag);

                        if (CheckIfCannotBeHandledAsStrings(item))
                        {
                            _logger.LogInformation("Skipping anonymization for tag {Tag} with VR {VR}", dcmTag, item.ValueRepresentation);
                            continue;
                        }

                        if (item is DicomElement element)
                        {
                            // Decimal string or integer string
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while anonymizing DICOM file.");
        }
    }

    private void RemoveUnmappedTags(DicomDataset dataset)
    {
        var unmappedTags = dataset
            .Select(x => new DICOMCoreTag(x.Tag.Group.ToString("X4", CultureInfo.InvariantCulture),
                                          x.Tag.Element.ToString("X4", CultureInfo.InvariantCulture)))
            .ToList();

        foreach (var tag in unmappedTags)
        {
            // Skip rules
            if (_config.Rules.Any(rule => rule.Tag.Equals(tag)))
            {
                continue;
            }

            var dcmTag = new DicomTag(Convert.ToUInt16(tag.Group, 16), Convert.ToUInt16(tag.Element, 16));

            // Determine the Value Representation (VR) of the tag
            if (!dataset.Contains(dcmTag))
            {
                continue;
            }

            var item = dataset.GetDicomItem<DicomItem>(dcmTag);
            var vr = item.ValueRepresentation;

            // Skip if VR cannot be handled as a string
            if (CheckIfCannotBeHandledAsStrings(item))
            {
                _logger.LogWarning("Skipping replacement for tag {Tag} with VR {VR}", dcmTag, vr);
                continue;
            }

            // Set Patient Name to "DEFAULT NAME"
            if (tag.Group == "0010" && tag.Element == "0010")
            {
                dataset.AddOrUpdate(dcmTag, "DEFAULT NAME");
                continue;
            }

            try
            {
                dataset.AddOrUpdate(dcmTag, "DEFAULT_VALUE");
            }
            catch (Exception)
            {
                // Swallow exception if default value cannot be added
            }
        }
    }

    // Helper method to check if VR cannot be handled as a string
    private static bool CheckIfCannotBeHandledAsStrings(DicomItem item)
    {
        return item.ValueRepresentation == DicomVR.OB  // Other Byte
            || item.ValueRepresentation == DicomVR.OW  // Other Word
            || item.ValueRepresentation == DicomVR.SQ  // Sequence
            || item.ValueRepresentation == DicomVR.UN  // Unknown
            || item.ValueRepresentation == DicomVR.OF  // Other Float
            || item.ValueRepresentation == DicomVR.UT; // Unlimited Text (may be too large)
    }

}