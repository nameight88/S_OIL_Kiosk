using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace s_oil.models
{
    public class BoxMaster
    {
        public String areaCode { get; set; } = "";
        public int BoxNo { get; set; } = 0;
        public int ServiceType { get; set; } = 0;
        public int BoxSizeType { get; set; } = 0;
        public int useState { get; set; } = 1; // useState 1: 판매 불가능 2: 판매 가능
        public String userCode { get; set; } = "";
        public String userName { get; set; } = "";
        public String userPhone { get; set; } = "";
        public int payType { get; set; } = 0;
        public String dong { get; set; } = "";
        public String addressNum { get; set; } = "";
        public String transCode { get; set; } = "";
        public String transPhone { get; set; } = "";
        public String barcode { get; set; } = "";
        public String deliverType { get; set; } = "";
        public String boxPassword { get; set; } = "";
        public String payCode { get; set; } = "";
        public String payAmount { get; set; } = "";
        public int userTimeType { get; set; } = 0;
        public String startTime { get; set; } = "";
        public String endTime { get; set; } = "";
        public int pwd_type { get; set; } = 0;
        public int useType { get; set; } = 0;
        public String NfcID { get; set; } = "";
        public int gasAdURL { get; set; } = 0;
        public String AdURL { get; set; } = "";
        public int TagCnt { get; set; } = 0;
        public int TagAdCnt { get; set; } = 0;
        public int isConsole { get; set; } =0;
        public int BoxHeight { get; set; } = 0;
        public String productCode { get; set; } = "";
        public String productName { get; set; } = "";
        public int price { get; set; } = 0;
        public int productType { get; set; } = 0; // 1:요소수, 2:차량보조제
    }
}    