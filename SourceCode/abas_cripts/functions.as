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

func int f4(int x[ ]) {
    print(x[0]);
    print(x[1]);
    x[2] = 10;
    
    return 0;
}

func int main() {
    # Functions
    print(f(5));
    print(f3(100, 500));

    int x[5];
    x[0] = 1;
    x[1] = 2;
    
    int y = f4(x);
    
    print(x[1]);
    print(x[2]);

    return 0;
}