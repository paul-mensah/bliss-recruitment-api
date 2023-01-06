using System.ComponentModel.DataAnnotations;

namespace BlissRecruitment.Core.Attributes;

public class AllowDuplicatesStringsAttribute : ValidationAttribute
{
    private readonly bool _allowDuplicates;
    private readonly int _minLength;

    public AllowDuplicatesStringsAttribute(bool allowDuplicates, int minLength)
    {
        _allowDuplicates = allowDuplicates;
        _minLength = minLength;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        string[] values = (string[]) value;

        if (!_allowDuplicates)
        {
            values = values.Distinct().ToArray();
        }

        string duplicateMessage = !_allowDuplicates ? "distinct" : string.Empty;
        
        return values.Length >= _minLength
            ? ValidationResult.Success
            : new ValidationResult($"Provide a minimum of {_minLength} {duplicateMessage} values");
    }
}