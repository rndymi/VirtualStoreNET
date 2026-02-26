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

        // si no está activo, no me importa el stock
        if (!isActive) return ValidationResult.Success;

        // si value es null, lo debería atrapar [Required] en el metadata
        if (value == null) return ValidationResult.Success;

        int stock;
        try { stock = (int)value; }
        catch { return ValidationResult.Success; }

        // ✅ Permitimos stock = 0, solo bloqueamos negativos
        if (stock < 0)
            return new ValidationResult(ErrorMessage ?? "Stock cannot be negative.");

        return ValidationResult.Success;
    }
}