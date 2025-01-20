using CompileLanguage.BaseAntlrClasses;
using Microsoft.VisualBasic.FileIO;

namespace CompileLanguage.InterpreterClasses;

public partial class CompiledAbaScriptInterpreter
{
    public override object? VisitEnterScope(CompiledAbaScriptParser.EnterScopeContext context)
    {
        _scopeStack.Push(_stackTop); // тут по сути в один стек умещаем несколько, под каждый скоуп
        return null;
    }

    public override object? VisitExitScope(CompiledAbaScriptParser.ExitScopeContext context)
    {
        var stackPointer = _scopeStack.Pop();
        // чистим стек
        for (int i = _stackTop - 1; i >= stackPointer; i--)
        {
            var j = _stackAddresses[i];
            // ищем название переменной по её адресу (возможно стоит добавить ещё одну мапу)
            var variable = new Variable();
            foreach (var kv in _variables.Where(kv => _stackAddresses[kv.Value.Peek().Address] == j))
            {
                variable = kv.Value.Peek();
            }

            if (variable.StorageType == StorageType.Heap)
            {
                DeleteHeapObject(variable.Type, _stackAddresses[variable.Address]);   
            }

            // в любом случае снимаем всё со стека
            _stackAddresses.Remove(i);
            _variables[variable.Name].Pop();
            if (_variables[variable.Name].Count == 0)
            {
                _variables.Remove(variable.Name);
            }
        }
        _stackTop = stackPointer;
        return null;
    }

    public void DeleteHeapObject(string type, int address)
    {
        // нет потребности удалять объект, если counter ненулевой
        _linkCounter[address]--;
        if (_linkCounter[address] > 0)
        {
            return;
        }
        
        // проходим по всем полям класса и чистим 
        for (int i = address; i < _classInfos[type].Fields.Count + address; i++)
        {
            var heapObject = _heapAddresses[i];
            if (!SimpleTypes.IsSimple(_classInfos[type].Fields[i - address].Type))
            {
                // в данном случае heapObject это указатель на другой объект в heap
                DeleteHeapObject(_classInfos[type].Fields[i - address].Type, heapObject);
            }
            // тут чистим само поле, его в любом случае надо чистить, так как это либо указатель на объект в heap, либо простой объект
            _heapAddresses.Remove(i);
        }
    }
}