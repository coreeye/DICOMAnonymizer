using DICOMAnonymizer.Core;
using DICOMAnonymizer.Core.Rules;
using Moq;

namespace DICOMAnonymizer.Tests;

[TestClass]
public class SubtractRandomNumberRuleTests
{
    [TestMethod]
    public void Apply_ValidDate_ReturnsAnonymizedDate()
    {
        // Arrange
        var tag = new DICOMCoreTag("0010", "0030");
        var mockRandomGenerator = new Mock<IRandomNumberGenerator>();
        mockRandomGenerator.Setup(g => g.Next(1, 365)).Returns(10);
        var rule = new SubtractRandomNumberRule(tag, mockRandomGenerator.Object);
        var element = new DICOMElement ( tag, "20231027" );

        // Act
        string? anonymizedValue = rule.Apply(element);

        // Assert
        Assert.AreEqual("20231017", anonymizedValue); // Expected date after subtracting 10 days
    }
}
