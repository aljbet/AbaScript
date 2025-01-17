func int main() {
    int arr[5];
    arr[0] = 100;
    arr[1] = 1;
    arr[2] = -1;
    arr[3] = 1000;
    arr[4] = 10000;
    int n = 5;
    int i = 0;
    int j = 0;
    int swapped = 0;

    while (i < n - 1) {
        j = 0;
        swapped = 0;
        while (j < n - i - 1) {
            if (arr[j] > arr[j + 1]) {
                int temp = arr[j];
                arr[j] = arr[j + 1];
                arr[j + 1] = temp;
                swapped = 1;
            }
            j = j + 1;
        }

        if (swapped == 0) {
            break;
        }
        i = i + 1;
    }

    i = 0;
    while (i < n) {
        print(arr[i]);
        i = i + 1;
    }
}