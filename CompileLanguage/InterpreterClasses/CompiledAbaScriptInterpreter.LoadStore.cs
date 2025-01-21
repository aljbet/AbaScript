using CompileLanguage.BaseAntlrClasses;
using CompileLanguage.Exceptions;

namespace CompileLanguage.InterpreterClasses;

public partial class CompiledAbaScriptInterpreter
{
    public override object? VisitLoadInstruction(CompiledAbaScriptParser.LoadInstructionContext context)
    {
        var varName = context.varName().GetText();
        var isArray = varName.EndsWith("[]");
        long idx = 0;
        if (isArray)
        {
            idx = _stack.Pop();
        }

        // ищем нужное поле в куче
        List<string> fields = varName.Split('.').ToList();
        long variableAddress = _variables[fields[0]].Peek().Address + idx;
        long stackValue = _stackAddresses[variableAddress];
        string lastClassName = _variables[fields[0]].Peek().Type;
        for (int i = 1; i < fields.Count; i++)
        {
            for (long j = stackValue; j < stackValue + _classInfos[lastClassName].Fields.Count; j++)
            {
                var fieldNum = j - stackValue;
                if (fields[i] == _classInfos[lastClassName].Fields[(int) fieldNum].Name)
                {
                    if (SimpleTypes.IsSimple(_classInfos[lastClassName].Fields[(int) fieldNum].Type))
                    {
                        stackValue = j;
                    }
                    else
                    {
                        stackValue = _heapAddresses[j];
                    }

                    lastClassName = _classInfos[lastClassName].Fields[(int) fieldNum].Type;
                    break;
                }
            }
        }

        if (!varName.Contains("."))
        {
            _stack.Push(_stackAddresses[variableAddress]);
        }
        else
        {
            _stack.Push(_heapAddresses[(int) stackValue]);
        }
        return null;
    }

    public override object? VisitInitInstruction(CompiledAbaScriptParser.InitInstructionContext context)
    {
        var varName = context.ID().GetText();
        var className = context.className().GetText();
        StorageType variableStorageType = DetermineStorage(varName, className);
        var isArray = varName.EndsWith("[]");
        long size = 1;
        if (isArray)
        {
            size = _stack.Pop();
        }

        var stackTop = _stackTop;
        for (int i = 0; i < size; i++)
        {
            int value = 0;
            if (variableStorageType == StorageType.Heap)
            {
                value = InitObject(className, _heapTop);
            }
        
            InitVariable(varName, value);   
        }
        _variables[varName].Push(new Variable(className, stackTop, variableStorageType, varName));
        return null;
    }

    // определяем, где будем хранить объект, если это поле класса или сам инстанс класса, то heap, иначе стек
    public StorageType DetermineStorage(String varName, String className)
    {
        if (!SimpleTypes.IsSimple(className))
        {
            return StorageType.Heap;
        }

        if (varName.Contains("."))
        {
            return StorageType.Heap;
        }

        return StorageType.Stack;
    }

    public override object? VisitStoreInstruction(CompiledAbaScriptParser.StoreInstructionContext context)
    {
        var varName = context.varName().GetText();
        if (_stack.Count == 0)
        {
            throw new RuntimeException("Stack underflow during STORE");
        }
        var isArray = varName.EndsWith("[]");
        long idx = 0;
        if (isArray)
        {
            idx = _stack.Pop();
        }

        long oldValue = 0;
        long newValue = 0;
        
        List<string> fields = varName.Split('.').ToList();
        long variableAddress = _variables[fields[0]].Peek().Address + idx;
        long stackValue = _stackAddresses[variableAddress];
        string lastClassName = _variables[fields[0]].Peek().Type;
        // ищем нужное поле в куче и присваиваем ему значение
        for (int i = 1; i < fields.Count; i++)
        {
            for (long j = stackValue; j < stackValue + _classInfos[lastClassName].Fields.Count; j++)
            {
                var fieldNum = j - stackValue;
                if (fields[i] == _classInfos[lastClassName].Fields[(int) fieldNum].Name)
                {
                    if (SimpleTypes.IsSimple(_classInfos[lastClassName].Fields[(int) fieldNum].Type) || i == fields.Count - 1)
                    {
                        stackValue = j;
                    }
                    else
                    {
                        stackValue = _heapAddresses[j];
                    }

                    lastClassName = _classInfos[lastClassName].Fields[(int) fieldNum].Type;
                    break;
                }
            }
        }

        if (varName.Contains("."))
        {
            oldValue = _heapAddresses[stackValue];
            newValue = _stack.Pop();
            _heapAddresses[stackValue] = newValue;
            if (!SimpleTypes.IsSimple(lastClassName))
            {
                _linkCounter[oldValue]--;
                _linkCounter[newValue]++;
                if (_linkCounter[oldValue] == 0)
                {
                    DeleteHeapObject(lastClassName, oldValue);
                }
            }   
        }
        else
        {
            oldValue = stackValue;
            newValue = _stack.Pop();
            _stackAddresses[variableAddress] = newValue;
            if (!SimpleTypes.IsSimple(lastClassName))
            {
                _linkCounter[oldValue]--;
                _linkCounter[newValue]++;
                if (_linkCounter[oldValue] == 0)
                {
                    DeleteHeapObject(lastClassName, oldValue);
                }
            }
        }

        return null;
    }

    public int InitObject(string className, int startAddress)
    {
        _linkCounter.TryGetValue(startAddress, out var value);
        _linkCounter[startAddress] = value + 1;

        _heapTop += _classInfos[className].Fields.Count;
        var varLimit = _heapTop;
        for (int i = _heapTop - _classInfos[className].Fields.Count; i < varLimit; i++)
        {
            var field = _classInfos[className].Fields[i - _heapTop + _classInfos[className].Fields.Count];
            if (!SimpleTypes.IsSimple(field.Type))
            {
                _heapAddresses[i] =
                    InitObject(field.Type, _heapTop); // задаём сразу указатели на объекты внутри объекта
            }
            else
            {
                _heapAddresses[i] = 0;
            }
        }

        return startAddress;
    }

    public void InitVariable(string varName, int value)
    {
        if (!_variables.TryGetValue(varName, out var st))
        {
            _variables[varName] = new Stack<Variable>();
        }
        
        _stackAddresses[_stackTop] = value;
        _stackTop++;
    }
}