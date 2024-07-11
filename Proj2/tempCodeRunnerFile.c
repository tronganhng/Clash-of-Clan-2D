#include <stdio.h>
#include <stdlib.h>
struct Date {
	int ngay;
	int thang;
	int nam;
};
enum trinhDo { nam_1, nam_2, nam_3, nam_4, nam_5};

struct SinhVien {
    long long mssv;
    char hoTen[50];
    struct Date ngaySinh;
    float GPA;
    enum trinhDo td;
};

int main() {

    int N;
    printf("So luong sinh vien la :");
    scanf("%d", &N);
    
    struct SinhVien dssv[N];
    for(int i=0; i<N ;i++) {
    	printf("\nThong tin cho sinh vien thu %d\n", i+1);
    	
    	printf("MSSV :");
    	scanf("%lld", &dssv[i].mssv);
    	
    	printf("Ho va ten :");
    	scanf("%s", &dssv[i].hoTen);
    	
    	printf("Ngay thang nam sinh :");
    	scanf("%d %d %d", &dssv[i].ngaySinh.ngay,&dssv[i].ngaySinh.thang, &dssv[i].ngaySinh.nam);
    	
    	printf("Diem GPA :");
    	scanf("%f", &dssv[i].GPA);
    	
    	printf("\nTrang thai hoc :");
    	int td;
    	scanf("%d", &td);
    	dssv[i].td = (enum trinhDo)td;
    }
    	printf("\nThong tin sinh vien:\n ");
    	for(int i=0;i<N; i++) {
    		printf("Sinh vien thu %d:\n", i+1);
    		printf("MSSV: %d\n", dssv[i].mssv);
    		printf("Ho va ten: %s\n", dssv[i].hoTen);
    		printf("Ngay thang nam sinh: %d %d %d\n", dssv[i].ngaySinh.ngay, dssv[i].ngaySinh.thang, dssv[i].ngaySinh.nam);
    		printf("Diem GPA: %.2f\n", dssv[i].GPA);
    		printf("Trang thai nam hoc: ");
    		switch (dssv[i].td) {
            case nam_1:
                printf("Nam thu nhat\n");
                break;
            case nam_2:
                printf("Nam thu hai\n");
                break;
            case nam_3:
                printf("Nam thu ba\n");
                break;
            case nam_4:
                printf("Nam thu tu\n");
                break;
            case nam_5:
			    printf("Nam thu nam\n");    
            default:
                printf("Khong xac dinh\n");
                break;
        }
		}
		return 0;
	}