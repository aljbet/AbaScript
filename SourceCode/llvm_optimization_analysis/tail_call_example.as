func int factorial(int x) {
    if (x == 1) {
        return 1;
    }
    else {
        return x * factorial(x - 1);
    }
}

func int main() {
    print(factorial(20));
    
    return 0;
}