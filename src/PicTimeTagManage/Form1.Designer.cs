/*****************************************
 *
 * 项目名称： PicTimeTagManage  
 * 文 件 名:  Form1.Designer.cs
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

namespace PicTimeTagManage
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            if (disposing)
            {
                // 释放托管资源
                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Dispose();
                    pictureBox1.Image = null;
                }

                // 清理缓存中的所有图片
                foreach (var keyValuePair in _thumbnailCache)
                {
                    keyValuePair.Value.Dispose();
                }
                _thumbnailCache.Clear();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.BottomToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.TopToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.RightToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.LeftToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.ContentPanel = new System.Windows.Forms.ToolStripContentPanel();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.btnSelectFolder = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.toolStripContainer2 = new System.Windows.Forms.ToolStripContainer();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.checkBoxThumbnail = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.comboBoxFolders = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.dataGridView2 = new System.Windows.Forms.DataGridView();
            this.lblSpendingName = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.btnRight90 = new System.Windows.Forms.Button();
            this.btnLeft90 = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.btnLoadMap = new System.Windows.Forms.Button();
            this.btnFlash = new System.Windows.Forms.Button();
            this.btnModifyGps = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxGpsLocation = new System.Windows.Forms.TextBox();
            this.btnModifyOriginTime = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxOrignTime = new System.Windows.Forms.TextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.toolStripContainer2.ContentPanel.SuspendLayout();
            this.toolStripContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).BeginInit();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // BottomToolStripPanel
            // 
            this.BottomToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.BottomToolStripPanel.Name = "BottomToolStripPanel";
            this.BottomToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.BottomToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.BottomToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // TopToolStripPanel
            // 
            this.TopToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.TopToolStripPanel.Name = "TopToolStripPanel";
            this.TopToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.TopToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.TopToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // RightToolStripPanel
            // 
            this.RightToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.RightToolStripPanel.Name = "RightToolStripPanel";
            this.RightToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.RightToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.RightToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // LeftToolStripPanel
            // 
            this.LeftToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.LeftToolStripPanel.Name = "LeftToolStripPanel";
            this.LeftToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.LeftToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.LeftToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // ContentPanel
            // 
            this.ContentPanel.AutoScroll = true;
            this.ContentPanel.Size = new System.Drawing.Size(784, 561);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToOrderColumns = true;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(980, 297);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.DataGridView1_CellFormatting);
            this.dataGridView1.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DataGridView1_CellMouseDoubleClick);
            // 
            // btnSelectFolder
            // 
            this.btnSelectFolder.Location = new System.Drawing.Point(679, 10);
            this.btnSelectFolder.Name = "btnSelectFolder";
            this.btnSelectFolder.Size = new System.Drawing.Size(93, 25);
            this.btnSelectFolder.TabIndex = 1;
            this.btnSelectFolder.Text = "选择";
            this.btnSelectFolder.UseVisualStyleBackColor = true;
            this.btnSelectFolder.Click += new System.EventHandler(this.btnSelectFolder_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "文件目录";
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(12, 35);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(113, 28);
            this.btnRefresh.TabIndex = 6;
            this.btnRefresh.Text = "刷新";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // toolStripContainer2
            // 
            this.toolStripContainer2.BottomToolStripPanelVisible = false;
            // 
            // toolStripContainer2.ContentPanel
            // 
            this.toolStripContainer2.ContentPanel.AutoScroll = true;
            this.toolStripContainer2.ContentPanel.Controls.Add(this.splitContainer1);
            this.toolStripContainer2.ContentPanel.Size = new System.Drawing.Size(984, 661);
            this.toolStripContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer2.LeftToolStripPanelVisible = false;
            this.toolStripContainer2.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer2.Name = "toolStripContainer2";
            this.toolStripContainer2.RightToolStripPanelVisible = false;
            this.toolStripContainer2.Size = new System.Drawing.Size(984, 661);
            this.toolStripContainer2.TabIndex = 1;
            this.toolStripContainer2.Text = "toolStripContainer2";
            this.toolStripContainer2.TopToolStripPanelVisible = false;
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.pictureBox2);
            this.splitContainer1.Panel1.Controls.Add(this.panel1);
            this.splitContainer1.Panel1MinSize = 70;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Panel2MinSize = 200;
            this.splitContainer1.Size = new System.Drawing.Size(984, 661);
            this.splitContainer1.SplitterDistance = 70;
            this.splitContainer1.TabIndex = 11;
            // 
            // pictureBox2
            // 
            this.pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBox2.Dock = System.Windows.Forms.DockStyle.Right;
            this.pictureBox2.Image = global::PicTimeTagManage.Properties.Resources.Icon;
            this.pictureBox2.Location = new System.Drawing.Point(901, 0);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(79, 66);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 10;
            this.pictureBox2.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.checkBoxThumbnail);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.checkBox2);
            this.panel1.Controls.Add(this.checkBox1);
            this.panel1.Controls.Add(this.comboBoxFolders);
            this.panel1.Controls.Add(this.btnSelectFolder);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.btnRefresh);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(780, 69);
            this.panel1.TabIndex = 9;
            // 
            // checkBoxThumbnail
            // 
            this.checkBoxThumbnail.AutoSize = true;
            this.checkBoxThumbnail.Location = new System.Drawing.Point(676, 41);
            this.checkBoxThumbnail.Name = "checkBoxThumbnail";
            this.checkBoxThumbnail.Size = new System.Drawing.Size(90, 16);
            this.checkBoxThumbnail.TabIndex = 11;
            this.checkBoxThumbnail.Text = "显示略缩图 ";
            this.toolTip1.SetToolTip(this.checkBoxThumbnail, "如果选中，读取exif信息会花费大量时间，造成界面假死，如非必须，刷新时取消选中。");
            this.checkBoxThumbnail.UseVisualStyleBackColor = true;
            this.checkBoxThumbnail.CheckedChanged += new System.EventHandler(this.checkBoxThumbnail_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(260, 43);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(11, 12);
            this.label5.TabIndex = 10;
            this.label5.Text = ".";
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(577, 42);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(96, 16);
            this.checkBox2.TabIndex = 9;
            this.checkBox2.Text = "读取EXIF-GPS";
            this.toolTip1.SetToolTip(this.checkBox2, "如果文件很多，选中该项exif信息会花费大量时间，造成界面假死，如非必须，刷新时取消选中。");
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.Visible = false;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(472, 42);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(102, 16);
            this.checkBox1.TabIndex = 8;
            this.checkBox1.Text = "读取EXIF-日期";
            this.toolTip1.SetToolTip(this.checkBox1, "如果文件很多，选中该项读取exif信息会花费大量时间，造成界面假死，如非必须，刷新时取消选中。");
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.Visible = false;
            // 
            // comboBoxFolders
            // 
            this.comboBoxFolders.FormattingEnabled = true;
            this.comboBoxFolders.Location = new System.Drawing.Point(69, 12);
            this.comboBoxFolders.Name = "comboBoxFolders";
            this.comboBoxFolders.Size = new System.Drawing.Size(604, 20);
            this.comboBoxFolders.TabIndex = 2;
            this.comboBoxFolders.SelectedIndexChanged += new System.EventHandler(this.comboBoxFolders_SelectedIndexChanged);
            this.comboBoxFolders.SelectionChangeCommitted += new System.EventHandler(this.comboBoxFolders_SelectionChangeCommitted);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(145, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 12);
            this.label2.TabIndex = 7;
            this.label2.Text = "共有0个文件";
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.dataGridView1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer2.Panel2MinSize = 100;
            this.splitContainer2.Size = new System.Drawing.Size(984, 587);
            this.splitContainer2.SplitterDistance = 301;
            this.splitContainer2.TabIndex = 1;
            // 
            // splitContainer3
            // 
            this.splitContainer3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.pictureBox1);
            this.splitContainer3.Panel1MinSize = 100;
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.splitContainer4);
            this.splitContainer3.Size = new System.Drawing.Size(984, 282);
            this.splitContainer3.SplitterDistance = 278;
            this.splitContainer3.TabIndex = 11;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(274, 278);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // splitContainer4
            // 
            this.splitContainer4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Name = "splitContainer4";
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.dataGridView2);
            this.splitContainer4.Panel1.Controls.Add(this.lblSpendingName);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.panel4);
            this.splitContainer4.Size = new System.Drawing.Size(702, 282);
            this.splitContainer4.SplitterDistance = 286;
            this.splitContainer4.TabIndex = 2;
            // 
            // dataGridView2
            // 
            this.dataGridView2.AllowUserToDeleteRows = false;
            this.dataGridView2.AllowUserToOrderColumns = true;
            this.dataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView2.Location = new System.Drawing.Point(0, 12);
            this.dataGridView2.Name = "dataGridView2";
            this.dataGridView2.ReadOnly = true;
            this.dataGridView2.RowTemplate.Height = 23;
            this.dataGridView2.Size = new System.Drawing.Size(282, 266);
            this.dataGridView2.TabIndex = 1;
            this.dataGridView2.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dataGridView2_CellFormatting);
            // 
            // lblSpendingName
            // 
            this.lblSpendingName.AutoSize = true;
            this.lblSpendingName.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSpendingName.Location = new System.Drawing.Point(0, 0);
            this.lblSpendingName.Name = "lblSpendingName";
            this.lblSpendingName.Size = new System.Drawing.Size(65, 12);
            this.lblSpendingName.TabIndex = 2;
            this.lblSpendingName.Text = "FileName：";
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.progressBar1);
            this.panel4.Controls.Add(this.btnRight90);
            this.panel4.Controls.Add(this.btnLeft90);
            this.panel4.Controls.Add(this.label6);
            this.panel4.Controls.Add(this.btnLoadMap);
            this.panel4.Controls.Add(this.btnFlash);
            this.panel4.Controls.Add(this.btnModifyGps);
            this.panel4.Controls.Add(this.label4);
            this.panel4.Controls.Add(this.textBoxGpsLocation);
            this.panel4.Controls.Add(this.btnModifyOriginTime);
            this.panel4.Controls.Add(this.label3);
            this.panel4.Controls.Add(this.textBoxOrignTime);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(408, 278);
            this.panel4.TabIndex = 2;
            // 
            // progressBar1
            // 
            this.progressBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar1.Location = new System.Drawing.Point(0, 265);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(408, 13);
            this.progressBar1.TabIndex = 11;
            // 
            // btnRight90
            // 
            this.btnRight90.Location = new System.Drawing.Point(97, 187);
            this.btnRight90.Name = "btnRight90";
            this.btnRight90.Size = new System.Drawing.Size(82, 33);
            this.btnRight90.TabIndex = 10;
            this.btnRight90.Text = "右旋90度";
            this.btnRight90.UseVisualStyleBackColor = true;
            this.btnRight90.Click += new System.EventHandler(this.btnRight90_Click);
            // 
            // btnLeft90
            // 
            this.btnLeft90.Location = new System.Drawing.Point(9, 187);
            this.btnLeft90.Name = "btnLeft90";
            this.btnLeft90.Size = new System.Drawing.Size(82, 33);
            this.btnLeft90.TabIndex = 9;
            this.btnLeft90.Text = "左旋90度";
            this.btnLeft90.UseVisualStyleBackColor = true;
            this.btnLeft90.Click += new System.EventHandler(this.btnLeft90_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 172);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 8;
            this.label6.Text = "旋转图片";
            // 
            // btnLoadMap
            // 
            this.btnLoadMap.Location = new System.Drawing.Point(9, 116);
            this.btnLoadMap.Name = "btnLoadMap";
            this.btnLoadMap.Size = new System.Drawing.Size(125, 26);
            this.btnLoadMap.TabIndex = 7;
            this.btnLoadMap.Text = "前往地图读取";
            this.btnLoadMap.UseVisualStyleBackColor = true;
            this.btnLoadMap.Click += new System.EventHandler(this.btnLoadMap_Click);
            // 
            // btnFlash
            // 
            this.btnFlash.Location = new System.Drawing.Point(9, 224);
            this.btnFlash.Name = "btnFlash";
            this.btnFlash.Size = new System.Drawing.Size(125, 35);
            this.btnFlash.TabIndex = 6;
            this.btnFlash.Text = "刷新当前文件信息";
            this.btnFlash.UseVisualStyleBackColor = true;
            this.btnFlash.Click += new System.EventHandler(this.btnRefreshSingleFileMesg_Click);
            // 
            // btnModifyGps
            // 
            this.btnModifyGps.Location = new System.Drawing.Point(309, 91);
            this.btnModifyGps.Name = "btnModifyGps";
            this.btnModifyGps.Size = new System.Drawing.Size(82, 19);
            this.btnModifyGps.TabIndex = 5;
            this.btnModifyGps.Text = "修改";
            this.btnModifyGps.UseVisualStyleBackColor = true;
            this.btnModifyGps.Click += new System.EventHandler(this.btnModifyGps_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 58);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(299, 24);
            this.label4.TabIndex = 4;
            this.label4.Text = "照片拍摄经纬度\r\n（会自动读取剪切板中文本,如匹配GPS数据,自动更新）";
            // 
            // textBoxGpsLocation
            // 
            this.textBoxGpsLocation.Location = new System.Drawing.Point(9, 89);
            this.textBoxGpsLocation.Name = "textBoxGpsLocation";
            this.textBoxGpsLocation.Size = new System.Drawing.Size(294, 21);
            this.textBoxGpsLocation.TabIndex = 3;
            // 
            // btnModifyOriginTime
            // 
            this.btnModifyOriginTime.Location = new System.Drawing.Point(309, 25);
            this.btnModifyOriginTime.Name = "btnModifyOriginTime";
            this.btnModifyOriginTime.Size = new System.Drawing.Size(82, 19);
            this.btnModifyOriginTime.TabIndex = 2;
            this.btnModifyOriginTime.Text = "修改";
            this.btnModifyOriginTime.UseVisualStyleBackColor = true;
            this.btnModifyOriginTime.Click += new System.EventHandler(this.btnModifyOriginTime_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 12);
            this.label3.TabIndex = 1;
            this.label3.Text = "照片原始拍摄日期";
            // 
            // textBoxOrignTime
            // 
            this.textBoxOrignTime.Location = new System.Drawing.Point(9, 24);
            this.textBoxOrignTime.Name = "textBoxOrignTime";
            this.textBoxOrignTime.Size = new System.Drawing.Size(294, 21);
            this.textBoxOrignTime.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 661);
            this.Controls.Add(this.toolStripContainer2);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "照片视频创建时间元数据修改器";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.toolStripContainer2.ContentPanel.ResumeLayout(false);
            this.toolStripContainer2.ResumeLayout(false);
            this.toolStripContainer2.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel1.PerformLayout();
            this.splitContainer4.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
            this.splitContainer4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).EndInit();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripPanel BottomToolStripPanel;
        private System.Windows.Forms.ToolStripPanel TopToolStripPanel;
        private System.Windows.Forms.ToolStripPanel RightToolStripPanel;
        private System.Windows.Forms.ToolStripPanel LeftToolStripPanel;
        private System.Windows.Forms.ToolStripContentPanel ContentPanel;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button btnSelectFolder;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.ToolStripContainer toolStripContainer2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox comboBoxFolders;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.DataGridView dataGridView2;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button btnFlash;
        private System.Windows.Forms.Button btnModifyGps;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxGpsLocation;
        private System.Windows.Forms.Button btnModifyOriginTime;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxOrignTime;
        private System.Windows.Forms.Button btnLoadMap;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnRight90;
        private System.Windows.Forms.Button btnLeft90;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.CheckBox checkBoxThumbnail;
        private System.Windows.Forms.Label lblSpendingName;
        private System.Windows.Forms.ProgressBar progressBar1;
    }
}

