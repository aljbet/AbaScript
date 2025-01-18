grammar CompiledAbaScript;

program: (statement)* EOF;

statement: label | instruction;

label: ID ':';

instruction: 
    loadInstruction
    | storeInstruction
    | pushInstruction
    | popInstruction
    | addInstruction
    | subInstruction
    | mulInstruction
    | divInstruction
    | modInstruction
    | jmpInstruction
    | jmpIfInstruction
    | callInstruction
    | retInstruction
    | printInstruction
    | haltInstruction
    ;

// Represents a LOAD instruction, which loads a variable's value onto the stack.
loadInstruction: LOAD ID;

// Represents a STORE instruction, which stores the top stack value into a variable.
storeInstruction: STORE ID;

// Represents a PUSH instruction, which pushes a value onto the stack.
pushInstruction: PUSH value;

// Represents a POP instruction, which removes the top value from the stack.
popInstruction: POP;

// Represents an ADD instruction, which adds the top two stack values.
addInstruction: ADD;

// Represents a SUB instruction, which subtracts the top stack value from the second top value.
subInstruction: SUB;

// Represents a MUL instruction, which multiplies the top two stack values.
mulInstruction: MUL;

// Represents a DIV instruction, which divides the second top stack value by the top value.
divInstruction: DIV;

// Represents a MOD instruction, which computes the modulus of the second top stack value by the top value.
modInstruction: MOD;

// Represents a JMP instruction, which jumps to a specified label.
jmpInstruction: JMP labelRef;

// Represents a JMP_IF instruction, which conditionally jumps to a label if the top stack value is non-zero.
jmpIfInstruction: JMP_IF labelRef;

// Represents a CALL instruction, which calls a function by its identifier.
callInstruction: CALL ID;

// Represents a RET instruction, which returns from a function call.
retInstruction: RET;

// Represents a PRINT instruction, which prints the top stack value.
printInstruction: PRINT;

// Represents a HALT instruction, which stops the program execution.
haltInstruction: HALT;

// A value can be a number or a string, used in PUSH instructions.
value: NUMBER;

// A label reference is an identifier used in jump instructions.
labelRef: ID;

ID  : [a-zA-Z_][a-zA-Z0-9_]*;
NUMBER : [0-9]+ ;
STRING : '"' ('\\"'|.)*? '"';

WS : [ \t\r\n]+ -> skip ;
COMMENT : '#' ~[\r\n]* -> skip ;

LOAD: 'LOAD';
STORE: 'STORE';
PUSH: 'PUSH';
POP: 'POP';
ADD: 'ADD';
SUB: 'SUB';
MUL: 'MUL';
DIV: 'DIV';
MOD: 'MOD';
JMP: 'JMP';
JMP_IF: 'JMP_IF';
CALL: 'CALL';
RET: 'RET';
PRINT: 'PRINT';
HALT: 'HALT';