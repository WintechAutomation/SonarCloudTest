using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace SonarCloudTest
{
    public partial class Form1 : Form
    {
        private bool IsPress = false;
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        System.IO.Ports.SerialPort serialport = new System.IO.Ports.SerialPort();

        private List<string> receivedList = new List<string>();
        
        Multiline multiLine = new Multiline();

        private struct Header
        {
            public static char STX = (char)0x02;
            public static char ETX = (char)0x03;
            public static char CR = (char)0x0D;
            public static char LF = (char)0x0A;
            public static char SPACE = (char)0x20;
            public static bool ACK = false;
        }
        private bool IsTx = false;
        private bool IsRx = false;

        private System.Threading.Thread ColorThread = null;

        private System.Timers.Timer timer = new System.Timers.Timer(2000);
        private System.Timers.Timer timer2 = new System.Timers.Timer(1000);
        public Form1()
        {
            InitializeComponent();
            multiLine.Location = new Point(this.Location.X + this.tbOneLine.Location.X, this.Location.Y + this.tbOneLine.Location.Y);
            multiLine.Hide();
            //Test2
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            IsPress = cbIsPress.Checked;
            comboBox1.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());

            string Com = Properties.Settings.Default.COM;
            for (int i = 0; i < comboBox1.Items.Count; i++)
            {
                if (comboBox1.Items[i].ToString() == Com)
                    comboBox1.SelectedIndex = i;
            }

            textBox1.Text = "0";
            textBox2.Text = "5";
            textBox3.Text = "0";
            textBox4.Text = "58.5";
            textBox5.Text = "120";
            textBox6.Text = "120";
            textBox7.Text = "0";
            textBox8.Text = "0";
            textBox9.Text = "120";
            textBox10.Text = "120";
            textBox11.Text = "0";
            textBox12.Text = "0";
            textBox13.Text = "WTAと小林プログラムのテスト";//"やめて";// "ウィンテックと小林プログラムのテスト";/*유니코드 1문자 2바이트, 최대20문자*/

            ColorThread = new System.Threading.Thread(delegate()
            {
                try
                {
                    while (true)
                    {
                        if (serialport.IsOpen)
                        {
                            if (InvokeRequired)
                            {
                                this.Invoke(new MethodInvoker(delegate()
                                {
                                    //lampCon.Visible = lampTx.Visible = lampRx.Visible = true;
                                    //lampTx.BackColor = IsTx ? Color.Lime : Color.Red;
                                    //lampRx.BackColor = IsRx ? Color.Lime : Color.Red;
                                    lbTx.ForeColor = IsTx ? Color.Lime : Color.White;
                                    lbRx.ForeColor = IsRx ? Color.Lime : Color.White;
                                    IsTx = IsRx = false;
                                }));
                            }
                            else
                            {
                                //lampCon.Visible = lampTx.Visible = lampRx.Visible = true;
                                //lampTx.BackColor = IsTx ? Color.Lime : Color.Red;
                                //lampRx.BackColor = IsRx ? Color.Lime : Color.Red;
                                lbTx.ForeColor = IsTx ? Color.Lime : Color.White;
                                lbRx.ForeColor = IsRx ? Color.Lime : Color.White;
                                IsTx = IsRx = false;
                            }
                        }
                        System.Threading.Thread.Sleep(50);
                    }
                }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.ToString()); }
            });
            ColorThread.Name = "ColorThread";
            ColorThread.IsBackground = true;
            ColorThread.Start();
        }

        private void open_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialport.IsOpen)
                {
                    serialport.DataReceived -= new System.IO.Ports.SerialDataReceivedEventHandler(serialport_DataReceived);
                    open.Text = "open";
                    comboBox1.Enabled = true;
                    send.Enabled = false;
                    //lampCon.BackColor = Color.Red;
                    lbCon.ForeColor = Color.Red;
                    lbRx.ForeColor = lbTx.ForeColor = Color.White;
                    serialport.Close();
                    DataRefreshTimer.Stop();
                }
                else
                {
                    serialport.Encoding = Encoding.ASCII;
                    serialport.BaudRate = 19200;
                    serialport.PortName = comboBox1.SelectedItem.ToString();
                    serialport.Parity = System.IO.Ports.Parity.Even;
                    serialport.DataBits = 8;
                    serialport.StopBits = System.IO.Ports.StopBits.One;
                    serialport.DtrEnable = false;
                    serialport.RtsEnable = false;
                    serialport.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(serialport_DataReceived);
                    serialport.Open();
                    serialport.DiscardInBuffer();
                    serialport.DiscardOutBuffer();
                    if (serialport.IsOpen)
                    {
                        open.Text = "close";
                        comboBox1.Enabled = false;
                        send.Enabled = true;
                        //lampCon.BackColor = Color.Lime;
                        lbCon.ForeColor = Color.Lime;
                        if (!IsPress)
                        {
                            DataRefreshTimer.Start();
                        }
                    }
                    send.Visible = IsPress;
                }
                ACK = string.Empty;
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        string ACK = string.Empty;
        List<byte> ReceivedByte = new List<byte>();
        byte[] buffer;

        private readonly object obj = new object();
        void serialport_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            IsRx = true;
            lock (obj)
            {
                if (serialport.IsOpen)
                {
                    if (IsPress)
                    {
                        System.Threading.Thread.Sleep(50);

                        byte[] buffer = new byte[serialport.BytesToRead];
                        char[] chbuf = new char[serialport.BytesToRead];
                        int DataLength = serialport.Read(buffer, 0, buffer.Length);
                        System.Diagnostics.Debug.WriteLine(ACK);

                        for (int i = 0; i < DataLength; i++)
                        {
                            chbuf[i] = Convert.ToChar(buffer[i]);
                            ACK += chbuf[i];
                        }
                        byte[] OK = new byte[2] { 0x4F, 0x4B };
                        //serialport.Write(OK, 0, 2);
                        //serialport.Write(new char[] { 'O','K'}, 0, 2);
                        System.Diagnostics.Debug.WriteLine(ACK);
                        if (ACK.IndexOf("OK") == -1) return;

                        if (ACK.IndexOf("OK") != -1)
                        {
                            Header.ACK = true;
                            if (InvokeRequired)
                                this.Invoke(new MethodInvoker(delegate ()
                                {
                                    send.BackColor = Color.Lime;
                                    send.Text = "OK";
                                }));
                            else send.Text = "OK";

                            System.Threading.Thread.Sleep(500);

                            if (InvokeRequired)
                                this.Invoke(new MethodInvoker(delegate ()
                                {
                                    send.BackColor = Color.DimGray;
                                    send.Enabled = true;
                                    send.Text = "send";
                                }));
                            else
                            {
                                send.BackColor = Color.DimGray;
                                send.Enabled = true;
                                send.Text = "send";
                            }
                        }
                        ACK = string.Empty;
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(50);
                        System.Text.StringBuilder sb = new System.Text.StringBuilder();

                        //정상 데이터 수신시 응답(OK)
                        byte[] OK = new byte[2] { 0x4F/*O*/, 0x4B/*K*/ };
                        byte[] STX_OK_ETX = new byte[4] { (byte)Header.STX, 0x4F/*O*/, 0x4B/*K*/, (byte)Header.ETX };
                        //데이터 읽어서 버퍼 저장
                        int DataLength = serialport.BytesToRead;
                        buffer = new byte[DataLength];
                        serialport.Read(buffer, 0, DataLength);
                        ReceivedByte.AddRange(buffer);

                        //STX나 ETX가 없거나 데이터 길이가 안맞으면 OK 안보냄
                        if (ReceivedByte.Count < 114 || ReceivedByte[0] != Header.STX || ReceivedByte[113] != Header.ETX) return;

                        byte[] ByteASCII = new byte[72];
                        byte[] ByteUnicode = new byte[40];
                        Array.Copy(ReceivedByte.ToArray(), 1, ByteASCII, 0, ByteASCII.Length);
                        Array.Copy(ReceivedByte.ToArray(), 73, ByteUnicode, 0, ByteUnicode.Length);

                        ACK = Convert.ToChar(ReceivedByte[0]).ToString();

                        ACK += Encoding.ASCII.GetString(ByteASCII);
                        ACK += Encoding.Unicode.GetString(ByteUnicode);

                        ACK = string.Concat(ACK, (char)ReceivedByte[113]);

                        int StartIndex = ACK.IndexOf((char)Header.STX) + 1;
                        int LastIndex = ACK.IndexOf((char)Header.ETX);                
                        int CurrentIndex = StartIndex;
                        try
                        {
                            //데이터 변환중 에러 발생하면 exception(OK 안보냄)
                            double value = 0;
                            tryParseDouble(out value, ACK.Substring(CurrentIndex, 6)); pressData.Thickness1          = value; CurrentIndex += 6;
                            tryParseDouble(out value, ACK.Substring(CurrentIndex, 6)); pressData.Thickness2          = value; CurrentIndex += 6;
                            tryParseDouble(out value, ACK.Substring(CurrentIndex, 6)); pressData.Gap2                = value; CurrentIndex += 6;
                            tryParseDouble(out value, ACK.Substring(CurrentIndex, 6)); pressData.Punch1LowerHome     = value; CurrentIndex += 6;
                            tryParseDouble(out value, ACK.Substring(CurrentIndex, 6)); pressData.Punch1UpperHome     = value; CurrentIndex += 6;
                            tryParseDouble(out value, ACK.Substring(CurrentIndex, 6)); pressData.Punch2UpperHome     = value; CurrentIndex += 6;
                            tryParseDouble(out value, ACK.Substring(CurrentIndex, 6)); pressData.PlateHome     = value; CurrentIndex += 6;
                            tryParseDouble(out value, ACK.Substring(CurrentIndex, 6)); pressData.Punch2LowerHome     = value; CurrentIndex += 6;
                            tryParseDouble(out value, ACK.Substring(CurrentIndex, 6)); pressData.Punch1UpperCleaning = value; CurrentIndex += 6;
                            tryParseDouble(out value, ACK.Substring(CurrentIndex, 6)); pressData.Punch2UpperCleaning = value; CurrentIndex += 6;
                            tryParseDouble(out value, ACK.Substring(CurrentIndex, 6)); pressData.PlateCleaning       = value; CurrentIndex += 6;
                            tryParseDouble(out value, ACK.Substring(CurrentIndex, 6)); pressData.Punch2LowerCleaning = value; CurrentIndex += 6;

                            var NameHex = ACK.Substring(CurrentIndex, LastIndex - CurrentIndex).Trim();


                            pressData.ModelName = NameHex; CurrentIndex += 40;
                        }
                        catch (Exception ex) { System.Diagnostics.Debug.WriteLine("", new Exception(ACK)); return; }

                        //받은 데이터가 정상이라면 OK 보냄
                        if (ACK[StartIndex - 1] == 2 && ACK[LastIndex] == 3) serialport.Write(OK, 0, OK.Length);

                        sb.AppendLine(string.Format("ReceivedDatas:{0}", ACK));
                        
                        sb.AppendLine("=============================프레스로부터 받은 정보================================");
                        sb.AppendLine(string.Format("pressData.Thickness1          : {0}", pressData.Thickness1         ));
                        sb.AppendLine(string.Format("pressData.Thickness2          : {0}", pressData.Thickness2         ));
                        sb.AppendLine(string.Format("pressData.Gap2                : {0}", pressData.Gap2               ));
                        sb.AppendLine(string.Format("pressData.Punch1LowerHome     : {0}", pressData.Punch1LowerHome    ));
                        sb.AppendLine(string.Format("pressData.Punch1UpperHome     : {0}", pressData.Punch1UpperHome    ));
                        sb.AppendLine(string.Format("pressData.Punch2UpperHome     : {0}", pressData.Punch2UpperHome    ));
                        sb.AppendLine(string.Format("pressData.PlateLower2Home     : {0}", pressData.PlateHome    ));
                        sb.AppendLine(string.Format("pressData.Punch2LowerHome     : {0}", pressData.Punch2LowerHome    ));
                        sb.AppendLine(string.Format("pressData.Punch1UpperCleaning : {0}", pressData.Punch1UpperCleaning));
                        sb.AppendLine(string.Format("pressData.Punch2UpperCleaning : {0}", pressData.Punch2UpperCleaning));
                        sb.AppendLine(string.Format("pressData.PlateCleaning       : {0}", pressData.PlateCleaning      ));
                        sb.AppendLine(string.Format("pressData.Punch2LowerCleaning : {0}", pressData.Punch2LowerCleaning));
                        sb.AppendLine(string.Format("pressData.ModelName           : {0}", pressData.ModelName          ));
                        sb.AppendLine("===================================================================================");
                        System.Diagnostics.Debug.WriteLine(sb.ToString());

#warning 높이까지 비교하도록 추가.
                        //모델파일이 바뀌면 이벤트 발생
                        //if (Global.Model.Insert_Info.ModelName.Equals(m_PressData.ModelName) == false)
                        //    Sequence.Events.Go_ModelChangeEvent();
                        ////안바뀌면 제품 두께만 수정
                        //else
                        System.Diagnostics.Debug.WriteLine(sb.ToString());
                        ACK = ACK.Remove(ACK.IndexOf((char)Header.STX), ACK.IndexOf((char)Header.ETX) + 1);
                        ReceivedByte.Clear();
                    }
                }
            }
        }

        private static void tryParseDouble(out double value, string textValue)
        {
            try
            {
                if (double.TryParse(textValue, out value))
                {
                    value = double.Parse(textValue) / 1000;
                    if (value < 0) value *= 10;
                }
                else throw new Exception();
            }
            catch (Exception ex) { System.Windows.Forms.MessageBox.Show(string.Format("{0} <--- 해당값을 변환할 수 없음", textValue)); value = 0; }
        }

        public struct PressData
        {
            /// <summary>
            ///     지시두께 1
            /// </summary>
            public double Thickness1 { get; set; }

            /// <summary>
            ///     지시두께 2
            /// </summary>
            public double Thickness2{ get; set; }

            /// <summary>
            ///     단차 2
            /// </summary>
            public double Gap2{ get; set; }

            /// <summary>
            ///     금형원점(펀치1 하부원점, 하펀치1과 하펀치 플레이트의 높이가 일치하는 위치. 하펀치1의 높이가 제품마다 달라짐. 하펀치1은 고정펀치)
            /// </summary>
            public double Punch1LowerHome{ get; set; }

            /// <summary>
            ///     펀치1 상부원점
            /// </summary>
            public double Punch1UpperHome{ get; set; }

            /// <summary>
            ///     펀치2 상부원점
            /// </summary>
            public double Punch2UpperHome{ get; set; }

            /// <summary>
            ///     원위치 절구
            /// </summary>
            public double PlateHome{ get; set; }

            /// <summary>
            ///     펀치2 하부원점
            /// </summary>
            public double Punch2LowerHome{ get; set; }

            /// <summary>
            ///     청소위치 금형1 상부
            /// </summary>
            public double Punch1UpperCleaning{ get; set; }

            /// <summary>
            ///     청소위치 금형2 상부
            /// </summary>
            public double Punch2UpperCleaning{ get; set; }

            /// <summary>
            ///     청소위치 절구 
            /// </summary>
            public double PlateCleaning{ get; set; }

            /// <summary>
            ///     청소위치 금형2 하부 
            /// </summary>
            public double Punch2LowerCleaning{ get; set; }

            /// <summary>
            ///     파일명
            /// </summary>
            public string ModelName{ get; set; }
        }

        private PressData pressData = new PressData();

        private void send_Click(object sender, EventArgs e)
        {
            send.Enabled = false;
            System.Threading.Thread th = new System.Threading.Thread(delegate ()
                {
                    System.Threading.Thread.Sleep(100);
                    IsTx = true;

                    List<byte> BytePacket = new List<byte>();
                    if (IsPress)
                    {
                        var sb = new StringBuilder();

                        sb.AppendLine("┌ASCII 패킷 변환---------------------------------------┐");
                        BytePacket.Add((byte)Header.STX);
                        
                        sb.AppendLine("├VALUE-----┬DECIMAL----------------┬HEX--------------┤");
                        BytePacket.AddRange(GetFormatBytes(Convert.ToDouble(textBox1.Text), ref sb));
                        BytePacket.AddRange(GetFormatBytes(Convert.ToDouble(textBox2.Text), ref sb));
                        BytePacket.AddRange(GetFormatBytes(Convert.ToDouble(textBox3.Text), ref sb));
                        BytePacket.AddRange(GetFormatBytes(Convert.ToDouble(textBox4.Text), ref sb));
                        BytePacket.AddRange(GetFormatBytes(Convert.ToDouble(textBox5.Text), ref sb));
                        BytePacket.AddRange(GetFormatBytes(Convert.ToDouble(textBox6.Text), ref sb));
                        BytePacket.AddRange(GetFormatBytes(Convert.ToDouble(textBox7.Text), ref sb));
                        BytePacket.AddRange(GetFormatBytes(Convert.ToDouble(textBox8.Text), ref sb));
                        BytePacket.AddRange(GetFormatBytes(Convert.ToDouble(textBox9.Text), ref sb));
                        BytePacket.AddRange(GetFormatBytes(Convert.ToDouble(textBox10.Text), ref sb));
                        BytePacket.AddRange(GetFormatBytes(Convert.ToDouble(textBox11.Text), ref sb));
                        BytePacket.AddRange(GetFormatBytes(Convert.ToDouble(textBox12.Text), ref sb));
                        sb.AppendLine("└----------┴-----------------------┴-----------------┘");

                        string FileName = textBox13.Text;
                        //while (FileName.Length < 20) FileName = string.Concat(FileName, (char)0x20/*' '임(ㄱ 한자 1로 만드는 스페이스문자)*/);
                        var encoding = Encoding.GetEncoding("Shift-JIS");

                        {
                            sb.AppendLine("┌모델이름--------------------------------------------------------------------------------------------------------------┐");
                            sb.AppendLine("├----┬----┬----┬----┬----┬----┬----┬----┬----┬----┬----┬----┬----┬----┬----┬----┬----┬----┬----┬----┤");
                            for (int i = 0; i < FileName.Length; i++) sb.Append(string.Format("│ {0} ", Encoding.Default.GetByteCount(new char[] { FileName[i] }) > 1 ? FileName[i].ToString() : " " + FileName[i].ToString()));
                            sb.AppendLine("│");
                            sb.AppendLine("├----┼----┼----┼----┼----┼----┼----┼----┼----┼----┼----┼----┼----┼----┼----┼----┼----┼----┼----┼----┤");
                            for (int i = 0; i < FileName.Length; i++)
                            {
                                var character  = encoding.GetBytes(FileName[i].ToString());

                            //System.Diagnostics.Debug.Write("│ ウ │ィ│ン│テ│ッ│ク│と│小│林│プ│ロ│グ│ラ│ム│の│テ│ス│ト│　│　│");
                                sb.Append("│");
                                if (character.Length <= 1) sb.Append("  ");
                                for (int c = 0; c < character.Length; c++) sb.Append(string.Format("{0:X2}", character[c]));
                            }
                            sb.AppendLine("│");
                            sb.AppendLine("└----┴----┴----┴----┴----┴----┴----┴----┴----┴----┴----┴----┴----┴----┴----┴----┴----┴----┴----┴----┘");
                            System.Diagnostics.Debug.WriteLine(sb.ToString());
                            sb.Clear();
                        }

                        //for (int i = 0; i < FileName.Length; i++)
                        //{
                        //    List<byte> character = new List<byte>();
                        //    character.AddRange(encoding.GetBytes(FileName[i].ToString()));
                        //    //if (character.Count <= 1) character.Insert(0, 0); 
                        //    BytePacket.AddRange(character.ToArray());
                        //}
                        List<byte> NameBytes = new List<byte>();
                        NameBytes.AddRange(encoding.GetBytes(FileName));
                        while (NameBytes.Count < 40) NameBytes.Add((byte)Header.SPACE);
                        BytePacket.AddRange(NameBytes.ToArray());
                        //var result = encoding.GetBytes(FileName);
                        //for (int i = result.Length - 1; i >= 0; i--) System.Diagnostics.Debug.Write(string.Format("{0:X2}\t", result[i]));
                        //System.Diagnostics.Debug.WriteLine("");
                        //result = Encoding.Unicode.GetBytes(FileName);
                        //for (int i = result.Length - 1; i >= 0; i--) System.Diagnostics.Debug.Write(string.Format("{0:X2}\t", result[i]));
                        BytePacket.Add((byte)Header.ETX);
                        sb.AppendLine("=============================핸들러로 보낼 정보================================");
                        sb.AppendLine(string.Format("지시두께 1           {0}", textBox1.Text));
                        sb.AppendLine(string.Format("지시두께 2           {0}", textBox2.Text));
                        sb.AppendLine(string.Format("단차 2               {0}", textBox3.Text));
                        sb.AppendLine(string.Format("금형원점             {0}", textBox4.Text));
                        sb.AppendLine(string.Format("펀치1 상부원점       {0}", textBox5.Text));
                        sb.AppendLine(string.Format("펀치2 상부원점       {0}", textBox6.Text));
                        sb.AppendLine(string.Format("원위치 절구          {0}", textBox7.Text));
                        sb.AppendLine(string.Format("펀치2 하부원점       {0}", textBox8.Text));
                        sb.AppendLine(string.Format("청소위치 금형1 상부  {0}", textBox9.Text));
                        sb.AppendLine(string.Format("청소위치 금형2 상부  {0}", textBox10.Text));
                        sb.AppendLine(string.Format("청소위치 절구        {0}", textBox11.Text));
                        sb.AppendLine(string.Format("청소위치 금형2 하부  {0}", textBox12.Text));
                        sb.AppendLine(string.Format("파일명               {0}", textBox13.Text));
                        sb.AppendLine("===================================================================================");
                        System.Diagnostics.Debug.WriteLine(sb.ToString());

                        if (serialport.IsOpen) serialport.Write(BytePacket.ToArray(), 0, BytePacket.Count);

                        timer2.Elapsed += new System.Timers.ElapsedEventHandler(timer2Event);
                        timer2.Start();

                        timer.Elapsed += new System.Timers.ElapsedEventHandler(timerEvent);
                        timer.Start();
                    }
                });
            th.IsBackground = true;
            th.Start();
        }

        private string GetFormat(double pValue)
        {
            if (pValue < 0) return string.Format("{0:000.00}", pValue);
            else return string.Format("{0:000.000}", pValue);
        }

        /// <summary>
        ///     포맷에 맞게 변환 후, ASCII형식 바이트배열로 변환함. 
        /// </summary>
        /// <param name="pValue"></param>
        /// <returns></returns>
        private byte[] GetFormatBytes(double pValue, ref StringBuilder pSb)
        {
            string ValueString = string.Empty;
            if (pValue < 0) ValueString = string.Format("{0:00.000}", pValue).Replace(".", null);
            else ValueString = string.Format("{0:000.000}", pValue).Replace(".", null);

            var ascii = Encoding.ASCII.GetBytes(ValueString);

            {
                string DecSTR = string.Empty;
                string HexSTR = string.Empty;

                for (int i = 0; i < ascii.Length; i++) DecSTR += string.Format("{0}{1}", ascii[i].ToString().PadLeft(3), i < ascii.Length - 1 ? " " : "");
                for (int i = 0; i < ascii.Length; i++) HexSTR += string.Format("{0:X2}{1}", (byte)ascii[i], i < ascii.Length - 1 ? " " : "");
                pSb.AppendLine(string.Format("│{0} │{1}│{2}│", pValue.ToString().PadLeft(9), DecSTR, HexSTR));
            }

            return ascii;
        }

        void timer2Event(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Header.ACK) { timer2.Stop(); }
            else { send.BackColor = Color.Red; timer2.Stop(); };
        }

        void timerEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (InvokeRequired)
                this.Invoke(new MethodInvoker(delegate()
                {
                    if (Header.ACK)
                    {
                        send.Text = "send";
                    }
                    send.Enabled = true;
                    send.BackColor = Color.DimGray;
                    Header.ACK = false;
                    timer2.Stop(); 
                    timer.Stop();
                }));
            else
            {
                if (Header.ACK)
                {
                    send.Text = "send";
                }
                send.Enabled = true;
                send.BackColor = Color.DimGray;
                Header.ACK = false;
                timer2.Stop(); 
                timer.Stop();
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void close_Click(object sender, EventArgs e)
        {
            if (serialport.IsOpen) serialport.Close();
            if (ColorThread != null && ColorThread.IsAlive) ColorThread.Abort();
            Properties.Settings.Default.COM =           serialport.PortName;
            Properties.Settings.Default.Save();
            multiLine.tbMultiLine.Clear();
            this.Close();
        }

        private void textBox7_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            multiLine.Location = new Point(this.Location.X + this.tbOneLine.Location.X, this.Location.Y + this.tbOneLine.Location.Y);
            multiLine.Show();
        }

        private void cbIsPress_CheckedChanged(object sender, EventArgs e)
        {
            IsPress = cbIsPress.Checked;

            textBox1.ReadOnly = !IsPress;
            textBox2.ReadOnly = !IsPress;
            textBox3.ReadOnly = !IsPress;
            textBox4.ReadOnly = !IsPress;
            textBox5.ReadOnly = !IsPress;
            textBox6.ReadOnly = !IsPress;
            textBox7.ReadOnly = !IsPress;
            textBox8.ReadOnly = !IsPress;
            textBox9.ReadOnly = !IsPress;
            textBox10.ReadOnly = !IsPress;
            textBox11.ReadOnly = !IsPress;
            textBox12.ReadOnly = !IsPress;
            textBox13.ReadOnly = !IsPress;
        }

        private void label11_Click(object sender, EventArgs e)
        {
            //byte[] OK = new byte[] { 0x02, 0x4F, 0x4B, 0x03 };
            byte[] OK = new byte[] { 0x4F, 0x4B};
            System.Threading.Thread.Sleep(50);
            serialport.Write(OK, 0, OK.Length);
        }

        private void DataRefreshTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                textBox1.Text = pressData.Thickness1.ToString();
                textBox2.Text = pressData.Thickness2.ToString();
                textBox3.Text = pressData.Gap2.ToString();
                textBox4.Text = pressData.Punch1LowerHome.ToString();
                textBox5.Text = pressData.Punch1UpperHome.ToString();
                textBox6.Text = pressData.Punch2UpperHome.ToString();
                textBox7.Text = pressData.PlateHome.ToString();
                textBox8.Text = pressData.Punch2LowerHome.ToString();
                textBox9.Text = pressData.Punch1UpperCleaning.ToString();
                textBox10.Text = pressData.Punch2UpperCleaning.ToString();
                textBox11.Text = pressData.PlateCleaning.ToString();
                textBox12.Text = pressData.Punch2LowerCleaning.ToString();
                textBox13.Text = pressData.ModelName;
            }
            catch (Exception ex) { }
        }
    }
}
