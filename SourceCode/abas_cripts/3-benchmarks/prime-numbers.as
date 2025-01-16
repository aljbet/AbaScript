func int checkDiv(int i, int j) {
    if (i % j == 0) {
        return 0;
    }
    else {
        return 1;
    }
}

func int isPrimeNumber(int i, int isPrime) {
    for (int j = 2; j * j <= i; j = j + 1;) {
       isPrime = isPrime * checkDiv(i, j);
    }
    if (isPrime == 1) {
        print(i);
    }
    else {}
}

func int main() {
    int n = 20;
    int a;
    for (int i = 2; i <= n; i = i + 1;) {
        a = isPrimeNumber(i, 1);
    }
}