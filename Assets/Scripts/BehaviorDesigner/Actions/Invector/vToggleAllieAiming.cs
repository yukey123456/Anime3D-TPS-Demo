using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Invector/ShooterInput")]
public class vToggleAllieAiming : Action
{
	public SharedvAllieShooterInput vAllieShooterInput;
	public bool IsAim;

	public override void OnStart()
	{
		
	}

	public override TaskStatus OnUpdate()
	{
		var allieShooterInput = vAllieShooterInput.Value;
        if (IsAim && !allieShooterInput.IsAiming)
		{
            allieShooterInput.AllieStartAiming();
        }
		else if (!IsAim && allieShooterInput.IsAiming)
		{
            allieShooterInput.AllieStopAiming();
        }
        return TaskStatus.Success;
	}
}