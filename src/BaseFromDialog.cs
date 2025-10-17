/*****************************************
 *
 * 项目名称： PicTimeTagManage  
 * 文 件 名:  BaseFromDialog.cs
 * 命名空间： PicTimeTagManage
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

using PicTimeTagManage.Properties;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace PicTimeTagManage
{
    /// <summary>
    /// 窗体风格统一基类
    /// </summary>
    public partial class BaseFormDialog: Form
    {
        // 统一的窗体配置常量
        private const int DEFAULT_WIDTH = 300;
        private const int DEFAULT_HEIGHT = 300;
        private static readonly Color DEFAULT_BACK_COLOR = Color.FromArgb(240, 240, 240);
        private static readonly Font DEFAULT_FONT = new Font("微软雅黑", 9F, FontStyle.Regular);
        public BaseFormDialog()
        {
            InitializeComponent();
            InitializeBaseSettings();
        }
        /// <summary>
        /// 初始化基础窗体设置
        /// </summary>
        protected virtual void InitializeBaseSettings()
        {
            // === 基本外观设置 ===
            this.BackColor = DEFAULT_BACK_COLOR;
            this.Font = DEFAULT_FONT;
            // === 窗体行为设置 ===
            this.WindowState = FormWindowState.Normal;
            this.FormBorderStyle = FormBorderStyle.Sizable;       // 可调整大小
            this.StartPosition = FormStartPosition.CenterScreen;  // 居中显示

            // 控制按钮显示
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ControlBox = true;

            this.DoubleBuffered = true;

            SubscribeToCommonEvents();
        }

        private void SetApplicationIcon()
        {
            try
            {
                using (Icon icon = Icon.FromHandle(Resources.Icon.GetHicon()))
                {
                    this.Icon = icon;
                }
            }
            catch
            {
                Debug.WriteLine("图标加载失败");
            }
        }

        /// <summary>
        /// 订阅通用窗体事件
        /// </summary>
        private void SubscribeToCommonEvents()
        {
            this.Load += BaseFormDialog_Load;
            this.FormClosing += BaseFormDialog_FormClosing;
        }
        /// <summary>
        /// 窗体加载事件 - 可在此添加各窗体的个性化初始化代码
        /// </summary>
        protected virtual void BaseFormDialog_Load(object sender, EventArgs e)
        {
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {this.GetType().Name} 已加载");
            this.Size = new Size(DEFAULT_WIDTH, DEFAULT_HEIGHT);
            SetApplicationIcon();
        }

        /// <summary>
        /// 窗体关闭事件 - 统一的关闭前处理
        /// </summary>
        protected virtual void BaseFormDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {this.GetType().Name} 已关闭");
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.Size = new Size(DEFAULT_WIDTH, DEFAULT_HEIGHT);
            if (!DesignMode)
            {
                this.CenterToScreen();
            }
        }
    }
}
