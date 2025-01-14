func int f2(int x) {
    x = x * 2;
    return x;
}

func int f(int x) {
    x = x + 1;
    print(x);
    print(f2(x));
    print(x);
    return x;
}

func int f3(int x, int y) {
    print(x);
    print(y);
    x = y;
    y = f2(y);
    print(x);
    print(y);
    return 0;
}

func int main() {
    # Functions
    print(f(5));
    print(f3(100, 500));

    return 0;
}