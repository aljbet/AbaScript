func int main() {
    int abas[10];
    abas[0] = 3;
    abas[1] = 9;
    abas[2] = 13;
    abas[3] = 4;
    abas[4] = 20;
    abas[5] = 18;
    abas[6] = 12;
    abas[7] = 17;
    abas[8] = 8;
    abas[9] = 14;
    
    int min;
    int max;
    int c;
    int i;
    int j;
    for (i = 9; i >= 0; i = i - 1;) {
        for (j = 0; j < i; j = j + 1;) {
            if (abas[j] > abas[j+1]) {
                c = abas[j];
                abas[j] = abas[j + 1];
                abas[j + 1] = c; 
            }
            else {}
        }
    }
    
    for (i = 0; i < 10; i = i+1;) {
        print(abas[i]);
    }
}