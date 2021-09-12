using UnityEngine;
using RavelTek.Disrupt;

public class OrientationView : NetHelper
{
    private Quaternion rotation = Quaternion.identity;
    private Vector3 position = Vector3.zero;

    public void Awake()
    {
        AddMethods(Rotation, Position);
    }
    public void LateUpdate()
    {
        if(transform.rotation != rotation)
        {
            rotation = transform.rotation;
            var packet = CreatePacket();
            Writer.Open(packet)
            .Add(rotation.x)
            .Add(rotation.y)
            .Add(rotation.z)
            .Add(rotation.w);
        }
        if(transform.position != position)
        {
            position = transform.position;
            var packet = CreatePacket();
            Writer.Open(packet)
            .Add(position.x)
            .Add(position.y)
            .Add(position.z);
        }
    }
    public void Rotation(Packet packet)
    {
        rotation.x = Reader.PullFloat(packet);
        rotation.y = Reader.PullFloat(packet);
        rotation.z = Reader.PullFloat(packet);
        rotation.w = Reader.PullFloat(packet);
    }
    public void Position(Packet packet)
    {
        position.x = Reader.PullFloat(packet);
        position.y = Reader.PullFloat(packet);
        position.z = Reader.PullFloat(packet);
    }
}
