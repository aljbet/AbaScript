func int f(int x) {
    print(x);
    return x;
}

func int allOps(int a, int b) {
    if (a==b) {
        int x = f(1);
    }
    else {
        int x = f(0);
    }
    
    if (a!=b) {
        int x = f(1);
    }
    else {
        int x = f(0);
    }
    
    if (a<b) {
        int x = f(1);
    }
    else {
        int x = f(0);
    }
    
    if (a<=b) {
        int x = f(1);
    }
    else {
        int x = f(0);
    }
    
    if (a>b) {
        int x = f(1);
    }
    else {
        int x = f(0);
    }
    
    if (a>=b) {
        int x = f(1);
    }
    else {
        int x = f(0);
    }
}

func int main() {
    int a = allOps(5, 4);
    if (5==5) {
        print(f(100));
    } else {
        print(f(200));
    }
    
    int a = f(5);
    
    # почему-то продолжает выполнение после return
    if (5==5) {
        return f(1);
    } else {
    }
    
    int b = f(5);
    
    return 0;
}