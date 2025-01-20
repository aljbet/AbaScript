namespace AbaScript.LlvmCompilerClasses;

public class ScopeManager<T>
{
    private readonly Stack<Dictionary<string, T>> _scopes = new();

    public ScopeManager()
    {
        // create global scope
        _scopes.Push(new Dictionary<string, T>());
    }
    
    public Dictionary<string, T> CurrentScope => _scopes.Peek();

    public List<string> CurrentParams { get; set; }

    public void EnterScope()
    {
        _scopes.Push(new Dictionary<string, T>());
    }

    public void ExitScope()
    {
        if (_scopes.Count <= 1)
        {
            throw new InvalidOperationException("manager should contain at least one (global) scope");
        }

        _scopes.Pop();
    }

    public T this[string name]
    {
        get => TryGetValue(name, out var retVal) ? retVal : throw new KeyNotFoundException(name);
        set => _scopes.Peek()[name] = value;
    }

    public bool TryGetValue(string name, out T value)
    {
        value = default;

        foreach (var scope in _scopes)
        {
            if (scope.TryGetValue(name, out value))
            {
                return true;
            }
        }

        return false;
    }
}