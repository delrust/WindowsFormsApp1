using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public static dataBaseManager dataBaseManager;

        // Паттерн для поиска подстроки вида " (число шт.)" в конце строки
        string pattern = @"\s*\(\d+\s*шт\.\)$";

        public Form1()
        {
            InitializeComponent();
            treeView.ContextMenuStrip = contextMenuStrip1;
            DataBaseConnect_Form dbForm = new DataBaseConnect_Form();
            dbForm.ShowDialog();
            FillTree();
            panel3.Visible = false;

        }

        private void AddUpperComponent_button_Click(object sender, EventArgs e)
        {
            GenerateAddingPanel(true);
        }

        private void GenerateAddingPanel(bool isUpperComponent = false, string parentName = "-")
        {
            panel2.Visible = true;
            if (parentName != "-")
            {
                ParentName_label.Text = Regex.Replace(parentName, pattern, "");
            }
            if (isUpperComponent)
            {
                label5.Visible = false;
                Count_textBox.Visible = false;
            }
            else
            {
                label5.Visible = true;
                Count_textBox.Visible = true;
            }

        }

        private void CancelAddComponent_button_Click(object sender, EventArgs e)
        {
            panel2.Visible = false;
            Name_textBox.Text = string.Empty;
            Count_textBox.Text = string.Empty;
        }

        private void AcseptAddComponent_button_Click(object sender, EventArgs e)
        {
            if (Name_textBox.Text != string.Empty)
            {
                if (Count_textBox.Visible == false && Count_textBox.Text == string.Empty) //верхнего уровня
                {
                    AddNewComponent(Name_textBox.Text);
                }
                else
                {
                    int count = Count_textBox.Text != string.Empty ? int.Parse(Count_textBox.Text) : 0;
                    if (count != 0)
                        AddNewComponent(Name_textBox.Text, ParentName_label.Text, count);
                    else
                        MessageBox.Show("Введите количество");
                }
            }
            panel2.Visible = false;
            Name_textBox.Text = string.Empty;
            Count_textBox.Text = string.Empty;
        }

        private void AddNewComponent(string name, string paren = "", int count = 0)
        {
            if(paren == string.Empty)
                dataBaseManager.CreateNewComponent(name);
            else
                dataBaseManager.AddComponentToObj(name, paren, count);

            FillTree();
        }

        private void FillTree()
        { 
            treeView.Nodes.Clear();
            List<Component> list = dataBaseManager.GetAllComponents();

            // Рекурсивно добавляем узлы
            foreach (Component rootComp in list)
            {
                TreeNode rootNode = new TreeNode(rootComp.Name);
                treeView.Nodes.Add(rootNode);

                AddChildNodes(rootNode, rootComp, rootComp.Children);
            }
            treeView.ExpandAll();
        }


        private void AddChildNodes(TreeNode parentNode, Component parentComponent, List<Component> children)
        {
            foreach (Component childComp in children)
            {
                TreeNode childNode = new TreeNode($"{childComp.Name} ({childComp.Quantity} шт.)");
                parentNode.Nodes.Add(childNode);

                // Рекурсивно добавляем детей следующего уровня
                AddChildNodes(childNode, childComp, childComp.Children);
            }
        }


        private void treeView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // Получаем узел под курсором
                TreeNode node = treeView.GetNodeAt(e.X, e.Y);

                // Если кликнули по узлу
                if (node != null)
                {
                    // Выделяем узел
                    treeView.SelectedNode = node;

                    // Показываем контекстное меню
                    contextMenuStrip1.Show(treeView, e.Location);

                    // Можно настроить видимость пунктов меню динамически
                    //SetupMenuItems(node);
                }
            }
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode != null)
            {
                // Подтверждение удаления
                DialogResult result = MessageBox.Show(
                    "Вы уверены, что хотите удалить этот элемент?",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    treeView.Nodes.Remove(treeView.SelectedNode);
                }
            }
        }


        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode != null)
            {
                GenerateAddingPanel(false, treeView.SelectedNode.Text);
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            string componentName = treeView.SelectedNode.Text;
            string parentName = treeView.SelectedNode.Parent.Text;

            componentName = Regex.Replace(componentName, pattern, "");
            parentName = Regex.Replace(parentName, pattern, "");

            dataBaseManager.DeleteComponents(componentName, parentName);
            FillTree();
        }

        private void ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panel3.Visible = true;

            string oldName = Regex.Replace(treeView.SelectedNode.Text, pattern, "");
            label7.Text = oldName;
        }


        private void button2_Click(object sender, EventArgs e)
        {
            string oldName = Regex.Replace(treeView.SelectedNode.Text, pattern, "");

            if (!dataBaseManager.Update(oldName, textBox2.Text))
                MessageBox.Show("Такой объект уже существует");

            panel3.Visible = false;
            textBox2.Text = string.Empty;
            FillTree();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            panel3.Visible = false;
            textBox2.Text = string.Empty;
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            TreeNode node = treeView.SelectedNode;
            var data = ExtractQuantitiesFromTree(node);

            string name = $"Отчет_{node.Text}";
            var creator = new OpenOfficeTableCreator();
            creator.CreateDictionaryTable(data, $"C:\\Users\\Денис\\Desktop\\{name}.docx");
        }

        public Dictionary<string, string> ExtractQuantitiesFromTree(TreeNode rootNode)
        {
            var result = new Dictionary<string, string>();
            ProcessTreeNode(rootNode, result);
            return result;
        }

        private void ProcessTreeNode(TreeNode node, Dictionary<string, string> result)
        {
            if (node == null) return;

            // Обрабатываем текущий узел
            var match = Regex.Match(node.Text, @"^(.*?)\s*\((\d+)\s*шт\.\)$");
            if (match.Success)
            {
                string name = match.Groups[1].Value.Trim();
                string quantity = match.Groups[2].Value;

                if (!result.ContainsKey(name))
                {
                    result.Add(name, quantity);
                }
            }

            // Рекурсивно обрабатываем дочерние узлы
            foreach (TreeNode childNode in node.Nodes)
            {
                ProcessTreeNode(childNode, result);
            }
        }
    }
}
