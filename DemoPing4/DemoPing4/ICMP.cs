using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;

namespace DemoPing4
{
    class ICMP
    {
        public byte Type;
        public byte Code;
        public UInt16 CheckSum;
        public int MessageSize;
        public byte[] Message = new byte[1024];
        public ICMP()
        { }
        public ICMP(byte[] data, int size)
        {
            Type = data[20];
            Code = data[21];
            CheckSum = BitConverter.ToUInt16(data, 22);
            MessageSize = size - 24;
            Buffer.BlockCopy(data, 24, Message, 0, MessageSize);
        }
        public byte[] getBytes()
        {
            byte[] data = new byte[MessageSize + 9];
            Buffer.BlockCopy(BitConverter.GetBytes(Type), 0, data, 0, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(Code), 0, data, 1, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(CheckSum), 0, data, 2, 2);
            Buffer.BlockCopy(Message, 0, data, 4, MessageSize);
            return data;
        }
        public UInt16 getCheckSum()
        {
            UInt32 checksm = 0;
            byte[] data = getBytes();
            int PacketSize = MessageSize + 8;
            int index = 0;
            while (index < PacketSize)
            {
                checksm += Convert.ToUInt32(BitConverter.ToUInt16(data, index));
                index += 2;
            }
            checksm = (checksm >> 16) + (checksm & 0xffff);
            checksm += (checksm >> 16);
            return (UInt16)(~checksm);
        }
    }
}
