namespace CompileLanguage.Services;

public interface IVariableStorage
{
    T? GetVariable<T>(string name);
    void SaveVariable<T>(string name, T value);
    void Clear();
}