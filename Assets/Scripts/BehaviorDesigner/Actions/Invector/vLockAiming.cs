using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Invector/ShooterInput")]
public class vLockAiming : Action
{
	public SharedvShooterMeleeInput vShooterInput;
	public bool IsAim;

	public override void OnStart()
	{
		
	}

	public override TaskStatus OnUpdate()
	{
		vShooterInput.Value.SetIsAiming = true;
		return TaskStatus.Success;
	}
}