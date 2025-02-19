using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Invector/ShooterInput")]
public class vLockAllieAiming : Action
{
	public SharedvAllieShooterInput vAllieShooterInput;
	public bool IsAim;

	public override void OnStart()
	{
		
	}

	public override TaskStatus OnUpdate()
	{
        vAllieShooterInput.Value.IsAllieAiming = IsAim;
		return TaskStatus.Success;
	}
}