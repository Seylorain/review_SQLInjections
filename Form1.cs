using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace SQLInjections
{
    public partial class Form1 : Form
    {
        
        private string dbFileName = "DBSqlLite";
        private string dbTableName = "TestTable";
        private SQLiteConnection m_dbConn;
        private SQLiteCommand m_sqlCmd;
        public Form1()
        {
            InitializeComponent();

            m_dbConn = new SQLiteConnection();
            m_sqlCmd = new SQLiteCommand();

            dbFileName = "DBSqlLite";
            OpenDB();
            CreateDBAndTable();
        }
        /*****************************************************************
            CWE                 521: Weak Password Requirements

            Описание            Не задан пароль при подключении к встроенной БД.

            Решение проблемы    Задать пароль при подключение к БД
                                
                                 
                                 m_dbConn = new SQLiteConnection("Data Source=" + dbFileName + ";Version=3;");
                                 m_dbConn.SetPassword(password);

                                 Пароль так же необходимо хранить в зашифрованном файле.

           Источник:             https://clck.ru/NMLz3
        */
        private void OpenDB()
        {
            if (!File.Exists(dbFileName))
                SQLiteConnection.CreateFile(dbFileName);
            try
            {
                m_dbConn = new SQLiteConnection("Data Source=" + dbFileName + ";Version=3;");
                m_dbConn.Open();
                m_sqlCmd.Connection = m_dbConn;
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private void CreateDBAndTable() 
        {    
            try
            {
                m_sqlCmd.CommandText = "CREATE TABLE IF NOT EXISTS "+ dbTableName + " (id INTEGER PRIMARY KEY AUTOINCREMENT, FirstName TEXT, SecondName TEXT, LastName TEXT)";
                m_sqlCmd.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private void DropTable()
        {
            try
            {
                m_sqlCmd.CommandText = "DELETE from  " + dbTableName;
                m_sqlCmd.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private void AddData()
        {
            Random rand = new Random();
            string word = "";
            int n = 3;
            for (int i = 0; i < n; i++)
            {
                word = Guid.NewGuid().ToString();
                m_sqlCmd.CommandText = "INSERT INTO "+ dbTableName  + " ('FirstName', 'SecondName','LastName') values ('" 
                    + word.Substring(0, rand.Next(1, 15)) + "' , '" 
                    + word.Substring(0, rand.Next(1, 15)) + "' , '" 
                    + word.Substring(0, rand.Next(1, 15)) + "')";
                m_sqlCmd.ExecuteNonQuery();
            }


        }
        private void GetData()
        {
            /*****************************************************************
                            CWE                 89: Improper Neutralization of Special Elements used in an SQL Command ('SQL Injection')

                            Описание            Возможность внедрения SQL - кода.
                            
                                                1) Можно внедрить SQL код, который выведет содержание системных таблиц:
                                        
                                                '1' UNION SELECT type,name,tbl_name,rootpage FROM sqlite_master

                                                    Аналогично с таблицами sqlite_sequence, sqlite_stat1.

                                                2) Можно внедрить SQL - код, который выведет содержимое всей таблицы dbTableName, избегая правильного ввода id:

                                                '1' OR 1=1;

                            Решение проблемы    Проверять поступающие на ввод данные.
                                                sqlQuery = "Select * from " + dbTableName + " where id= @id";
                                                m_sqlCmd.Parameters.AddWithValue("@id", textBoxId.Text);


                            Источник:            https://oracleplsql.ru/system-tables-sqlite.html
            */
            DataTable dTable = new DataTable();
            String sqlQuery;
            sqlQuery = "Select * from " + dbTableName + " where id= " + textBoxId.Text;

            try
            {
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqlQuery, m_dbConn);
                adapter.Fill(dTable);
                dataGridView1.Rows.Clear();
                dataGridView1.Columns.Clear();
                for (int i = 0; i < dTable.Columns.Count; i++)
                    dataGridView1.Columns.Add(i.ToString(), i.ToString());

                for (int i = 0; i < dTable.Rows.Count; i++)
                    dataGridView1.Rows.Add(dTable.Rows[i].ItemArray);
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            GetData();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            AddData();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            DropTable();
        }
    }
}
