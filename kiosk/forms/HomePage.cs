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

namespace s_oil.Forms
{
    public partial class HomePage : Form, IContextAware
    {
        private s_oil.Services.ApplicationContext? _context;  
        public HomePage()
        {
            InitializeComponent();
        }

        public void SetContext(s_oil.Services.ApplicationContext context)  
        {
            _context = context;
            InitializePageData();
        }

        private void InitializePageData()
        {
            if (_context != null)
            {
                CheckDeviceStatus();
                LoadInitialData();
            }
        }

        private void CheckDeviceStatus()
        {
            var buStatus = _context?.BUDevice?.IsConnected ?? false;
            var printerStatus = _context?.PrinterDevice?.IsConnected ?? false;
            //var payStatus = _context?.PayDevice?.IsConnected ?? false;

            UpdateDeviceStatusUI(buStatus, printerStatus, false);
        }

        private void UpdateDeviceStatusUI(bool bu, bool printer, bool pay)
        {
            Console.WriteLine($"Device Status - BU: {bu}, Printer: {printer}, Pay: {pay}");
            
            // 로깅으로 디바이스 상태 기록
            Logger.Info($"Device Status Check - BU: {bu}, Printer: {printer}, Pay: {pay}");
            
            // 프린터 연결 상태가 false인 경우 경고 로그
            if (!printer)
            {
                Logger.Warning("Printer device is not connected. Receipt printing may not work.");
            }
            
            // BU 연결 상태가 false인 경우 경고 로그
            if (!bu)
            {
                Logger.Warning("BU device is not connected. Box control may not work.");
            }
        }

        private async void LoadInitialData()
        {

            //if (_context?.Database != null)
            {
                // var menuData = await _context.Database.GetMenuDataAsync();
                // UpdateMenuUI(menuData);
            }
        }

        private void HomePage_Load(object sender, EventArgs e)
        {
     
        }


        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                _context?.Navigator?.ShowCustomerProductBuyPage();
                this.ActiveControl = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($" {ex.Message}", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void admin_button_Click(object sender, EventArgs e)
        {
            try
            {
                _context?.Navigator?.ShowAdminPasswordPage();
                this.ActiveControl = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($": {ex.Message}", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void productstatus_Click(object sender, EventArgs e)
        {
            try
            {
                _context?.Navigator?.ShowCustomerProductStatusPage();
                this.ActiveControl = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($": {ex.Message}", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
