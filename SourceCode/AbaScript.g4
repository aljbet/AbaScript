grammar AbaScript;

// Входная точка программы
script: (statement | functionDef | classDef)* EOF;

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
    | methodCall
    | breakStatement
    | continueStatement
    | classInstantiation
    | fieldAccess
    ;

// Объявление переменной
variableDeclaration
    : type ID ('[' NUMBER ']')? ('=' expr)? ';'
    ;

// Присваивание значения переменной
assignment
    : (ID ('[' expr ']')?) | fieldAccess '=' expr ';'
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

// Определение функции с типами
functionDef
    : 'func' returnType ID '(' (typedParam (',' typedParam)*)? ')' block
    ;

// Параметры функции с типами
typedParam
    : type ID
    ;

// Вызов функции (типизация будет проверяться на этапе семантического анализа)
funcCall
    : ID '(' (expr (',' expr)*)? ')'
    ;

// Типы данных
type
    : 'int'
    | 'string'
    | 'bool'
    ;

// Возвращаемый тип функции
returnType
    : type
    | 'void'
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
    | fieldAccess               # ClassFieldAccess
    | funcCall                  # FunctionalCall
    | methodCall                # MethCall
    ;

// Break statement
breakStatement
    : 'break' ';'
    ;

// Continue statement
continueStatement
    : 'continue' ';'
    ;
    
    
// Определение класса
classDef
    : 'class' ID '{' classMember* '}'
    ;

// Члены класса (поля и функции)
classMember
    : variableDeclaration
    | functionDef
    ;

// Создание экземпляра класса
classInstantiation
    : 'new' ID ';'
    ;
    
methodCall
    : ID '.' ID '(' (expr (',' expr)*)? ')'
    ;
    
fieldAccess
    : ID '.' ID
    ;
    

// Лексеры
ID: [a-zA-Z_][a-zA-Z0-9_]*;       // Идентификаторы (добавлена поддержка _)
NUMBER: [0-9]+;                   // Целые числа
STRING: '"' (~["])* '"';          // Строки
WS: [ \t\r\n]+ -> skip;           // Пробелы
COMMENT: '#' ~[\r\n]* -> skip;    // Комментарии