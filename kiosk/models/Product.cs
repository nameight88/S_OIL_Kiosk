using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace s_oil.models
{
    public class Product
    {
        public String productCode { get; set; } = ""; // 제품코드
        public String productName { get; set; } = ""; // 제품명
        public int price { get; set; } = 0; // 판매가
        public int productType { get; set; } = 0; // 1:요소수, 2:차량보조제

    }
}
