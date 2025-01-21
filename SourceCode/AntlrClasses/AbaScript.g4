grammar AbaScript;

// Точка входа
script
    : (functionDef | classDef)* EOF
    ;

// ---------------------
//      Инструкции
// ---------------------
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
    | fieldAccess
    ;

// Объявление переменной
variableDeclaration
    : type ID ('[' NUMBER ']')? ('=' expr)? ';'
    ;

// Присваивание
assignment
    : (ID ('[' expr ']')? | fieldAccess) '=' expr ';'
    ;

// Ввод данных
inputStatement
    : 'input' '(' (ID ('[' expr ']')?) ')' ';'
    ;

// Вывод данных
outputStatement
    : 'print' '(' expr ')' ';'
    ;

// Возврат
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

// ---------------------
//    Функции
// ---------------------
functionDef
    : 'func' returnType ID '(' (typedParam (',' typedParam)*)? ')' block
    ;

// Параметры функции
typedParam
    : type ID ('[' ']')?
    ;

// Вызов функции
funcCall
    : ID '(' (expr (',' expr)*)? ')'
    ;

// Типы
type
    : 'int'
    | 'string'
    | 'bool'
    | ID
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

// ---------------------
//   Логические выражения
// ---------------------
logicalExpr
    : logicalExpr '&&' condition   # AndExpr
    | logicalExpr '||' condition   # OrExpr
    | condition                    # ConditionExpr
    ;

condition
    : expr comparisonOp expr
    | NOT '(' logicalExpr ')'
    | '(' logicalExpr ')'
    ;

comparisonOp
    : '==' | '!=' | '<' | '<=' | '>' | '>='
    ;

// ---------------------
//   Выражения
// ---------------------
expr
    : expr ('+' | '-') term                 # AddSub
    | term                                  # TermExpr
    ;

term
    : term ('*' | '/' | '%') factor         # MulDivMod
    | factor                                # FactorTerm
    ;

factor
    : '-' factor                            # UnaryMinus
    | '(' expr ')'                          # Parens
    | NUMBER                                # Number
    | STRING                                # String
    | ID ('[' expr ']')?                    # VariableOrArrayAccess
    | funcCall                              # FunctionalCall
    | methodCall                            # MethCall
    | fieldAccess                           # ClassFieldAccess
    | 'new' ID                              # NewClass
    ;

// ---------------------
// Операторы управления
// ---------------------
breakStatement
    : 'break' ';'
    ;

continueStatement
    : 'continue' ';'
    ;

// ---------------------
//      Классы
// ---------------------
classDef
    : 'class' ID '{' classMember* '}'
    ;

classMember
    : variableDeclaration
    | functionDef
    ;

// ---------------------
//   Методы и поля
// ---------------------
methodCall
    : ID '.' ID '(' (expr (',' expr)*)? ')'
    ;

fieldAccess
    : ID '.' ID
    ;

// ---------------------
//     Лексические правила
// ---------------------
ID
    : [a-zA-Z_][a-zA-Z0-9_]*    // Идентификатор
    ;

NUMBER
    : [0-9]+
    ;

STRING
    : '"' (~["])* '"'
    ;
NOT
    : '!'
    ;
WS
    : [ \t\r\n]+ -> skip
    ;
COMMENT
    : '#' ~[\r\n]* -> skip
    ;