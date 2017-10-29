using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Server
{
    public partial class Server : Form
    {
        public Server()
        {
            InitializeComponent();
        }
        //定义回调:解决跨线程访问问题
        private delegate void SetTextValueCallBack(string strValue);
        //定义接收客户端发送消息的回调
        private delegate void ReceiveMsgCallBack(string strReceive);
        //声明回调
        private SetTextValueCallBack setCallBack;
        private ReceiveMsgCallBack receiveCallBack;
        //定义、声明回调：给ComboBox控件添加元素
        private delegate void SetCmbCallBack(string strItem);
        private SetCmbCallBack setCmbCallBack;
        //定义、声明发送文件的回调
        private delegate void SendFileCallBack(byte[] bf);
        private SendFileCallBack sendCallBack;

        //通信Socket
        Socket socketSend;
        //监听SOCKET
        Socket socketListen;
        //将远程连接的客户端的IP地址和Socket存入集合中
        Dictionary<string, Socket> Clients = new Dictionary<string, Socket>();

        //创建监听连接的线程
        Thread AcceptSocketThread;
        //接收客户端发送消息的线程
        Thread threadReceive;

        private const int MAXClients = 10;

        ///开始监听
        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            //创建一个负责监听IP地址和端口号的Socket
            socketListen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //获取ip地址
            IPAddress ip = IPAddress.Parse(textBox1.Text.Trim());
            //创建端口号
            IPEndPoint point = new IPEndPoint(ip, Convert.ToInt32(textBox2.Text.Trim()));
            //绑定IP地址和端口号
            try
            {
                socketListen.Bind(point);
            }
            catch
            {
                textBox4.AppendText(@"监听失败" + Environment.NewLine);
                button1.Enabled = true;
                return;
            }
            textBox4.AppendText(@"监听成功" + Environment.NewLine);
            button2.Enabled = true;
            //开始监听:设置最大可以同时连接多少个请求
            socketListen.Listen(MAXClients);

            //实例化回调
            setCallBack = SetTextValue;
            receiveCallBack = ReceiveMsg;
            setCmbCallBack = AddCmbItem;
            sendCallBack = SendFile;

            //创建线程
            AcceptSocketThread = new Thread(StartListen)
            {
                IsBackground = true
            };
            AcceptSocketThread.Start(socketListen);
        }

        ///等待客户端的连接，并且创建与之通信用的Socket
        private void StartListen(object obj)
        {
            while (true)
            {
                socketSend = (obj as Socket).Accept();
                //获取远程主机的ip地址和端口号
                string strIp = socketSend.RemoteEndPoint.ToString();
                Clients.Add(strIp, socketSend);
                comboBox1.Invoke(setCmbCallBack, strIp);
                textBox4.Invoke(setCallBack, @"远程主机: " + socketSend.RemoteEndPoint + @"连接成功");
                //创建接收客户端消息的线程
                threadReceive = new Thread(Receive)
                {
                    IsBackground = true
                };
                threadReceive.Start(socketSend);
            }
        }

        ///服务器端不停的接收客户端发送的消息
        private void Receive(object obj)
        {
            try
            {
                Socket _socketSend = obj as Socket;
                while (true)
                {
                    //客户端连接成功后，服务器接收客户端发送的消息
                    byte[] buffer = new byte[2048];
                    //实际接收到的有效字节数
                    int count = _socketSend.Receive(buffer);
                    if (count == 0) //count 表示客户端关闭，要退出循环
                    {
                        break;
                    }
                    string str = Encoding.UTF8.GetString(buffer, 0, count);
                    string strReceiveMsg =
                        @"接收：" + _socketSend.RemoteEndPoint + @" 发送的消息: " + Environment.NewLine + str;
                    textBox4.Invoke(receiveCallBack, strReceiveMsg);
                }
            }
            /*catch (SocketException)
            {
                
            }*/
            catch (Exception ex)
            {
                MessageBox.Show(@"连接服务端出错:" + ex, @"出错啦", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        ///回调委托需要执行的方法
        private void SetTextValue(string strValue)
        {
            textBox4.AppendText(strValue + Environment.NewLine);
        }


        private void ReceiveMsg(string strMsg)
        {
            textBox4.AppendText(strMsg + Environment.NewLine);
        }

        private void AddCmbItem(string strItem)
        {
            comboBox1.Items.Add(strItem);
            comboBox1.SelectedIndex = 0;
        }

        ///服务器给客户端发送消息
        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                string strMsg = textBox5.Text.Trim();
                byte[] buffer = Encoding.UTF8.GetBytes(strMsg);
                List<byte> list = new List<byte> {0};
                list.AddRange(buffer);
                //将泛型集合转换为数组
                byte[] newBuffer = list.ToArray();
                //获得用户选择的IP地址
                string ip = comboBox1.SelectedItem.ToString();
                Clients[ip].Send(newBuffer);
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"给客户端发送消息出错:" + ex.Message);
            }
        }

        ///选择要发送的文件
        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog dia = new OpenFileDialog
            {
                InitialDirectory = @"",
                Title = @"请选择要发送的文件",
                Filter = @"所有文件|*.*"
            };
            dia.ShowDialog();
            textBox3.Text = dia.FileName;
        }

        ///发送文件
        private void button4_Click(object sender, EventArgs e)
        {
            List<byte> list = new List<byte>();
            //获取要发送的文件的路径
            string strPath = textBox3.Text.Trim();
            using (FileStream sw = new FileStream(strPath, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[2048];
                sw.Read(buffer, 0, buffer.Length);
                //移除末尾的 \0
                string t=Encoding.UTF8.GetString(buffer).TrimEnd('\0');
                buffer = Encoding.UTF8.GetBytes(t);

                list.Add(1);
                list.AddRange(buffer);
                
                byte[] newBuffer = list.ToArray();
                button4.Invoke(sendCallBack, newBuffer);
            }

        }

        private void SendFile(byte[] sendBuffer)
        {

            try
            {
                Clients[comboBox1.SelectedItem.ToString()].Send(sendBuffer, SocketFlags.None);
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"发送文件出错:" + ex.Message);
            }
        }

        ///停止监听
        private void button2_Click(object sender, EventArgs e)
        {
            socketListen?.Close();
            socketSend?.Close();
            AcceptSocketThread?.Abort();
            threadReceive?.Abort();
            textBox4.AppendText(@"停止监听" + Environment.NewLine);
            button1.Enabled = true;
            button2.Enabled = false;
        }
    }
}
