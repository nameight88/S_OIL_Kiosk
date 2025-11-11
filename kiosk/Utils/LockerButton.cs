using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace s_oil.Utils
{
    // Locker를 선택 혹은 상태를 나타낼때의 커스텀으로 보여줄 클래스
    /// <summary>
    /// 사물함 전용 커스텀 버튼 클래스
    /// Normal, Pressed, Using 상태에 따른 이미지를 지원합니다.
    /// </summary>
    public class LockerButton : Button
    {
        private Image? _normalImage;
        private Image? _pressedImage;
        private Image? _usingImage;
        private bool _isSelected = false;
        private bool _isUsing = false;
        private string _productName = "";
        private int _price = 0;
        private bool _isAdminMode = false; // 관리자 모드 플래그 추가
        private bool _showOnlyProductName = false; // 제품명만 표시하는지 여부

        #region 공개 속성

        /// <summary>
        /// 일반 상태(사용 가능)일 때 표시할 이미지
        /// </summary>
        [Category("Locker Images")]
        [Description("일반 상태(사용 가능)일 때 표시할 이미지")]
        public Image? NormalImage
        {
            get => _normalImage;
            set
            {
                _normalImage = value;
                UpdateButtonImage();
                Invalidate();
            }
        }

        /// <summary>
        /// 눌린 상태일 때 표시할 이미지
        /// </summary>
        [Category("Locker Images")]
        [Description("눌린 상태일 때 표시할 이미지")]
        public Image? PressedImage
        {
            get => _pressedImage;
            set
            {
                _pressedImage = value;
                UpdateButtonImage();
                Invalidate();
            }
        }

        /// <summary>
        /// 사용 중 상태일 때 표시할 이미지
        /// </summary>
        [Category("Locker Images")]
        [Description("사용 중 상태일 때 표시할 이미지")]
        public Image? UsingImage
        {
            get => _usingImage;
            set
            {
                _usingImage = value;
                UpdateButtonImage();
                Invalidate();
            }
        }

        /// <summary>
        /// 사용 중 상태 여부
        /// </summary>
        [Category("Locker State")]
        [Description("사용 중 상태 여부")]
        public bool IsUsing
        {
            get => _isUsing;
            set
            {
                _isUsing = value;
                if (_isUsing && !_isAdminMode)
                {
                    _isSelected = false; // 사용 중 상태가 되면 선택 상태는 해제 (관리자 모드 제외)
                }
                UpdateButtonImage();
                UpdateButtonText(); // 텍스트도 업데이트
                Invalidate();
            }
        }

        /// <summary>
        /// 선택 상태 여부
        /// </summary>
        [Category("Locker State")]
        [Description("선택 상태 여부")]
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                // 관리자 모드가 아니고 사용 중일 때는 선택 불가
                if (_isUsing && !_isAdminMode) return;
                _isSelected = value;
                UpdateButtonImage();
                Invalidate();
            }
        }

        /// <summary>
        /// 관리자 모드 여부 (관리자 모드에서는 품절 함도 선택 가능)
        /// </summary>
        [Category("Locker State")]
        [Description("관리자 모드 여부 (관리자 모드에서는 품절 함도 선택 가능)")]
        public bool IsAdminMode
        {
            get => _isAdminMode;
            set
            {
                _isAdminMode = value;
                // 관리자 모드 변경 시 Enabled 상태 업데이트
                UpdateEnabledState();
                Invalidate();
            }
        }

        /// <summary>
        /// 사물함 번호
        /// </summary>
        [Category("Locker State")]
        [Description("사물함 번호")]
        public int LockerNumber
        {
            get => Tag is int number ? number : 0;
            set => Tag = value;
        }

        /// <summary>
        /// 제품명
        /// </summary>
        [Category("Product Info")]
        [Description("제품명")]
        public string ProductName
        {
            get => _productName;
            set
            {
                _productName = value;
                UpdateButtonText();
                Invalidate();
            }
        }

        /// <summary>
        /// 제품 가격
        /// </summary>
        [Category("Product Info")]
        [Description("제품 가격")]
        public int Price
        {
            get => _price;
            set
            {
                _price = value;
                UpdateButtonText();
                Invalidate();
            }
        }

        /// <summary>
        /// 마우스 다운 시 배경색 (ImageButton 호환성을 위해)
        /// </summary>
        [Category("Appearance")]
        [Description("마우스 다운 시 배경색 (LockerButton에서는 사용하지 않음)")]
        public Color MouseDownBackColor { get; set; } = Color.Empty;

        /// <summary>
        /// 마우스 오버 시 배경색 (ImageButton 호환성을 위해)
        /// </summary>
        [Category("Appearance")]
        [Description("마우스 오버 시 배경색 (LockerButton에서는 사용하지 않음)")]
        public Color MouseOverBackColor { get; set; } = Color.Empty;

        #endregion

        #region 생성자

        public LockerButton()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // 기본 버튼 스타일 설정
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            //FlatAppearance.BorderColor = Color.Transparent;
            FlatAppearance.MouseDownBackColor = Color.Transparent;
            FlatAppearance.MouseOverBackColor = Color.Transparent;

            BackgroundImageLayout = ImageLayout.Stretch;
            UseVisualStyleBackColor = false;
            BackColor = Color.Transparent; // 기본 배경을 투명으로 설정

            // 기본 크기 설정
            Size = new Size(70, 70);
            
            // 텍스트 스타일 설정
            ForeColor = Color.Black;
            Font = new Font("pretendard", 8F, FontStyle.Bold);
            TextAlign = ContentAlignment.BottomCenter;
        }

        #endregion

        #region 이벤트 오버라이드

        protected override void OnClick(EventArgs e)
        {
            // 관리자 모드에서는 품절 함도 클릭 가능, 일반 모드에서는 품절 함 클릭 불가
            if (Enabled && (!_isUsing || _isAdminMode))
            {
                _isSelected = !_isSelected;
                UpdateButtonImage();
                Invalidate();
            }
            
            base.OnClick(e);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            UpdateButtonImage();
            base.OnEnabledChanged(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if ((!_isUsing || _isAdminMode) && Enabled)
            {
                base.OnMouseEnter(e);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if ((!_isUsing || _isAdminMode) && Enabled)
            {
                base.OnMouseLeave(e);
            }
        }

        #endregion

        #region 비공개 메서드

        /// <summary>
        /// 현재 상태에 따라 버튼 이미지를 업데이트합니다
        /// </summary>
        public void UpdateButtonImage()
        {
            Image? imageToShow = null;
            string imageType = "none";

            // ? 관리자 모드에서 품절 함이 선택된 경우 Pressed 이미지 표시
            if (_isAdminMode && _isUsing && _isSelected && _pressedImage != null)
            {
                // 관리자 모드 + 품절 함 + 선택됨 -> Pressed 이미지 표시
                imageToShow = _pressedImage;
                imageType = "pressed (admin mode - soldout selected)";
            }
            // 품절 상태 (관리자 모드에서 선택되지 않은 경우)
            else if (_isUsing && _usingImage != null)
            {
                // 판매불가 상태 (품절) - 최우선
                imageToShow = _usingImage;
                imageType = "using";
            }
            // 선택된 상태
            else if (_isSelected && _pressedImage != null)
            {
                // 선택된 상태
                imageToShow = _pressedImage;
                imageType = "pressed";
            }
            // 기본 상태 (판매가능)
            else if (_normalImage != null)
            {
                // 기본 상태 (판매가능)
                imageToShow = _normalImage;
                imageType = "normal";
            }

            // 백그라운드 이미지 적용
            if (BackgroundImage != imageToShow)
            {
                BackgroundImage = imageToShow;
            }
        }

        /// <summary>
        /// 버튼 텍스트를 업데이트합니다
        /// </summary>
        private void UpdateButtonText()
        {
            if (_isUsing)
            {
                // 품절/사용불가 상태
                if (_isAdminMode && !string.IsNullOrEmpty(_productName))
                {
                    Text = $"{_productName} (품절)";
                }
                else
                {
                    Text = "품절";
                }
            }
            else if (!string.IsNullOrEmpty(_productName))
            {
                // 제품 정보가 있는 경우
                if (_showOnlyProductName)
                {
                    // 제품명만 표시 (줄바꿈 적용)
                    Text = FormatProductNameWithLineBreak(_productName);
                }
                else if (_price > 0)
                {
                    Text = $"{_productName}\n{_price:N0}원";
                }
                else
                {
                    Text = $"{_productName}";
                }
            }
            else
            {
                // 제품 정보가 없는 경우 사물함 번호만 표시
                Text = LockerNumber.ToString();
            }
        }

        /// <summary>
        /// 제품명을 줄바꿈 형식으로 변환합니다 ("요소수 N개" -> "요소수\nN개")
        /// </summary>
        /// <param name="productName">제품명 (예: "요소수 3개")</param>
        /// <returns>줄바꿈이 적용된 제품명</returns>
        private string FormatProductNameWithLineBreak(string productName)
        {
            if (string.IsNullOrEmpty(productName))
                return productName;

            // 마지막 공백을 찾아서 줄바꿈으로 변경
            int lastSpaceIndex = productName.LastIndexOf(' ');
            if (lastSpaceIndex > 0)
            {
                // "요소수 3개" -> "요소수\n3개"
                return productName.Substring(0, lastSpaceIndex) + "\n" + productName.Substring(lastSpaceIndex + 1);
            }

            return productName;
        }

        /// <summary>
        /// 관리자 모드에 따라 Enabled 상태를 업데이트합니다
        /// </summary>
        private void UpdateEnabledState()
        {
            if (_isAdminMode)
            {
                // 관리자 모드에서는 항상 Enabled
                Enabled = true;
            }
            else
            {
                // 일반 모드에서는 사용 중이면 비활성화
                Enabled = !_isUsing;
            }
        }

        #endregion

        #region 공개 메서드

        /// <summary>
        /// 사물함 상태를 설정합니다
        /// </summary>
        /// <param name="isUsing">사용 중 여부</param>
        /// <param name="isEnabled">활성화 여부</param>
        public void SetLockerState(bool isUsing, bool isEnabled = true)
        {
            _isUsing = isUsing;
            if (isUsing && !_isAdminMode)
            {
                _isSelected = false;
            }
            
            // 관리자 모드에서는 강제로 활성화, 일반 모드에서는 isEnabled 값 사용
            if (_isAdminMode)
            {
                Enabled = true;
            }
            else
            {
                Enabled = isEnabled && !isUsing; // 사용 중이면 비활성화
            }
            
            UpdateButtonImage();
            UpdateButtonText();
            Invalidate();
        }

        /// <summary>
        /// 모든 이미지를 한번에 설정합니다
        /// </summary>
        /// <param name="normalImage">일반 상태 이미지</param>
        /// <param name="pressedImage">눌린 상태 이미지</param>
        /// <param name="usingImage">사용 중 상태 이미지</param>
        public void SetImages(Image? normalImage, Image? pressedImage, Image? usingImage)
        {
            _normalImage = normalImage;
            _pressedImage = pressedImage;
            _usingImage = usingImage;
            UpdateButtonImage();
            Invalidate();
        }

        /// <summary>
        /// 제품 정보를 설정합니다
        /// </summary>
        /// <param name="productName">제품명</param>
        /// <param name="price">가격</param>
        public void SetProductInfo(string productName, int price)
        {
            _productName = productName;
            _price = price;
            _showOnlyProductName = false; // 가격 포함 모드
            UpdateButtonText();
            Invalidate();
        }

        /// <summary>
        /// 제품명만 표시합니다 (가격 없이, 줄바꿈 적용)
        /// 예: "요소수 N개" -> "요소수\nN개" 형식으로 표시
        /// 가격 정보는 내부적으로 유지되어 결제 시 사용됩니다.
        /// </summary>
        /// <param name="productName">제품명 (예: "요소수 3개")</param>
        /// <param name="price">가격 (내부 저장용, 화면에는 표시 안 함)</param>
        public void SetProductNameOnly(string productName, int price)
        {
            _productName = productName;
            _price = price; // 가격 정보 유지 (화면에는 표시 안 하지만 내부적으로 저장)
            _showOnlyProductName = true; // 제품명만 표시 모드
            UpdateButtonText();
            Invalidate();
        }

        /// <summary>
        ///  상품 함을 클릭을 하였을 때 상품 정보를 보여주는 메소드
        /// </summary>
        /// <param name="productName">제품명</param>
        /// <param name="price">가격 (선택적)</param>
        public void SetBoxProductInfo(string productName, int price = 0)
        {
            _productName = productName;
            _price = price; // 가격 정보 유지
            _showOnlyProductName = true; // 제품명만 표시 모드
            UpdateButtonText();
            Invalidate();
        }

        #endregion

        #region 리소스 정리

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 이미지 리소스는 외부에서 관리되므로 여기서는 해제하지 않음
                _normalImage = null;
                _pressedImage = null;
                _usingImage = null;
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}

