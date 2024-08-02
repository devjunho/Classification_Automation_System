#ifndef OPENCV_DB11
#define OPENCV_DB11

#include <iostream>
#include <string>
#include <sstream>
#include <mariadb/conncpp.hpp>
#include <nlohmann/json.hpp>    
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <pthread.h>
#include <arpa/inet.h>
#include <sys/socket.h>
#include <signal.h>
#include <fstream>
// #include "cppcodec/base64_rfc4648.hpp"


using namespace std;
using nlohmann::json;


#define BUF_SIZE 2048
#define MAX_CLNT 256
#define Serv_IP "10.10.21.125"
#define IMG_SIZE 3000
#define PortNumber "12345"

class DB_CONN
{
private:
    sql::Connection* conn;
public:
    DB_CONN();
    // DB 연결함수
    void IDCARD_List(const int sock);
    // 등록된 인원 목록 보내는 함수
    json Idcard_info(const string name);
    // 신분증 세부 정보 읽어오는 함수
    string Idcard_Image(const string name);
    // 신분증 이미지 정보 읽어오는 함수
    void insert_idcard(const json temp);
    // 신분증 내부 DB 추가 함수
    void insert_image(const string temp, const string idnumber);
    // 신분증 사진 DB 추가 함수
    void Visual_data(int sock);
    // 차트용 데이터 읽어오는 함수
};

void protocol(string& msg, int sock, DB_CONN& db);
// 프로토콜 확인 함수
void Image_Read(json proto, int sock, DB_CONN& db);
// 이미지 읽는 함수
void Image_Send(json proto, int sock, DB_CONN& db);
// 이미지 전송 함수
void IDinfo_Send(json proto, int sock, DB_CONN& db);
// 신분증 내부정보 전송 함수

bool read_image_from_file(const char *filepath, char **buf, int *len);
#endif //OPENCV_DB