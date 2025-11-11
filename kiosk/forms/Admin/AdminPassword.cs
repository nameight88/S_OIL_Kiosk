using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using s_oil.Services;
using s_oil.Utils;
using System.IO;

namespace s_oil.Forms
{
    public partial class AdminPassword : Form, IContextAware
    {
        private s_oil.Services.ApplicationContext? _context;
        
        // ? INI 파일에서 읽어온 관리자 비밀번호를 저장
        private string _adminPassword = "9596"; // 기본값 (INI 파일 로드 실패 시 사용)

        public AdminPassword()
        {
            InitializeComponent();
            LoadAdminPasswordFromINI(); // ? 생성자에서 INI 파일 로드
        }

        /// <summary>
        /// SMT_Kiosk.ini 파일에서 관리자 비밀번호를 로드합니다
        /// </summary>
        private void LoadAdminPasswordFromINI()
        {
            try
            {
                var iniPath = Path.Combine(Application.StartupPath, "SMT_Kiosk.ini");
                
                if (!File.Exists(iniPath))
                {
                    Logger.Warning($"INI 파일을 찾을 수 없습니다: {iniPath}");
                    Logger.Info("기본 비밀번호 사용: 9596");
                    return;
                }

                var iniParser = new IniParser(iniPath);
                string passwordFromINI = iniParser.GetSetting("adminpwd", "password", "9596");

                if (string.IsNullOrWhiteSpace(passwordFromINI))
                {
                    Logger.Warning("INI 파일에서 비밀번호를 읽을 수 없습니다. 기본값 사용");
                    passwordFromINI = "9596";
                }

                _adminPassword = passwordFromINI.Trim();
                Logger.Info($"관리자 비밀번호 로드 완료 (길이: {_adminPassword.Length}자)");
            }
            catch (Exception ex)
            {
                Logger.Error($"관리자 비밀번호 로드 중 오류: {ex.Message}", ex);
                _adminPassword = "9596"; // 오류 발생 시 기본값 사용
                Logger.Info("오류 발생으로 기본 비밀번호 사용: 9596");
            }
        }

        public void SetContext(s_oil.Services.ApplicationContext context)
        {
            _context = context;
        }

        private void AdminPassword_Load(object sender, EventArgs e)
        {
            // 비밀번호 입력 필드 초기화
            admin_password_input.Text = "";
            admin_password_input.Focus();
        }

        private void FormProducts_Shown(object sender, EventArgs e)
        {
            this.ActiveControl = null;       
        }
        
        // 숫자 버튼 클릭 이벤트들
        private void button1_Click(object sender, EventArgs e)
        {
            AddDigit("1");
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            AddDigit("2");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            AddDigit("3");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            AddDigit("4");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            AddDigit("5");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            AddDigit("6");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            AddDigit("7");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            AddDigit("8");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            AddDigit("9");
        }

        private void button11_Click(object sender, EventArgs e) // button0
        {
            AddDigit("0");
        }

        // 숫자 추가 메서드
        private void AddDigit(string digit)
        {
            //  INI 파일의 비밀번호 길이에 맞춰 동적으로 제한
            if (admin_password_input.Text.Length < _adminPassword.Length)
            {
                admin_password_input.Text += digit;
            }
        }

        // 취소 버튼 (비밀번호 지우기)
        private void button_reset_Click(object sender, EventArgs e)
        {
            admin_password_input.Text = "";
        }

        // 백스페이스 기능 (마지막 글자 삭제)
        private void button_cancel_Click(object sender, EventArgs e)
        {
            if (admin_password_input.Text.Length > 0)
            {
                admin_password_input.Text = admin_password_input.Text.Substring(0, admin_password_input.Text.Length - 1);
            }
        }

        // 확인 버튼
        private void button13_Click(object sender, EventArgs e)
        {
            ValidatePassword();
        }

        // ? 비밀번호 검증 (INI 파일 기반)
        private void ValidatePassword()
        {
            // ? 입력된 비밀번호 길이 체크 (INI 파일의 비밀번호 길이와 비교)
            if (admin_password_input.Text.Length != _adminPassword.Length)
            {
                MessageBox.Show($"비밀번호는 {_adminPassword.Length}자리를 입력해주세요.", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ? INI 파일에서 로드한 비밀번호와 비교
            if (admin_password_input.Text == _adminPassword)
            {
                // 비밀번호가 맞으면 관리자 제어 페이지로 이동
                try
                {
                    Logger.Info("관리자 비밀번호 인증 성공");
                    _context?.Navigator?.ShowAdminControlPage();
                    admin_password_input.Text = "";
                }
                catch (Exception ex)
                {
                    Logger.Error($"페이지 이동 중 오류: {ex.Message}", ex);
                    MessageBox.Show($"페이지 이동 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                Logger.Warning("관리자 비밀번호 인증 실패");
                MessageBox.Show("비밀번호가 틀렸습니다.", "인증 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                admin_password_input.Text = ""; // 비밀번호 초기화
            }
        }

        // 홈 버튼 클릭 (HomePage로 이동)
        private void button_home_Click(object sender, EventArgs e)
        {
            try
            {
                _context?.Navigator?.ShowHomePage();
                this.ActiveControl = null;
                admin_password_input.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"페이지 이동 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {
            // 비밀번호 입력 필드 변경 시
            // ? INI 파일의 비밀번호 길이에 도달하면 자동 검증 (선택사항)
            if (admin_password_input.Text.Length == _adminPassword.Length)
            {
                // 자동 검증을 원하면 주석 해제
                // ValidatePassword();
            }
        }
    }
}
