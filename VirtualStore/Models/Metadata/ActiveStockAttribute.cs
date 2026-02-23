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
        var activeProp = validationContext.ObjectType.GetProperty(_activePropertyName, BindingFlags.Public | BindingFlags.Instance);
        if (activeProp == null)
            return new ValidationResult($"Property '{_activePropertyName}' not found.");

        var isActive = (bool)activeProp.GetValue(validationContext.ObjectInstance);

        if (!isActive) return ValidationResult.Success;

        var stock = (int)value;
        if (stock <= 0)
            return new ValidationResult(ErrorMessage ?? "Stock must be greater than 0 for active products.");

        return ValidationResult.Success;
    }
}