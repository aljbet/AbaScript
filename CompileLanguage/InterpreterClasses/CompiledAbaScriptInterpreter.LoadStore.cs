using CompileLanguage.BaseAntlrClasses;
using CompileLanguage.Exceptions;

namespace CompileLanguage.InterpreterClasses;

public partial class CompiledAbaScriptInterpreter
{
    public override object? VisitLoadInstruction(CompiledAbaScriptParser.LoadInstructionContext context)
    {
        var varName = context.ID().GetText();
        if (!_variables.TryGetValue(varName, out var value))
        {
            throw new RuntimeException("Variable not found: " + varName);
        }
        
        if (!varName.Contains("."))
        {
            // просто вытаскиваем значение переменной или адрес объекта
            return _stackAddresses[value.Address];
        }
        
        // ищем нужное поле в куче
        List<string> fields = varName.Split('.').ToList();
        int address = _stackAddresses[_variables[fields[0]].Address];
        string lastClassName = _variables[fields[0]].Type;
        for (int i = 0; i < fields.Count; i++)
        {
            for (int j = _heapAddresses[address]; j < _classInfos[lastClassName].Fields.Length; j++)
            {
                if (fields[i] == _classInfos[lastClassName].Fields[j].Name)
                {
                    address = j;
                    lastClassName = _classInfos[lastClassName].Fields[j].Type;
                    break;
                }
            }
        }
        
        return _heapAddresses[address];
    }
    
    // определяем, где будем хранить объект, если это поле класса или сам инстанс класса, то heap, иначе стек
    public Storage DetermineStorage(String varName, String className)
    {
        if (!SimpleTypes.IsSimple(className))
        {
            return Storage.Heap;
        }

        if (varName.Contains("."))
        {
            return Storage.Heap;
        }
        return Storage.Stack;
    }

    public override object? VisitStoreInstruction(CompiledAbaScriptParser.StoreInstructionContext context)
    {
        var varName = context.ID().GetText();
        if (_stack.Count == 0)
        {
            throw new RuntimeException("Stack underflow during STORE");
        }

        var className = ""; // TODO: get class name
        Storage variableStorage = DetermineStorage(varName, className);

        // просто добавляем переменную в стек
        if (variableStorage == Storage.Stack)
        {
            _variables[varName] = new Variable(className, _stackTop, Storage.Stack, varName);
            _stackAddresses[_stackTop] = _stack.Pop();
            _stackTop++;
            return null;   
        }

        if (!varName.Contains("."))
        {
            _variables[varName] = new Variable(className, _stackTop, Storage.Heap, varName);
            _stackAddresses[_stackTop] = InitObject(className, _heapTop);
            _stackTop++;
            return null;
        }
        
        // ищем нужное поле в куче и присваиваем ему значение
        List<string> fields = varName.Split('.').ToList();
        int address = _stackAddresses[_variables[fields[0]].Address];
        string lastClassName = _variables[fields[0]].Type;
        for (int i = 0; i < fields.Count; i++)
        {
            for (int j = _heapAddresses[address]; j < _classInfos[lastClassName].Fields.Length; j++)
            {
                if (fields[i] == _classInfos[lastClassName].Fields[j].Name)
                {
                    address = j;
                    lastClassName = _classInfos[lastClassName].Fields[j].Type;
                    break;
                }
            }
        }
        
        _heapAddresses[address] = _stack.Pop();
        return null;
    }

    public int InitObject(string className, int startAddress)
    {
        _linkCounter[startAddress]++;
        _heapTop += _classInfos[className].Fields.Length;
        for (int i = _heapTop; i < _classInfos[className].Fields.Length + _heapTop; i++)
        {
            var field = _classInfos[className].Fields[i];
            if (!SimpleTypes.IsSimple(field.Type))
            {
                _heapAddresses[i] = InitObject(field.Type, _heapTop); // задаём сразу указатели на объекты внутри объекта
            }
        }

        return startAddress;
    } 
}