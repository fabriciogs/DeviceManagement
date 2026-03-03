using DeviceManagement.Application.DTOs;
using DeviceManagement.Application.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace DeviceManagement.Tests.UnitTests;

public class ValidatorTests
{
    [Fact]
    public void CreateDeviceValidator_ValidDto_Passes()
    {
        // Arrange
        var validator = new CreateDeviceValidator();
        var dto = new CreateDeviceDTO("ValidName", "ValidBrand", "Available");

        // Act
        var result = validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateDeviceValidator_NameEmpty_Fails()
    {
        // Arrange
        var validator = new CreateDeviceValidator();
        var dto = new CreateDeviceDTO("", "Brand", "Available");

        // Act
        var result = validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateDeviceValidator_NameTooLong_Fails()
    {
        // Arrange
        var validator = new CreateDeviceValidator();
        var dto = new CreateDeviceDTO(new string('a', 201), "Brand", "Available");

        // Act
        var result = validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateDeviceValidator_BrandEmpty_Fails()
    {
        // Arrange
        var validator = new CreateDeviceValidator();
        var dto = new CreateDeviceDTO("Name", "", "Available");

        // Act
        var result = validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Brand);
    }

    [Fact]
    public void CreateDeviceValidator_BrandTooLong_Fails()
    {
        // Arrange
        var validator = new CreateDeviceValidator();
        var dto = new CreateDeviceDTO("Name", new string('a', 101), "Available");

        // Act
        var result = validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Brand);
    }

    [Fact]
    public void CreateDeviceValidator_InvalidState_Fails()
    {
        // Arrange
        var validator = new CreateDeviceValidator();
        var dto = new CreateDeviceDTO("Name", "Brand", "Invalid");

        // Act
        var result = validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.State).WithErrorMessage("Invalid device state.");
    }

    [Fact]
    public void UpdateDeviceValidator_ValidDto_Passes()
    {
        // Arrange
        var validator = new UpdateDeviceValidator();
        var dto = new UpdateDeviceDTO("ValidName", "ValidBrand", "InUse");

        // Act
        var result = validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateDeviceValidator_NameTooLong_Fails()
    {
        // Arrange
        var validator = new UpdateDeviceValidator();
        var dto = new UpdateDeviceDTO(new string('a', 201), "", "");

        // Act
        var result = validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void UpdateDeviceValidator_BrandTooLong_Fails()
    {
        // Arrange
        var validator = new UpdateDeviceValidator();
        var dto = new UpdateDeviceDTO("", new string('a', 101), "");

        // Act
        var result = validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Brand);
    }

    [Fact]
    public void UpdateDeviceValidator_InvalidState_Fails()
    {
        // Arrange
        var validator = new UpdateDeviceValidator();
        var dto = new UpdateDeviceDTO("", "", "Invalid");

        // Act
        var result = validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.State).WithErrorMessage("Invalid device state.");
    }

    //[Fact]
    //public void UpdateDeviceValidator_StringEmptyProperties_Passes()
    //{
    //    // Arrange
    //    var validator = new UpdateDeviceValidator();
    //    var dto = new UpdateDeviceDTO(string.Empty, string.Empty, string.Empty);

    //    // Act
    //    var result = validator.TestValidate(dto);

    //    // Assert
    //    result.ShouldNotHaveAnyValidationErrors();
    //}

    [Fact]
    public void UpdateDeviceValidator_PartialValid_Passes()
    {
        // Arrange
        var validator = new UpdateDeviceValidator();
        var dto = new UpdateDeviceDTO("Valid", "", "Inactive");

        // Act
        var result = validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}