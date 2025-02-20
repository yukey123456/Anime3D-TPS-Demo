using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Unity/Collider")]
public class vGetColliderCenter : Action
{
	public SharedCollider SharedCollider;
	public SharedVector3 Output;

	public override void OnStart()
	{
		
	}

	public override TaskStatus OnUpdate()
	{
		var collid = SharedCollider.Value;
		Output.SetValue(collid ? collid.bounds.center : Vector3.zero);
		return TaskStatus.Success;
	}
}