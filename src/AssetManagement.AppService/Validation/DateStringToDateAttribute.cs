using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace AssetManagement.AppService.Validators;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class DateStringToDateAttribute : ValidationAttribute
{
    private readonly string _targetPropertyName;
    private readonly string _format;
    private readonly bool _required;

    public DateStringToDateAttribute(string targetPropertyName, string format = "MM/dd/yyyy", bool required = false)
    {
        _targetPropertyName = targetPropertyName;
        _format = format;
        _required = required;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var str = value as string;

        if (string.IsNullOrWhiteSpace(str))
        {
            if (_required)
                return new ValidationResult("Field is mandatory.", new[] { validationContext.MemberName ?? _targetPropertyName });

            SetTargetProperty(validationContext.ObjectInstance, null);
            return ValidationResult.Success;
        }

        if (DateTime.TryParseExact(str.Trim(), new[] { _format }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            SetTargetProperty(validationContext.ObjectInstance, date);
            return ValidationResult.Success;
        }

        return new ValidationResult($"date should be in {_format.ToUpperInvariant()} format", new[] { validationContext.MemberName ?? _targetPropertyName });
    }

    private void SetTargetProperty(object instance, DateTime? value)
    {
        if (instance == null) return;

        var prop = instance.GetType().GetProperty(_targetPropertyName, BindingFlags.Public | BindingFlags.Instance);
        if (prop == null || !prop.CanWrite) return;

        if (prop.PropertyType == typeof(DateTime?) || prop.PropertyType == typeof(DateTime))
        {
            prop.SetValue(instance, value);
        }
    }
}
