func int main() {
    int a[10];
    a[0] = 5;
    a[1] = 10;
    print(a[1]);
    a[1] = a[1] * a[0];
    print(a[0]);
    print(a[1]);
    print(a[2]);
    # print(a[1001]);
    
    if (a[0] < a[1]) {
        print(1);
    } else {
        print(0);
    }

    return 0;
}