using UnityEngine;

namespace RavelTek.Disrupt
{
    public class OrientationView : NetHelper
    {
        public Enums.SendType SendType;
        public Protocol TransportLayer;
        private Vector3 priorPosition;
        private Quaternion priorRotation;
        public bool ShouldLoop;
        private byte bits;
        private Packet packet;

        public void Awake()
        {
            if(ShouldLoop)
                AddMethodToTicker(OrientationCheck);

            AddMethods(
                SetRotationW,
                SetRotationX,
                SetRotationXW,
                SetRotationXY,
                SetRotationXYW,
                SetRotationXYZ,
                SetRotationXYZW,
                SetRotationXZ,
                SetRotationXZW,
                SetRotationY,
                SetRotationYW,
                SetRotationYZ,
                SetRotationYZW,
                SetRotationZ,
                SetRotationZW,
                SetPositionX,
                SetPositionXY,
                SetPositionXYZ,
                SetPositionXZ,
                SetPositionY,
                SetPositionYZ,
                SetPositionZ
                );
            priorPosition = transform.position;
            priorRotation = transform.rotation;
        }
        public void OrientationCheck()
        {
            packet = CreatePacket();
            bits = 0;
            bits |= (byte)(transform.position.x == priorPosition.x ? 0 : 1);
            bits |= (byte)(transform.position.y == priorPosition.y ? 0 : 2);
            bits |= (byte)(transform.position.z == priorPosition.z ? 0 : 4);
            switch (bits)
            {
                case 0:
                    break;
                case 1:
                    Writer.Open(packet)
                        .Add(transform.position.x);
                    Send(nameof(SetPositionX), SendType, packet, TransportLayer);
                    break;
                case 2:
                    Writer.Open(packet).Add(transform.position.y);
                    Send(nameof(SetPositionY), SendType, packet, TransportLayer);
                    break;
                case 3:
                    Writer.Open(packet).Add(transform.position.x).Add(transform.position.y);
                    Send(nameof(SetPositionXY), SendType, packet, TransportLayer);
                    break;
                case 4:
                    Writer.Open(packet).Add(transform.position.z);
                    Send(nameof(SetPositionZ), SendType, packet, TransportLayer);
                    break;
                case 5:
                    Writer.Open(packet).Add(transform.position.x).Add(transform.position.z);
                    Send(nameof(SetPositionXZ), SendType, packet, TransportLayer);
                    break;
                case 6:
                    Writer.Open(packet).Add(transform.position.y).Add(transform.position.z);
                    Send(nameof(SetPositionYZ), SendType, packet, TransportLayer);
                    break;
                case 7:
                    Writer.Open(packet).Add(transform.position.x).Add(transform.position.y).Add(transform.position.z);
                    Send(nameof(SetPositionXYZ), SendType, packet, TransportLayer);
                    break;

            }
            bits = 0;
            bits |= (byte)(transform.eulerAngles.x == priorRotation.x ? 0 : 1);
            bits |= (byte)(transform.eulerAngles.y == priorRotation.y ? 0 : 2);
            bits |= (byte)(transform.eulerAngles.z == priorRotation.z ? 0 : 4);
            bits |= (byte)(transform.eulerAngles.z == priorRotation.w ? 0 : 8);
            switch (bits)
            {
                case 0:
                    break;
                case 1:
                    Writer.Open(packet).Add((sbyte)(transform.rotation.x * 100));
                    Send(nameof(SetRotationX), SendType, packet, TransportLayer);
                    break;
                case 2:
                    Writer.Open(packet).Add((sbyte)(transform.rotation.y * 100));
                    Send(nameof(SetRotationY), SendType, packet, TransportLayer);
                    break;
                case 3:
                    Writer.Open(packet).Add((sbyte)(transform.rotation.x * 100)).Add((sbyte)(transform.rotation.y * 100));
                    Send(nameof(SetRotationXY), SendType, packet, TransportLayer);
                    break;
                case 4:
                    Writer.Open(packet).Add((sbyte)(transform.rotation.z * 100));
                    Send(nameof(SetRotationZ), SendType, packet, TransportLayer);
                    break;
                case 5:
                    Writer.Open(packet).Add((sbyte)(transform.rotation.x * 100)).Add((sbyte)(transform.rotation.z * 100));
                    Send(nameof(SetRotationXZ), SendType, packet, TransportLayer);
                    break;
                case 6:
                    Writer.Open(packet).Add((sbyte)(transform.rotation.y * 100)).Add((sbyte)(transform.rotation.z * 100));
                    Send(nameof(SetRotationYZ), SendType, packet, TransportLayer);
                    break;
                case 7:
                    Writer.Open(packet).Add((sbyte)(transform.rotation.x * 100)).Add((sbyte)(transform.rotation.y * 100)).Add((sbyte)(transform.rotation.z * 100));
                    Send(nameof(SetRotationXYZ), SendType, packet, TransportLayer);
                    break;
                case 8:
                    Writer.Open(packet).Add((sbyte)((transform.rotation.w * 100)));
                    Send(nameof(SetRotationW), SendType, packet, TransportLayer);
                    break;
                case 9:
                    Writer.Open(packet).Add((sbyte)(transform.rotation.x * 100)).Add((sbyte)(transform.rotation.w * 100));
                    Send(nameof(SetRotationXW), SendType, packet, TransportLayer);
                    break;
                case 10:
                    Writer.Open(packet).Add((sbyte)(transform.rotation.y * 100)).Add((sbyte)(transform.rotation.w * 100));
                    Send(nameof(SetRotationYW), SendType, packet, TransportLayer);
                    break;
                case 11:
                    Writer.Open(packet).Add((sbyte)(transform.rotation.x * 100)).Add((sbyte)(transform.rotation.y * 100)).Add((sbyte)(transform.rotation.w * 100));
                    Send(nameof(SetRotationXYW), SendType, packet, TransportLayer);
                    break;
                case 12:
                    Writer.Open(packet).Add((sbyte)(transform.rotation.z * 100)).Add((sbyte)(transform.rotation.w * 100));
                    Send(nameof(SetRotationZW), SendType, packet, TransportLayer);
                    break;
                case 13:
                    Writer.Open(packet).Add((sbyte)(transform.rotation.x * 100)).Add((sbyte)(transform.rotation.z * 100)).Add((sbyte)(transform.rotation.w * 100));
                    Send(nameof(SetRotationXZW), SendType, packet, TransportLayer);
                    break;
                case 14:
                    Writer.Open(packet).Add((sbyte)(transform.rotation.y * 100)).Add((sbyte)(transform.rotation.z * 100)).Add((sbyte)(transform.rotation.w * 100));
                    Send(nameof(SetRotationYZW), SendType, packet, TransportLayer);
                    break;
                case 15:
                    Writer.Open(packet).Add((sbyte)(transform.rotation.x * 100)).Add((sbyte)(transform.rotation.y * 100)).Add((sbyte)(transform.rotation.z * 100)).Add((sbyte)(transform.rotation.w * 100));
                    Send(nameof(SetRotationXYZW), SendType, packet, TransportLayer);
                    break;
            }
            priorPosition = transform.position;
            priorRotation = transform.rotation;
        }

        private void SetPositionX(Packet packet)
        {
            priorPosition.x = Reader.PullFloat(packet);
            transform.position = priorPosition;
        }

        private void SetPositionY(Packet packet)
        {
            priorPosition.y = Reader.PullFloat(packet);
            transform.position = priorPosition;
        }

        private void SetPositionZ(Packet packet)
        {
            priorPosition.z = Reader.PullFloat(packet);
            transform.position = priorPosition;
        }

        private void SetPositionXY(Packet packet)
        {
            priorPosition.x = Reader.PullFloat(packet);
            priorPosition.y = Reader.PullFloat(packet);
            transform.position = priorPosition;

        }

        private void SetPositionXZ(Packet packet)
        {
            priorPosition.x = Reader.PullFloat(packet);
            priorPosition.z = Reader.PullFloat(packet);
            transform.position = priorPosition;
        }

        private void SetPositionYZ(Packet packet)
        {
            priorPosition.y = Reader.PullFloat(packet);
            priorPosition.z = Reader.PullFloat(packet);
            transform.position = priorPosition;
        }

        private void SetPositionXYZ(Packet packet)
        {
            priorPosition.x = Reader.PullFloat(packet);
            priorPosition.y = Reader.PullFloat(packet);
            priorPosition.z = Reader.PullFloat(packet);
            transform.position = priorPosition;
        }


        private void SetRotationX(Packet packet)
        {
            priorRotation.x = Reader.PullSByte(packet) * .01F;
            transform.rotation = priorRotation;
        }

        private void SetRotationY(Packet packet)
        {
            priorRotation.y = Reader.PullSByte(packet) * .01F;
            transform.rotation = priorRotation;
        }

        private void SetRotationZ(Packet packet)
        {
            priorRotation.z = Reader.PullSByte(packet) * .01F;
            transform.rotation = priorRotation;
        }

        private void SetRotationW(Packet packet)
        {
            priorRotation.w = Reader.PullSByte(packet) * .01F;
            transform.rotation = priorRotation;
        }

        private void SetRotationXY(Packet packet)
        {
            priorRotation.x = Reader.PullSByte(packet) * .01F;
            priorRotation.y = Reader.PullSByte(packet) * .01F;
            transform.rotation = priorRotation;
        }

        private void SetRotationXZ(Packet packet)
        {
            priorRotation.x = Reader.PullSByte(packet) * .01F;
            priorRotation.z = Reader.PullSByte(packet) * .01F;
            transform.rotation = priorRotation;
        }

        private void SetRotationXW(Packet packet)
        {
            priorRotation.x = Reader.PullSByte(packet) * .01F;
            priorRotation.w = Reader.PullSByte(packet) * .01F;
            transform.rotation = priorRotation;
        }

        private void SetRotationYZ(Packet packet)
        {
            priorRotation.y = Reader.PullSByte(packet) * .01F;
            priorRotation.z = Reader.PullSByte(packet) * .01F;
            transform.rotation = priorRotation;
        }

        private void SetRotationYW(Packet packet)
        {
            priorRotation.y = Reader.PullSByte(packet) * .01F;
            priorRotation.w = Reader.PullSByte(packet) * .01F;
            transform.rotation = priorRotation;
        }

        private void SetRotationZW(Packet packet)
        {
            priorRotation.z = Reader.PullSByte(packet) * .01F;
            priorRotation.w = Reader.PullSByte(packet) * .01F;
            transform.rotation = priorRotation;
        }

        private void SetRotationXYZ(Packet packet)
        {
            priorRotation.x = Reader.PullSByte(packet) * .01F;
            priorRotation.y = Reader.PullSByte(packet) * .01F;
            priorRotation.z = Reader.PullSByte(packet) * .01F;
            transform.rotation = priorRotation;
        }

        private void SetRotationXYW(Packet packet)
        {
            priorRotation.x = Reader.PullSByte(packet) * .01F;
            priorRotation.y = Reader.PullSByte(packet) * .01F;
            priorRotation.w = Reader.PullSByte(packet) * .01F;
            transform.rotation = priorRotation;
        }

        private void SetRotationXZW(Packet packet)
        {
            priorRotation.x = Reader.PullSByte(packet) * .01F;
            priorRotation.z = Reader.PullSByte(packet) * .01F;
            priorRotation.w = Reader.PullSByte(packet) * .01F;
            transform.rotation = priorRotation;
        }

        private void SetRotationYZW(Packet packet)
        {
            priorRotation.y = Reader.PullSByte(packet) * .01F;
            priorRotation.z = Reader.PullSByte(packet) * .01F;
            priorRotation.w = Reader.PullSByte(packet) * .01F;
            transform.rotation = priorRotation;
        }

        private void SetRotationXYZW(Packet packet)
        {
            priorRotation.x = Reader.PullSByte(packet) * .01F;
            priorRotation.y = Reader.PullSByte(packet) * .01F;
            priorRotation.z = Reader.PullSByte(packet) * .01F;
            priorRotation.w = Reader.PullSByte(packet) * .01F;
            transform.rotation = priorRotation;
        }
    }
}
