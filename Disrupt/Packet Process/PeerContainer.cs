using System.Collections.Generic;

namespace RavelTek.Disrupt
{
    public class PeerContainer
    {
        public Queue<Packet> AwaitingPackets = new Queue<Packet>();
        public Queue<Packet> RecvFragPackets = new Queue<Packet>();
        public Packet[] SentPackets = new Packet[32];
        public Packet[] RecvPackets = new Packet[32];
        private byte bit;
        //Transport Layer dont think in ACK NACK we need to use a ping to keep the nat hole open so instead of sending extra data just send the 
        //missing packets as the ping every x interval (I believe its 15 seconds before the hole closes not sure but every second a ping cant hurt)
        //math caculation on 1 second ping 1 / 32 = 0.03125 milliseconds equivalent to ack nack back and foreth many times much better performance.
        //awaitingPackets holds all of the packets that could not be sent yet 
        //recvFragPackets holds all of the fragmented data packets once the transmission completes push the packet together and event it
        //round about the sent and recv packets
        //if we have recieved packet 31 and received 0 is null slot the recieved in the bottom layer
        //ack the whole buffer in bits 0 - 31. 0s will be not recieved and 1s will be recieved
        //when a ack comes through for the client sent packets will do the same

        public void PacketUpdate(Packet packet, DisruptClient client)
        {
            for(int i = 0; i < 32; i++)
            {
                bit = 0;
                //if bit is 1 they received the packet clear it from the buffer
                client.Exchange.RecyclePacket(SentPackets[bit]);
                SentPackets[bit] = null;
                //if bit is 0 we need to resend that packet send it raw so it doesnt calculate again
                client.Exchange.SendRaw(SentPackets[bit]);
            }
        }
    }
}
