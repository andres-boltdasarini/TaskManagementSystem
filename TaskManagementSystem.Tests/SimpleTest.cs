using NUnit.Framework;

namespace TaskManagementSystem.Tests;

[TestFixture]
public class SimpleTest
{
    [Test]
    public void Test_AlwaysPasses()
    {
        Assert.Pass("Тест работает!");
    }

    [Test]
    public void Test_Addition_WorksCorrectly()
    {
        // Arrange
        int a = 2;
        int b = 3;

        // Act
        int result = a + b;

        // Assert
        Assert.That(result, Is.EqualTo(5));
    }
}