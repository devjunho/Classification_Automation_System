#include "DB11_Server.h"



/* 지금 PRI KEY 설정안되어 있음 */

void error_handling(string message);
void* handle_clnt(void* arg);

int clnt_cnt = 0;
int clnt_socks[MAX_CLNT];
pthread_mutex_t mutx;

int main(int argc, char* argv[])
{
    signal(SIGPIPE,SIG_IGN);
    int serv_sock, clnt_sock;
    char message[BUF_SIZE];
    int str_len, i;

    sockaddr_in serv_adr, clnt_adr;
    socklen_t clnt_adr_sz;
    pthread_t t_id;

    serv_sock = socket(PF_INET, SOCK_STREAM, 0);
    memset(&serv_adr, 0, sizeof(serv_adr));
    serv_adr.sin_family = AF_INET;
    serv_adr.sin_addr.s_addr = htonl(INADDR_ANY);
    serv_adr.sin_port = htons(atoi(PortNumber));

    if( bind(serv_sock, (sockaddr*)&serv_adr, sizeof(serv_adr)) == -1)
        error_handling("bind() error");
    if( listen(serv_sock,5) == -1)
        error_handling("listen() error");
    printf("Server is Online at Port : %s , IP : %s\n", PortNumber, Serv_IP);
    while(1)
    {
        clnt_adr_sz = sizeof(clnt_adr);
        clnt_sock = accept(serv_sock, (sockaddr*)&clnt_adr, &clnt_adr_sz);
        printf("Connected client IP: %s , Port: %d\n", inet_ntoa(clnt_adr.sin_addr), ntohs(clnt_adr.sin_port));
        pthread_mutex_lock(&mutx);
        clnt_socks[clnt_cnt++] = clnt_sock;
        pthread_mutex_unlock(&mutx);

        pthread_create(&t_id, NULL, handle_clnt, (void*)&clnt_sock);
        pthread_detach(t_id);
    }
    close(serv_sock);
    return 0;
}

void* handle_clnt(void* arg)
{
    int clnt_sock = *((int*)arg);
    int str_len = 0;
    char msg[BUF_SIZE];
    DB_CONN db;
    string serv_msg;
    memset(msg, 0, BUF_SIZE);
    while((str_len = read(clnt_sock,msg,BUF_SIZE))!=0)
    {
       cout << str_len << endl;
        if(str_len == -1)
        {
            cerr << "Read Error : " << strerror(errno) << endl;
        }
        msg[str_len] = 0;
        serv_msg = msg;
        cout << serv_msg << endl;
        protocol(serv_msg,clnt_sock, db);
        memset(msg, 0, BUF_SIZE); 
    }
    pthread_mutex_lock(&mutx);
    for ( int i = 0; i < clnt_cnt; i++ )
    {
        if(clnt_sock == clnt_socks[i])
        {
            while(i++ < clnt_cnt-1)
                clnt_socks[i] = clnt_socks[i+1];
            break;
        }
    }
    clnt_cnt--;
    pthread_mutex_unlock(&mutx);
    close(clnt_sock);
    return NULL;
}

void error_handling(string message)
{
    cerr<<message<<endl;
    exit(1);
}