/*****************************************
 *
 * 项目名称： PicTimeTagManage  
 * 文 件 名:  Form1.laodHistoryPath.cs
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PicTimeTagManage
{
    partial class Form1
    {
        private const string HistoryFileName = "folder_history.txt"; // 历史记录文件名
        private const int MaxHistoryCount = 20; // 最大历史记录数
        private void LoadFolderHistory()
        {
            // 暂时移除事件订阅，避免任何潜在的干扰
            comboBoxFolders.SelectionChangeCommitted -= comboBoxFolders_SelectionChangeCommitted;
            comboBoxFolders.Items.Clear();
            if (File.Exists(HistoryFileName))
            {
                try
                {
                    string[] lines = File.ReadAllLines(HistoryFileName);
                    foreach (string line in lines) // 最近的项，保存在最前面，也显示在最前面
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            comboBoxFolders.Items.Add(line.Trim());
                        }
                    }
                    if (comboBoxFolders.Items.Count > 0)
                    {
                        comboBoxFolders.SelectedIndex = 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"加载历史记录失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            // 重新订阅事件
            comboBoxFolders.SelectionChangeCommitted += comboBoxFolders_SelectionChangeCommitted;
        }

        private void UpdateFolderHistory(string newFolderPath)
        {
            List<string> historyList = new List<string>();
            if (File.Exists(HistoryFileName))
            {
                historyList = File.ReadAllLines(HistoryFileName).Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
            }
            int existingIndex = historyList.FindIndex(path => string.Equals(path, newFolderPath, StringComparison.OrdinalIgnoreCase));
            if (existingIndex >= 0)
            {
                historyList.RemoveAt(existingIndex);
            }
            historyList.Insert(0, newFolderPath);
            if (historyList.Count > MaxHistoryCount)
            {
                historyList = historyList.Take(MaxHistoryCount).ToList();
            }
            try
            {
                File.WriteAllLines(HistoryFileName, historyList, Encoding.UTF8); // 指定编码防止乱码
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存历史记录失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            LoadFolderHistory();//修改保存后，重新加载历史目录
        }
    }
}
