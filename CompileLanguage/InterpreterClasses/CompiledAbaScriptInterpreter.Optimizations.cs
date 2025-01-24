using CompileLanguage.BaseAntlrClasses;

namespace CompileLanguage.InterpreterClasses;

public partial class CompiledAbaScriptInterpreter
{
    private void LoopUnroll(string curLabel)
    {
        // проверяем есть ли в кеше
        if (_optimizationCache.ContainsKey(curLabel))
        {
            for (int index = 0; index < _optimizationCache[curLabel].Count; index++)
            {
                Visit(_optimizationCache[curLabel][index]);
                if (_jumpDestination != -1)
                {
                    index = _jumpDestination - 1 - _commandPos;
                    _jumpDestination = -1;
                }
            }

            _commandPos = _labels[Keywords.FOR_END_LABEL + curLabel.Substring(Keywords.FOR_LABEL.Length,
                curLabel.Length - Keywords.FOR_LABEL.Length - 1)] + 1;
            return;
        }

        // узнаем число n
        // команда LOAD что то на 3 выше for_label_
        Visit(_statements[_commandPos - 4]);
        Visit(_statements[_commandPos - 3]);
        var n = _stack.Pop() - _stack.Pop();
        // number = на что заканчивается label
        var numberFor = curLabel.Substring(Keywords.FOR_LABEL.Length,
            curLabel.Length - Keywords.FOR_LABEL.Length - 1);
        if (_statements[_labels[Keywords.FOR_END_LABEL + numberFor] - 4].GetText() !=
            Keywords.PUSH + " 1")
        {
            return;
        }

        // считываем все до JMP for_logic_label_{number} и запоминаем в особую структуру (массив или одна строка).
        var posInForLoop = _commandPos + 1;
        var statementsInForLoop = new List<CompiledAbaScriptParser.StatementContext>();
        for (int i = 0; i < n; i++)
        {
            while (_statements[posInForLoop].GetText() != Keywords.JMP + Keywords.FOR_LOGIC_LABEL + numberFor)
            {
                statementsInForLoop.Add(_statements[posInForLoop]);
                posInForLoop++;
            }
            
            posInForLoop = _commandPos + 1;
        }

        _optimizationCache.Add(curLabel, statementsInForLoop);
        for (int index = 0; index < statementsInForLoop.Count; index++)
        {
            Visit(statementsInForLoop[index]);
            if (_jumpDestination != -1)
            {
                index = _jumpDestination - 1 - _commandPos;
                _jumpDestination = -1;
            }
        }

        _commandPos = posInForLoop + 1;
    }
    
    private void TailCall() {}
}