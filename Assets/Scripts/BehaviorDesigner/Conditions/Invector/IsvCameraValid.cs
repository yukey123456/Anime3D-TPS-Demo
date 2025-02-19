using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Invector/Camera")]
public class IsvCameraValid : Conditional
{
	public SharedvCameraHandler vCameraHandler;

	public override TaskStatus OnUpdate()
	{
		return vCameraHandler.Value.IsCameraValid() ? TaskStatus.Success : TaskStatus.Failure;
	}
}