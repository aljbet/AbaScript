func int main() {
    int n = 20;
    int a;
    int i;
    int j;
    int isPrime = 1;
    for (i = 2; i <= n; i = i + 1;) {
        isPrime = 1;
        for (j = 2; j * j <= i; j = j + 1;) {
            if (i % j == 0) {
                isPrime = 0;
            }
            else {}
        }
        if (isPrime == 1) {
            print(i);
        }
        else {}
    }
    
    return 0;
}