using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Client
{
    public partial class Window : Form
    {
        public Window()
        {
            InitializeComponent();
        }

        private delegate void SetTextCallBack(string strValue);
        private SetTextCallBack setCallBack;

        private delegate void ReceiveMsgCallBack(string strMsg);
        private ReceiveMsgCallBack receiveCallBack;

        private delegate void SetCmbCallBack(string strItem);
        private SetCmbCallBack setCmbCallBack;

        //创建连接的Socket
        Socket socketSend;
        //创建接收客户端发送消息的线程
        Thread threadReceive;

        Dictionary<string, Socket> Servers = new Dictionary<string, Socket>();

        Dictionary<string, Thread> ReceiveThreads = new Dictionary<string, Thread>();
       
        ///连接
        private void button1_Click(object sender, EventArgs e)
        {
            string strIp = textBox1.Text.Trim() + @":" + textBox2.Text.Trim();
            if (Servers.ContainsKey(strIp))
            {
                textBox3.AppendText(@"已与 " + strIp + @" 连接" + Environment.NewLine);
                return;
            }
            try
            {
                socketSend= new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ip = IPAddress.Parse(textBox1.Text.Trim());
                socketSend.Connect(ip, Convert.ToInt32(textBox2.Text.Trim()));
                Servers.Add(strIp, socketSend);
                socketSend = null;

                //实例化回调
                setCallBack = SetValue;
                receiveCallBack = SetValue;
                setCmbCallBack = AddCmbItem;
                comboBox1.Invoke(setCmbCallBack, strIp);
                textBox3.Invoke(setCallBack, @"与 " + strIp + @" 连接成功");
                button2.Enabled = true;

                //开启一个新的线程不停的接收服务器发送消息的线程
                threadReceive = new Thread(Receive)
                {
                    IsBackground = true
                };
                //设置为后台线程
                ReceiveThreads.Add(strIp, threadReceive);
                threadReceive.Start(strIp);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(@"连接服务端出错:" + ex, @"出错啦", MessageBoxButtons.OK, MessageBoxIcon.Error);
                socketSend?.Close();
                textBox3.AppendText(@"与 " + strIp + @" 连接失败" + Environment.NewLine);
            }
        }

        ///接收服务器发送的消息
        private void Receive(object obj)
        {
            try
            {
                var socket = Servers[obj as String];
                while (true)
                {
                    byte[] buffer = new byte[2048];
                    //实际接收到的字节数
                    int r = socket.Receive(buffer);
                    if (r == 0)
                    {
                        break;
                    }
                    //判断发送的数据的类型
                    if (buffer[0] == 0) //表示发送的是文字消息
                    {
                        string str = Encoding.UTF8.GetString(buffer, 1, r - 1);
                        textBox3.Invoke(receiveCallBack, @"接收远程服务器:" + socket.RemoteEndPoint + @"发送的消息:" + str);
                    }
                    //表示发送的是文件
                    if (buffer[0] == 1)
                    {
                        SaveFileDialog sfd = new SaveFileDialog
                        {
                            InitialDirectory = @"",
                            Title = @"请选择要保存的文件",
                            Filter = @"所有文件|*.*"
                        };
                        sfd.ShowDialog(this);

                        string strPath = sfd.FileName;
                        using (FileStream fsWrite = new FileStream(strPath, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            fsWrite.Write(buffer, 1, r - 1);
                        }

                        MessageBox.Show(@"保存文件成功", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                textBox3.Invoke(setCallBack, @"已与 " + obj + @" 断开连接");
            }
            catch (SocketException)
            {
                textBox3.Invoke(setCallBack, @"与 " + obj + @" 连接丢失");
                Disconnect(obj as String);
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"接收服务端发送的消息出错:" + ex, @"出错啦", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddCmbItem(string strItem)
        {
            comboBox1.Items.Add(strItem);
            comboBox1.SelectedIndex = 0;
        }

        private void SetValue(string strValue)
        {
            textBox3.AppendText(strValue + Environment.NewLine);
        }

        ///客户端给服务器发送消息
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                string strMsg = textBox4.Text.Trim();
                var buffer = Encoding.UTF8.GetBytes(strMsg);
                Servers[comboBox1.SelectedItem.ToString()].Send(buffer);
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"发送消息出错:" + ex.Message, @"出错啦", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Disconnect(string strIP)
        {
            //终止线程
            ReceiveThreads[strIP]?.Abort();
            ReceiveThreads.Remove(strIP);
            //关闭socket
            Servers[strIP]?.Close();
            Servers.Remove(strIP);
            
            comboBox1.Items.Remove(strIP);
            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
            }
            else
            {
                button2.Enabled = false;
            }
        }
        ///断开连接
        private void button2_Click(object sender, EventArgs e)
        {
            Disconnect(comboBox1.SelectedItem.ToString());
        }

        private void Window_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
        }
    }
}
