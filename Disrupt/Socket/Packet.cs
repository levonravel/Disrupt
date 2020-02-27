using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;

namespace RavelTek.Disrupt
{
    [System.Serializable]
    public class Packet
    {
        public const int HeaderSize = 3;
        public static int count = 0;

        public EndPoint Address = new IPEndPoint(IPAddress.Any, 0);
        public bool SingleSend;
        public byte[] PayLoad = new byte[1256];
        public int PayloadLength;
        public int CurrentIndex = HeaderSize;
        public Client Client;
        public string lastUsage;
        public int packetCount;

        public Fragment Fragmented
        {
            get
            {
                return (PayLoad[0] & (1 << 7)) != 0 ? Fragment.Begin : Fragment.End;
            }
            set
            {
                PayLoad[0] |= (byte)(value == Fragment.Begin ? 128 : 0);
            }
        }
        public byte Id
        {
            get
            {
                return PayLoad[1];
            }
            set
            {
                PayLoad[1] = value;
            }
        }
        public bool ReliableBufferFlag
        {
            get
            {
                if (Protocol != Protocol.Reliable)
                    throw new Exception("Reliable Buffer Flag being read from non reliable packet");
                return PayLoad[2] == 1;
            }
            set
            {
                PayLoad[2] = (value ? (byte)1 : (byte)0);
            }
        }
        public Protocol Protocol
        {
            get
            {
                return (PayLoad[0] & (1 << 6)) != 0 ? Protocol.Reliable : Protocol.Sequenced;
            }
            set
            {
                PayLoad[0] |= (byte)(value == Protocol.Reliable ? 64 : 0);
            }
        }
        public Flags Flag
        {
            get
            {
                int flag = 0;
                for (byte i = 0; i < 6; i++)
                {
                    if ((PayLoad[0] & (1 << i)) != 0)
                    {
                        flag += (byte)(1 << i);
                    }
                }
                return (Flags)flag;
            }
            set
            {
                PayLoad[0] |= (byte)value;
            }
        }
        public void Reset()
        {
            PayLoad[0] = 0;
			PayLoad[1] = 0;
			PayLoad[2] = 0;
            CurrentIndex = HeaderSize;
            SingleSend = false;
        }
        public Packet Clone()
        {
            var packet = Client.CreatePacket();
            Buffer.BlockCopy(PayLoad, 0, packet.PayLoad, 0, PayLoad.Length);
            packet.CurrentIndex = CurrentIndex;
            return packet;
        }
    }
}
