#func int f2(int x) {
#    x = x * 2;
#    return x;
#}
#
#func int f(int x) {
#    x = x + 1;
#    print(x);
#    print(f2(x));
#    print(x);
#    return x;
#}
#
#func int f3(int x, int y) {
#    print(x);
#    print(y);
#    x = y;
#    y = f2(y);
#    print(x);
#    print(y);
#    return 0;
#}

func int f(int x) {
    print(x);
    return x;
}

func int factorial(int x) {
    if (x > 0) {
        return x * factorial(x - 1);
    } else {
        return 1; 
    }
}

func int main() {
#    # Functions
#    print(f(5));
#    print(f3(100, 500));
#
#    # Arithmetic operations
#    print(48 - 50);
#    print(5 + 21 - 12);
#    print(3 + 2 + 1);
#    print(50 * 2);
#    print(50 / 2);
#    print((0 - 50) / 2);
#    print(7 % 2);
#    print(6 % 3);
#    print((0 - 50) % 2);
    if (5==4) {
        int x = f(1);
    }
    else {
        int x = f(0);
    }
    
    if (5!=4) {
        int x = f(1);
    }
    else {
        int x = f(0);
    }
    
    if (5<5) {
        int x = f(1);
    }
    else {
        int x = f(0);
    }
    
    if (5<=5) {
        int x = f(1);
    }
    else {
        int x = f(0);
    }
    
    if (5>4) {
        int x = f(1);
    }
    else {
        int x = f(0);
    }
    
    if (4>=4) {
        int x = f(1);
    }
    else {
        int x = f(0);
    }
    
    if (5==5) {
        print(100);
    } else {
        print(200);
    }
    
    int a = f(5);
    
    print(factorial(5));
    
    return 5;
}

# Variables, input, output, + and -
# int x;
# input(x);
# int y = 20;
# string hello = "Hello";
# string name;
# input(name);
# print(x + y);
# print(x-y + 1);
# print(hello + ", " + name + "!");
#
# Assignment
# x = x + y;
# print(x);
# name = "dear " + name;
# print(hello + ", " + name + "!");
#
# Function definition
# func int add(int a, int b) {
#     return a + b;
# }
# 
# # Function call
# int result = add(x, y);
# 
# # Print result
# print("The result of adding x and y is: ");
# print(result);
# 
# # Conditional statement
# if (x < y) {
#     print("x is less than y");
# } elif (x == y) {
#     print("x is equal to y");
# } else {
#     print("x is greater than y");
# }
# 
# # While loop
# int counter = 0;
# while (counter < 5) {
#     print("Counter: ");
#     print(counter);
#     counter = counter + 1;
# }
# 
# # For loop
# int i = 0;
# for (i = 0;i < 3; i = i + 1;) {
#     print("For loop iteration: ");
#     print(i);
# }
# 
# # Input statement (commented out as it requires user interaction)
# # input(x);
# 
# # Break and continue statements
# int j = 0;
# for (j = 0; j < 10; j = j + 1;) {
#     if (j == 5) {
#         break;
#     }
#     if (j % 2 == 0) {
#         continue;
#     }
#     print("Odd number: ");
#     print(j);
# }# 