func int main() {
    int abas[1000];
    int n = 1000;

    int i;
    int j;
    int c;
    
    for (int i = 0; i < n; i = i + 1;) {
        abas[i] = n - i;
    }

    for (i = n - 1; i >= 0; i = i - 1;) {
        for (j = 0; j < i; j = j + 1;) {
            if (abas[j] > abas[j+1]) {
                c = abas[j];
                abas[j] = abas[j + 1];
                abas[j + 1] = c; 
            }
            else {}
        }
    }
    
    for (i = 0; i < n; i = i + 1;) {
        print(abas[i]);
    }
}