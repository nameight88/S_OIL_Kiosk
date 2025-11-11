using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace s_oil.Utils
{
    /// <summary>
    /// 버튼 클릭 시 이미지 변화 이벤트를 처리할 수 있는 커스텀 버튼 클래스
    /// Normal/Pressed 상태에 따른 이미지를 지원합니다.
    /// </summary>
    public class ImageButton : Button
    {
        private Image? _normalImage;
        private Image? _pressedImage;
        private bool _isPressed = false;
        private bool _isMouseDown = false;

        #region 공개 속성

        /// <summary>
        /// 일반 상태일 때 표시할 이미지
        /// </summary>
        [Category("Appearance")]
        [Description("일반 상태일 때 표시할 이미지")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [RefreshProperties(RefreshProperties.Repaint)]
        public Image? NormalImage
        {
            get => _normalImage;
            set
            {
                _normalImage = value;
                if (!DesignMode && !_isPressed && _normalImage != null)
                {
                    this.BackgroundImage = _normalImage;
                }
                this.Invalidate();
            }
        }

        /// <summary>
        /// 눌린 상태일 때 표시할 이미지
        /// </summary>
        [Category("Appearance")]
        [Description("눌린 상태일 때 표시할 이미지")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [RefreshProperties(RefreshProperties.Repaint)]
        public Image? PressedImage
        {
            get => _pressedImage;
            set
            {
                _pressedImage = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 마우스 오버 시 배경색
        /// </summary>
        [Category("Appearance")]
        [Description("마우스 오버 시 배경색")]
        public Color MouseOverBackColor
        {
            get => this.FlatAppearance.MouseOverBackColor;
            set => this.FlatAppearance.MouseOverBackColor = value;
        }

        /// <summary>
        /// 마우스 다운 시 배경색
        /// </summary>
        [Category("Appearance")]
        [Description("마우스 다운 시 배경색")]
        public Color MouseDownBackColor
        {
            get => this.FlatAppearance.MouseDownBackColor;
            set => this.FlatAppearance.MouseDownBackColor = value;
        }

        #endregion

        #region 생성자

        public ImageButton()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // 더블 버퍼링 및 스타일 설정
            this.SetStyle(ControlStyles.Selectable, false);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            this.DoubleBuffered = true;

            // 기본 버튼 스타일
            this.BackColor = Color.Transparent;
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.FlatAppearance.MouseOverBackColor = Color.Transparent;
            this.FlatAppearance.MouseDownBackColor = Color.Transparent;
            this.UseVisualStyleBackColor = false;
            this.BackgroundImageLayout = ImageLayout.Stretch;

            // 디자인 모드에서 버튼 이름 표시
            if (DesignMode)
            {
                this.Text = this.Name;
            }
        }

        #endregion

        #region 이벤트 오버라이드

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            
            if (!DesignMode)
            {
                // BackgroundImage가 있고 NormalImage가 없으면 BackgroundImage를 NormalImage로 사용
                if (this.BackgroundImage != null && _normalImage == null)
                {
                    _normalImage = this.BackgroundImage;
                }
                
                // PressedImage가 없고 NormalImage가 있으면 자동 생성
                if (_pressedImage == null && _normalImage != null)
                {
                    _pressedImage = CreateDarkenedImage(_normalImage);
                    Console.WriteLine($"ImageButton {this.Name}: Auto-generated pressed image from normal image");
                }
                
                // Normal 이미지 설정
                if (_normalImage != null && !_isPressed)
                {
                    this.BackgroundImage = _normalImage;
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isMouseDown = true;
                _isPressed = true;
                
                if (_pressedImage != null)
                {
                    this.BackgroundImage = _pressedImage;
                    this.Invalidate();
                    this.Update();
                }
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _isMouseDown)
            {
                _isMouseDown = false;
                _isPressed = false;
                
                if (_normalImage != null)
                {
                    this.BackgroundImage = _normalImage;
                    this.Invalidate();
                    this.Update();
                }
            }
            base.OnMouseUp(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            
            if (_isPressed || _isMouseDown)
            {
                _isPressed = false;
                _isMouseDown = false;
                
                if (_normalImage != null)
                {
                    this.BackgroundImage = _normalImage;
                    this.Invalidate();
                }
            }
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            
            if (_isPressed || _isMouseDown)
            {
                _isPressed = false;
                _isMouseDown = false;
                
                if (_normalImage != null)
                {
                    this.BackgroundImage = _normalImage;
                    this.Invalidate();
                }
            }
        }

        protected override void OnClick(EventArgs e)
        {
            // 클릭 이벤트 발생 시 짧은 Pressed 효과 표시
            if (_pressedImage != null && _normalImage != null)
            {
                this.BackgroundImage = _pressedImage;
                this.Invalidate();
                this.Update();
                
                // 짧은 지연 후 Normal 이미지로 복원
                Task.Delay(100).ContinueWith(_ => 
                {
                    if (this.InvokeRequired)
                    {
                        this.Invoke(new Action(() => 
                        {
                            if (!_isPressed) // 여전히 눌려있지 않은 경우에만
                            {
                                this.BackgroundImage = _normalImage;
                                this.Invalidate();
                            }
                        }));
                    }
                    else
                    {
                        if (!_isPressed)
                        {
                            this.BackgroundImage = _normalImage;
                            this.Invalidate();
                        }
                    }
                });
            }
            
            base.OnClick(e);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            
            // 디자인 모드에서 버튼 경계선과 이름 표시
            if (DesignMode)
            {
                using (Pen borderPen = new Pen(Color.Gray, 1))
                {
                    pevent.Graphics.DrawRectangle(borderPen, 0, 0, this.Width - 1, this.Height - 1);
                }
                
                if (!string.IsNullOrEmpty(this.Text))
                {
                    using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    using (Brush textBrush = new SolidBrush(Color.Black))
                    {
                        pevent.Graphics.DrawString(this.Text, this.Font, textBrush, this.ClientRectangle, sf);
                    }
                }
            }
        }

        #endregion

        #region 공개 메서드

        /// <summary>
        /// Normal과 Pressed 이미지를 한번에 설정하는 메서드
        /// </summary>
        /// <param name="normalImage">일반 상태 이미지</param>
        /// <param name="pressedImage">눌린 상태 이미지</param>
        public void SetImages(Image? normalImage, Image? pressedImage)
        {
            _normalImage = normalImage;
            _pressedImage = pressedImage;
            
            // Pressed 이미지가 없으면 자동 생성
            if (_pressedImage == null && _normalImage != null)
            {
                _pressedImage = CreateDarkenedImage(_normalImage);
            }
            
            if (!DesignMode && _normalImage != null && !_isPressed)
            {
                this.BackgroundImage = _normalImage;
            }
            
            this.Invalidate();
        }

        /// <summary>
        /// 수동으로 Pressed 상태로 설정
        /// </summary>
        public void SetPressedState()
        {
            if (_pressedImage != null)
            {
                _isPressed = true;
                this.BackgroundImage = _pressedImage;
                this.Invalidate();
                this.Update();
            }
        }

        /// <summary>
        /// 수동으로 Normal 상태로 설정
        /// </summary>
        public void SetNormalState()
        {
            if (_normalImage != null)
            {
                _isPressed = false;
                _isMouseDown = false;
                this.BackgroundImage = _normalImage;
                this.Invalidate();
                this.Update();
            }
        }

        #endregion

        #region 비공개 메서드

        /// <summary>
        /// 이미지를 어둡게 처리하여 Pressed 효과를 만듭니다
        /// </summary>
        /// <param name="original">원본 이미지</param>
        /// <returns>어둡게 처리된 이미지</returns>
        private Image CreateDarkenedImage(Image original)
        {
            try
            {
                Bitmap darkened = new Bitmap(original.Width, original.Height);
                using (Graphics g = Graphics.FromImage(darkened))
                {
                    // 고품질 렌더링 설정
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                    // 원본 이미지를 먼저 그리기
                    g.DrawImage(original, 0, 0, original.Width, original.Height);
                    
                    // 반투명 어두운 오버레이로 눌린 효과
                    using (Brush overlay = new SolidBrush(Color.FromArgb(100, 0, 0, 0)))
                    {
                        g.FillRectangle(overlay, 0, 0, darkened.Width, darkened.Height);
                    }
                    
                    // 약간의 테두리 효과
                    using (Pen borderPen = new Pen(Color.FromArgb(60, 0, 0, 0), 2))
                    {
                        g.DrawRectangle(borderPen, 1, 1, darkened.Width - 2, darkened.Height - 2);
                    }
                }
                return darkened;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create darkened image: {ex.Message}");
                return original; // 실패하면 원본 반환
            }
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
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
