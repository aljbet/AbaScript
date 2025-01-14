# func int f(int x) {
#     print(x);
#     return x;
# }
# 
# func int allOps(int a, int b) {
#     if (a==b) {
#         int x = f(1);
#     }
#     else {
#         int x = f(0);
#     }
#     
#     if (a!=b) {
#         int x = f(1);
#     }
#     else {
#         int x = f(0);
#     }
#     
#     if (a<b) {
#         int x = f(1);
#     }
#     else {
#         int x = f(0);
#     }
#     
#     if (a<=b) {
#         int x = f(1);
#     }
#     else {
#         int x = f(0);
#     }
#     
#     if (a>b) {
#         int x = f(1);
#     }
#     else {
#         int x = f(0);
#     }
#     
#     if (a>=b) {
#         int x = f(1);
#     }
#     else {
#         int x = f(0);
#     }
#     
#     return 0;
# }
# 
# func int conditions() {
#     if ((!(!(5==5)))) {
#         return 1;
#     }
#     else {
#         return 0;
#     }
# }

func int andOr() {
    if ((5!=5) && (3==3)) {
        print(1);
    }
    else {
        print(0);
    }
    
    if ((3==3) && (5!=5)) {
        print(1);
    }
    else {
        print(0);
    }
}

func int main() {
    # int a = allOps(5, 4);
    # if (5==5) {
    #     print(f(100));
    # } else {
    #     print(f(200));
    # }
    # 
    # int a = f(5);
    # 
    # print(conditions());
    
    int ao = andOr();
    
    return 0;
}