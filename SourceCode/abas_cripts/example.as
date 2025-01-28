class MyClass {
    int a;
    func int inc() {
        a = a + 1;
        return a;
    }
}

func int main() {
    MyClass mc = new MyClass;
    print(mc.inc());
    return 0;
}