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

        //定义并声明回调
        private delegate void WriteTxtJob1CallBack(string strValue);
        WriteTxtJob1CallBack _writeTxtJobOneCallBack;
        private delegate void WriteTxtJob2CallBack(string strValue);
        WriteTxtJob2CallBack _writeTxtJobTwoCallBack;
        private delegate void Label1CallBack(string strValue);
        Label1CallBack _label1CallBack;
        private delegate void Label2CallBack(string strValue);
        Label2CallBack _label2CallBack;

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
                _writeToTextBox = WriteTextBox1;
                WriteText(_writeToTextBox);              
                label1?.Invoke(_label1CallBack, @"任务1完成");
            }
        }

        private void Execute2()
        {
            if (checkBox2 != null && checkBox2.Checked)
            {
                _writeToTextBox = WriteTextBox2;
                WriteText(_writeToTextBox);
                label2?.Invoke(_label2CallBack, @"任务2完成");
            }
        }

        private void WriteText(WriteToTextBox writeMethod)
        {
            if (this.textBox1 != null)
            {
                writeMethod(textBox1.Text);
            }
        }

        private void WriteTextBox1(string strTxt)
        {
            textBox2?.Invoke(_writeTxtJobOneCallBack, strTxt);
        }

        private void WriteTextBox2(string strTxt)
        {
            textBox3?.Invoke(_writeTxtJobTwoCallBack, strTxt);
        }

        private void Write1(string str)
        {
            var textBox = this.textBox2;
            if (textBox != null)
                textBox.Text = str;
        }

        private void Write2(string str)
        {
            var textBox = this.textBox3;
            if (textBox != null)
                textBox.Text = str;
        }

        private void Showlabel1(string str)
        {
            if (label1 != null)
                label1.Text = str;
        }

        private void Showlabel2(string str)
        {
            if (label2 != null)
                label2.Text = str;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //设置文本框获取焦点
            this.ActiveControl = this.textBox1;
            //允许跨线程调用
            //CheckForIllegalCrossThreadCalls = false;
            //实例化回调
            _writeTxtJobOneCallBack = Write1;
            _writeTxtJobTwoCallBack = Write2;
            _label1CallBack = Showlabel1;
            _label2CallBack = Showlabel2;
        }
    }
}
