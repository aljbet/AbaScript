using System.Text;
using System.Text.RegularExpressions;
using AbaScript.AntlrClasses;
using Antlr4.Runtime;
using CompileLanguage.BaseAntlrClasses;

namespace CompileLanguage.InterpreterClasses;

public partial class CompiledAbaScriptInterpreter
{
    // private void LoopUnroll(string curLabel)
    // {
    //     //Console.WriteLine(curLabel);
    //     //Console.WriteLine(_optimizationCache.ContainsKey(curLabel));
    //     // проверяем есть ли в кеше
    //     if (_optimizationCache.ContainsKey(curLabel))
    //     {
    //         for (int index = 0; index < _optimizationCache[curLabel].Count; index++)
    //         {
    //             Visit(_optimizationCache[curLabel][index]);
    //             if (_jumpDestination != -1)
    //             {
    //                 index = _jumpDestination - 1 - _commandPos;
    //                 _jumpDestination = -1;
    //             }
    //         }
    //
    //         _commandPos = _labels[Keywords.FOR_END_LABEL + curLabel.Substring(Keywords.FOR_LABEL.Length,
    //             curLabel.Length - Keywords.FOR_LABEL.Length - 1)] + 1;
    //         return;
    //     }
    //     
    //     var numberFor = curLabel.Substring(Keywords.FOR_LABEL.Length,
    //         curLabel.Length - Keywords.FOR_LABEL.Length - 1);
    //     if (_statements[_labels[Keywords.FOR_END_LABEL + numberFor] - 4].GetText() !=
    //         Keywords.PUSH + "1")
    //     {
    //         return;
    //     }
    //
    //     // узнаем число n
    //     // команда LOAD что то на 3 выше for_label_
    //     Visit(_statements[_commandPos - 4]);
    //     Visit(_statements[_commandPos - 3]);
    //     var n = _stack.Pop() - _stack.Pop();
    //     // number = на что заканчивается label
    //
    //     // считываем все до JMP for_logic_label_{number} и запоминаем в особую структуру (массив или одна строка).
    //     var posInForLoop = _commandPos + 1;
    //     var statementsInForLoop = new List<CompiledAbaScriptParser.StatementContext>();
    //     for (int i = 0; i < n; i++)
    //     {
    //         while (_statements[posInForLoop].GetText() != Keywords.JMP + Keywords.FOR_LOGIC_LABEL + numberFor)
    //         {
    //             statementsInForLoop.Add(_statements[posInForLoop]);
    //             posInForLoop++;
    //         }
    //         
    //         posInForLoop = _commandPos + 1;
    //     }
    //
    //     _optimizationCache.Add(curLabel, statementsInForLoop);
    //     // Console.WriteLine(_optimizationCache.ContainsKey(curLabel));
    //     for (int index = 0; index < statementsInForLoop.Count; index++)
    //     {
    //         Visit(statementsInForLoop[index]);
    //         if (_jumpDestination != -1)
    //         {
    //             index = _jumpDestination - 1 - _commandPos;
    //             _jumpDestination = -1;
    //         }
    //     }
    //
    //     // _commandPos = posInForLoop + 1;
    //     _commandPos = _labels[Keywords.FOR_END_LABEL + curLabel.Substring(Keywords.FOR_LABEL.Length,
    //         curLabel.Length - Keywords.FOR_LABEL.Length - 1)] + 1;
    // }


    private bool IsPossibleToUnroll(string numberFor, BlockInfo block)
    {
        var forLabel = block.Labels[Keywords.FOR_LABEL + numberFor];
        var forLogicLabel = block.Labels[Keywords.FOR_LOGIC_LABEL + numberFor];
        var forEndLabel = block.Labels[Keywords.FOR_END_LABEL + numberFor];

        if (block.Statements[forLabel - 2].GetText() != Keywords.LT || forLabel - forLogicLabel != 5)
        {
            return false;
        }

        if (block.Statements[forEndLabel - 4].GetText() != "PUSH1" && block.Statements[forEndLabel - 3].GetText() != "ADD")
        {
            return false;
        }

        if (block.Statements[forLogicLabel - 2].GetText() != "PUSH0")
        {
            return false;
        }

        return true;
    }

    private BlockInfo Unroll(string numberFor, BlockInfo block)
    {
        var forLabel = block.Labels[Keywords.FOR_LABEL + numberFor];
        var forLogicLabel = block.Labels[Keywords.FOR_LOGIC_LABEL + numberFor];
        var forEndLabel = block.Labels[Keywords.FOR_END_LABEL + numberFor];

        StringBuilder stringBuilder = new();
        Dictionary<string, int> newLabels = new();
        List<string> newCompiled = new();
        
        const int bodyCnt = 10;
        
        // same condition
        int line = 0;
        for (int i = forLogicLabel; i <= forLabel; i++)
        {
            if (block.Statements[i].label() != null)
            {
                var label = block.Statements[i].GetText();
                newLabels[label.Remove(label.Length - 1)] = line;
            }

            stringBuilder.AppendLine(block.Compiled[i]);
            newCompiled.Add(block.Compiled[i]);
            line++;

            if (i - forLogicLabel == 2) // n - n % bodyCnt
            {
                var nLine = newCompiled[newCompiled.Count - 1];
                stringBuilder.AppendLine(nLine);
                newCompiled.Add(nLine);
                line++;
                nLine = Keywords.PUSH + " " + bodyCnt;
                stringBuilder.AppendLine(nLine);
                newCompiled.Add(nLine);
                line++;
                nLine = Keywords.MOD;
                stringBuilder.AppendLine(nLine);
                newCompiled.Add(nLine);
                line++;
                nLine = Keywords.SUB;
                stringBuilder.AppendLine(nLine);
                newCompiled.Add(nLine);
                line++;
            }
        }

        // body x n
        for (int i = 0; i < bodyCnt; i++)
        {
            for (int j = forLabel + 1; j < forEndLabel - 1; j++)
            {
                if (block.Statements[j].label() != null) // make new unique label
                {
                    var label = block.Statements[j].GetText();
                    string newLabel = label.Remove(label.Length - 1) + i;
                    stringBuilder.AppendLine(newLabel);
                    newCompiled.Add(newLabel);
                    newLabels[newLabel] = line;
                } 
                else if (block.Statements[j].instruction().ifThenElseInstruction() != null) // update labels
                {
                    var statement = block.Statements[j].instruction().ifThenElseInstruction();
                    var line1 = statement.labelRef(0).GetText();
                    var line2 = statement.labelRef(1).GetText();
                    var newStatement = Keywords.IF_THEN_ELSE + " " + line1.Remove(line1.Length) + i + " " + line2.Remove(line2.Length) + i;
                    stringBuilder.AppendLine(newStatement);
                    newCompiled.Add(newStatement);
                } 
                else if (block.Statements[j].instruction().jmpInstruction() != null)
                {
                    var statement = block.Statements[j].instruction().jmpInstruction();
                    var oldLabel = statement.labelRef().GetText();
                    var newLabel = oldLabel + i;
                    var newStatement = Keywords.JMP + " " + newLabel;
                    stringBuilder.AppendLine(newStatement);
                    newCompiled.Add(newStatement);
                }
                else
                {
                    stringBuilder.AppendLine(block.Compiled[j]);
                    newCompiled.Add(block.Compiled[j]);
                }

                line++;
            }
        }
        
        // finish loop
        stringBuilder.AppendLine(block.Compiled[forEndLabel - 1]); // jump
        newCompiled.Add(block.Compiled[forEndLabel - 1]);
        line++;
        stringBuilder.AppendLine(block.Compiled[forEndLabel]);
        newCompiled.Add(block.Compiled[forEndLabel]);
        line++;
        var endLabel = block.Statements[forEndLabel].GetText();
        newLabels[endLabel.Remove(endLabel.Length - 1)] = line - 1;
        
        
        // remaining iterations
        //init
        var iName = "t";
        var newLine = Keywords.INIT + " " + iName + " int";
        stringBuilder.AppendLine(newLine);
        newCompiled.Add(newLine);
        line++;
        
        newLine = Keywords.ENTER_SCOPE; // возможно надо удалить
        stringBuilder.AppendLine(newLine);
        newCompiled.Add(newLine);

        line++;
        newLine = Keywords.PUSH + " 0";
        stringBuilder.AppendLine(newLine);
        newCompiled.Add(newLine);
        line++;
        newLine = Keywords.STORE + " " + iName;
        stringBuilder.AppendLine(newLine);
        newCompiled.Add(newLine);
        line++;
        
        // loop cond
        numberFor = "10000"; // numberFor + "." + bodyCnt;
        var loopLabel = Keywords.FOR_LOGIC_LABEL + numberFor;
        newLabels[loopLabel] = line;
        newLine = loopLabel + ":";
        stringBuilder.AppendLine(newLine);
        newCompiled.Add(newLine);
        line++;
        newLine = Keywords.LOAD + " " + iName;
        stringBuilder.AppendLine(newLine);
        newCompiled.Add(newLine);
        line++;
        // stringBuilder.AppendLine(block.Compiled[forLogicLabel + 1]);
        // newCompiled.Add(block.Compiled[forLogicLabel + 1]);
        
        // newLine = Keywords.PUSH + " " + bodyCnt;
        // stringBuilder.AppendLine(newLine);
        // newCompiled.Add(newLine);
        // line++;
        
        stringBuilder.AppendLine(block.Compiled[forLogicLabel + 2]); // возможно вот это и предыдущее надо местами поменять
        newCompiled.Add(block.Compiled[forLogicLabel + 2]);
        line++;
        
        newLine = Keywords.PUSH + " " + bodyCnt;
        stringBuilder.AppendLine(newLine);
        newCompiled.Add(newLine);
        line++;
        
        newLine = Keywords.MOD;
        stringBuilder.AppendLine(newLine);
        newCompiled.Add(newLine);
        line++;
        newLine = Keywords.LT;
        stringBuilder.AppendLine(newLine);
        newCompiled.Add(newLine);
        line++;
        newLine = Keywords.IF_THEN_ELSE + " " + Keywords.FOR_LABEL + numberFor + " " + Keywords.FOR_END_LABEL + numberFor;
        stringBuilder.AppendLine(newLine);
        newCompiled.Add(newLine);
        line++;
        
        // body
        newLine = Keywords.FOR_LABEL + numberFor;
        newLabels[newLine] = line;
        newLine += ":";
        stringBuilder.AppendLine(newLine);
        newCompiled.Add(newLine);
        line++;
        for (int j = forLabel + 1; j < forEndLabel - 1; j++)
        {
            if (block.Statements[j].label() != null) // make new unique label
            {
                var label = block.Statements[j].GetText();
                string newLabel = label.Remove(label.Length - 1) + "1000";
                stringBuilder.AppendLine(newLabel);
                newCompiled.Add(newLabel);
                newLabels[newLabel] = line;
            } 
            else if (block.Statements[j].instruction().ifThenElseInstruction() != null) // update labels
            {
                var statement = block.Statements[j].instruction().ifThenElseInstruction();
                var line1 = statement.labelRef(0).GetText();
                var line2 = statement.labelRef(1).GetText();
                var newStatement = Keywords.IF_THEN_ELSE + " " + line1 + "1000" + " " + line2 + "1000";
                stringBuilder.AppendLine(newStatement);
                newCompiled.Add(newStatement);
            } 
            else if (block.Statements[j].instruction().jmpInstruction() != null)
            {
                var statement = block.Statements[j].instruction().jmpInstruction();
                var oldLabel = statement.labelRef().GetText();
                var newLabel = oldLabel + "1000";
                var newStatement = Keywords.JMP + " " + newLabel;
                stringBuilder.AppendLine(newStatement);
                newCompiled.Add(newStatement);
            }
            else
            {
                stringBuilder.AppendLine(block.Compiled[j]);
                newCompiled.Add(block.Compiled[j]);
            }

            line++;
        }
        
        //inc
        newLine = Keywords.PUSH + " 1";
        stringBuilder.AppendLine(newLine);
        newCompiled.Add(newLine);
        line++;
        newLine = Keywords.LOAD + " " + iName;
        stringBuilder.AppendLine(newLine);
        newCompiled.Add(newLine);
        line++;
        newLine = Keywords.ADD;
        stringBuilder.AppendLine(newLine);
        newCompiled.Add(newLine);
        line++;
        newLine = Keywords.STORE + " " + iName;
        stringBuilder.AppendLine(newLine);
        newCompiled.Add(newLine);
        line++;
        
        // finish loop
        newLine = Keywords.JMP + " " + Keywords.FOR_LOGIC_LABEL + numberFor;
        stringBuilder.AppendLine(newLine);
        newCompiled.Add(newLine);
        line++;
        newLine = Keywords.FOR_END_LABEL + numberFor;
        newLabels[newLine] = line;
        newLine += ":";
        stringBuilder.AppendLine(newLine);
        newCompiled.Add(newLine);
        line++;
        newLine = Keywords.EXIT_SCOPE;
        stringBuilder.AppendLine(newLine);
        newCompiled.Add(newLine);
        
        var lexer = new CompiledAbaScriptLexer(new AntlrInputStream(stringBuilder.ToString()));
        var parser = new CompiledAbaScriptParser(new CommonTokenStream(lexer));
        var newBlock = new BlockInfo(newLabels, parser.program().statement(), newCompiled);

        return newBlock;
    }

    private void RunBlock(BlockInfo block, bool isJitNeeded, int startPos=0)
    {
        var oldLabels = _labels;
        _labels = block.Labels;
        for (var i = startPos; i < block.Statements.Count; i++)
        {
            _commandPos = i;
            if (isJitNeeded)
            {
                if (block.Statements[i].label() != null)
                {
                    var curLabel = _statements[i].label().GetText();
                    if (Regex.IsMatch(curLabel, Keywords.FOR_LABEL)) // возможно можем оптимизировать
                    {
                        var numberFor = curLabel.Substring(Keywords.FOR_LABEL.Length,
                            curLabel.Length - Keywords.FOR_LABEL.Length - 1);
                        
                        if (_optimizationCache.ContainsKey(curLabel))
                        {
                            RunBlock(_optimizationCache[curLabel], false);
                            i = block.Labels[Keywords.FOR_END_LABEL + numberFor];
                            continue;
                        }
                        if (IsPossibleToUnroll(numberFor, block))
                        {
                            var jitBlock = Unroll(numberFor, block);
                            _optimizationCache.Add(curLabel, jitBlock);

                            RunBlock(jitBlock, false);
                            i = block.Labels[Keywords.FOR_END_LABEL + numberFor];
                            continue;
                        }
                    }
                }
            }

            Visit(block.Statements[i]);
            if (_jumpDestination != -1)
            {
                i = _jumpDestination - 1;
                _jumpDestination = -1;
            }
        }
        _labels = oldLabels;
    }
}