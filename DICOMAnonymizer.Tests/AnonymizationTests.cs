using DICOMAnonymizer.Application;
using DICOMAnonymizer.Core;
using DICOMAnonymizer.Infrastructure;
using FellowOakDicom;
using Microsoft.Extensions.Logging.Abstractions;
using System.Globalization;
using System.Reflection;

namespace DICOMAnonymizer.Tests;

[TestClass]
public class AnonymizationTests
{
#pragma warning disable CS8604 // Possible null reference argument.
    private static string GetProjectRoot() => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..", "..", ".."));
#pragma warning restore CS8604 // Possible null reference argument.

    private static string GetTestDicomFilePath() => Path.Combine(GetProjectRoot(), "test.dcm");

    private static DicomFile CreateTestDicomFile(params (DicomTag, object)[] elements)
    {
        var dataset = new DicomDataset
        {
            { DicomTag.SOPClassUID, DicomUID.CTImageStorage },
            { DicomTag.SOPInstanceUID, DicomUID.Generate() }
        };

        foreach (var (tag, value) in elements)
        {
            var vr = tag.DictionaryEntry.ValueRepresentations.FirstOrDefault()?.Code;

            switch (vr)
            {
                case "DA": // Date
                    dataset.AddOrUpdate(tag, value.ToString());
                    break;
                case "PN": // Person Name
                    dataset.AddOrUpdate(tag, value.ToString());
                    break;
                case "UI": // UID
                    dataset.AddOrUpdate(tag, DicomUID.Parse(value.ToString()));
                    break;
                default:
                    dataset.AddOrUpdate(tag, value);
                    break;
            }
        }

        var dicomFile = new DicomFile(dataset);
        dicomFile.Save(GetTestDicomFilePath());
        return dicomFile;
    }

    private static DICOMAnonymizationService GetAnonymizationService()
    {
        var logger = new NullLogger<DICOMAnonymizationService>();
        var fileService = new DICOMFileService();
        return new DICOMAnonymizationService(logger, new AnonymizationConfiguration(new TestRandomNumberGenerator()), fileService);
    }


    [TestInitialize]
    public void Setup()
    {
        if (File.Exists(GetTestDicomFilePath()))
        {
            File.Delete(GetTestDicomFilePath());
        }
    }


    [TestCleanup]
    public void Teardown()
    {
        if (File.Exists(GetTestDicomFilePath()))
        {
            File.Delete(GetTestDicomFilePath());
        }
    }

    [TestMethod]
    public void AnonymizeFile_PatientBirthDateReplacedCorrectly()
    {
        // Arrange
        CreateTestDicomFile((DicomTag.PatientBirthDate, "20000101"), (DicomTag.PatientName, "Test^Patient"));
        var anonymizationService = GetAnonymizationService();

        // Act
        anonymizationService.AnonymizeFolder(GetProjectRoot(), GetProjectRoot());
        var datasetWrapper = new DICOMDatasetWrapper(DicomFile.Open(GetTestDicomFilePath()).Dataset);

        // Assert
        Assert.IsTrue(datasetWrapper.Contains(new DICOMCoreTag("0010", "0030")));
        Assert.IsTrue(DateTime.TryParseExact(datasetWrapper.GetString(new DICOMCoreTag("0010", "0030")), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _));
        Assert.AreNotEqual("20000101", datasetWrapper.GetString(new DICOMCoreTag("0010", "0030")));
    }

    [TestMethod]
    public void AnonymizeFile_PatientNameRemoved()
    {
        // Arrange
        CreateTestDicomFile((DicomTag.PatientName, "Test^Patient"));
        var anonymizationService = GetAnonymizationService();

        // Act
        anonymizationService.AnonymizeFolder(GetProjectRoot(), GetProjectRoot());
        var datasetWrapper = new DICOMDatasetWrapper(DicomFile.Open(GetTestDicomFilePath()).Dataset);

        // Assert
        Assert.IsTrue(datasetWrapper.Contains(new DICOMCoreTag("0010", "0010")));
        Assert.AreEqual("DEFAULT NAME", datasetWrapper.GetString(new DICOMCoreTag("0010", "0010")));
    }

    [TestMethod]
    public void AnonymizeFile_StudyDateAdjustedCorrectly()
    {
        // Arrange
        CreateTestDicomFile((DicomTag.StudyDate, "20200101"), (DicomTag.PatientBirthDate, "20000101"));
        var anonymizationService = GetAnonymizationService();

        // Act
        anonymizationService.AnonymizeFolder(GetProjectRoot(), GetProjectRoot());
        var datasetWrapper = new DICOMDatasetWrapper(DicomFile.Open(GetTestDicomFilePath()).Dataset);

        // Assert
        Assert.IsTrue(datasetWrapper.Contains(new DICOMCoreTag("0008", "0020")));
        Assert.IsTrue(DateTime.TryParseExact(datasetWrapper.GetString(new DICOMCoreTag("0008", "0020")), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _));
        Assert.AreNotEqual("20200101", datasetWrapper.GetString(new DICOMCoreTag("0008", "0020")));
    }
}

// Mock class
public class TestRandomNumberGenerator : IRandomNumberGenerator
{
    public int Next(int minValue, int maxValue) => 10;
}
