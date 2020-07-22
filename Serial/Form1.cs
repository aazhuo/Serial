using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Collections;
using System.Threading;
namespace Serial
{
	public partial class Form1 : Form
	{
		//支持的波特率、停止位，数据位，奇偶校验位
		public static int[] baudRateList = { 1382400, 460800, 230400, 115200, 38400, 9600 };
		public static StopBits[] stopBitList = { StopBits.None, StopBits.One,  StopBits.OnePointFive, StopBits.Two };
		public static int[] dataBitList = { 5,6,7,8 };
		public static Parity[] parityList = { Parity.None, Parity.Odd, Parity.Even, Parity.Mark, Parity.Space };
		private Thread receiveThread=null, ScanSerilaThread = null, AutoThread = null;
		private static SerialPort SerialPorts = new SerialPort();
		private delegate void SafeCallDelegate(object data);
		private delegate void SetSeriallistCallback();
		private bool AutoThread_flag = false;
		
		public Form1()
		{
			InitializeComponent();

		
		}
	

		//串口接收函数
		void ReceiveData(object sender, SerialDataReceivedEventArgs e)
		{

			try
			{
				System.Threading.Thread.Sleep(10);
			//	SerialPort sp = (SerialPort)sender;
				string str=SerialPorts.ReadExisting();
			//	byte[] buffer = new byte[sp.BytesToRead];
			//	sp.Read(buffer, 0, buffer.Length);
		         receiveThread = new Thread(displayReceiveData);
			    receiveThread.Start(str);
				//receiveThread.SetApartmentState();
				//sp.DiscardInBuffer();

			}
			catch(Exception)
			{



			}

		
		
		}

		//更新窗体显示
		void displayReceiveData(object data)
		{

		
			if (serial_recv.InvokeRequired)
			{
				var d = new SafeCallDelegate(displayReceiveData);
				Invoke(d, new object[] { data });
			}
			else
			{
			
				//16+ tiem
				if (checkBox1.CheckState == CheckState.Checked && checkBox2.CheckState == CheckState.Checked) {

					serial_recv.AppendText(DateTime.Now.ToString() + "-->" + stringToHexStr(data.ToString()));
				
				}
				//16+not time
				else if (checkBox1.CheckState == CheckState.Checked && checkBox2.CheckState == CheckState.Unchecked)
				{
					serial_recv.AppendText( stringToHexStr(data.ToString()));
				}

				//NOT 16+TIME
				else if (checkBox1.CheckState == CheckState.Unchecked && checkBox2.CheckState == CheckState.Checked)
				{


					serial_recv.AppendText(DateTime.Now.ToString() + "-->" + data.ToString());

				}
		     else if (checkBox1.CheckState == CheckState.Unchecked)
				{

					 serial_recv.AppendText( data.ToString());
				}
				label9.Text =(Convert.ToInt64(label9.Text)+ Getstr_count(data.ToString())).ToString();
			}
			

		}

		// open/close
		private void button1_Click(object sender, EventArgs e)
		{

			if (button1.Text == "打开串口")
			{
			
				if (!SerialPorts.IsOpen)
				{
					try
					{
	                   
						SerialPorts.PortName = comboBox1.Text;

						SerialPorts.Encoding = System.Text.Encoding.GetEncoding("GB2312");
						SerialPorts.BaudRate = baudRateList[baud_list.SelectedIndex];
						SerialPorts.DataBits = dataBitList[data_list.SelectedIndex];
						SerialPorts.Parity = parityList[parity_list.SelectedIndex];
						SerialPorts.StopBits = stopBitList[stop_list.SelectedIndex];
						SerialPorts.Open();
						button1.Text = "关闭串口";
					}
					catch (Exception)
					{

						button1.Text = "打开串口";
						MessageBox.Show("串口" + SerialPorts.PortName + "无法打开，请检查后重新打开");
					}

			
				}

			
					
				

			}
			else if (button1.Text == "关闭串口")
			{
				if (SerialPorts.IsOpen)
				{
					try
					{
						SerialPorts.Close();
						button1.Text = "打开串口";
					}
					catch (Exception)
					{


					}

				}

			}
			

		}

	

		//字符串转16进制字符串
		public string stringToHexStr(string str)
		{
			string returnStr = "";

			byte[] byteArray = System.Text.Encoding.GetEncoding("GB2312").GetBytes(str);
			if (byteArray != null)
			{
				for (int i = 0; i < byteArray.Length; i++)
				{
					returnStr += byteArray[i].ToString("X2").Insert(2, " ");//两个字符之间加入空格

				}

			}
			return returnStr;
		}
		//16进制字符串转换字符串
		public static string HexStrToString(string strstr)
		{
			string str = "";
			try
			{

				string[] strArr = strstr.Trim().Split(' ');//移除头尾空格+按照空格
				byte[] b = new byte[strArr.Length];
				for (int i = 0; i < strArr.Length; i++)   //逐个字符变为16进制字节数据
				{
					b[i] = Convert.ToByte(strArr[i], 16);
				}
				str = System.Text.Encoding.Default.GetString(b);
			}
			catch (Exception )
			{
				MessageBox.Show("格式不对");
				return null;
			}
			return str;
		}

		private void button4_Click(object sender, EventArgs e)
		{


			if (SerialPorts.IsOpen)
			{

				if (checkBox4.CheckState == CheckState.Checked&&checkBox6.CheckState==CheckState.Checked)
				{
					SerialPorts.Write("AT+"+serial_send.Text + "\r\n");
					label8.Text = (Getstr_count("AT+" + serial_send.Text.ToString()+"\r\n") + Convert.ToInt64(label8.Text)).ToString();
				}
				else if(checkBox4.CheckState == CheckState.Checked && checkBox6.CheckState == CheckState.Unchecked)
				{

					SerialPorts.Write( serial_send.Text + "\r\n");
					label8.Text = (Getstr_count(serial_send.Text.ToString() + "\r\n") + Convert.ToInt64(label8.Text)).ToString();
				
				}else if (checkBox4.CheckState == CheckState.Unchecked && checkBox6.CheckState == CheckState.Checked)
				{
					SerialPorts.Write("AT+" + serial_send.Text); 
					label8.Text = (Getstr_count("AT+" + serial_send.Text.ToString()) + Convert.ToInt64(label8.Text)).ToString();

				}
				else
				{

					SerialPorts.Write(serial_send.Text);
					label8.Text = (Getstr_count(serial_send.Text) + Convert.ToInt64(label8.Text)).ToString();
				}
			
			}

		}



		//定时发送
		private void Send_data()
		{

			while (AutoThread_flag)
			{
				if (SerialPorts.IsOpen)
				{

					Action action = delegate ()
				{
					

						if (checkBox4.CheckState == CheckState.Checked && checkBox6.CheckState == CheckState.Checked)
						{
							SerialPorts.Write("AT+" + serial_send.Text + "\r\n");
							label8.Text = (Getstr_count("AT+" + serial_send.Text.ToString() + "\r\n") + Convert.ToInt64(label8.Text)).ToString();
						}
						else if (checkBox4.CheckState == CheckState.Checked && checkBox6.CheckState == CheckState.Unchecked)
						{

							SerialPorts.Write(serial_send.Text + "\r\n");
							label8.Text = (Getstr_count(serial_send.Text.ToString() + "\r\n") + Convert.ToInt64(label8.Text)).ToString();

						}
						else if (checkBox4.CheckState == CheckState.Unchecked && checkBox6.CheckState == CheckState.Checked)
						{
							SerialPorts.Write("AT+" + serial_send.Text);
							label8.Text = (Getstr_count("AT+" + serial_send.Text.ToString()) + Convert.ToInt64(label8.Text)).ToString();

						}
						else
						{

							SerialPorts.Write(serial_send.Text);
							label8.Text = (Getstr_count(serial_send.Text) + Convert.ToInt64(label8.Text)).ToString();
						}






				


				};
					this.Invoke(action);
				}
				else
			{


					break;
				}
			
				Thread.Sleep(Convert.ToInt32(textBox1.Text));

			}

		}
		private void button5_Click(object sender, EventArgs e)
		{
			serial_recv.Clear();
		
			label9.Text = "0";
		}

		private void button3_Click(object sender, EventArgs e)
		{
			serial_send.Clear();
			label8.Text = "0";
		}

	
		//定时扫描串口
		void Scan_SerilaPort()
		{

	
      
			if (this.comboBox1.InvokeRequired)
			{		
						SetSeriallistCallback d = new SetSeriallistCallback(Scan_SerilaPort);
						this.Invoke(d);
				
			}
			else
			{

				comboBox1.Items.Clear();
				foreach (string portName in SerialPort.GetPortNames())
				{
					comboBox1.Items.Add(portName);
			
					 
				}
				comboBox1.Text = SerialPort.GetPortNames()[0].ToString();




				}


		
			
			}

			
		
			

	

		private void Form1_SizeChanged(object sender, EventArgs e)
		{
			if (this.WindowState == FormWindowState.Minimized) //判断是否最小化
			{
				this.ShowInTaskbar = false; //不显示在系统任务栏
				notifyIcon1.Visible = true; //托盘图标可见
			}
		}

	

		private void checkBox3_CheckedChanged(object sender, EventArgs e)
		{
			if (checkBox3.CheckState == CheckState.Checked)
			{
				serial_send.Text = stringToHexStr(serial_send.Text);

			}

			else if (checkBox3.CheckState == CheckState.Unchecked)
			{
				serial_send.Text = HexStrToString(serial_send.Text);
			}
		}



		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
	
			System.Environment.Exit(System.Environment.ExitCode);
			this.Dispose();
		}

		private void Form1_Load(object sender, EventArgs e)
		{


			ScanSerilaThread = new Thread(Scan_SerilaPort);
			ScanSerilaThread.Start();
			SerialPorts.DataReceived += new SerialDataReceivedEventHandler(ReceiveData);
		}


		public bool isChinese(char c)
		{
			return c >= 0x4E00 && c <= 0x9FA5;
		}

		private void checkBox5_CheckedChanged(object sender, EventArgs e)
		{
		


			if (checkBox5.CheckState == CheckState.Checked)
			{
				AutoThread_flag = true;
				AutoThread = new Thread(Send_data);
				AutoThread.Start();

			}
			else if(checkBox5.CheckState == CheckState.Unchecked)
			{

				AutoThread_flag = false;
			

			}
		}

	
	
		private void baud_list_SelectedIndexChanged(object sender, EventArgs e)
		{
			SerialPorts.BaudRate = baudRateList[baud_list.SelectedIndex];
		
		}

		private void data_list_SelectedIndexChanged(object sender, EventArgs e)
		{
			SerialPorts.DataBits = dataBitList[data_list.SelectedIndex];

		}

		private void stop_list_SelectedIndexChanged(object sender, EventArgs e)
		{
			
			SerialPorts.StopBits = stopBitList[stop_list.SelectedIndex];
		}

		private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (this.WindowState == FormWindowState.Minimized)
			{
				this.ShowInTaskbar = true; //显示在系统任务栏
				this.WindowState = FormWindowState.Normal; //还原窗体
				notifyIcon1.Visible = false; //托盘图标隐藏
			}
		}

	
		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (button1.Text == "关闭串口")
			{
				button1.Text = "打开串口";
				try
				{
					SerialPorts.Close();
					SerialPorts.PortName = comboBox1.Text;
					SerialPorts.Open();
					SerialPorts.Encoding = System.Text.Encoding.GetEncoding("GB2312");
					button1.Text = "关闭串口";
				}
				catch (Exception)
				{

					button1.Text = "打开串口";
					MessageBox.Show("串口" + SerialPorts.PortName + "无法打开，请检查后重新打开");
				}


			}
		}

		private void comboBox1_MouseDown(object sender, MouseEventArgs e)
		{



			ScanSerilaThread = new Thread(Scan_SerilaPort);
			ScanSerilaThread.Start();


		}

		private void button2_Click(object sender, EventArgs e)
		{

			byte[] mySigArray = new byte[1400];
			UInt32 file_len;

			OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			openFileDialog1.Filter = "bin文件(*.bin)|*.bin";
			if (openFileDialog1.ShowDialog() == DialogResult.OK)
			{
				openfile_name.Text = openFileDialog1.FileName;//文件名称
				FileStream file_Read = new FileStream(openfile_name.Text, FileMode.Open);
		
				file_len = (UInt32)file_Read.Length;//获取文件长度
				byte[] file_Byte = new byte[file_len];//开辟空间存放数据
				int r = file_Read.Read(file_Byte, 0, file_Byte.Length);//读取数据
				file_Read.Close();

			}

		}

		private void button6_Click(object sender, EventArgs e)
		{

			label8.Text = "0";
			Thread thread = new Thread(Serial_send);
			thread.Start();

		}
		private void SerialFileThread_send()
		{
			


		}
		private void Serial_send()
		{
			FileStream fsRead = new FileStream(openfile_name.Text, FileMode.Open);
           long AllLen = (UInt32)fsRead.Length;
			long fsLen = AllLen;
			byte[] heByte = new byte[fsLen];
		    int r = fsRead.Read(heByte,0, heByte.Length);
			int pack_count = 0;
	         bool FLAG = true;
			while (FLAG)
			{
				if (SerialPorts.IsOpen)
				{
					Action action = delegate ()
				{


					if (fsLen >= 500)
					{

						SerialPorts.Write(heByte, pack_count * 500, 500);
						fsLen = (fsLen - 500);
						label8.Text = ((pack_count * 500)).ToString();
						pack_count++;
						
					}
					else if (fsLen < 500)
					{

						SerialPorts.Write(heByte, pack_count * 500, (int)fsLen);
						fsRead.Close();
						FLAG = false;
						label8.Text =( fsLen + (pack_count * 500)).ToString();
					}

			
				

				};
					this.Invoke(action);
				}
				
                else
					{
				fsRead.Close();
			    	FLAG =false;
				}
			}
		}

		private void parity_list_SelectedIndexChanged(object sender, EventArgs e)
		{
			SerialPorts.Parity = parityList[parity_list.SelectedIndex];
		}

	

		public Int64 Getstr_count(string str)
		{

			Int64 count = 0;
			char[] ch = str.ToCharArray();
			if (str != null)
			{
				for (int i = 0; i < ch.Length; i++)
				{
					if (isChinese(ch[i]))
					{
						count += 2;
					}
					else
						count++;
				}
			}
			return count;
		}



	}
}
