int x = 10;
int y = 20;
string message = "Hello, AbaScript!";

# Function definition
func int add(int a, int b) {
    return a + b;
}

# Function call
int result = add(x, y);

# Print result
print("The result of adding x and y is: ");
print(result);

# Conditional statement
if (x < y) {
    print("x is less than y");
} elif (x == y) {
    print("x is equal to y");
} else {
    print("x is greater than y");
}

# While loop
int counter = 0;
while (counter < 5) {
    print("Counter: ");
    print(counter);
    counter = counter + 1;
}

# For loop
int i = 0;
for (i = 0;i < 3; i = i + 1;) {
    print("For loop iteration: ");
    print(i);
}

# Input statement (commented out as it requires user interaction)
# input(x);

# Break and continue statements
int j = 0;
for (j = 0; j < 10; j = j + 1;) {
    if (j == 5) {
        break;
    }
    if (j % 2 == 0) {
        continue;
    }
    print("Odd number: ");
    print(j);
}