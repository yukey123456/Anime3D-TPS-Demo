using Invector.vCharacterController;

namespace BehaviorDesigner.Runtime
{
    [System.Serializable]
    public class SharedvCameraHandler : SharedVariable<vCameraHandler>
    {
        public static implicit operator SharedvCameraHandler(vCameraHandler value) { return new SharedvCameraHandler { mValue = value }; }
    }
}