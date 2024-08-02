#include "DB11_Server.h"

DB_CONN::DB_CONN()
{
    try
    {
        sql::Driver* driver = sql::mariadb::get_driver_instance();
        sql::SQLString url("jdbc:mariadb://10.10.21.125:3306/IDCARD_INFO");
        sql::Properties properties({{"user", "AAA"}, {"password", "1234"}});
        this->conn = driver->connect(url, properties);
    }
    catch(const sql::SQLException& ex)
    {
        cerr<<ex.what()<<endl;
        cout << "DB connect error" << endl;
        exit(1);
    } 
}

json DB_CONN::Idcard_info(const string name)
{
    json idcard;
    try
    {
        unique_ptr<sql::PreparedStatement>stmnt(conn->prepareStatement("SELECT * FROM IDCARD WHERE idcard_name = ?;"));
        stmnt->setString(1, name);
        unique_ptr<sql::ResultSet> res(stmnt->executeQuery());
        if(res->next())
        {
            idcard["name"] = res->getString(3);
            idcard["idnumber"] = res->getString(4);
            idcard["address"] = res->getString(5);
            idcard["issue_date"] = res->getString(6);
            idcard["issuer"] = res->getString(7);
        }
        else
        {
            idcard["error"] = "empty";
        }
    }
    catch(const sql::SQLException& e)
    {
        std::cerr << "Error : DB_CONN::Idcard_info = " << e.what() << '\n';
        idcard["error"] = "error";
    }
    return idcard;    
}

string DB_CONN::Idcard_Image(const string name)
{
    string temp;
    try
    {
        unique_ptr<sql::PreparedStatement>stmnt(conn->prepareStatement("SELECT idcard_image FROM IDCARD WHERE idcard_name = ?;"));
        stmnt->setString(1, name);
        unique_ptr<sql::ResultSet> res(stmnt->executeQuery());
        if(res->next())
        {
            temp = res->getString(1);
        }
    }
    catch(const std::exception& e)
    {
        cerr << e.what() << '\n';
        temp = "error";
    } 
    return temp;
}

void DB_CONN::insert_idcard(const json temp)
{
    try
    {
        string name = temp["name"], 
        idnumber = temp["idnumber"], 
        address = temp["address"], 
        issue_date = temp["issue_date"], 
        issuer = temp["issuer"];
        unique_ptr<sql::PreparedStatement>stmnt(conn->prepareStatement("INSERT INTO IDCARD (idcard_name,idcard_idnumber,idcard_address,idcard_issue_date,idcard_issuer) VALUES(?,?,?,?,?);"));
        stmnt->setString(1,name);
        stmnt->setString(2,idnumber);
        stmnt->setString(3,address);
        stmnt->setString(4,issue_date);
        stmnt->setString(5,issuer);
        stmnt->executeQuery();
        cout << "insert_idcard is active!" << endl;
    }
    catch(const std::exception& e)
    {
        cerr << "DB : insert_idcard error = " << e.what() << '\n';
    }
}

void DB_CONN::insert_image(const string temp, const string idnumber)
{
    try
    {
        unique_ptr<sql::PreparedStatement>stmnt(conn->prepareStatement("UPDATE IDCARD SET idcard_image = ? WHERE idcard_idnumber = ?;"));
        stmnt->setString(1,temp);
        stmnt->setString(2,idnumber);
        stmnt->executeUpdate();
        cout << "Image Saved on DB!\n";
    }
    catch(const std::exception& e)
    {
        std::cerr << "DB : insert_image error = " << e.what() << '\n';
    }
}

void DB_CONN::Visual_data(int sock)
{
    json temp;
    try
    {
        int number;
        string region;
        json temp;
        string send_msg;
        char recv_temp[BUF_SIZE];

        unique_ptr<sql::PreparedStatement>stmnt(conn->prepareStatement("SELECT DISTINCT SUBSTRING_INDEX(idcard_address, ' ', 1) AS COLM, COUNT(SUBSTRING_INDEX(idcard_address, ' ', 1)) AS COUNT FROM IDCARD GROUP BY SUBSTRING_INDEX(idcard_address, ' ', 1);"));
        unique_ptr<sql::ResultSet> res(stmnt->executeQuery());
        while(res->next())
        {
            region = res->getString(1);
            number = res->getInt(2);
            temp["protocol"] = 19;
            temp["region"] = region;
            temp["count"] = number;
            send_msg = temp.dump();
            cout << send_msg << endl;
            send(sock, send_msg.c_str(), send_msg.size(), 0);
            read(sock, recv_temp, BUF_SIZE);
        }
        temp["protocol"] = 18;
        send_msg = temp.dump();
        send(sock, send_msg.c_str(), send_msg.size(), 0);
        read(sock, recv_temp, BUF_SIZE);
        cout << "== protocol 18 send ==\n" << send_msg << endl; 
    }
    catch(const std::exception& e)
    {
        std::cerr << "DB : Visual_data error = " << e.what() << '\n';
    }
}

void DB_CONN::IDCARD_List(const int sock)
{
    try
    {
        json temp;
        string send_msg;
        char recv_temp[BUF_SIZE];
        unique_ptr<sql::PreparedStatement>stmnt2(conn->prepareStatement("SELECT idcard_name, idcard_idnumber, idcard_address, idcard_issue_date, idcard_issuer FROM IDCARD;"));
        unique_ptr<sql::ResultSet> res2(stmnt2->executeQuery());
        while(res2->next())
        {
            temp["protocol"] = 19;
            temp["name"] = res2->getString(1);
            temp["idnumber"] = res2->getString(2);
            temp["address"] = res2->getString(3);
            temp["date"] = res2->getString(4);
            temp["issuer"] = res2->getString(5);
            send_msg = temp.dump();
            cout << send_msg << endl;
            send(sock, send_msg.c_str(), send_msg.size(), 0);
            read(sock, recv_temp, BUF_SIZE);
        }
        temp["protocol"] = 18;
        send_msg = temp.dump();
        cout << "== protocol 18 send ==\n" << send_msg << endl;
        send(sock, send_msg.c_str(), send_msg.size(), 0);
    }
    catch(const std::exception& e)
    {
        std::cerr << "IDCARD_LIST : " << e.what() << '\n';
    }   
}


// 프로토콜 확인 함수
void protocol(string& msg, int sock, DB_CONN& db)
{   
    cout << "protocol is online!\n";
    try
    {
        json j = json::parse(msg);
        // string proto = j["protocol"];
        int protocol = j["protocol"];
        printf("Go to the Protocol : %d \n", protocol);
        // int protocol = stoi(proto);
        switch (protocol)
        {
            // 이미지를 받는 함수
            case 10: Image_Read(j, sock, db);
                break;
            // 신분증을 받는 함수
            case 12: db.insert_idcard(j);
                    break;
            // 이미지를 보내는 함수
            case 20: Image_Send(j, sock, db);
                    break;
            // 세부정보를 보내는 함수
            case 22: IDinfo_Send(j, sock, db);
                    break;
            // 그래프용을 보내는 함수
            case 24: db.Visual_data(sock);
                    break;
            // 리스트 목록을 보내는 함수
            case 26: db.IDCARD_List(sock);
                    break;
            default:
                break;  
        }
    }
    catch(const json::exception ex)
    {
        cerr << ex.what() << endl;
    }
    catch(const exception ex)
    {
        cerr << "Protocol : " << strerror(errno) << endl;
    }
}

// 이미지 읽는 함수
void Image_Read(json proto, int sock, DB_CONN& db)
{
    int str_len;
    char msg[1001];
    // string recv_msg;
    vector<char> recv_msg;
    string recv_message;
    int repeat_count = 1;
    int count = proto["count"];
    cout << count << endl;
    string idnumber = proto["idnumber"];
    string image;
    string imagepath = "/home/lms/VSCODE/Team/OpenCV/Image_Save/" + idnumber;
    string filepath = imagepath+ ".jpg";
    for(int i = 0; i < count; i++)
    {
        str_len = read(sock,msg,1000);
        msg[str_len] = '\0';
        recv_message = msg;
        if(str_len == -1)
        {
            cerr << "Image_Read : Read Error" << endl;
            return;
        }
        try
        {
            json temp = json::parse(recv_message);
            if(temp["protocol"] == 35)
            {
                printf("Protocol 35 : Client ImageSend Error\n");
                return;
            }
        }
        catch(const std::exception& e)
        {
            cout << repeat_count++ << endl;
            for(int i = 0; i < str_len; i++)
                recv_msg.push_back(msg[i]);
        } 
    }
    ofstream file(filepath,ios::out|ios::binary);
    if(file.is_open())
    {
        file.write(recv_msg.data(), recv_msg.size());
        file.close();
        cout << "image save succesful\n";
    }
    
    db.insert_image(filepath, idnumber);
}

// 이미지 전송 함수
void Image_Send(json proto, int sock, DB_CONN& db)
{
    string name = proto["name"];
    string send_msg = db.Idcard_Image(name);
    char *buf = nullptr;
    int len = 0;
    if (read_image_from_file(send_msg.c_str(), &buf, &len))
    {
        // 이미지 데이터 크기 전송
        int image_size = htonl(len); // 크기를 네트워크 바이트 순서로 변환
        if (send(sock, &image_size, sizeof(image_size), 0) == -1)
        {
            perror("이미지 크기 전송 실패");
        }
        std::cout << "이미지 크기" << image_size << std::endl;

        // 이미지 데이터 전송
        if (send(sock, buf, len, 0) == -1)
        {
            perror("이미지 전송 실패");
        }
        else
        {
            std::cout << "이미지 전송 완료" << std::endl;
            std::cout << "이미지 데이터" << buf << std::endl;
        }
        free(buf);
    }
}

// 신분증 내부정보 전송 함수
void IDinfo_Send(json proto, int sock, DB_CONN& db)
{   
    try
    {
        json temp;
        string name = proto["name"];
        temp = db.Idcard_info(name);
        try
        {
            if(temp["error"] == "empty")
            {
                printf("%s is empty\n", name.c_str());
                return;
            }
            else if (temp["error"] == "error")
            {
                printf("%s / DB: Idcard_info error\n", name.c_str());
                return;
            }
        }
        catch(const std::exception& e)
        {
            temp["protocol"] = 17;
            name = temp.dump();
            send(sock, name.c_str(), name.length(), 0);
        }
        

    }
    catch(const std::exception& e)
    {
        std::cerr << e.what() << '\n';
    }
    
}

bool read_image_from_file(const char *filepath, char **buf, int *len)
{
    FILE *fp = fopen(filepath, "rb");
    
    if (!fp)
    {
        perror("파일 열기 실패");
        return false;
    }

    fseek(fp, 0, SEEK_END);
    
    *len = ftell(fp);
   
    fseek(fp, 0, SEEK_SET);
  
    *buf = (char *)malloc(*len);
  
    if (!*buf)
    {
        perror("메모리 할당 실패");
        fclose(fp);
        return false;
    }

    size_t bytesRead = fread(*buf, 1, *len, fp);


    if (bytesRead != *len)
    {
        perror("파일 읽기 실패");
        free(*buf);
        fclose(fp);
        return false;
    }

    fclose(fp);
    return true;
}