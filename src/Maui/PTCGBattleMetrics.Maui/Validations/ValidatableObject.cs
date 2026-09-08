using CommunityToolkit.Mvvm.ComponentModel;

namespace PTCGBattleMetrics.Maui.Validations;

public class ValidatableObject<T> : ObservableObject, IValidity
{
    private IEnumerable<string> _errors = Enumerable.Empty<string>();
    private bool _isValid = true;
    private T? _value;

    public List<IValidationRule<T>> Validations { get; } = new();

    public IEnumerable<string> Errors
    {
        get => _errors;
        private set => SetProperty(ref _errors, value);
    }

    public bool IsValid
    {
        get => _isValid;
        set => SetProperty(ref _isValid, value);
    }

    public T? Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    public bool Validate()
    {
        Errors = Validations
            .Where(v => !v.Check(Value!))
            .Select(v => v.ValidationMessage)
            .ToArray();

        IsValid = !Errors.Any();
        return IsValid;
    }
}
