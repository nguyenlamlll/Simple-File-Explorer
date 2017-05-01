using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _15520405
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// Is user performing copying task or not.
        /// </summary>
        private bool isCopying = false;

        /// <summary>
        /// Is user performing cutting task or not.
        /// </summary>
        private bool isCutting = false;

        private bool isFolder = false;

        private bool isRenaming = false;

        /// <summary>
        /// Is the list view focused or not.
        /// </summary>
        private bool isListView = false;

        /// <summary>
        /// The item to paste.
        /// </summary>
        private ListViewItem itemPaste;

        private string currentPath;

        private string pathFolder;
        private string pathFile;
        private string pathNode;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            TreeListView.CreateTreeView(this.treeView1);

            toolStripComboBox_Path.Width = this.Width - 120;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode currentNode = e.Node;
            bool isOK = false;
            if (currentNode.Text != "My Computer")
            {
                isOK = TreeListView.ShowFolderTree(this.treeView1, this.listView1, currentNode);
                toolStripComboBox_Path.Text = TreeListView.GetFullPath(currentNode.FullPath);
            }
            if (isOK)
            {
                //Get path to copy/cut
                pathNode = toolStripComboBox_Path.Text;
                //Current folder in Listview (paste)
                currentPath = pathNode;
            }
        }

        /// <summary>
        /// When user double-clicks on the items in the list view, perform also:
        /// - Get the path to display on the path field.
        /// - Open the item (if "Folder")
        /// </summary>
        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewItem item = this.listView1.FocusedItem;
            bool isOK = TreeListView.ClickItem(this.listView1, item);
            if (isOK)
            {
                if (item.SubItems[1].Text == "Folder")
                    toolStripComboBox_Path.Text = item.SubItems[4].Text + "\\";
                currentPath = toolStripComboBox_Path.Text;
            }
        }

        /// <summary>
        /// Perform Click on the focused item in the List View with Enter Key on they keyboard   
        /// </summary>
        private void listView1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                bool isOK = TreeListView.ShowContent(this.listView1, toolStripComboBox_Path.Text);
                if (isOK)
                {
                    currentPath = toolStripComboBox_Path.Text;
                }
            }
        }

        private void toolStripButton_Go_Click(object sender, EventArgs e)
        {
            try
            {
                if (toolStripComboBox_Path.Text != "")
                {
                    bool isOk;
                    FileInfo f = new FileInfo(toolStripComboBox_Path.Text.Trim());
                    if (f.Exists)
                    {
                        System.Diagnostics.Process.Start(toolStripComboBox_Path.Text.Trim());
                        DirectoryInfo parent = f.Directory;
                        toolStripComboBox_Path.Text = parent.FullName;
                        return;
                    }
                    else
                    {
                        isOk =
                        TreeListView.ShowContent(listView1, toolStripComboBox_Path.Text);
                    }
                    if (isOk)
                    {
                        currentPath = toolStripComboBox_Path.Text;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void toolStripComboBox_Path_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                TreeListView.ShowContent(this.listView1, toolStripComboBox_Path.Text);
            }
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            toolStripComboBox_Path.Width = Width - 120;
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isCopying = true;
            if (listView1.Focused)
            {
                isListView = true;
                itemPaste = listView1.FocusedItem;
                if (itemPaste == null) return;
                if (itemPaste.SubItems[1].Text.Trim() == "Folder")
                {
                    isFolder = true;
                    pathFolder = itemPaste.SubItems[4].Text + "\\";
                }
                else
                {
                    isFolder = false;
                    pathFile = itemPaste.SubItems[4].Text;
                }
            }
            else if (treeView1.Focused)
            {
                isListView = false;
                isFolder = true;
            }
            pasteToolStripMenuItem.Enabled = true;
            toolStripButton_Paste.Enabled = true;
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isCutting = true;
            if (listView1.Focused)
            {
                isListView = true;
                itemPaste = listView1.FocusedItem;
                itemPaste.ForeColor = Color.LightGray; // Distinguish the item
                if (itemPaste.SubItems[1].Text.Trim() == "Folder")
                {
                    isFolder = true;
                    pathFolder = itemPaste.SubItems[4].Text + "\\";
                }
                else
                {
                    isFolder = false;
                    pathFile = itemPaste.SubItems[4].Text;
                }
            }
            else if (treeView1.Focused)
            {
                isListView = false;
                isFolder = true;
            }
            pasteToolStripMenuItem.Enabled = true;
            toolStripButton_Paste.Enabled = true;
            
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string pathSource, pathDest;
                if (isListView)
                {
                    if (isFolder)
                    {
                        pathSource = pathFolder;
                        pathDest = currentPath;
                    }
                    else
                    {
                        pathSource = pathFile;
                        pathDest = currentPath + itemPaste.Text;
                    }
                }
                else
                {
                    pathSource = pathNode;
                    pathDest = currentPath;
                }
                if (isCopying)
                {
                    if (isFolder) Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(pathSource, pathDest);
                    else Microsoft.VisualBasic.FileIO.FileSystem.CopyFile(pathSource, pathDest);
                    isCopying = false;
                }
                if (isCutting)
                {
                    if (isFolder) Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(pathSource, pathDest);
                    else Microsoft.VisualBasic.FileIO.FileSystem.MoveFile(pathSource, pathDest);
                    isCutting = false;
                }
                string strPath;
                if (!isFolder) strPath = TreeListView.GetPathDir(pathDest);
                else strPath = pathDest;

                //Refresh
                TreeListView.ShowContent(listView1, strPath);

                pasteToolStripMenuItem.Enabled = false;
                toolStripButton_Paste.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void toolStripButton_Copy_Click(object sender, EventArgs e)
        {
            copyToolStripMenuItem.PerformClick();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            cutToolStripMenuItem.PerformClick();
        }

        private void toolStripButton_Paste_Click(object sender, EventArgs e)
        {
            pasteToolStripMenuItem.PerformClick();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.Focused)
                {
                    ListViewItem item = new ListViewItem();
                    item = listView1.FocusedItem;
                    TreeListView.DeleteItem(listView1, item);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void toolStripButton_Delete_Click(object sender, EventArgs e)
        {
            deleteToolStripMenuItem.PerformClick();
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isRenaming = true;
            listView1.SelectedItems[0].BeginEdit();
        }

        private void listView1_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            try
            {
                if (isRenaming)
                {
                    ListViewItem item = listView1.FocusedItem;
                    string path = item.SubItems[4].Text;
                    if (e.Label == null)
                    {
                        return;
                    }
                    FileInfo fi = new FileInfo(path);
                    if (fi.Exists)
                    {
                        Microsoft.VisualBasic.FileIO.FileSystem.RenameFile(path, e.Label);
                        string pathFolder = TreeListView.GetPathDir(path);
                        TreeListView.ShowContent(listView1, pathFolder);
                    }
                    else
                    {
                        Microsoft.VisualBasic.FileIO.FileSystem.RenameDirectory(path, e.Label);
                        string pathFolder = TreeListView.GetPathDir(path);
                        TreeListView.ShowContent(listView1, pathFolder);
                    }
                    e.CancelEdit = true;
                    isRenaming = false;
                }
            }
            catch (IOException)
            {
                MessageBox.Show("File hoặc thư mục đã tồn tại");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void toolStripButton_Refresh_Click(object sender, EventArgs e)
        {
            if (currentPath != "")
            {
                TreeListView.ShowContent(listView1, currentPath);
            }
        }

        private void toolStripButton_Up_Click(object sender, EventArgs e)
        {
            try
            {
                string path = currentPath;
                if (path != "")
                {
                    if (path.LastIndexOf(":\\") != path.Length - 2 && path.LastIndexOf("\\") == path.Length - 1)
                        path = path.Remove(path.Length - 1);
                    string parentDir = TreeListView.GetPathDir(path);
                    toolStripComboBox_Path.Text = parentDir;
                    currentPath = parentDir;
                    TreeListView.ShowContent(listView1, parentDir);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void largeIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.LargeIcon;
        }

        private void smallIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.SmallIcon;
        }

        private void listToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.List;
        }

        private void detailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.Details;
        }

        private void largeIconsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            largeIconsToolStripMenuItem.PerformClick();
        }

        private void smallIconsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            smallIconsToolStripMenuItem.PerformClick();
        }

        private void listToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            listToolStripMenuItem.PerformClick();
        }

        private void detailsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            detailsToolStripMenuItem.PerformClick();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm form = new AboutForm();
            form.Show();
        }
    }
}
