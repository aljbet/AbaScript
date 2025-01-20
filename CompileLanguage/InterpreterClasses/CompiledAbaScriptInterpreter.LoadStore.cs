using CompileLanguage.BaseAntlrClasses;
using CompileLanguage.Exceptions;

namespace CompileLanguage.InterpreterClasses;

public partial class CompiledAbaScriptInterpreter
{
    public override object? VisitLoadInstruction(CompiledAbaScriptParser.LoadInstructionContext context)
    {
        var varName = context.varName().GetText();
        if (!varName.Contains("."))
        {
            // просто вытаскиваем значение переменной или адрес объекта
            if (!_variables.TryGetValue(varName, out var value))
            {
                throw new RuntimeException("Variable not found: " + varName);
            }
            _stack.Push(_stackAddresses[value.Peek().Address]);
            return null;
        }
        
        // ищем нужное поле в куче
        List<string> fields = varName.Split('.').ToList();
        int address = _stackAddresses[_variables[fields[0]].Peek().Address];
        string lastClassName = _variables[fields[0]].Peek().Type;
        for (int i = 1; i < fields.Count; i++)
        {
            for (int j = address; j < address + _classInfos[lastClassName].Fields.Length; j++)
            {
                var fieldNum = j - address;
                if (fields[i] == _classInfos[lastClassName].Fields[fieldNum].Name)
                {
                    if (SimpleTypes.IsSimple(_classInfos[lastClassName].Fields[fieldNum].Type))
                    {
                        address = j;
                    }
                    else
                    {
                        address = _heapAddresses[j];
                    }
                    
                    lastClassName = _classInfos[lastClassName].Fields[fieldNum].Type;
                    break;
                }
            }
        }
        
        _stack.Push(_heapAddresses[address]);
        return null;
    }

    public override object? VisitInitInstruction(CompiledAbaScriptParser.InitInstructionContext context)
    {
        var varName = context.ID().GetText();
        var className = context.className().GetText();
        Storage variableStorage = DetermineStorage(varName, className);

        // просто добавляем переменную в стек
        if (variableStorage == Storage.Stack)
        {
            if (!_variables.TryGetValue(varName, out var st1))
            {
                _variables[varName] = new Stack<Variable>();
            }
            _variables[varName].Push(new Variable(className, _stackTop, Storage.Stack, varName));
            _stackAddresses[_stackTop] = 0;
            _stackTop++;
            return null;
        }
        
        if (!_variables.TryGetValue(varName, out var st2))
        {
            _variables[varName] = new Stack<Variable>();
        }
        _variables[varName].Push(new Variable(className, _stackTop, Storage.Heap, varName));
        _stackAddresses[_stackTop] = InitObject(className, _heapTop);
        _stackTop++;
        return null;
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
        var varName = context.varName().GetText();
        if (_stack.Count == 0)
        {
            throw new RuntimeException("Stack underflow during STORE");
        }

        var oldValue = 0;
        var newValue = 0;
        
        if (!varName.Contains("."))
        {
            if (!_variables.TryGetValue(varName, out var value))
            {
                throw new RuntimeException("Variable not found: " + varName);
            }

            oldValue = _stackAddresses[value.Peek().Address];
            newValue = _stack.Pop();
            _stackAddresses[value.Peek().Address] = newValue;
            if (!SimpleTypes.IsSimple(value.Peek().Type))
            {
                _linkCounter[oldValue]--;
                _linkCounter[newValue]++;
                if (_linkCounter[oldValue] == 0)
                {
                    DeleteHeapObject(value.Peek().Type, oldValue);
                }
            }
            
            return null;
        }
        
        // ищем нужное поле в куче и присваиваем ему значение
        List<string> fields = varName.Split('.').ToList();
        int address = _stackAddresses[_variables[fields[0]].Peek().Address];
        string lastClassName = _variables[fields[0]].Peek().Type;
        for (int i = 1; i < fields.Count; i++)
        {
            for (int j = address; j < address + _classInfos[lastClassName].Fields.Length; j++)
            {
                var fieldNum = j - address; 
                if (fields[i] == _classInfos[lastClassName].Fields[fieldNum].Name)
                {
                    if (SimpleTypes.IsSimple(_classInfos[lastClassName].Fields[fieldNum].Type) || i == fields.Count - 1)
                    {
                        address = j;
                    }
                    else
                    {
                        address = _heapAddresses[j];
                    }
                    
                    lastClassName = _classInfos[lastClassName].Fields[fieldNum].Type;
                    break;
                }
            }
        }
        
        oldValue = _heapAddresses[address];
        newValue = _stack.Pop();
        _heapAddresses[address] = newValue;
        if (!SimpleTypes.IsSimple(lastClassName))
        {
            _linkCounter[oldValue]--;
            _linkCounter[newValue]++;
            if (_linkCounter[oldValue] == 0)
            {
                DeleteHeapObject(lastClassName, oldValue);
            }
        }
        return null;
    }

    public int InitObject(string className, int startAddress)
    {
        _linkCounter.TryGetValue(startAddress, out var value);
        _linkCounter[startAddress] = value + 1;
        
        _heapTop += _classInfos[className].Fields.Length;
        var varLimit = _heapTop;
        for (int i = _heapTop - _classInfos[className].Fields.Length; i < varLimit; i++)
        {
            var field = _classInfos[className].Fields[i - _heapTop + _classInfos[className].Fields.Length];
            if (!SimpleTypes.IsSimple(field.Type))
            {
                _heapAddresses[i] = InitObject(field.Type, _heapTop); // задаём сразу указатели на объекты внутри объекта
            }
            else
            {
                _heapAddresses[i] = 0;
            }
        }

        return startAddress;
    } 
}