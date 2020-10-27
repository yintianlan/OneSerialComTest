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
        #region 日记记录，在richtextbox中显示不同颜色文字的方法
        public delegate void LogAppendDelegate(Color color, string text);

        //追加显示文本
        public void LogAppend(Color color, string msg)
        {
            txtReceive.SelectionColor = color;
            txtReceive.AppendText(msg);
            txtReceive.AppendText("\n");
        }

        //显示错误信息
        public void LogError(string msg)
        {
            LogAppendDelegate lab = new LogAppendDelegate(LogAppend);
            txtReceive.Invoke(lab, Color.Red, DateTime.Now.ToString("HH:mm:ss ") + msg);
        }

        //显示警告信息
        public void LogWarning(string msg)
        {
            LogAppendDelegate lab = new LogAppendDelegate(LogAppend);
            txtReceive.Invoke(lab, Color.Violet, DateTime.Now.ToString("HH:mm:ss ") + msg);
        }

        //显示信息
        public void LogMessage(string msg)
        {
            LogAppendDelegate lab = new LogAppendDelegate(LogAppend);
            txtReceive.Invoke(lab, Color.Black, DateTime.Now.ToString("HH:mm:ss ") + msg);
        }

        //发送信息的设置
        public void SendMessage(string msg)
        {
            LogAppendDelegate lab = new LogAppendDelegate(LogAppend);
            txtReceive.Invoke(lab, Color.Green, DateTime.Now.ToString("HH:mm:ss ") + msg);
        }

        //接收信息的设置
        public void RcvMessage(string msg)
        {
            LogAppendDelegate lab = new LogAppendDelegate(LogAppend);
            txtReceive.Invoke(lab, Color.Blue, DateTime.Now.ToString("HH:mm:ss ") + msg);
        }
        #endregion

        #region HEX16进制、Byte字节数组和字符串之间的转换
        //byte字节数组转16进制
        private string ByteToHex(byte[] comByte)
        {
            StringBuilder builder = new StringBuilder(comByte.Length * 3);

            foreach (byte data in comByte)
            {
                builder.Append(Convert.ToString(data, 16).PadLeft(2, '0').PadRight(3, ' '));
            }
            return builder.ToString().ToUpper();
        }

        //16进制转字节数组
        private byte[] HexToByte(string msg)
        {
            msg = msg.Replace(" ", "");
            msg = msg.Replace("0x", "");
            msg = msg.Replace("0X", "");

            byte[] comBuffer = new byte[msg.Length / 2];

            for(int i=0; i<msg.Length; i+=2)
            {
                comBuffer[i / 2] = (byte)Convert.ToByte(msg.Substring(i, 2), 16);
            }

            return comBuffer;
        }

        //字符串转字节数组
        private byte[] StringToByte(string msgtxt)
        {
            return System.Text.Encoding.UTF8.GetBytes(msgtxt);
        }

        //字节型数组转字符串
        private string ByteToString(byte[] buffer)
        {
            return System.Text.Encoding.UTF8.GetString(buffer);
        }
        #endregion

        #region SettingControls 设置控件可用状态的方法
        private void SettingControls(int canUse)
        {
            //canUse 参数1：可用 0：不可用
            if (canUse == 1)
            {
                cbSerial.Enabled = true;
                cbBaudRate.Enabled = true;
                cbDataBits.Enabled = true;
                cbStop.Enabled = true;
                cbParity.Enabled = true;
            }
            else if(canUse == 0)
            {
                //设置必要控件不可用
                cbSerial.Enabled = false;
                cbBaudRate.Enabled = false;
                cbDataBits.Enabled = false;
                cbStop.Enabled = false;
                cbParity.Enabled = false;
            }
        }
        #endregion

        SerialPort sp1 = new SerialPort();

        public Form1()
        {
            InitializeComponent();
        }

        #region Form1_Load 窗口加载
        private void Form1_Load(object sender, EventArgs e)//加载
        {
            INIFILE.Profile.LoadProfile();//加载所有

            //预置波特率
            switch (Profile.G_BAUDRATE)
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
                        MessageBox.Show("波特率预置参数错误", "异常提示信息",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                        MessageBox.Show("数据位预置参数错误", "异常提示信息",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                        MessageBox.Show("停止位预置参数错误", "异常提示信息",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                        MessageBox.Show("校验位预置参数错误", "异常提示信息",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
            }

            //检查是否含有串口
            string[] str = SerialPort.GetPortNames();
            if (str == null)
            {
                MessageBox.Show("本机没有串口！", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //添加串口项目
            foreach (string s in System.IO.Ports.SerialPort.GetPortNames())
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

            //关闭串口
            sp1.Close();
        }
        #endregion

        #region sp1_DataReceived 处理接收数据
        void sp1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (sp1.IsOpen)//判断是否打开串口
            {
                byte[] byetRead = new byte[sp1.BytesToRead];//BytesToRead:sp1接收的字符个数
                sp1.Read(byetRead, 0, byetRead.Length);
                sp1.DiscardInBuffer();

                if (rbRecStr.Checked)//‘接收字符串’单选按钮
                {
                    string receiveStr = ByteToString(byetRead);
                    RcvMessage(receiveStr);
                 }
                else//'接收16进制按钮'
                {
                    try
                    {
                        string strRev = ByteToHex(byetRead);
                        RcvMessage(strRev);

                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show(ex.Message, "错误提示",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtSend.Text = "";
                    }
                }
            }
            else
            {
                MessageBox.Show("请打开某个串口", "信息提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion

        #region btnClear_Click 清空按钮
        private void btnClear_Click(object sender, EventArgs e)
        {
            txtReceive.Text = "";//清空文本
        }
        #endregion

        #region btnExit_Click 退出按钮
        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion

        #region Form1_FormClosing 关闭窗口时事件
        private void Form1_FormClosing(object sender, FormClosedEventArgs e)
        {
            //保存配置到INI文件
            INIFILE.Profile.SaveProfile();
            sp1.Close();
        }
        #endregion

        #region btnSend_Click 发送按钮:处理数字转换
        private void btnSend_Click(object sender, EventArgs e)
        {
            if (cbTimeSend.Checked)//选中‘定时发送’
            {
                tmSend.Enabled = true;
            }
            else
            {
                tmSend.Enabled = false;
            }

            if (!sp1.IsOpen)//如果没有打开串口
            {
                MessageBox.Show("请先打开串口！", "提示信息",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            String strSend = txtSend.Text;
            if (rbSend16.Checked == true)//‘HEX发送’按钮
            {
                //如果以16进制形式发送，将16进制数转换为byte数组进行发送
                try
                {
                    byte[] byteBuffer = HexToByte(strSend);
                    sp1.Write(byteBuffer, 0, byteBuffer.Length);
                    SendMessage(ByteToHex(byteBuffer));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("16进制数据的格式或长度不正确，请检查格式后重新发送！" + ex.Message, "Error");
                }
            }
            else if (rbSendStr.Checked == true)//‘字符发送’按钮
            {
                //处理数字转换
                //如果以字符串形式发送，将字符串转换为byte数组进行发送
                byte[] byteBuffer = StringToByte(strSend);
                sp1.Write(byteBuffer, 0, byteBuffer.Length);
                SendMessage(strSend);
            }
            else
            {
                MessageBox.Show("请选择数据发送的格式！", "提示信息",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion

        #region btnSwitck_Click 开关串口按钮
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
                    switch (cbStop.Text)         //停止位
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
                    switch (cbParity.Text)       //校验位
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

                    if (sp1.IsOpen == true)//如果打开状态，则先关闭一下
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
                    SettingControls(0);

                    //打开串口
                    sp1.Open();
                    //将按钮内容设置为"关闭串口"
                    btnSwitck.Text = "关闭串口";
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Error:" + ex.Message, "Error");
                    tmSend.Enabled = false;

                    //设置必要控件为可用状态
                    SettingControls(1);
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
                SettingControls(1);

                //关闭串口
                sp1.Close();
                //将按钮内容设置为"打开串口"
                btnSwitck.Text = "打开串口";
                //关闭计时器
                tmSend.Enabled = false;
            }
        }
        #endregion

        #region btnSave_Click 保存设置按钮
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
            switch (cbStop.Text) //停止位
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
                    MessageBox.Show("Error:停止位参数不正确！", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
            }
            switch (cbParity.Text)//校验位
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
                    MessageBox.Show("Error:校验位参数不正确！", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
            }

            //保存设置
            Profile.SaveProfile();

            //保存成功提示
            MessageBox.Show("设置保存成功！", "信息提示",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        #region tmSend_Tick 定时器
        private void tmSend_Tick(object sender, EventArgs e)
        {
            //转换时间间隔
            string strSecond = txtSecond.Text;
            try
            {
                //Interval以微妙为单位
                int isecond = int.Parse(strSecond) * 1000;
                tmSend.Interval = isecond;
                if (tmSend.Enabled == true)
                {
                    //如果timeSend空闲是可用状态，用按钮的PerformClick()方法生成按钮的点击事件
                    btnSend.PerformClick();
                }
            }
            catch (System.Exception ex)
            {
                tmSend.Enabled = false;
                MessageBox.Show("错误的定时输入！" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region txtSend_KeyPress 发送输入框校验
        private void txtSend_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (rbSend16.Checked == true)
            {
                //正则匹配，“\b”:退格键（对16进制的输入进行格式校验）
                string patten = "[0-9a-fA-F]|\b|0x|0X| ";
                Regex r = new Regex(patten);
                Match m = r.Match(e.KeyChar.ToString());

                if (m.Success)
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
        #endregion

        #region txtSecond_KeyPress 定时输入校验
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
        #endregion

        #region rbSend16_CheckedChanged 发送方式改变时清空原来的输入框
        //这个功能待完善
        private void rbSend16_CheckedChanged(object sender, EventArgs e)
        {
            if (rbRec16.Checked == true)
            {
                cbSendHex.Enabled = false;
                txtStrTo16.Text = "";
            }
            else
            {
                cbSendHex.Enabled = true;
            }
            txtSend.Text = "";
        }
        #endregion

        #region 用于将输入框的字符串实时翻译为16进制
        //用于监听cbSendHex按钮的状态
        private void cbSendHex_CheckedChanged(object sender, EventArgs e)
        {
            //如果cbSendHex按钮选中就翻译发送框的内容
            if (cbSendHex.Checked == true)
            {
                txtStrTo16.Text = ByteToHex(StringToByte(txtSend.Text));
            }
            else
            {
                txtStrTo16.Text = "";
            }
        }

        //用于监听txtSend的文本输入
        private void txtSend_TextChanged(object sender, EventArgs e)
        {
            //如果txtSend的内容发送改变，txtStrTo16实时将字符串翻转
            if (cbSendHex.Checked == true && rbRec16.Checked == false)
            {
                txtStrTo16.Text = ByteToHex(StringToByte(txtSend.Text));
            }
        }
        #endregion


    }
}
