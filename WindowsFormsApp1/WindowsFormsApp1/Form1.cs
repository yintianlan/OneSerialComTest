using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using INIFILE;
using System.Text.RegularExpressions;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        SerialPort sp1 = new SerialPort();

        public Form1()
        {
            InitializeComponent();
        }

        //加载
        private void Form1_Load(object sender, EventArgs e)
        {
            INIFILE.Profile.LoadProfile();//加载所有

            //预置波特率
            switch(Profile.G_BAUDRATE)
            {
                case "300":
                    cbBaudRate.SelectedIndex = 0;
                    break;
                case "600":
                    cbBaudRate.SelectedIndex = 1;
                    break;
                case "1200":
                    cbBaudRate.SelectedIndex = 2;
                    break;
                case "2400":
                    cbBaudRate.SelectedIndex = 3;
                    break;
                case "4800":
                    cbBaudRate.SelectedIndex = 4;
                    break;
                case "9600":
                    cbBaudRate.SelectedIndex = 5;
                    break;
                case "19200":
                    cbBaudRate.SelectedIndex = 6;
                    break;
                case "38400":
                    cbBaudRate.SelectedIndex = 7;
                    break;
                case "115200":
                    cbBaudRate.SelectedIndex = 8;
                    break;

                default:
                    {
                        MessageBox.Show("波特率预置参数错误");
                        return;
                    }
            }

            //预置数据位
            switch (Profile.G_DATABITS)
            {
                case "5":
                    cbDataBits.SelectedIndex = 0;
                    break;
                case "6":
                    cbDataBits.SelectedIndex = 1;
                    break;
                case "7":
                    cbDataBits.SelectedIndex = 2;
                    break;
                case "8":
                    cbDataBits.SelectedIndex = 3;
                    break;

                default:
                    {
                        MessageBox.Show("数据位预置参数错误");
                        return;
                    }
            }

            //预置停止位
            switch (Profile.G_STOP)
            {
                case "1":
                    cbStop.SelectedIndex = 0;
                    break;
                case "1.5":
                    cbStop.SelectedIndex = 1;
                    break;
                case "2":
                    cbStop.SelectedIndex = 2;
                    break;

                default:
                    {
                        MessageBox.Show("停止位预置参数错误");
                        return;
                    }
            }

            //预置校验位
            switch (Profile.G_PARITY)
            {
                case "NONE":
                    cbParity.SelectedIndex = 0;
                    break;
                case "ODD":
                    cbParity.SelectedIndex = 1;
                    break;
                case "EVEN":
                    cbParity.SelectedIndex = 2;
                    break;

                default:
                    {
                        MessageBox.Show("校验位预置参数错误");
                        return;
                    }
            }

            //检查是否含有串口
            string[] str = SerialPort.GetPortNames();
            if(str == null)
            {
                MessageBox.Show("本机没有串口！", "Error");
                return;
            }

            //添加串口项目
            foreach(string s in System.IO.Ports.SerialPort.GetPortNames())
            {
                //获取有多少个COM口
                cbSerial.Items.Add(s);
            }

            //串口设置默认选择项
            cbSerial.SelectedIndex = 0;
            sp1.BaudRate = 9600;

            //这个类中我们不检查跨线程的调用是否合法(因为.net 2.0以后加强了安全机制,，不允许在winform中直接跨线程访问控件的属性)
            Control.CheckForIllegalCrossThreadCalls = false;
            sp1.DataReceived += new SerialDataReceivedEventHandler(sp1_DataReceived);

            rbSend16.Checked = true;//单选按钮默认是选中的
            rbRec16.Checked = true;

            //准备就绪
            sp1.DtrEnable = true;
            sp1.RtsEnable = true;
            //设置数据读取超时为1s
            sp1.ReadTimeout = 1000;

            sp1.Close();

        }

        void sp1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if(sp1.IsOpen)//判断是否打开串口
            {
                //输出当前时间
                DateTime dt = DateTime.Now;
                txtReceive.Text += dt.GetDateTimeFormats('f')[0].ToString() + "\r\n";
                txtReceive.SelectAll();
                txtReceive.SelectionColor = Color.Blue;     //改变字体的颜色

                byte[] byetRead = new byte[sp1.BytesToRead];//BytesToRead:sp1接收的字符个数
                if (rbSendStr.Checked)//‘发送字符串’单选按钮
                {
                    txtReceive.Text += sp1.ReadLine() + "\r\n"; //注意：回车换行必须这样写，单独使用"\r"和"\n"都不会有效果
                    sp1.DiscardInBuffer();                      //清空SerialPort控件的Buffer
                }
                else//'发送16进制按钮'
                {
                    try
                    {
                        Byte[] receivedData = new Byte[sp1.BytesToRead];//创建接收字节数组
                        sp1.Read(receivedData, 0, receivedData.Length); //读取数据
                        sp1.DiscardInBuffer();                          //清空SerialPort控件的Buffer

                        string strRev = null;
                        for(int i=0; i< receivedData.Length; i++)       //显示窗体
                        {
                            strRev += receivedData[i].ToString("X2");   //16进制显示
                        }
                        txtReceive.Text += strRev + "\r\n";
                    }
                    catch(System.Exception ex)
                    {
                        MessageBox.Show(ex.Message, "错误提示");
                        txtSend.Text = "";
                    }
                }
            }
            else
            {
                MessageBox.Show("请打开某个串口", "错误提示");
            }
        }

        //清除按钮
        private void btnClear_Click(object sender, EventArgs e)
        {
            txtReceive.Text = "";//清空文本
        }

        //退出按钮
        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //关闭时事件
        private void Form1_FormClosing(object sender, FormClosedEventArgs e)
        {
            INIFILE.Profile.SaveProfile();
            sp1.Close();
        }

        //发送按钮
        private void btnSend_Click(object sender, EventArgs e)
        {
            if(cbTimeSend.Checked)//选中‘定时发送’
            {
                tmSend.Enabled = true;
            }
            else
            {
                tmSend.Enabled = false;
            }

            if (!sp1.IsOpen)//如果没有打开串口
            {
                MessageBox.Show("请先打开串口！", "Error");
                return;
            }

            String strSend = txtSend.Text;
            if(rbSend16.Checked == true)//‘HEX发送’按钮
            {
                //处理数字转换
                string sendBuf = strSend;
                string sendnoNull = sendBuf.Trim();
                string sendNOComma = sendnoNull.Replace(',', ' ');//去掉英文逗号
                string sendNOComma1 = sendNOComma.Replace(',', ' ');//去掉中文逗号
                string strSendNoComma2 = sendNOComma1.Replace("0x", "");//去掉0x
                strSendNoComma2.Replace("0x", "");//去掉0x
                string[] strArray = strSendNoComma2.Split(' ');

                int byteBufferLength = strArray.Length;
                for(int i=0; i< strArray.Length;i++)
                {
                    if(strArray[i] == "")
                    {
                        byteBufferLength--;
                    }
                }

                byte[] byteBuffer = new byte[byteBufferLength];
                int ii = 0;
                for (int i = 0; i < strArray.Length; i++)//对获取的字符做相加运算
                {
                    Byte[] byteOfStr = Encoding.Default.GetBytes(strArray[i]);
                    int decNum = 0;
                    if(strArray[i] == "")
                    {
                        continue;
                    }
                    else
                    {
                        decNum = Convert.ToInt32(strArray[i], 16);//strArray[i] == 12时，实际值temp == 18
                    }

                    try//防止输错，使其只能输入一个字节的字符
                    {
                        byteBuffer[ii] = Convert.ToByte(decNum);
                    }
                    catch(System.Exception ex)
                    {
                        MessageBox.Show("字节越界，请逐个字符输入" + ex.Message, "Error");
                        tmSend.Enabled = false;
                        return;
                    }

                    ii++;
                }
                sp1.Write(byteBuffer, 0, byteBuffer.Length);
            }
            else//以字符串形式发送时
            {
                sp1.WriteLine(txtSend.Text);//写入数据
            }
        }

        //开关按钮
        private void btnSwitck_Click(object sender, EventArgs e)
        {
            //serialPort1.IsOpen
            if (!sp1.IsOpen)
            {
                try
                {
                    //设置串口号
                    string serialName = cbSerial.SelectedItem.ToString();
                    sp1.PortName = serialName;

                    //设置各“串口设置”
                    string strBaudRate = cbBaudRate.Text;
                    string strDateBits = cbDataBits.Text;
                    string strStopBits = cbStop.Text;
                    Int32 iBaudRate = Convert.ToInt32(strBaudRate);
                    Int32 iDateBits = Convert.ToInt32(strDateBits);

                    sp1.BaudRate = iBaudRate;   //波特率
                    sp1.DataBits = iDateBits;   //数据位
                    switch(cbStop.Text)         //停止位
                    {
                        case "1":
                            sp1.StopBits = StopBits.One;
                            break;
                        case "1.5":
                            sp1.StopBits = StopBits.OnePointFive;
                            break;
                        case "2":
                            sp1.StopBits = StopBits.Two;
                            break;
                        default:
                            MessageBox.Show("Error:参数不正确！", "Error");
                            break;
                    }
                    switch(cbParity.Text)       //校验位
                    {
                        case "无":
                            sp1.Parity = Parity.None;
                            break;
                        case "奇校验":
                            sp1.Parity = Parity.Odd;
                            break;
                        case "偶校验":
                            sp1.Parity = Parity.Even;
                            break;
                        default:
                            MessageBox.Show("Error:参数不正确！", "Error");
                            break;
                    }

                    if(sp1.IsOpen == true)//如果打开状态，则先关闭一下
                    {
                        sp1.Close();
                    }

                    //状态栏设置
                    tsSpNum.Text = "串口号：" + sp1.PortName + "|";
                    tsBaudRate.Text = "波特率：" + sp1.BaudRate + "|";
                    tsDataBits.Text = "数据位：" + sp1.DataBits + "|";
                    tsStopBits.Text = "停止位：" + sp1.StopBits + "|";
                    tsParity.Text = "校验位：" + sp1.Parity + "|";

                    //设置必要控件不可用
                    cbSerial.Enabled = false;
                    cbBaudRate.Enabled = false;
                    cbDataBits.Enabled = false;
                    cbStop.Enabled = false;
                    cbParity.Enabled = false;

                    sp1.Open(); //打开串口
                    btnSwitck.Text = "关闭串口";
                }
                catch(System.Exception ex)
                {
                    MessageBox.Show("Error:" + ex.Message, "Error");
                    tmSend.Enabled = false;
                    return;
                }
            }
            else
            {
                //状态栏设置
                tsSpNum.Text = "串口号：未指定|";
                tsBaudRate.Text = "波特率：未指定|";
                tsDataBits.Text = "数据位：未指定|";
                tsStopBits.Text = "停止位：未指定|";
                tsParity.Text = "校验位：未指定|";

                //恢复控件功能
                //设置必要控件不可用
                cbSerial.Enabled = true;
                cbBaudRate.Enabled = true;
                cbDataBits.Enabled = true;
                cbStop.Enabled = true;
                cbParity.Enabled = true;

                sp1.Close();            //关闭串口
                btnSwitck.Text = "打开串口";
                tmSend.Enabled = false; //关闭计时器
            }
        }

        //保存按钮
        private void btnSave_Click(object sender, EventArgs e)
        {
            //设置各“串口设置”
            string strBaudRate = cbBaudRate.Text;
            string strDateBits = cbDataBits.Text;
            string strStopBits = cbStop.Text;
            Int32 iBaudRate = Convert.ToInt32(strBaudRate);
            Int32 iDateBits = Convert.ToInt32(strDateBits);

            Profile.G_BAUDRATE = iBaudRate + "";//波特率
            Profile.G_DATABITS = iDateBits + "";//数据位
            switch(cbStop.Text) //停止位
            {
                case "1":
                    Profile.G_STOP = "1";
                    break;
                case "1.5":
                    Profile.G_STOP = "1.5";
                    break;
                case "2":
                    Profile.G_STOP = "2";
                    break;
                default:
                    MessageBox.Show("Error:参数不正确！", "Error");
                    break;
            }
            switch(cbParity.Text)//校验位
            {
                case "无":
                    Profile.G_PARITY = "NONE";
                    break;
                case "奇校验":
                    Profile.G_PARITY = "ODD";
                    break;
                case "偶校验":
                    Profile.G_PARITY = "EVEN";
                    break;
                default:
                    MessageBox.Show("Error:参数不正确！", "Error");
                    break;
            }

            //保存设置
            Profile.SaveProfile();
        }

        //定时器
        private void tmSend_Tick(object sender, EventArgs e)
        {
            //转换时间间隔
            string strSecond = txtSecond.Text;
            try
            {
                //Interval以微妙为单位
                int isecond = int.Parse(strSecond) * 1000;
                tmSend.Interval = isecond;
                if(tmSend.Enabled == true)
                {
                    btnSend.PerformClick();
                }
            }
            catch(System.Exception ex)
            {
                tmSend.Enabled = false;
                MessageBox.Show("错误的定时输入！" + ex.Message, "Error");
            }
        }

        private void txtSend_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(rbSend16.Checked == true)
            {
                //正则匹配
                string patten = "[0-9a-fA-F]|\b|0x|0X| ";//“\b”:退格键
                Regex r = new Regex(patten);
                Match m = r.Match(e.KeyChar.ToString());

                if(m.Success)
                {
                    e.Handled = false;
                }
                else
                {
                    e.Handled = true;
                }
            }
            else
            {
                e.Handled = false;
            }
        }

        private void txtSecond_KeyPress(object sender, KeyPressEventArgs e)
        {
            string patten = "[0-9]|\b";//“\b”:退格键
            Regex r = new Regex(patten);
            Match m = r.Match(e.KeyChar.ToString());

            if (m.Success)
            {
                e.Handled = false;//没操作“过”，系统会处理事件
            }
            else
            {
                e.Handled = true;
            }
        }
    }
}
