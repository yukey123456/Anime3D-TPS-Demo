using Invector.vCharacterController;

namespace BehaviorDesigner.Runtime
{
    [System.Serializable]
    public class SharedvAllieShooterInput : SharedVariable<vAllieShooterInput>
    {
        public static implicit operator SharedvAllieShooterInput(vAllieShooterInput value) { return new SharedvAllieShooterInput { mValue = value }; }
    }
}