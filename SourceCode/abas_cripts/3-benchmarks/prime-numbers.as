func int main() {
    int n = 0;
    for (int i = 2; i <= n; i = i + 1;) {
        int isPrime = 1;
        for (int j = 2; j * j <= i; j = j + 1;) {
            if (i % j == 0) {
                isPrime = 0;
            }
            else {
                isPrime = 1;
            }
        }
        if (isPrime == 1) {
            print(i);
        }
        else {}
    }
}