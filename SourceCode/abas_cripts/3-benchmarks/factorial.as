func int factorial(int x) {
    if (x==1) {
        return 1;
    }
    else {
        return x*factorial(x-1);
    }
}

# работает только до 12
func int main() {
    print(factorial(12));
}