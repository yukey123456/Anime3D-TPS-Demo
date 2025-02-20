using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Invector/ShooterInput")]
public class IsvControlByPlayer : Conditional
{
    public SharedvAllieShooterInput vAllieShooterInput;

    public override TaskStatus OnUpdate()
	{
		return vAllieShooterInput.Value.IsControlledByPlayer ? TaskStatus.Success : TaskStatus.Failure;
	}

}