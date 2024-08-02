using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Media.Imaging;

namespace Opencv_Pj
{
    public class Data
    {
        public string protocol { get; set; }        // 프로토콜

        public BitmapImage image { get; set; }      // 서버로부터 받아온 이미지

        public string name { get; set; }            // 이름

        public string idnumber { get; set; }        // 주민번호

        public string address { get; set; }         // 주소

        public string issue_date { get; set; }      // 발급일자

        public string issuer { get; set; }          // 발급장소
    }
}
