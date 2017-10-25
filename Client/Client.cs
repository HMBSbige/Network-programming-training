using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
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
        //定义回调
        private delegate void SetTextCallBack(string strValue);
        //声明
        private SetTextCallBack setCallBack;

        //定义接收服务端发送消息的回调
        private delegate void ReceiveMsgCallBack(string strMsg);
        //声明
        private ReceiveMsgCallBack receiveCallBack;

        //创建连接的Socket
        Socket socketSend;
        //创建接收客户端发送消息的线程
        Thread threadReceive;

        ///连接
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ip = IPAddress.Parse(textBox1.Text.Trim());
                socketSend.Connect(ip, Convert.ToInt32(textBox2.Text.Trim()));
                //实例化回调
                setCallBack = SetValue;
                receiveCallBack = SetValue;
                textBox3.Invoke(setCallBack, @"连接成功");

                //开启一个新的线程不停的接收服务器发送消息的线程
                threadReceive = new Thread(Receive)
                {
                    IsBackground = true
                };
                //设置为后台线程
                threadReceive.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"连接服务端出错:" + ex);
            }
        }

        ///接口服务器发送的消息
        private void Receive()
        {
            try
            {
                while (true)
                {
                    byte[] buffer = new byte[2048];
                    //实际接收到的字节数
                    int r = socketSend.Receive(buffer);
                    if (r == 0)
                    {
                        break;
                    }
                    //判断发送的数据的类型
                    if (buffer[0] == 0)//表示发送的是文字消息
                    {
                        string str = Encoding.UTF8.GetString(buffer, 1, r - 1);
                        textBox3.Invoke(receiveCallBack, @"接收远程服务器:" + socketSend.RemoteEndPoint + @"发送的消息:" + str);
                    }
                    //表示发送的是文件
                    if (buffer[0] == 1)
                    {
                        SaveFileDialog sfd = new SaveFileDialog();
                        sfd.InitialDirectory = @"";
                        sfd.Title = @"请选择要保存的文件";
                        sfd.Filter = @"所有文件|*.*";
                        sfd.ShowDialog(this);

                        string strPath = sfd.FileName;
                        using (FileStream fsWrite = new FileStream(strPath, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            fsWrite.Write(buffer, 1, r - 1);
                        }

                        MessageBox.Show(@"保存文件成功");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"接收服务端发送的消息出错:" + ex);
            }
        }


        private void SetValue(string strValue)
        {
            textBox3.AppendText(strValue + "\r\n");
        }

        ///客户端给服务器发送消息
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                string strMsg = textBox4.Text.Trim();
                byte[] buffer = new byte[2048];
                buffer = Encoding.UTF8.GetBytes(strMsg);
                int receive = socketSend.Send(buffer);
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"发送消息出错:" + ex.Message);
            }
        }

        ///断开连接
        private void button2_Click(object sender, EventArgs e)
        {
            //关闭socket
            socketSend.Close();
            //终止线程
            threadReceive.Abort();
        }

        private void Window_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
        }
    }
}
