/*****************************************
 *
 * 项目名称： PicTimeTagManage  
 * 文 件 名:  LatLongInputDialog.cs
 * 命名空间： %Namespace%
 * 描    述:  
 * 
 * 版    本：  V1.0
 * 创 建 者：  liuxin
 * 电子邮件：  359193585@qq.com(leison)
 * 创建时间：  2025/10/17 9:17
 * ======================================
 * 历史更新记录
 * 版本：V          修改时间：         修改人：
 * 修改内容：
 * ======================================
*********************************************/

using PicTimeTagManage;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace PicTimeTagManage
{
    public partial class LatLongInputDialog : BaseFormDialog
    {
        #region 用于调用Windows API的辅助类
        internal static class NativeMethods
        {
            public const int WM_CLIPBOARDUPDATE = 0x031D;

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern bool AddClipboardFormatListener(IntPtr hwnd);

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
        }
        #endregion
        public string Latitude { get; private set; }
        public string Longitude { get; private set; }
        public bool Overwrite { get; private set; }
        public TextBox textBoxGpsLocation;

        private Label label1;
        private CheckBox checkBox1;
        private Button button2;
        private SplitContainer splitContainer1;
        private Panel panel1;
        private Button button1;

        public LatLongInputDialog()
        {
            InitializeComponent();
            // 将当前窗口设置为可以接收剪贴板更新消息
            NativeMethods.AddClipboardFormatListener(this.Handle);
        }
        private void InitializeComponent()
        {
            this.textBoxGpsLocation = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.button2 = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBoxGpsLocation
            // 
            this.textBoxGpsLocation.Dock = System.Windows.Forms.DockStyle.Top;
            this.textBoxGpsLocation.Location = new System.Drawing.Point(0, 0);
            this.textBoxGpsLocation.Name = "textBoxGpsLocation";
            this.textBoxGpsLocation.Size = new System.Drawing.Size(284, 23);
            this.textBoxGpsLocation.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(32, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "label1";
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(23, 79);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(86, 37);
            this.button1.TabIndex = 2;
            this.button1.Text = "确定";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Location = new System.Drawing.Point(23, 28);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(218, 21);
            this.checkBox1.TabIndex = 3;
            this.checkBox1.Text = "覆盖图片里已有的拍摄位置GPS坐标";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.Click += new System.EventHandler(this.CheckBox_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(115, 79);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(124, 37);
            this.button2.TabIndex = 4;
            this.button2.Text = "在地图选取";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            this.splitContainer1.Panel2.Controls.Add(this.textBoxGpsLocation);
            this.splitContainer1.Size = new System.Drawing.Size(284, 261);
            this.splitContainer1.SplitterDistance = 65;
            this.splitContainer1.TabIndex = 5;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.checkBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 23);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(284, 139);
            this.panel1.TabIndex = 5;
            // 
            // LatLongInputDialog
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.splitContainer1);
            this.Name = "LatLongInputDialog";
            this.Text = "输入经纬度坐标";
            this.Load += new System.EventHandler(this.LatLongInputDialog_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }
        private void CheckBox_Click(object sender, EventArgs e)
        {
            Overwrite = checkBox1.Checked;
        }
        private void BtnOK_Click(object sender, EventArgs e)
        {
            // 简单的验证
            string[] strings = textBoxGpsLocation.Text.Split(',');
            if (!IsValidCoordinate(strings[0]) || !IsValidCoordinate(strings[1]))
            {
                MessageBox.Show("请输入有效的经纬度坐标", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.None; // 阻止关闭
                return;
            }
            Latitude = strings[0];
            Longitude = strings[1];


        }
        private void button2_Click(object sender, EventArgs e)
        {
            new MyMethod().ShowGPSSelection(textBoxGpsLocation.Text);
        }

       

        private bool IsValidCoordinate(string coord)
        {
            // 简单的坐标验证逻辑
            return double.TryParse(coord, out double result);
        }
        private void LatLongInputDialog_Load(object sender, EventArgs e)
        {
            label1.Text = "请输入经纬度坐标，\r\n格式为“纬度，经度”，\r\n用半角逗号分隔，\r\n如：32.123412,118.121212";
            Overwrite = checkBox1.Checked;
        }

        #region 系统剪切板监听
        protected override void WndProc(ref Message m)
        {
            // 监听剪贴板更新消息（WM_CLIPBOARDUPDATE）
            if (m.Msg == NativeMethods.WM_CLIPBOARDUPDATE)
            {
                OnClipboardUpdated();
            }
            base.WndProc(ref m);
        }

        private void OnClipboardUpdated()
        {
            // 安全地在UI线程上执行
            if (InvokeRequired)
            {
                Invoke(new Action(OnClipboardUpdated));
                return;
            }

            try
            {
                if (Clipboard.ContainsText())
                {
                    string copiedText = Clipboard.GetText();
                    // 在这里处理你获取到的文本，例如显示在TextBox中或进行其他操作
                    var result = GpsCoordinateValidator.ValidateGpsCoordinate(copiedText);
                    if (result.isValid) textBoxGpsLocation.Text = copiedText;
                    Console.WriteLine($"捕获到文本: {copiedText}");
                }
            }
            catch (Exception ex)
            {
                // 处理剪贴板访问冲突（其他进程可能正在使用剪贴板）
                Console.WriteLine($"访问剪贴板失败: {ex.Message}");
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //try
            //{
            //    // 窗体关闭时，移除剪贴板监听
            //    NativeMethods.RemoveClipboardFormatListener(this.Handle);
            //    base.OnFormClosing(e);
            //}
            //catch { }
        }
        #endregion

    }
}