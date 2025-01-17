#func int checkDiv(int i, int j) {
#    if (i % j == 0) {
#        return 0;
#    }
#    else {
#        return 1;
#    }
#}
#
func int isPrimeNumber(int i, int isPrime) {
    for (int j = 2; j * j <= i; j = j + 1;) {
        if (i % j == 0) {
            isPrime = 0;
        }
        else {}
        #isPrime = isPrime * checkDiv(i, j);
    }
    if (isPrime == 1) {
        print(i);
    }
    else {}
}

func int main() {
    int n = 20;
    int a;
    #int isPrime = 1;
    for (int i = 2; i <= n; i = i + 1;) {
        #isPrime = 1;
        #if (1==1) {} else {}
        #for (int j = 2; j < i; j = j + 1;) {}
        #for (int j = 2; j * j <= i; j = j + 1;) {
            #print(i);
            #print(j);
            #print(734234);
            #if (i % j == 0) {
            #    isPrime = 0;
            #}
            #else {}
            ##isPrime = isPrime * checkDiv(i, j);
        #}
        #if (isPrime == 1) {
        #    print(i);
        #}
        #else {}
        a = isPrimeNumber(i, 1);
    }
}