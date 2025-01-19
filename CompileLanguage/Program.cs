using CompileLanguage.InterpreterClasses;

const string program = @"
PUSH 10        # Push 10 onto the stack
STORE x         # Store 10 in variable x

PUSH 10         # Push 5 onto the stack
STORE y         # Store 5 in variable y

LOAD x         # Load x onto the stack
LOAD y         # Load y onto the stack

SUB             # Subtract y from x (10 - 5 = 5)

IF_THEN_ELSE true_block false_block   # Jump to 'else' if the result of the subtraction was not 0

true_block:
PUSH 1         # Push 1 (true) onto the stack
STORE result    # Store 1 in 'result'
JMP end_if      # Jump to 'end_if'

false_block:          # else block label
PUSH 0         # Push 0 (false) onto the stack
STORE result    # Store 0 in 'result'

end_if:       # end of if-else block

LOAD result    # Load the value of 'result'
PRINT          # Print the result (should be 1)

HALT
        ";

var interpreter = new CompiledAbaScriptInterpreter();
interpreter.Interpret(program);