using UnityEngine;

namespace RavelTek.Disrupt {
  public class OrientationView : NetHelper {
    public enum ControlTypes {
      Instantiator = 0,
      Host = 1,
    }
    public SendTo sendType;
    public ControlTypes ControlledBy;
    private Vector3 priorPosition;
    private Quaternion priorRotation;
    private byte bits;

    public void Start() {
      priorPosition = transform.position;
      priorRotation = transform.rotation;
      AddComponentToNetLoop();
    }
    private bool CheckOwnership() {
      switch (ControlledBy) {
        case ControlTypes.Instantiator:
          if (View.Ownership == OwnerType.SharedView) {
            Debug.LogError($"Shared view {View.name} has an orientation view controlled by Instantiator in {gameObject.name}. This is invalid, because Shared Views have no Instantiator");
            return false;
          }
          return true;
        case ControlTypes.Host:
          return Disrupt.IsHost;
        default:
          Debug.LogError($"You should never share a orientation view,please change {gameObject.name} to instantiator or host.");
          return false;
      }
    }
    public override bool ShouldLoop {
      get {
        if (!View.IsMine || !CheckOwnership())
          return false;
        return priorPosition != transform.position || priorRotation != transform.rotation;
      }
    }
    public override void NetworkingLoop() {
      bits = 0;
      bits |= (byte)(transform.position.x == priorPosition.x ? 0 : 1);
      bits |= (byte)(transform.position.y == priorPosition.y ? 0 : 2);
      bits |= (byte)(transform.position.z == priorPosition.z ? 0 : 4);
      switch (bits) {
        case 0:
          break;
        case 1:
          Sync("SetPositionX").Add(transform.position.x).Send(sendType);
          break;
        case 2:
          Sync("SetPositionY").Add(transform.position.y).Send(sendType);
          break;
        case 3:
          Sync("SetPositionXY").Add(transform.position.x).Add(transform.position.y).Send(sendType);
          break;
        case 4:
          Sync("SetPositionZ").Add(transform.position.z).Send(sendType);
          break;
        case 5:
          Sync("SetPositionXZ").Add(transform.position.x).Add(transform.position.z).Send(sendType);
          break;
        case 6:
          Sync("SetPositionYZ").Add(transform.position.y).Add(transform.position.z).Send(sendType);
          break;
        case 7:
          Sync("SetPositionXYZ").Add(transform.position.x).Add(transform.position.y).Add(transform.position.z).Send(sendType);
          break;

      }
      bits = 0;
      bits |= (byte)(transform.eulerAngles.x == priorRotation.x ? 0 : 1);
      bits |= (byte)(transform.eulerAngles.y == priorRotation.y ? 0 : 2);
      bits |= (byte)(transform.eulerAngles.z == priorRotation.z ? 0 : 4);
      bits |= (byte)(transform.eulerAngles.z == priorRotation.w ? 0 : 8);
      switch (bits) {
        case 0:
          break;
        case 1:
          Sync("SetRotationX").Add((sbyte)(transform.rotation.x * 100)).Send(sendType);
          break;
        case 2:
          Sync("SetRotationY").Add((sbyte)(transform.rotation.y * 100)).Send(sendType);
          break;
        case 3:
          Sync("SetRotationXY").Add((sbyte)(transform.rotation.x * 100)).Add((sbyte)(transform.rotation.y * 100)).Send(sendType);
          break;
        case 4:
          Sync("SetRotationZ").Add((sbyte)(transform.rotation.z * 100)).Send(sendType);
          break;
        case 5:
          Sync("SetRotationXZ").Add((sbyte)(transform.rotation.x * 100)).Add((sbyte)(transform.rotation.z * 100)).Send(sendType);
          break;
        case 6:
          Sync("SetRotationYZ").Add((sbyte)(transform.rotation.y * 100)).Add((sbyte)(transform.rotation.z * 100)).Send(sendType);
          break;
        case 7:
          Sync("SetRotationXYZ").Add((sbyte)(transform.rotation.x * 100)).Add((sbyte)(transform.rotation.y * 100)).Add((sbyte)(transform.rotation.z * 100)).Send(sendType);
          break;
        case 8:
          Sync("SetRotationW").Add((sbyte)((transform.rotation.w * 100))).Send(sendType);
          break;
        case 9:
          Sync("SetRotationXW").Add((sbyte)(transform.rotation.x * 100)).Add((sbyte)(transform.rotation.w * 100)).Send(sendType);
          break;
        case 10:
          Sync("SetRotationYW").Add((sbyte)(transform.rotation.y * 100)).Add((sbyte)(transform.rotation.w * 100)).Send(sendType);
          break;
        case 11:
          Sync("SetRotationXYW").Add((sbyte)(transform.rotation.x * 100)).Add((sbyte)(transform.rotation.y * 100)).Add((sbyte)(transform.rotation.w * 100)).Send(sendType);
          break;
        case 12:
          Sync("SetRotationZW").Add((sbyte)(transform.rotation.z * 100)).Add((sbyte)(transform.rotation.w * 100)).Send(sendType);
          break;
        case 13:
          Sync("SetRotationXZW").Add((sbyte)(transform.rotation.x * 100)).Add((sbyte)(transform.rotation.z * 100)).Add((sbyte)(transform.rotation.w * 100)).Send(sendType);
          break;
        case 14:
          Sync("SetRotationYZW").Add((sbyte)(transform.rotation.y * 100)).Add((sbyte)(transform.rotation.z * 100)).Add((sbyte)(transform.rotation.w * 100)).Send(sendType);
          break;
        case 15:
          Sync("SetRotationXYZW").Add((sbyte)(transform.rotation.x * 100)).Add((sbyte)(transform.rotation.y * 100)).Add((sbyte)(transform.rotation.z * 100)).Add((sbyte)(transform.rotation.w * 100)).Send(sendType);
          break;
      }
      priorPosition = transform.position;
      priorRotation = transform.rotation;
    }

    //Optimized Orientation Deliveries
    [RD(Protocol = Protocol.Sequenced)]
    private void SetPositionX(float value) {
      priorPosition.x = value;
      transform.position = priorPosition;
    }
    [RD(Protocol = Protocol.Sequenced)]
    private void SetPositionY(float value) {
      priorPosition.y = value;
      transform.position = priorPosition;
    }
    [RD(Protocol = Protocol.Sequenced)]
    private void SetPositionZ(float value) {
      priorPosition.z = value;
      transform.position = priorPosition;
    }
    [RD(Protocol = Protocol.Sequenced)]
    private void SetPositionXY(float value, float value2) {
      priorPosition.x = value;
      priorPosition.y = value2;
      transform.position = priorPosition;

    }
    [RD(Protocol = Protocol.Sequenced)]
    private void SetPositionXZ(float value, float value2) {
      priorPosition.x = value;
      priorPosition.z = value2;
      transform.position = priorPosition;
    }
    [RD(Protocol = Protocol.Sequenced)]
    private void SetPositionYZ(float value, float value2) {
      priorPosition.y = value;
      priorPosition.z = value2;
      transform.position = priorPosition;
    }
    [RD(Protocol = Protocol.Sequenced)]
    private void SetPositionXYZ(float value, float value2, float value3) {
      priorPosition.x = value;
      priorPosition.y = value2;
      priorPosition.z = value3;
      transform.position = priorPosition;
    }

    [RD(Protocol = Protocol.Sequenced)]
    private void SetRotationX(sbyte value) {
      priorRotation.x = value * .01F;
      transform.rotation = priorRotation;
    }
    [RD(Protocol = Protocol.Sequenced)]
    private void SetRotationY(sbyte value) {
      priorRotation.y = value * .01F;
      transform.rotation = priorRotation;
    }
    [RD(Protocol = Protocol.Sequenced)]
    private void SetRotationZ(sbyte value) {
      priorRotation.z = value * .01F;
      transform.rotation = priorRotation;
    }
    [RD(Protocol = Protocol.Sequenced)]
    private void SetRotationW(sbyte value) {
      priorRotation.w = value * .01F;
      transform.rotation = priorRotation;
    }
    [RD(Protocol = Protocol.Sequenced)]
    private void SetRotationXY(sbyte value, sbyte value2) {
      priorRotation.x = value * .01F;
      priorRotation.y = value2 * .01F;
      transform.rotation = priorRotation;
    }
    [RD(Protocol = Protocol.Sequenced)]
    private void SetRotationXZ(sbyte value, sbyte value2) {
      priorRotation.x = value * .01F;
      priorRotation.z = value2 * .01F;
      transform.rotation = priorRotation;
    }
    [RD(Protocol = Protocol.Sequenced)]
    private void SetRotationXW(sbyte value, sbyte value2) {
      priorRotation.x = value * .01F;
      priorRotation.w = value2 * .01F;
      transform.rotation = priorRotation;
    }
    [RD(Protocol = Protocol.Sequenced)]
    private void SetRotationYZ(sbyte value, sbyte value2) {
      priorRotation.y = value * .01F;
      priorRotation.z = value2 * .01F;
      transform.rotation = priorRotation;
    }
    [RD(Protocol = Protocol.Sequenced)]
    private void SetRotationYW(sbyte value, sbyte value2) {
      priorRotation.y = value * .01F;
      priorRotation.w = value2 * .01F;
      transform.rotation = priorRotation;
    }
    [RD(Protocol = Protocol.Sequenced)]
    private void SetRotationZW(sbyte value, sbyte value2) {
      priorRotation.z = value * .01F;
      priorRotation.w = value2 * .01F;
      transform.rotation = priorRotation;
    }
    [RD(Protocol = Protocol.Sequenced)]
    private void SetRotationXYZ(sbyte value, sbyte value2, sbyte value3) {
      priorRotation.x = value * .01F;
      priorRotation.y = value2 * .01F;
      priorRotation.z = value3 * .01F;
      transform.rotation = priorRotation;
    }
    [RD(Protocol = Protocol.Sequenced)]
    private void SetRotationXYW(sbyte value, sbyte value2, sbyte value3) {
      priorRotation.x = value * .01F;
      priorRotation.y = value2 * .01F;
      priorRotation.w = value3 * .01F;
      transform.rotation = priorRotation;
    }
    [RD(Protocol = Protocol.Sequenced)]
    private void SetRotationXZW(sbyte value, sbyte value2, sbyte value3) {
      priorRotation.x = value * .01F;
      priorRotation.z = value2 * .01F;
      priorRotation.w = value3 * .01F;
      transform.rotation = priorRotation;
    }
    [RD(Protocol = Protocol.Sequenced)]
    private void SetRotationYZW(sbyte value, sbyte value2, sbyte value3) {
      priorRotation.y = value * .01F;
      priorRotation.z = value2 * .01F;
      priorRotation.w = value3 * .01F;
      transform.rotation = priorRotation;
    }
    [RD(Protocol = Protocol.Sequenced)]
    private void SetRotationXYZW(sbyte value, sbyte value2, sbyte value3, sbyte value4) {
      priorRotation.x = value * .01F;
      priorRotation.y = value2 * .01F;
      priorRotation.z = value3 * .01F;
      priorRotation.w = value4 * .01F;
      transform.rotation = priorRotation;
    }
  }
}
