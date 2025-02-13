﻿grammar CompiledAbaScript;

program: (statement)* EOF;

statement: label | instruction;

label: ID ':';

instruction: 
    loadInstruction
    | storeInstruction
    | initInstruction
    | pushInstruction
    | popInstruction
    | addInstruction
    | subInstruction
    | mulInstruction
    | divInstruction
    | modInstruction
    | jmpInstruction
    | ifThenElseInstruction
    | callInstruction
    | retInstruction
    | printInstruction
    | haltInstruction
    | enterScope
    | exitScope
    | andInstruction
    | orInstruction
    | notInstruction
    | eqInstruction
    | neInstruction
    | ltInstruction
    | leInstruction
    | gtInstruction
    | geInstruction
    ;

// Represents a LOAD instruction, which loads a variable's value onto the stack.
loadInstruction: LOAD varName;

// Represents an INIT instruction, which init the variable with default value;
initInstruction: INIT ID className;

// Represents a STORE instruction, which stores the top stack value into a variable.
storeInstruction: STORE varName;

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

// Represents an AND instruction, which computes the logical and of two top stack values.
andInstruction: AND;

// Represents an OR instruction, which computes the logical or of two top stack values.
orInstruction: OR;

// Represents a NOT instruction, which computes the logical not of the top stack value.
notInstruction: NOT;

// Represents an EQ instruction, which puts 1 on stack if two top stack values are equal, 0 otherwise.
eqInstruction: EQ;

// Represents a NE instruction, which puts 0 on stack if two top stack values are equal, 1 otherwise.
neInstruction: NE;

// Represents a LT instruction, which puts 1 on stack if second top value is less than top value, 0 otherwise.
ltInstruction: LT;

// Represents a LE instruction, which puts 1 on stack if second top value is less than or equal to top value, 0 otherwise.
leInstruction: LE;

// Represents a LT instruction, which puts 1 on stack if second top value is greater than top value, 0 otherwise.
gtInstruction: GT;

// Represents a LT instruction, which puts 1 on stack if second top value is greate than or equal to top value, 0 otherwise.
geInstruction: GE;

// Represents a JMP instruction, which jumps to a specified label.
jmpInstruction: JMP labelRef;

// Represents a IF_THEN_ELSE instruction, which jumps to the first label
// if the top stack value is non-zero and to the second label otherwise.
ifThenElseInstruction: IF_THEN_ELSE labelRef labelRef;

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

// A class name, used in store instruction.
className: ID;

// An ENTER_SCOPE instruction that uses in the beginning of functions, methods and if-else statements.
enterScope: ENTER_SCOPE;

// An EXIT_SCOPE instruction that uses in the end of functions, methods and if-else statements.
exitScope: EXIT_SCOPE;

// VarName can contain dot for fields.
varName: (ID|'.')*;

LOAD: 'LOAD';
STORE: 'STORE';
PUSH: 'PUSH';
POP: 'POP';
ADD: 'ADD';
SUB: 'SUB';
MUL: 'MUL';
DIV: 'DIV';
MOD: 'MOD';
AND: 'AND';
OR: 'OR';
NOT: 'NOT';
EQ: 'EQ';
NE: 'NE';
LT: 'LT';
LE: 'LE';
GT: 'GT';
GE: 'GE';
JMP: 'JMP';
IF_THEN_ELSE: 'IF_THEN_ELSE';
CALL: 'CALL';
RET: 'RET';
PRINT: 'PRINT';
HALT: 'HALT';
ENTER_SCOPE: 'ENTER_SCOPE';
EXIT_SCOPE: 'EXIT_SCOPE';
INIT: 'INIT';

ID  : [a-zA-Z_][a-zA-Z0-9_$]*('['']')?;
NUMBER : [0-9]+ ;
STRING : '"' ('\\"'|.)*? '"';

WS : [ \t\r\n]+ -> skip ;
COMMENT : '#' ~[\r\n]* -> skip ;