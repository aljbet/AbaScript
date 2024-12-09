grammar AbaScript;

// Входная точка программы
script: (statement | functionDef)* EOF;

// Определения инструкций
statement
    : variableDeclaration
    | assignment
    | inputStatement
    | outputStatement
    | ifStatement
    | whileStatement
    | forStatement
    | returnStatement
    | funcCall
    | breakStatement
    | continueStatement
    ;

// Объявление переменной
variableDeclaration
    : 'var' ID ('[' NUMBER ']')? ('=' expr)? ';'
    ;

// Присваивание значения переменной
assignment
    : (ID ('[' expr ']')?) '=' expr ';'
    ;

// Ввод данных
inputStatement
    : 'input' '(' (ID ('[' expr ']')?) ')' ';'
    ;

// Вывод данных
outputStatement
    : 'print' '(' expr ')' ';'
    ;

// Возврат значения из функции
returnStatement
    : 'return' expr ';'
    ;

// Условная конструкция
ifStatement
    : 'if' '(' logicalExpr ')' block
      ('elif' '(' logicalExpr ')' block)*
      ('else' block)?
    ;

// Цикл while
whileStatement
    : 'while' '(' logicalExpr ')' block
    ;

// Цикл for
forStatement
    : 'for' '(' (variableDeclaration | assignment)? logicalExpr? ';' assignment? ')' block
    ;

// Определение функции
functionDef
    : 'func' ID '(' (ID (',' ID)*)? ')' block
    ;

// Вызов функции
funcCall
    : ID '(' (expr (',' expr)*)? ')'
    ;

// Блок инструкций
block
    : '{' statement* '}'
    ;

// Логическое выражение
logicalExpr
    : logicalExpr '&&' condition  # AndExpr
    | logicalExpr '||' condition  # OrExpr
    | condition                   # ConditionExpr
    ;

// Условие (логическое выражение)
condition
    : expr comparisonOp expr
    ;

// Операции сравнения
comparisonOp
    : '==' | '!=' | '<' | '<=' | '>' | '>='
    ;

// Выражения
expr
    : expr ('+' | '-') term        # AddSub
    | term                        # TermExpr
    ;

term
    : term ('*' | '/' | '%') factor  # MulDivMod
    | factor                        # FactorTerm
    ;

factor
    : '-' factor                 # UnaryMinus
    | '(' expr ')'               # Parens
    | NUMBER                    # Number
    | STRING                    # String
    | ID ('[' expr ']')?        # VariableOrArrayAccess
    | funcCall                  # FunctionalCall
    ;

// Break statement
breakStatement
    : 'break' ';'
    ;

// Continue statement
continueStatement
    : 'continue' ';'
    ;

// Лексеры
ID: [a-zA-Z_][a-zA-Z0-9_]*;       // Идентификаторы (добавлена поддержка _)
NUMBER: [0-9]+;                   // Целые числа
STRING: '"' (~["])* '"';          // Строки
WS: [ \t\r\n]+ -> skip;           // Пробелы
COMMENT: '#' ~[\r\n]* -> skip;    // Комментарии