using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

public class ActiveStockAttribute : ValidationAttribute
{
    private readonly string _activePropertyName;

    public ActiveStockAttribute(string activePropertyName)
    {
        _activePropertyName = activePropertyName;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var activeProp = validationContext.ObjectType.GetProperty(
            _activePropertyName,
            BindingFlags.Public | BindingFlags.Instance
        );

        if (activeProp == null)
            return new ValidationResult($"Property '{_activePropertyName}' not found.");

        var isActiveObj = activeProp.GetValue(validationContext.ObjectInstance);
        var isActive = isActiveObj is bool b && b;

        if (!isActive) return ValidationResult.Success;

        if (value == null) return ValidationResult.Success;

        int stock;
        try { stock = (int)value; }
        catch { return ValidationResult.Success; }

        if (stock < 0)
            return new ValidationResult(ErrorMessage ?? "Stock cannot be negative.");

        return ValidationResult.Success;
    }
}