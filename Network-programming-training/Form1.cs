using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace Network_programming_training
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private delegate void WriteToTextBox(string str);

        private WriteToTextBox _writeToTextBox;

        private void button1_Click(object sender, EventArgs e)
        {
            Thread t1=new Thread(Execute1);
            t1.Start();
            Thread t2 = new Thread(Execute2);
            t2.Start();
        }

        private void Execute1()
        {
            if (checkBox1 != null && checkBox1.Checked)
            {
                if (label1 != null)
                {
                    label1.Text = @"运行中......";
                    label1.Refresh();
                }
                textBox2?.Clear();
                _writeToTextBox = WriteTextBox1;
                WriteText(_writeToTextBox);
                if (label1 != null)
                    label1.Text = @"任务1完成";
            }
        }

        private void Execute2()
        {
            if (checkBox2 != null && checkBox2.Checked)
            {
                if (label2 != null)
                {
                    label2.Text = @"运行中......";
                    label2.Refresh();
                }
                textBox3?.Clear();
                _writeToTextBox = WriteTextBox2;
                WriteText(_writeToTextBox);
                if (label2 != null)
                    label2.Text = @"任务2完成";
            }
        }

        private void WriteText(WriteToTextBox writeMethod)
        {
            var textBox = this.textBox1;
            if (textBox != null)
            {
                string strData = textBox.Text;
                writeMethod?.Invoke(strData);
            }
        }

        private void WriteTextBox1(string strTxt)
        {
            var textBox = this.textBox2;
            if (textBox != null)
                textBox.Text = strTxt;
        }

        private void WriteTextBox2(string strTxt)
        {
            var textBox = this.textBox3;
            if (textBox != null)
                textBox.Text = strTxt;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //设置文本框获取焦点
            this.ActiveControl = this.textBox1;
            //允许跨线程调用
            CheckForIllegalCrossThreadCalls = false;
        }
    }
}
