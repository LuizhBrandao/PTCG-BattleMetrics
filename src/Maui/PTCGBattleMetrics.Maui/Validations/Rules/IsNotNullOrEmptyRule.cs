namespace PTCGBattleMetrics.Maui.Validations.Rules;

public class IsNotNullOrEmptyRule<T> : IValidationRule<T>
{
    public string ValidationMessage { get; set; } = "Campo obrigatório";

    public bool Check(T value)
    {
        if (value is null)
            return false;

        if (value is string str)
            return !string.IsNullOrWhiteSpace(str);

        return true;
    }
}
