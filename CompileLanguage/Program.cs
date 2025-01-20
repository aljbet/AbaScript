using CompileLanguage.InterpreterClasses;

const string program = @"
INIT x int
INIT y int
INIT z testClass1

PUSH 1          # Push 1 onto the stack
STORE z.test1
PUSH 2          # Push 2 onto the stack
STORE z.test3

PUSH 10         # Push 10 onto the stack
STORE x         # Store 10 in variable x

PUSH 5          # Push 5 onto the stack
STORE y         # Store 5 in variable y

LOAD x         # Load x onto the stack
LOAD y         # Load y onto the stack

SUB             # Subtract y from x (10 - 5 = 5)

INIT result int
IF_THEN_ELSE true_block false_block   # Jump to 'else' if the result of the subtraction was not 0

true_block:
ENTER_SCOPE
INIT w testClass2
PUSH 7          # Push 7 onto the stack
STORE w.test1
PUSH 8          # Push 8 onto the stack
STORE w.test2

LOAD w
STORE z.test2

INIT z testClass1
PUSH 4          # Push 4 onto the stack
STORE z.test1
PUSH 5          # Push 5 onto the stack
STORE z.test3

LOAD z.test1    # Print the result (should be 4)
PRINT
LOAD z.test3    # Print the result (should be 5)
PRINT

JMP end_if      # Jump to 'end_if'

false_block:          # else block label
ENTER_SCOPE
PUSH 0          # Push 0 (false) onto the stack
STORE result    # Store 0 in 'result'

end_if:         # end of if-else block
EXIT_SCOPE

LOAD z.test1   # Print the result (should be 1)
PRINT
LOAD z.test3
PRINT          # Print the result (should be 2)

LOAD z.test2.test1
PRINT          # Print the result (should be 7)
LOAD z.test2.test2
PRINT          # Print the result (should be 8)

HALT
        ";

var classesDict = new Dictionary<string, ClassInfo>();
var fields1 = new List<FieldInfo>(new []
{
        new FieldInfo("int", "test1"),
        new FieldInfo("testClass2", "test2"),
        new FieldInfo("int", "test3"),
        new FieldInfo("testClass2", "test4"),
});
var fields2 = new List<FieldInfo>(new []
{
        new FieldInfo("int", "test1"),
        new FieldInfo("int", "test2"),
});
classesDict.Add("testClass1", new ClassInfo(fields1));
classesDict.Add("testClass2", new ClassInfo(fields2));

var interpreter = new CompiledAbaScriptInterpreter(classesDict);
interpreter.Interpret(program);