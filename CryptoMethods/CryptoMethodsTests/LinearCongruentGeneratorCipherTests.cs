using CryptoMethods.CryptoMethods;
using NUnit.Framework;
using FluentAssertions;

namespace CryptoMethodsTests;

public class LinearCongruentGeneratorCipherTests
{
    [SetUp]
    public void Setup()
    {
    }

    /// <summary>
    /// Проверяет на правильность алгоритма шифрования
    /// </summary>
    [Test]
    public void WillEncryptCorrectly()
    {
        // Arrange
        const string encryptionString = "привет";
        const string expectedEncryptedString = "174147220293366";

        var linearCongruentGeneratorCipher = new LinearCongruentGeneratorCipher();
        // Act
        var result = linearCongruentGeneratorCipher.EncryptString(encryptionString);

        // Assert
        result.Should().Be(expectedEncryptedString);
    }
    
    /// <summary>
    /// Проверяет на правильность алгоритма дешифрования
    /// </summary>
    [Test]
    public void WillDecryptCorrectly()
    {
        // Arrange
        const string encryptionString = "привет";

        var linearCongruentGeneratorCipher = new LinearCongruentGeneratorCipher();
        var encryptedString = linearCongruentGeneratorCipher.EncryptString(encryptionString);
        
        // Act
        var result = linearCongruentGeneratorCipher.DecryptString();
        
        // Assert
        result.Should().Be(encryptionString);
    }
}