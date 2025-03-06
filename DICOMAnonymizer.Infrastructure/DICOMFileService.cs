using DICOMAnonymizer.Core;
using FellowOakDicom;

namespace DICOMAnonymizer.Infrastructure;

public class DICOMFileService()
{
    public DICOMFile LoadDicomFile(string filePath)
    {
        try
        {
            // Load the DICOM file using fo-dicom
            var dcmFile = DicomFile.Open(filePath);

            // Create a clone of the dataset that includes file meta info
            var fullDataset = dcmFile.Dataset.Clone();

            // Copy file meta info into the dataset
            foreach (var item in dcmFile.FileMetaInfo)
            {
                if (!fullDataset.Contains(item.Tag))
                {
                    fullDataset.Add(item);
                }
            }

            return new DICOMFile(
                filePath, new DICOMDatasetWrapper(fullDataset)
            );
        }
        catch (DicomFileException ex)
        {
            throw new Exception($"Error loading DICOM file: {filePath}", ex);
        }
        catch (FileNotFoundException)
        {
            throw new Exception($"DICOM file not found: {filePath}");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error loading DICOM file: {filePath}", ex);
        }
    }

    public void SaveDicomFile(DICOMFile file, string path)
    {
        try
        {
            var dcmDatasetWrapper = (DICOMDatasetWrapper)file.Dataset;
            var dataset = dcmDatasetWrapper.DicomDataset;

            // Add missing required attributes
            AddMissingAttributes(dataset);

            // Ensure Instance UID is valid (regenerate if needed)
            try
            {
                DicomUID.Parse(dataset.GetSingleValue<string>(DicomTag.SOPInstanceUID));
            }
            catch (DicomDataException)
            {
                Console.WriteLine("Invalid SOP Instance UID, regenerating.");
                dataset.AddOrUpdate(DicomTag.SOPInstanceUID, DicomUID.Generate().UID);
            }

            // Create a DicomFile with the dataset and file meta information
            var dcmFile = new DicomFile(dataset);

            // Save with explicit error handling
            try
            {
                dcmFile.Save(path);

                // Verify the saved file can be reopened
                var verifyFile = DicomFile.Open(path);
                Console.WriteLine("File successfully saved and verified");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during file save or verification: {ex.Message}");
                throw;
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error saving DICOM file to: {path}", ex);
        }
    }

    private static void AddMissingAttributes(DicomDataset dataset)
    {
        if (!dataset.Contains(DicomTag.BodyPartExamined))
        {
            dataset.AddOrUpdate(DicomTag.BodyPartExamined, "UNKNOWN");
        }

        if (!dataset.Contains(DicomTag.ViewPosition))
        {
            dataset.AddOrUpdate(DicomTag.ViewPosition, "AP");
        }

        if (!dataset.Contains(DicomTag.InstanceNumber))
        {
            dataset.AddOrUpdate(DicomTag.InstanceNumber, "1");
        }

        if (!dataset.Contains(DicomTag.SeriesInstanceUID))
        {
            if (!dataset.Contains(DicomTag.SeriesInstanceUID))
            {
                dataset.AddOrUpdate(DicomTag.SeriesInstanceUID, DicomUID.Generate().UID);
            }
        }

        if (!dataset.Contains(DicomTag.SeriesNumber))
        {
            dataset.AddOrUpdate(DicomTag.SeriesNumber, "1");
        }

        if (!dataset.Contains(DicomTag.StudyDate))
        {
            dataset.AddOrUpdate(DicomTag.StudyDate, "20240101");
        }

        if (!dataset.Contains(DicomTag.StudyTime))
        {
            dataset.AddOrUpdate(DicomTag.StudyTime, "120000");
        }

        if (!dataset.Contains(DicomTag.AccessionNumber))
        {
            dataset.AddOrUpdate(DicomTag.AccessionNumber, "ANON12345");
        }

        if (!dataset.Contains(DicomTag.ReferringPhysicianName))
        {
            dataset.AddOrUpdate(DicomTag.ReferringPhysicianName, "Anonymized Physician");
        }

        if (!dataset.Contains(DicomTag.StudyInstanceUID))
        {
            if (!dataset.Contains(DicomTag.StudyInstanceUID))
            {
                dataset.AddOrUpdate(DicomTag.StudyInstanceUID, DicomUID.Generate().UID);
            }
        }

        if (!dataset.Contains(DicomTag.StudyID))
        {
            dataset.AddOrUpdate(DicomTag.StudyID, "ANONStudyID");
        }

        if (!dataset.Contains(DicomTag.SamplesPerPixel))
        {
            dataset.AddOrUpdate(DicomTag.SamplesPerPixel, (ushort)1);
        }

        if (!dataset.Contains(DicomTag.PatientBirthDate))
        {
            dataset.AddOrUpdate(DicomTag.PatientBirthDate, "20000101");
        }

        if (!dataset.Contains(DicomTag.PatientSex))
        {
            dataset.AddOrUpdate(DicomTag.PatientSex, "O");
        }
    }
}