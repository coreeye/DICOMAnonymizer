using DICOMAnonymizer.Core;
using DICOMAnonymizer.Infrastructure;
using FellowOakDicom;

namespace DICOMAnonymizer.Tests;

[TestClass]
public class DicomDatasetWrapperTests
{
    [TestMethod]
    public void GetString_TagExists_ReturnsValue()
    {
        // Arrange
        var dataset = new DicomDataset();
        var tag = new DICOMCoreTag("0010", "0010");
        var dicomTag = new DicomTag(0x0010, 0x0010);
        dataset.Add(dicomTag, "Test Value");

        var wrapper = new DICOMDatasetWrapper(dataset);

        // Act
        string value = wrapper.GetString(tag);

        // Assert
        Assert.AreEqual("Test Value", value);
    }

    [TestMethod]
    public void AddOrUpdate_TagAndValue_CallsAddOrUpdateOnDataset()
    {
        // Arrange
        var dataset = new DicomDataset();
        var tag = new DICOMCoreTag("0010", "0010");
        var dicomTag = new DicomTag(0x0010, 0x0010);

        var wrapper = new DICOMDatasetWrapper(dataset);

        // Act
        wrapper.AddOrUpdate(tag, "New Value");

        // Assert
        Assert.AreEqual(dataset.GetString(dicomTag), "New Value");
    }

    [TestMethod]
    public void Contains_TagExists_ReturnsTrue()
    {
        // Arrange
        var dataset = new DicomDataset();
        var tag = new DICOMCoreTag("0010", "0010");
        var dicomTag = new DicomTag(0x0010, 0x0010);
        dataset.Add(dicomTag, "Test Value");

        var wrapper = new DICOMDatasetWrapper(dataset);

        // Act
        bool contains = wrapper.Contains(tag);

        // Assert
        Assert.IsTrue(contains);
    }

    [TestMethod]
    public void Contains_TagDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var dataset = new DicomDataset();
        var tag = new DICOMCoreTag("0010", "0010");

        var wrapper = new DICOMDatasetWrapper(dataset);

        // Act
        bool contains = wrapper.Contains(tag);

        // Assert
        Assert.IsFalse(contains);
    }
}