using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sinan.Extensions;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Bson;
using Sinan.Util;
using Sinan.FastJson;
using System.Configuration;

namespace WindowsConfig
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string baseConnect = this.textBox1.Text;
            string path = GetPath();
            int x = ConfigFile.ConfigToFile(baseConnect, path);
            MessageBox.Show("生成成功:" + x + "个文件");
        }

        private string GetPath()
        {
            string path = this.textBox2.Text;
            if (string.IsNullOrWhiteSpace(path))
            {
                path = AppDomain.CurrentDomain.BaseDirectory;
                path = Path.Combine(path, "game");
            }
            return path;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string path = GetPath();
            System.IO.Directory.Delete(path, true);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //读取文件..
            string path = this.tmFile.Text;
            Dictionary<string, int> orders = ReadFile(path);
            string filename = Path.GetFileNameWithoutExtension(path);
            string data = filename.Substring(6, 10);

            ReadDb(data, orders);
        }

        private void ReadDb(string data, Dictionary<string, int> orders)
        {
            DateTime d = DateTime.Parse(data).Date;
            string connect = this.orderDB.Text;

            string x = d.ToString("-yyyyMMdd-");
            var m_collection = MongoDatabase.Create(connect).GetCollection("Order");
            var query = Query.And(Query.GT("Created", d.AddHours(-1)),
                Query.LT("Created", d.AddHours(25)),
                Query.Matches("billno", new BsonRegularExpression(".*" + x + ".*", "i"))
                );
            var all = m_collection.FindAs<Sinan.Util.Variant>(query);

            List<Variant> lost = new List<Variant>();
            List<Variant> canOrder = new List<Variant>();

            StringBuilder sb = new StringBuilder();
            int count = 0;
            foreach (Variant order in all)
            {
                count++;
                string billno = order.GetStringOrDefault("billno");
                int amt;
                if (!orders.TryGetValue(billno, out amt))
                {
                    lost.Add(order);
                }

                else if (amt == 0)
                {
                    canOrder.Add(order);
                }
            }
            LogDiff(data, lost, canOrder);
            MessageBox.Show("成功:" + count + ",lost:" + lost.Count + ",cancel:" + canOrder.Count);
        }

        private static void LogDiff(string data, List<Variant> lost, List<Variant> canOrder)
        {
            using (FileStream fs = File.Open("lost" + data + ".txt", FileMode.OpenOrCreate))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                foreach (Variant order in lost)
                {
                    sw.Write("丢失:");
                    sw.WriteLine(JsonConvert.SerializeObject(order));
                }
            }

            using (FileStream fs = File.Open("can" + data + ".txt", FileMode.OpenOrCreate))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                foreach (Variant order in canOrder)
                {
                    sw.Write("取消:");
                    sw.WriteLine(JsonConvert.SerializeObject(order));
                }
            }
        }

        //TM的订单
        private static Dictionary<string, int> ReadFile(string path)
        {
            Dictionary<string, int> orders = new Dictionary<string, int>();
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader sr = new StreamReader(fs, Encoding.Default, false))
            {
                string key = sr.ReadLine();
                while (key != null)
                {
                    string[] t = key.Split('|');
                    if (t.Length > 4)
                    {
                        int amt;
                        if (int.TryParse(t[2], out amt))
                        {
                            orders.SetOrInc(t[0], t[3] == "支付" ? amt : -amt);
                        }
                    }
                    key = sr.ReadLine();
                }
            }
            return orders;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string key = this.comboBox1.Text;
            this.textBox1.Text = ConfigurationManager.ConnectionStrings[key].ConnectionString;
            string path = ConfigurationManager.AppSettings[key];
            if (!string.IsNullOrEmpty(path))
            {
                this.textBox2.Text = path;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

            try
            {
                this.textBox1.Text = ConfigurationManager.AppSettings["defaultDB"];
                this.textBox2.Text = ConfigurationManager.AppSettings["defaultSaveDir"];
            }
            catch { }
            foreach (ConnectionStringSettings v in ConfigurationManager.ConnectionStrings)
            {
                string name = v.Name;
                if (v.ConnectionString.StartsWith("mongodb"))
                {
                    this.comboBox1.Items.Add(name);
                }
            }
            int count = ConfigurationManager.ConnectionStrings.Count;
            //string[] keys = ConfigurationManager.ConnectionStrings.Count;
            //this.comboBox1.Items.AddRange(keys);
            this.comboBox1.SelectedIndex = 0;
        }
    }
}
