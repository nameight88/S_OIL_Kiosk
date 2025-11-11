using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace s_oil.models
{
    /// <summary>
    /// 결제 정보 모델
    /// </summary>
    public class Payment
    {
        public String areaCode { get; set; } = "";
        public int boxNo { get; set; } = 0;
        public String userCode { get; set; } = "";

        /// <summary>
        /// 결제 유형 (1: 카드결제)
        /// </summary>
        public int payType { get; set; } = 1;
        
        /// <summary>
        /// 결제 금액
        /// </summary>
        public int payAmount { get; set; } = 0;
        
        public String payPhone { get; set; } = "";
        public String confirmKey { get; set; } = "";
        public String cardNumber { get; set; } = "";
        public String payTime { get; set; } = "";
        
        /// <summary>
        /// 승인번호
        /// </summary>
        public String approvalNumber { get; set; } = "";
        
        /// <summary>
        /// 결제 상태 (0: 대기, 1: 성공, 2: 실패, 3: 취소)
        /// </summary>
        public int payStatus { get; set; } = 0;
        
        /// <summary>
        /// 오류 메시지
        /// </summary>
        public String errorMessage { get; set; } = "";
        
        /// <summary>
        /// 생성일시
        /// </summary>
        public DateTime createdAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// 수정일시
        /// </summary>
        public DateTime updatedAt { get; set; } = DateTime.Now;
    }
}
