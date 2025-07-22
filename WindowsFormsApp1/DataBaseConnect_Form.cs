using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Sql;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.SqlServer.Management.Smo;
using System.Data.SqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WindowsFormsApp1
{
    public partial class DataBaseConnect_Form : Form
    {
        dataBaseManager dbManager = new dataBaseManager();

        public DataBaseConnect_Form()
        {
            InitializeComponent();
            Find();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != string.Empty)
                if (dbManager.CreateNewDB(textBox1.Text))
                    MessageBox.Show("База данных создана");
            else
                    MessageBox.Show("Ошибка создания базы данных");
            Find();

            comboBox1.SelectedText = textBox1.Text;
            textBox1.Text = string.Empty;
        }

        void Find()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(dbManager.GetDataBases().ToArray());
        }

        private void Create_button_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text != string.Empty)
                dbManager.ConnectToDB(comboBox1.Text);
            
            Form1.dataBaseManager = dbManager;
            Close();
        }
    }
}
