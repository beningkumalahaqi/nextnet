namespace NextNet.TemplateEngine.Tests.Variables;

using NextNet.TemplateEngine.Variables;
using NextNet.Templates.Models;
using Xunit;

public class VariableTypeValidatorTests
{
    [Fact]
    public void Validate_Should_ReturnEmpty_When_ValidString()
    {
        var validator = new VariableTypeValidator();
        var def = new TemplateVariable(Name: "x", Type: "string");
        var errors = validator.Validate(def, "hello");
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_Should_ReturnEmpty_When_ValidBool()
    {
        var validator = new VariableTypeValidator();
        var def = new TemplateVariable(Name: "x", Type: "bool");
        var errors = validator.Validate(def, true);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_Should_ReturnEmpty_When_ValidNumber()
    {
        var validator = new VariableTypeValidator();
        var def = new TemplateVariable(Name: "x", Type: "number");
        var errors = validator.Validate(def, 42);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_Should_ReturnEmpty_When_ValidEnum()
    {
        var validator = new VariableTypeValidator();
        var def = new TemplateVariable(Name: "x", Type: "enum", AllowedValues: new[] { "a", "b" });
        var errors = validator.Validate(def, "a");
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_Should_ReturnError_When_BoolIsInvalidString()
    {
        var validator = new VariableTypeValidator();
        var def = new TemplateVariable(Name: "x", Type: "bool");
        var errors = validator.Validate(def, "notabool");
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void Validate_Should_ReturnError_When_NumberIsNonNumericString()
    {
        var validator = new VariableTypeValidator();
        var def = new TemplateVariable(Name: "x", Type: "number");
        var errors = validator.Validate(def, "abc");
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void Validate_Should_ReturnError_When_EnumValueNotInAllowedValues()
    {
        var validator = new VariableTypeValidator();
        var def = new TemplateVariable(Name: "x", Type: "enum", AllowedValues: new[] { "a", "b" });
        var errors = validator.Validate(def, "c");
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void Validate_Should_ReturnError_When_RequiredVariableIsNull()
    {
        var validator = new VariableTypeValidator();
        var def = new TemplateVariable(Name: "x", Type: "string", Required: true);
        var errors = validator.Validate(def, null);
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void ValidateAll_Should_AccumulateMultipleErrors()
    {
        var validator = new VariableTypeValidator();
        var defs = new List<TemplateVariable>
        {
            new(Name: "a", Type: "string", Required: true),
            new(Name: "b", Type: "number")
        };
        var ctx = VariableContext.CreateBuilder().Set("b", "notanumber").Build();
        var result = validator.ValidateAll(defs, ctx);
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors!);
    }

    [Fact]
    public void TryCoerce_Should_CoerceStringToBool()
    {
        var validator = new VariableTypeValidator();
        var def = new TemplateVariable(Name: "x", Type: "bool");
        Assert.True(validator.TryCoerce(def, "true", out var v));
        Assert.Equal(true, v);
    }

    [Fact]
    public void TryCoerce_Should_CoerceStringToNumber()
    {
        var validator = new VariableTypeValidator();
        var def = new TemplateVariable(Name: "x", Type: "number");
        Assert.True(validator.TryCoerce(def, "42.5", out var v));
        Assert.Equal(42.5, v);
    }
}
