using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.IO;
using System.Diagnostics;

namespace _15520405
{
    static class TreeListView
    {
        public static void CreateTreeView(TreeView treeView)
        {
            const int RemovableDisk = 2;
            const int LocalDisk = 3;
            const int NetworkDisk = 4;
            const int CDDisk = 5;

            TreeNode tnMyComputer = new TreeNode("My Computer", 0, 0);
            //Make sure the treeView is clear before adding to it.
            treeView.Nodes.Clear();
            treeView.Nodes.Add(tnMyComputer);

            TreeNodeCollection nodeCollection = tnMyComputer.Nodes;

            //Retrieve Logical Disks
            ManagementObjectSearcher query = new ManagementObjectSearcher("Select * From Win32_LogicalDisk");
            ManagementObjectCollection queryCollection = query.Get();

            TreeNode diskTreeNode;
            foreach (ManagementObject mo in queryCollection)
            {
                int imageIndex, selectIndex;
                switch (int.Parse(mo["DriveType"].ToString()))
                {
                    case RemovableDisk:
                        {
                            imageIndex = 1;
                            selectIndex = 1;
                            break;
                        }
                    case LocalDisk:
                        {
                            imageIndex = 2;
                            selectIndex = 2;
                            break;
                        }
                    case CDDisk:
                        {
                            imageIndex = 3;
                            selectIndex = 3;
                            break;
                        }
                    case NetworkDisk:
                        {
                            imageIndex = 4;
                            selectIndex = 4;
                            break;
                        }
                    default:
                        {
                            imageIndex = 5;
                            selectIndex = 6;
                            break;
                        }
                }


                diskTreeNode = new TreeNode(mo["Name"].ToString() + "\\", imageIndex, selectIndex);
                nodeCollection.Add(diskTreeNode);
            }
        }

        internal static bool ClickItem(ListView listView, ListViewItem LVitem)
        {
            try
            {
                string path = LVitem.SubItems[4].Text;
                FileInfo fi = new FileInfo(path);
                if (fi.Exists) { Process.Start(path); }
                else
                {
                    ListViewItem item;
                    DirectoryInfo directory = new DirectoryInfo(path + "\\");
                    if (!directory.Exists)
                    {
                        MessageBox.Show("Không tồn tại thư mục", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    listView.Items.Clear();

                    foreach(DirectoryInfo folder in directory.GetDirectories())
                    {
                        item = GetLVItems(folder);
                        listView.Items.Add(item);
                    }
                    foreach (FileInfo file in directory.GetFiles())
                    {
                        item = GetLVItems(file);
                        listView.Items.Add(item);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return false;
        }

        public static bool ShowFolderTree(TreeView treeView, ListView listView, TreeNode currentNode)
        {
            if (currentNode.Text != "My Computer")
            {
                try
                {
                    if (Directory.Exists(GetFullPath(currentNode.FullPath)) == false)
                    {
                        MessageBox.Show("Ổ đĩa hoặc thư mục không tồn tại");
                    }
                    else
                    {
                        string[] Directories = Directory.GetDirectories(GetFullPath(currentNode.FullPath));

                        string Name = "";
                        TreeNode nodeDir;
                        foreach (string Dir in Directories)
                        {
                            Name = GetName(Dir);
                            nodeDir = new TreeNode(Name, 5, 6);
                            currentNode.Nodes.Add(nodeDir);
                        }
                        ShowContent(listView, currentNode);

                    }
                    return true;
                }
                catch (IOException ex)
                {
                    MessageBox.Show("Ổ đĩa hoặc thư mục không tồn tại: " + ex.Message);
                }
                catch (UnauthorizedAccessException ex2)
                {
                    MessageBox.Show("Bạn không có quyền truy cập vào ổ đĩa hoặc thư mục này: " + ex2.Message);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
            return false;
        }

        public static string GetFullPath(string strPath)
        {
            return strPath.Replace("My Computer\\", "").Replace("\\\\", "\\");
        }

        public static string GetName(string strPath)
        {
            string[] Split = strPath.Split('\\');
            int maxIndex = Split.Length;
            return Split[maxIndex - 1];
        }

        public static void ShowContent(ListView listView, TreeNode currentNode)
        {
            try
            {
                listView.Items.Clear();
                ListViewItem item;

                DirectoryInfo directory = GetPathDir(currentNode);

                foreach (DirectoryInfo folder in directory.GetDirectories())
                {
                    item = GetLVItems(folder);
                    listView.Items.Add(item);
                }

                foreach (FileInfo file in directory.GetFiles())
                {
                    item = GetLVItems(file);
                    listView.Items.Add(item);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public static bool ShowContent(ListView listView, string strPath)
        {
            try
            {
                if (!strPath.EndsWith("\\")) strPath += "\\";
                ListViewItem item;
                DirectoryInfo directory = new DirectoryInfo(strPath);
                if (!directory.Exists)
                {
                    MessageBox.Show("Thư mục không tồn tại", "Lỗi");
                    return false;
                }
                listView.Items.Clear();

                foreach (DirectoryInfo folder in directory.GetDirectories())
                {
                    item = GetLVItems(folder);
                    listView.Items.Add(item);
                }

                foreach(FileInfo file in directory.GetFiles())
                {
                    item = GetLVItems(file);
                    listView.Items.Add(item);
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return false;
        }

        public static ListViewItem GetLVItems(DirectoryInfo folder)
        {
            string[] item = new string[5];
            item[0] = folder.Name;
            item[1] = "Folder";
            item[2] = folder.CreationTime.ToString();
            item[3] = folder.LastWriteTime.ToString();
            item[4] = folder.FullName;

            ListViewItem LVItem = new ListViewItem(item);
            LVItem.ImageIndex = 3; //Folder Image
            return LVItem;
        }

        public static ListViewItem GetLVItems(FileInfo file)
        {
            long size = 0;
            string[] s = { file.Name, file.Extension.ToUpper(), size + "KB", file.LastWriteTime.ToString(),
                file.FullName, file.Directory.FullName };

            string[] item = new string[5];
            item[0] = file.Name;
            item[1] = (file.Length / 1024).ToString() + " KB";
            item[2] = file.CreationTime.ToString();
            item[3] = file.LastWriteTime.ToString();
            item[4] = file.FullName;

            ListViewItem LVItem = new ListViewItem(item);
            LVItem.ImageIndex = GetImageIndex(file);
            return LVItem;
        }

        private static int GetImageIndex(FileInfo file)
        {
            switch (file.Extension.ToUpper())
            {
                case ".DOC": return 0;
                case ".EXE": return 1;
                case ".HTM": return 4;
                case ".HTML": return 5;
                case ".MP3": return 6;
                case ".PDF": return 7;
                case ".JPG": return 8;
                case ".PNG": return 8;
                case ".PPT": return 9;
                case ".PPTX": return 9;
                case ".RAR": return 10;
                case ".TXT": return 11;
                default: return 2;
            }
        }

        private static DirectoryInfo GetPathDir(TreeNode currentNode)
        {
            string[] List = currentNode.FullPath.Split('\\');
            string strPath = List.GetValue(1).ToString();
            for (int i = 2; i < List.Length; i++)
            {
                strPath += List.GetValue(i) + "\\";
            }
            return new DirectoryInfo(strPath);
        }

        public static string GetPathDir(string path)
        {
            string[] strList = path.Split('\\');
            string strPath = "";
            for (int i = 0; i< strList.Length - 1; i++)
            {
                strPath += strList.GetValue(i) + "\\";
            }
            return strPath;
        }
        public static void DeleteItem(ListView listView, ListViewItem LVitem)
        {
            try
            {
                string path = LVitem.SubItems[4].Text;
                if (LVitem.SubItems[1].Text == "Folder")
                {
                    DirectoryInfo directory = new DirectoryInfo(path + "\\");
                    if (!directory.Exists)
                    {
                        MessageBox.Show("Không tồn tại thư mục", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    else
                    {
                        DialogResult dialog = MessageBox.Show("Bạn có chắc chắn muốn xóa thư mục " + LVitem.Text.ToString() + "?",
                            "Cảnh Báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                        if (dialog == DialogResult.Yes)
                        {
                            directory.Delete(true);
                        }
                        else
                        {
                            return;
                        }
                        string pathFolder = GetPathDir(path);
                        ShowContent(listView, pathFolder);
                    }
                }
                else
                {
                    FileInfo file = new FileInfo(path);
                    if (!file.Exists)
                    {
                        MessageBox.Show("File không tồn tại", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    else
                    {
                        DialogResult dialog = MessageBox.Show("Bạn có chắc chắn muốn xóa file " + LVitem.Text.ToString() + "?",
                            "Cảnh Báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                        if (dialog == DialogResult.Yes)
                        {
                            file.Delete();
                        }
                        else return;
                        string pathFolder = GetPathDir(path);
                        ShowContent(listView, pathFolder);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
