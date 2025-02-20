using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Invector;

[TaskCategory("Invector/ShooterInput")]
public class IsvHealthControllerAlive : Conditional
{
	public SharedCollider TargetCollider;

	public override TaskStatus OnUpdate()
	{
		var collid = TargetCollider.Value;

		if (!collid || !collid.TryGetComponent(out vIHealthController iHeath) || iHeath.isDead)
		{
			TargetCollider.Value = null;
            return TaskStatus.Failure;
        }

        return TaskStatus.Success;
	}
}