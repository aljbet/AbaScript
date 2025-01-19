namespace CompileLanguage.Services;

public class VariableStorage : IVariableStorage
{
    private readonly Dictionary<string, object?> _variables = new();

    public T? GetVariable<T>(string name)
    {
        if (_variables.TryGetValue(name, out var value))
        {
            if (value is T typedValue)
            {
                return typedValue;
            }
            return default;
        }

        return default;
    }


    public void SaveVariable<T>(string name, T value)
    {
        _variables[name] = value;
    }

    public void Clear()
    {
        _variables.Clear();
    }
}