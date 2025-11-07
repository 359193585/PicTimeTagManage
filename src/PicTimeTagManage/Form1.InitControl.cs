/*****************************************
 *
 * 项目名称： PicTimeTagManage  
 * 文 件 名:  Form1.InitControl.cs
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

using System;
using System.Data;
using System.Windows.Forms;

namespace PicTimeTagManage
{
    partial class Form1
    {
        private void InitContextMenu()
        {
            // 创建并关联右键菜单
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem menuFileCreateTimeBatchEdit = new ToolStripMenuItem("修改文件名和创建时间 <= 从文件名猜测，选中文件");
            menuFileCreateTimeBatchEdit.Click += menuFileCreateTimeBatchEdit_Click;
            contextMenu.Items.Add(menuFileCreateTimeBatchEdit);

            ToolStripMenuItem menuExifTimeModifyFilename = new ToolStripMenuItem("修改文件名和创建时间 <= 用exif拍摄日期，选中文件");
            menuExifTimeModifyFilename.Click += menuExifTimeModifyFilename_Click;
            contextMenu.Items.Add(menuExifTimeModifyFilename);

            ToolStripMenuItem menuSplitLine1 = new ToolStripMenuItem("--------");//分隔行1
            contextMenu.Items.Add(menuSplitLine1);//分隔行

            ToolStripMenuItem menuExifBatch = new ToolStripMenuItem("exif写入拍摄日期 <= 从文件的创建时间，全部文件");
            menuExifBatch.Click += menuWriteModifyTimeToExifInAllFile_Click;
            contextMenu.Items.Add(menuExifBatch);

            ToolStripMenuItem menuWriteExitToSelectedFile = new ToolStripMenuItem("exif写入拍摄日期 <= 从文件的创建时间，选中文件");
            menuWriteExitToSelectedFile.Click += menuWriteModifyTimeToExifInSelectedFile_Click;
            contextMenu.Items.Add(menuWriteExitToSelectedFile);

            ToolStripMenuItem menuExifGPS = new ToolStripMenuItem("exif写入GPS信息 <= 从用户输入的位置");
            menuExifGPS.Click += menuWriteGpsToExif_Click;
            contextMenu.Items.Add(menuExifGPS);

            ToolStripMenuItem menuRotation90 = new ToolStripMenuItem("批量旋转-顺时针90度 <= 选中文件");
            //1 = 正常（不旋转）3 = 旋转180度 6 = 顺时针旋转90度 8 = 逆时针旋转90度
            menuRotation90.Tag = 6;
            menuRotation90.Click += menuRotation_Click;
            contextMenu.Items.Add(menuRotation90);

            ToolStripMenuItem menuRotation180 = new ToolStripMenuItem("批量旋转-180度 <= 选中文件");
            menuRotation180.Tag = 3;
            menuRotation180.Click += menuRotation_Click;
            contextMenu.Items.Add(menuRotation180);

            ToolStripMenuItem menuRotation270 = new ToolStripMenuItem("批量旋转-逆时针90度 <= 选中文件");
            menuRotation270.Tag = 8;
            menuRotation270.Click += menuRotation_Click;
            contextMenu.Items.Add(menuRotation270);

            ToolStripMenuItem menuSplitLine2 = new ToolStripMenuItem("--------");//分隔行2
            contextMenu.Items.Add(menuSplitLine2);//分隔行

            ToolStripMenuItem menuExifGPSCheck = new ToolStripMenuItem("查询/更新显示选中文件的exif信息");
            menuExifGPSCheck.Click += menuReadTimeGpsFromExif_Click;
            contextMenu.Items.Add(menuExifGPSCheck);

            ToolStripMenuItem menuSplitLine3 = new ToolStripMenuItem("--------");//分隔行3
            contextMenu.Items.Add(menuSplitLine3);//分隔行

            ToolStripMenuItem menuCopyTimeToClipboard = new ToolStripMenuItem("复制选中行的时间值");
            menuCopyTimeToClipboard.Click += menuCopyTimeToClipboard_Click;
            contextMenu.Items.Add(menuCopyTimeToClipboard);

            ToolStripMenuItem menuCopyGPSToClipboard = new ToolStripMenuItem("复制选中行的GPS值");
            menuCopyGPSToClipboard.Click += menuCopyGPSToClipboard_Click;
            contextMenu.Items.Add(menuCopyGPSToClipboard);

            ToolStripMenuItem menuSplitLine4= new ToolStripMenuItem("--------");//分隔行4
            contextMenu.Items.Add(menuSplitLine4);//分隔行

            ToolStripMenuItem menuSaveTimeGpsMesgToFile = new ToolStripMenuItem("查询结果保存到日志文件");
            menuSaveTimeGpsMesgToFile.Click += menuSaveTimeGpsMesgToFile_Click;
            contextMenu.Items.Add(menuSaveTimeGpsMesgToFile);

            ToolStripMenuItem menuCleanSavedMesgFile = new ToolStripMenuItem("查询结果日志文件清理");
            menuCleanSavedMesgFile.Click += menuCleanSavedMesgFile_Click;
            contextMenu.Items.Add(menuCleanSavedMesgFile);

            dataGridView1.ContextMenuStrip = contextMenu;
        }

        private void InitDataGrid()
        {
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.ReadOnly = true;
            //dataGridView1.RowHeadersVisible = false;
            dataGridView1.Columns.Clear();
            // 初始化DataGridView列
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FixedRowNumber",
                DataPropertyName = "FixedRowNumber", // 绑定到FileInfoDisplay的FileName属性
                HeaderText = "行号",
                ReadOnly = true,
                Width = 70
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FileName",
                DataPropertyName = "FileName", // 绑定到FileInfoDisplay的FileName属性
                HeaderText = "文件名",
                Width = 200
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CreationTime",
                DataPropertyName = "CreationTime",
                HeaderText = "创建时间",
                Width = 120
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "LastWriteTime",
                DataPropertyName = "LastWriteTime",
                HeaderText = "修改时间",
                Width = 120
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ExifTime",
                DataPropertyName = "ExifTime",
                HeaderText = "原始拍摄时间",
                Width = 120
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ExifGPS",
                DataPropertyName = "ExifGPS",
                HeaderText = "GPS位置",
                Width = 120
            }); 
            dataGridView1.Columns.Add(new DataGridViewImageColumn
            {
                Name = "ImageColumn",
                DataPropertyName = "ImageColumn",
                HeaderText = "缩略图",
                Width = 200,
                Visible=false,
                ImageLayout = DataGridViewImageCellLayout.Zoom  // 等比例缩放图片，保持不失真
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FullPath",
                DataPropertyName = "FullPath",
                HeaderText = "文件路径",
                Width = 450
            });
           

            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = true; // 启用多行选择
            dataGridView1.CellClick += DataGridView1_CellClick;
            // 订阅DataGridView的选择改变事件
            dataGridView1.SelectionChanged += DataGridView1_SelectionChanged;
            SetupSelectionTimer();
            dataGridView1.DataSource = _fileBindingList;


            // 启用虚拟模式
            dataGridView1.VirtualMode = true;
            dataGridView1.CellValueNeeded += DataGridView1_CellValueNeeded; // 为需要显示的单元格提供数据
            dataGridView1.Scroll += DataGridView1_Scroll;

            // 通过反射设置DataGridView的双缓冲属性为true
            Type dgvType = dataGridView1.GetType();
            System.Reflection.PropertyInfo pi = dgvType.GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            pi.SetValue(dataGridView1, true, null);
        }
        private void InitDataGridBindDataTable()
        {
            try
            {
                keyValueExifData = new DataTable();
                this.keyValueExifData.Columns.Add("Key", typeof(string));
                this.keyValueExifData.Columns.Add("Value", typeof(string));
                dataGridView2.DataSource = keyValueExifData;
                dataGridView2.RowHeadersVisible = false; // 隐藏行头
                dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // 自动调整列宽
                dataGridView2.ReadOnly = true; // 设置为只读
            }
            catch { }
        }
        #region not used
        //private void InitDataGridExifMesg()
        //{
        //    dataGridView2.AutoGenerateColumns = false;
        //    dataGridView2.Columns.Clear();
        //    // 初始化DataGridView列
        //    dataGridView2.Columns.Add(new DataGridViewTextBoxColumn
        //    {
        //        Name = "FileCreateTime",
        //        DataPropertyName = "FileCreateTime",
        //        HeaderText = "创建时间",
        //        Width = 200
        //    });
        //    dataGridView2.Columns.Add(new DataGridViewTextBoxColumn
        //    {
        //        Name = "FileModifyTime",
        //        DataPropertyName = "FileModifyTime",
        //        HeaderText = "修改时间",
        //        Width = 200
        //    });
        //    dataGridView2.Columns.Add(new DataGridViewTextBoxColumn
        //    {
        //        Name = "Exif.DateTimeOriginal",
        //        DataPropertyName = "Exif.DateTimeOriginal",
        //        HeaderText = "原始拍摄时间",
        //        Width = 200
        //    });
        //    dataGridView2.Columns.Add(new DataGridViewTextBoxColumn
        //    {
        //        Name = "Exif.GPSLatitude",
        //        DataPropertyName = "Exif.GPSLatitude",
        //        HeaderText = "纬度",
        //        Width = 200
        //    });
        //    dataGridView2.Columns.Add(new DataGridViewTextBoxColumn
        //    {
        //        Name = "Exif.GPSLongitude",
        //        DataPropertyName = "Exif.GPSLongitude",
        //        HeaderText = "经度",
        //        Width = 200
        //    });
        //    DataGridViewImageColumn imageColumn = new DataGridViewImageColumn();
        //    imageColumn.Name = "ImageColumn";
        //    imageColumn.DataPropertyName = "ImageColumn";
        //    imageColumn.HeaderText = "缩略图";
        //    imageColumn.Width = 200;
        //    imageColumn.ImageLayout = DataGridViewImageCellLayout.Zoom; // 等比例缩放图片，保持不失真
        //    dataGridView2.Columns.Add(imageColumn);

        //    dataGridView2.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        //    dataGridView2.MultiSelect = false;
        //    dataGridView2.DataSource = _exifMetadataBindingList;
        //}
        #endregion
    }
}
