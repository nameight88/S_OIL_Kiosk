using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace s_oil.models
{
    public class BoxHistory
    {
        public int eventType { get; set; } = 0;
        public String areaCode { get; set; } = "";
        public int boxNo { get; set; } = 0;
        public int serviceType { get; set; } = 0;
        public int boxSizeType { get; set; } = 0;
        public int useState { get; set; } = 0;
        public String userCode { get; set; } = "";
        public String userName { get; set; } = "";
        public String userPhone { get; set; } = "";
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
        public String createTime { get; set; } = "";
    }
}
