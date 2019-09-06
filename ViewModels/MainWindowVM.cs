﻿using OSerialPort.Models;
using System;
using System.Globalization;
using System.IO.Ports;
using System.Windows.Media;
using System.Windows.Threading;

namespace OSerialPort.ViewModels
{
    class MainWindowViewModel : MainWindowBase
    {
        public HelpModel HelpModel { get; set; }
        public RecvModel RecvModel { get; set; }
        public SendModel SendModel { get; set; }
        public SerialPortModel SerialPortModel { get; set; }
        public TimerModel TimerModel { get; set; }

        #region 菜单栏
        public void Update()
        {

        }
        #endregion

        #region 打开/关闭串口
        public bool OpenSP()
        {
            if (SPserialPort != null && SPserialPort.IsOpen)
            {
                return CloseSP();
            }

            try
            {
                SPserialPort = new SerialPort
                {
                    PortName = SerialPortModel.SPPort,
                    BaudRate = SerialPortModel.SPBaudRate,
                    DataBits = SerialPortModel.SPDataBits,
                    StopBits = SerialPortModel.GetStopBits(SerialPortModel.SPStopBits.ToString()),
                    Parity = SerialPortModel.GetParity(SerialPortModel.SPParity.ToString()),
                    WriteBufferSize = 1048576,   /* 设置串行端口输出缓冲区的大小为1048576字节，即1MB */
                    ReadBufferSize = 2097152,    /* 设置串行端口输入缓冲区的大小为2097152字节，即2MB */
                    Encoding = System.Text.Encoding.UTF8,
                    Handshake = Handshake.None,
                    RtsEnable = true
                };

                SPserialPort.DataReceived += new SerialDataReceivedEventHandler(RecvModel.SerialPort_DataReceived);

                SPserialPort.Open();

                if (SPserialPort.IsOpen)
                {
                    SerialPortModel.SPBrush = Brushes.GreenYellow;
                    SerialPortModel.OpenCloseSP = "关闭串口";
                    DepictInfo = string.Format("成功打开串行端口{0}、波特率{1}、数据位{2}、停止位{3}、校验位{4}",
                        SPserialPort.PortName, SPserialPort.BaudRate.ToString(), SPserialPort.DataBits.ToString(),
                        SPserialPort.StopBits.ToString(), SPserialPort.Parity.ToString());

                    SerialPortModel.SPPortEnable = false;
                    SerialPortModel.SPBaudRateEnable = false;
                    SerialPortModel.SPDataBitsEnable = false;
                    SerialPortModel.SPStopBitsEnable = false;
                    SerialPortModel.SPParityEnable = false;

                    return true;
                }
                else
                {
                    DepictInfo = "串行端口打开失败";

                    return false;
                }
            }
            catch
            {
                DepictInfo = "串行端口发生意外，打开失败，请检查线路";

                return false;
            }
        }

        public bool CloseSP()
        {
            try
            {
                if (SPserialPort.IsOpen)
                {
                    SPserialPort.Close();

                    SerialPortModel.SPBrush = Brushes.Red;
                    SerialPortModel.OpenCloseSP = "打开串口";

                    DepictInfo = "串行端口关闭成功";

                    SerialPortModel.SPPortEnable = true;
                    SerialPortModel.SPBaudRateEnable = true;
                    SerialPortModel.SPDataBitsEnable = true;
                    SerialPortModel.SPStopBitsEnable = true;
                    SerialPortModel.SPParityEnable = true;

                    RecvModel.ReceAutoSave = "已停止";
                    RecvModel.ReceHeader = "接收区：已接收" + RecvModel.ReceDataCount +
                            "字节，接收自动保存[" + RecvModel.ReceAutoSave + "]";

                    return SPserialPort.IsOpen;
                }
                else
                {
                    DepictInfo = "串行端口已关闭";

                    return SPserialPort.IsOpen;
                }
            }
            catch
            {
                DepictInfo = "串行端口发生意外，关闭失败，请检查线路";

                return false;
            }
        }
        #endregion

        #region 自动发送
        public DispatcherTimer AutoSendDispatcherTimer = new DispatcherTimer();

        public void InitAutoSendTimer()
        {
            AutoSendDispatcherTimer.IsEnabled = false;
            AutoSendDispatcherTimer.Tick += AutoSendDispatcherTimer_Tick;
        }

        public void AutoSendDispatcherTimer_Tick(object sender, EventArgs e)
        {
            Send();
        }

        public void StartAutoSendTimer(int interval)
        {
            AutoSendDispatcherTimer.IsEnabled = true;
            AutoSendDispatcherTimer.Interval = TimeSpan.FromMilliseconds(interval);
            AutoSendDispatcherTimer.Start();
        }

        public void StopAutoSendTimer()
        {
            AutoSendDispatcherTimer.IsEnabled = false;
            AutoSendDispatcherTimer.Stop();
        }
        #endregion

        #region 发送
        public void Send()
        {
            try
            {
                if (SPserialPort != null && SPserialPort.IsOpen)
                {
                    if (SendModel.HexSend)
                    {
                        int cnt = 0;
                        string[] _sendData = SendModel.SendData.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        char[] sendData = new char[_sendData.Length];

                        foreach (var tmp in _sendData)
                        {
                            sendData[cnt++] = (char)Int16.Parse(tmp, NumberStyles.AllowHexSpecifier);
                        }

                        SendModel.SendDataCount += cnt;
                        SPserialPort.Write(sendData, 0, cnt);

                    }
                    else
                    {
                        SendModel.SendDataCount += SendModel.SendData.Length;
                        SPserialPort.Write(SendModel.SendData.ToCharArray(), 0, SendModel.SendData.Length);
                    }
                }
            }
            catch
            {
                DepictInfo = "发送异常，请检查发送数据";
            }
        }
        #endregion

        #region 多项发送
        public void Sends()
        {
            if (SPserialPort != null && SPserialPort.IsOpen)
            {
                if (SendModel.HexSend)
                {

                }
                else
                {

                }

                SendModel.SendDataCount += SendModel.SendData.Length;
            }
        }
        #endregion

        #region 路径选择
        public void SaveRecePath()
        {
            RecvModel.RecePath();
        }
        #endregion

        #region 辅助区
        public bool _AutoSend;
        public bool AutoSend
        {
            get
            {
                return _AutoSend;
            }
            set
            {
                if (SPserialPort != null && SPserialPort.IsOpen)
                {
                    if (_AutoSend != value)
                    {
                        _AutoSend = value;
                        RaisePropertyChanged("AutoSend");
                    }

                    if (AutoSend == true)
                    {
                        StartAutoSendTimer(SendModel.AutoSendNum);
                    }
                    else
                    {
                        StopAutoSendTimer();
                    }
                }
            }
        }
        #endregion

        #region 清接收区
        public void ClarReceData()
        {
            RecvModel.ReceData.Delete();
        }
        #endregion

        #region 清发送区
        public void ClearSendData()
        {
            SendModel.SendData = string.Empty;
        }
        #endregion

        #region 清空计数
        public void ClearCount()
        {
            RecvModel.ReceDataCount = 0;
            RecvModel.ReceHeader = "接收区：已接收" + RecvModel.ReceDataCount + "字节，接收自动保存[" + RecvModel.ReceAutoSave + "]";

            SendModel.SendDataCount = 0;
        }
        #endregion

        public MainWindowViewModel()
        {
            HelpModel = new HelpModel();
            HelpModel.HelpDataContext();

            RecvModel = new RecvModel();
            RecvModel.RecvDataContext();

            SendModel = new SendModel();
            SendModel.SendDataContext();

            SerialPortModel = new SerialPortModel();
            SerialPortModel.SerialPortDataContext();

            DepictInfo = "串行端口调试助手";

            TimerModel = new TimerModel();
            TimerModel.TimerDataContext();

            AutoSend = false;
            InitAutoSendTimer();
        }
    }
}
