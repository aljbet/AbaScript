func int main() {
    int x[10000];
    int n = 10000;
    
    int i;
    int j;
    
    for (i = 0; i < n; i = i + 1;) {
        x[i] = 1;
    }

    for (i = 2; i * i <= n; i = i + 1;) {
        if (x[i] == 1) {
            for (j = i * i; j < n; j = j + i;) {
                x[j] = 0;
            }
        } else {}
    }
    
    for (i = 2; i < n; i = i + 1;) {
        if (x[i] == 1) {
            print(i);
        } else {}
    }
    
    return 0;
}