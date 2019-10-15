using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DemoPing4
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
           
        }
        
        public DataTable dt = new DataTable();
        
        //Socket host = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
        
        private void Form1_Load(object sender, EventArgs e)
        {
            dt.Columns.Add("IP", typeof(string));
            dt.Columns.Add("RespondFrom", typeof(string));
            dt.Columns.Add("Type", typeof(string));
            dt.Columns.Add("Code", typeof(string));
            dt.Columns.Add("Identifer", typeof(string));
            dt.Columns.Add("Sequence", typeof(string));
            dt.Columns.Add("Data", typeof(string));
        }
        List<string> ListIP=new List<string>(20);
        private void BtnPing_Click(object sender, EventArgs e)
        {
            // Lấy danh sách địa chỉ 
            
           
            for (int i = 0; i<richTextBox2.Lines.Length;i++)
            {
                ListIP.Add ( richTextBox2.Lines[i]);
                DataRow row;
                row = dt.NewRow();
                row["IP"] = richTextBox2.Lines[i];
                dt.Rows.Add(row);
                
                
            }
            dataGridView1.DataSource = dt;
            dataGridView1.Refresh();
            
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Thread PingThread = new Thread(ping);
                PingThread.Start(i);
            }
        }
        public void ping(object No)
        {
            
            byte[] data = new byte[1024];
            int recv;
            int stt = Convert.ToInt32(No);
            string IP = dt.Rows[stt].ItemArray[0].ToString();
            IPEndPoint iep = null;
           
            iep = new IPEndPoint(IPAddress.Parse(IP), 0);
            EndPoint ep = (EndPoint)iep;
            ICMP packet = new ICMP();
            packet.Type = 0x08;
            packet.Code = 0x00;
            packet.CheckSum = 0;
            Buffer.BlockCopy(BitConverter.GetBytes((short)1), 0, packet.Message, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((short)1), 0, packet.Message, 2, 2);
            data = Encoding.ASCII.GetBytes("test Packet");
            Buffer.BlockCopy(data, 0, packet.Message, 4, data.Length);
            packet.MessageSize = data.Length + 4;
            int PacketSize = packet.MessageSize + 4;
            UInt16 checksm = packet.getCheckSum();
            packet.CheckSum = checksm;
            Socket host = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
            host.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 3000);
            host.SendTo(packet.getBytes(), PacketSize, SocketFlags.None, iep);
            try
            {
                data = new byte[1024];
                recv = host.ReceiveFrom(data, ref ep);

            }
            catch (SocketException)
            {
                Console.WriteLine("No repsonse from remote host");
                dataGridView1.Invoke(new Action(() => dataGridView1.Rows[stt].Cells[6].Value = "No repsonse from remote host"));
                    //dataGridView1.Rows[stt].Cells[6].Value = "No repsonse from remote host";
                return;
            }
            ICMP Response = new ICMP(data, recv);
           
            int Identifier = BitConverter.ToInt16(Response.Message, 0);
            int Sequence = BitConverter.ToInt16(Response.Message, 2);
            
            string stringData = Encoding.ASCII.GetString(Response.Message, 4, Response.MessageSize - 4);

            dataGridView1.Invoke(new Action(() => {
                dataGridView1.Rows[stt].Cells[1].Value = ep.ToString();
                dataGridView1.Rows[stt].Cells[2].Value = Response.Type;
                dataGridView1.Rows[stt].Cells[3].Value = Response.Code;
                dataGridView1.Rows[stt].Cells[4].Value = Identifier;
                dataGridView1.Rows[stt].Cells[5].Value = Sequence;
                dataGridView1.Rows[stt].Cells[6].Value = stringData;
            }));
            //dataGridView1.Rows[stt].Cells[1].Value = ep.ToString();
            //dataGridView1.Rows[stt].Cells[2].Value = Response.Type;
            //dataGridView1.Rows[stt].Cells[3].Value = Response.Code;
            //dataGridView1.Rows[stt].Cells[4].Value = Identifier;
            //dataGridView1.Rows[stt].Cells[5].Value = Sequence;
            //dataGridView1.Rows[stt].Cells[6].Value = stringData;

            host.Close();
            Thread.Sleep(1500);
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            MessageBox.Show(dataGridView1.Rows.Count.ToString());
            dataGridView1.DataSource = null;
            richTextBox2.ResetText();
            for(int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                dt.Rows.RemoveAt(i);
            }
        }
    }
}
