using Invector.vCharacterController;

namespace BehaviorDesigner.Runtime
{
    [System.Serializable]
    public class SharedvShooterMeleeInput : SharedVariable<vShooterMeleeInput>
    {
        public static implicit operator SharedvShooterMeleeInput(vShooterMeleeInput value) { return new SharedvShooterMeleeInput { mValue = value }; }
    }
}