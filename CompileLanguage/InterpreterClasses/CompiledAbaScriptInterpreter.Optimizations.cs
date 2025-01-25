using System.Text;
using System.Text.RegularExpressions;
using AbaScript.AntlrClasses;
using Antlr4.Runtime;
using CompileLanguage.BaseAntlrClasses;

namespace CompileLanguage.InterpreterClasses;

public partial class CompiledAbaScriptInterpreter
{
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

    private string CreateNewName(string oldLabel, int additionalNumber)
    {
        return $"{oldLabel.TrimEnd(':')}${additionalNumber}";
    }

    private BlockInfo Unroll(string numberFor, BlockInfo block)
    {
        var forLabel = block.Labels[Keywords.FOR_LABEL + numberFor];
        var forLogicLabel = block.Labels[Keywords.FOR_LOGIC_LABEL + numberFor];
        var forEndLabel = block.Labels[Keywords.FOR_END_LABEL + numberFor];

        Dictionary<string, int> newLabels = new();
        List<string> newCompiled = new();
        
        const int bodyCnt = 10;
        
        string newLine;

        // condition
        for (int i = forLogicLabel; i <= forLabel; i++)
        {
            if (block.Statements[i].label() != null)
            {
                var label = block.Statements[i].GetText().TrimEnd(':');
                newLabels[label] = newCompiled.Count;
            }

            newCompiled.Add(block.Compiled[i]);

            if (i - forLogicLabel == 2) // n - n % bodyCnt
            {
                newLine = newCompiled[newCompiled.Count - 1];
                newCompiled.Add(newLine);
                newLine = $"{Keywords.PUSH} {bodyCnt}";
                newCompiled.Add(newLine);
                newLine = Keywords.MOD;
                newCompiled.Add(newLine);
                newLine = Keywords.SUB;
                newCompiled.Add(newLine);
            }
        }

        // body unrolling
        for (int i = 0; i < bodyCnt; i++)
        {
            for (int j = forLabel + 1; j < forEndLabel - 1; j++)
            {
                if (block.Statements[j].label() != null) // make new unique label
                {
                    string newLabel = CreateNewName(block.Statements[j].GetText(), i);
                    newLabels[newLabel] = newCompiled.Count;
                    newCompiled.Add(newLabel);
                } 
                else if (block.Statements[j].instruction().ifThenElseInstruction() != null) // update labels
                {
                    var statement = block.Statements[j].instruction().ifThenElseInstruction();
                    var newLabelThen = CreateNewName(statement.labelRef(0).GetText(), i);
                    var newLabelElse = CreateNewName(statement.labelRef(1).GetText(), i);
                    var newStatement = $"{Keywords.IF_THEN_ELSE} {newLabelThen} {newLabelElse}";
                    newCompiled.Add(newStatement);
                } 
                else if (block.Statements[j].instruction().jmpInstruction() != null)
                {
                    var statement = block.Statements[j].instruction().jmpInstruction();
                    var newLabel = CreateNewName(statement.labelRef().GetText(), i);
                    var newStatement = $"{Keywords.JMP} {newLabel}";
                    newCompiled.Add(newStatement);
                }
                else
                {
                    newCompiled.Add(block.Compiled[j]);
                }
            }
        }
        
        // finish loop
        newCompiled.Add(block.Compiled[forEndLabel - 1]); // jump
        newCompiled.Add(block.Compiled[forEndLabel]);
        var endLabel = block.Statements[forEndLabel].GetText();
        newLabels[endLabel.Remove(endLabel.Length - 1)] = newCompiled.Count - 1;
        
        
        // remaining iterations
        _uniqueIndex += bodyCnt;
        //init
        var iName = CreateNewName("i", _uniqueIndex);
        newLine = $"{Keywords.INIT} {iName} int";
        newCompiled.Add(newLine);
        
        newLine = Keywords.ENTER_SCOPE;
        newCompiled.Add(newLine);

        newLine = $"{Keywords.PUSH} 0";
        newCompiled.Add(newLine);
        newLine = $"{Keywords.STORE} {iName}";
        newCompiled.Add(newLine);
        
        // loop cond
        var newLoopConditionLabel = CreateNewName(Keywords.FOR_LOGIC_LABEL, _uniqueIndex);
        var newForLabel = CreateNewName(Keywords.FOR_LABEL, _uniqueIndex);
        var newForEndLabel = CreateNewName(Keywords.FOR_END_LABEL, _uniqueIndex);
        newLabels[newLoopConditionLabel] = newCompiled.Count;
        newLine = newLoopConditionLabel + ":";
        newCompiled.Add(newLine);
        newLine = $"{Keywords.LOAD} {iName}";
        newCompiled.Add(newLine);
        
        newCompiled.Add(block.Compiled[forLogicLabel + 2]);
        
        newLine = $"{Keywords.PUSH} {bodyCnt}";
        newCompiled.Add(newLine);
        
        newLine = Keywords.MOD;
        newCompiled.Add(newLine);
        newLine = Keywords.LT;
        newCompiled.Add(newLine);
        newLine = $"{Keywords.IF_THEN_ELSE} {newForLabel} {newForEndLabel}";
        newCompiled.Add(newLine);
        
        // body
        newLine = newForLabel;
        newLabels[newLine] = newCompiled.Count;
        newLine += ":";
        newCompiled.Add(newLine);
        for (int j = forLabel + 1; j < forEndLabel - 1; j++)
        {
            if (block.Statements[j].label() != null) // make new unique label
            {
                string newLabel = CreateNewName(block.Statements[j].GetText(), _uniqueIndex);
                newLabels[newLabel] = newCompiled.Count;
                newCompiled.Add(newLabel);
            } 
            else if (block.Statements[j].instruction().ifThenElseInstruction() != null) // update labels
            {
                var statement = block.Statements[j].instruction().ifThenElseInstruction();
                var newLabelThen = CreateNewName(statement.labelRef(0).GetText(), _uniqueIndex);
                var newLabelElse = CreateNewName(statement.labelRef(1).GetText(), _uniqueIndex);
                var newStatement = $"{Keywords.IF_THEN_ELSE} {newLabelThen} {newLabelElse}";
                newCompiled.Add(newStatement);
            } 
            else if (block.Statements[j].instruction().jmpInstruction() != null)
            {
                var statement = block.Statements[j].instruction().jmpInstruction();
                var newLabel = CreateNewName(statement.labelRef().GetText(), _uniqueIndex);
                var newStatement = $"{Keywords.JMP} {newLabel}";
                newCompiled.Add(newStatement);
            }
            else
            {
                newCompiled.Add(block.Compiled[j]);
            }
        }
        
        //inc
        newLine = $"{Keywords.PUSH} 1";
        newCompiled.Add(newLine);
        newLine = $"{Keywords.LOAD} {iName}";
        newCompiled.Add(newLine);
        newLine = Keywords.ADD;
        newCompiled.Add(newLine);
        newLine = $"{Keywords.STORE} {iName}";
        newCompiled.Add(newLine);
        
        // finish loop
        newLine = $"{Keywords.JMP} {newLoopConditionLabel}";
        newCompiled.Add(newLine);
        newLine = newForEndLabel;
        newLabels[newLine] = newCompiled.Count;
        newLine += ":";
        newCompiled.Add(newLine);
        newLine = Keywords.EXIT_SCOPE;
        newCompiled.Add(newLine);
        
        var lexer = new CompiledAbaScriptLexer(new AntlrInputStream(String.Join("\n", newCompiled)));
        var parser = new CompiledAbaScriptParser(new CommonTokenStream(lexer));
        var newBlock = new BlockInfo(newLabels, parser.program().statement(), newCompiled);
        _uniqueIndex++;

        return newBlock;
    }
}