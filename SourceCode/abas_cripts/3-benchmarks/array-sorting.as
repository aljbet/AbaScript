func int getMax(int x, int y) {
    if (x > y) {
        return x;
    }
    else {
        return y;
    }
}

#func int getMin(int x, int y) {
#    if (x < y) {
#        return x;
#    }
#    else {
#        return y;
#    }
#}

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
    #int c;
    #for (int j = 0; j < 9; j = j + 1;) {
        #min = getMin(abas[1], abas[1+1]);
        max = getMax(abas[1], abas[1+1]);
        abas[1] = min;
        abas[1+1] = max;
        #if (abas[j] < abas[j+1]) {
        #    c = abas[j];
        #    #abas[j] = abas[j + 1];
        #    abas[j]=1;
        #    #abas[j + 1] = c; 
        #}
        #else {
        #    c = abas[j];
        #}
    #}
    
    for (int i = 0; i < 10; i = i+1;) {
        print(abas[i]);
    }
}