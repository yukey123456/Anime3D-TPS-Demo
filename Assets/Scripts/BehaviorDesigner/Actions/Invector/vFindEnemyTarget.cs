using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Invector;

[TaskCategory("Invector/ShooterInput")]
public class vFindEnemyTarget : Action
{
	public SharedvAllieShooterInput vAllieShooterInput;
	public LayerMask TargetLayer;
	public SharedFloat DetectRadius;
	public bool FailureIfNullTarget;
	public SharedCollider OutputTarget;

	public override void OnStart()
	{
		
	}

	public override TaskStatus OnUpdate()
	{
		OutputTarget.Value = null;

		var originPoint = vAllieShooterInput.Value.transform.position;
		var colliders = Physics.OverlapSphere(originPoint, DetectRadius.Value, TargetLayer);

        System.Array.Sort(colliders, (a, b) =>
        {
            float distA = Vector3.SqrMagnitude(a.transform.position - originPoint);
            float distB = Vector3.SqrMagnitude(b.transform.position - originPoint);
            return distA.CompareTo(distB);
        });

        for (int i = 0, count = colliders.Length; i < count; i++)
        {
			var collid = colliders[i];
			if (!collid.TryGetComponent(out vIHealthController vIHeath) || vIHeath.isDead)
				continue;

			OutputTarget.SetValue(collid);
			break;
        }

        return !OutputTarget.Value && FailureIfNullTarget ? TaskStatus.Failure : TaskStatus.Success;
	}
}