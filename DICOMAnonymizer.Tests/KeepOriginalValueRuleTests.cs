using DICOMAnonymizer.Core;
using DICOMAnonymizer.Core.Rules;

namespace DICOMAnonymizer.Tests;

[TestClass]
public class KeepOriginalValueRuleTests
{
    [TestMethod]
    public void Apply_AnyValue_ReturnsOriginalValue()
    {
        // Arrange
        var tag = new DICOMCoreTag("0010", "0040");
        var rule = new KeepOriginalValueRule(tag);
        var element = new DICOMElement ( tag, "Original Value" );

        // Act
        string? anonymizedValue = rule.Apply(element);

        // Assert
        Assert.AreEqual("Original Value", anonymizedValue);
    }

    [TestMethod]
    public void Apply_EmptyValue_ReturnsEmptyValue()
    {
        // Arrange
        var tag = new DICOMCoreTag("0010", "0040");
        var rule = new KeepOriginalValueRule(tag);
        var element = new DICOMElement ( tag, "" );

        // Act
        string? anonymizedValue = rule.Apply(element);

        // Assert
        Assert.AreEqual("", anonymizedValue);
    }

    [TestMethod]
    public void Apply_NullValue_ReturnsNullValue()
    {
        // Arrange
        var tag = new DICOMCoreTag("0010", "0040");
        var rule = new KeepOriginalValueRule(tag);
        var element = new DICOMElement (tag, null);

        // Act
        string? anonymizedValue = rule.Apply(element);

        // Assert
        Assert.IsNull(anonymizedValue);
    }
}