using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Invector/ShooterInput")]
public class vAllieShoot : Action
{
	public SharedvAllieShooterInput vAllieShooterInput;
	public SharedVector3 ShootPostion;
	public bool Shoot;

	public override void OnStart()
	{
		
	}

	public override TaskStatus OnUpdate()
	{
		var vShooterInput = vAllieShooterInput.Value;
        if (Shoot)
		{
            vShooterInput.AllieShoot(ShootPostion.Value);
        }
        else
		{
            vShooterInput.AllieStopShooting();
		}
		return TaskStatus.Success;
	}
}