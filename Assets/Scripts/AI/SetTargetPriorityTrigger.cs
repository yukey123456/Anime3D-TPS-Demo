using Invector;
using Invector.vCharacterController.AI;
using UnityEngine;

public class SetTargetPriorityTrigger : MonoBehaviour
{
    [SerializeField]
    private LayerMask _targetLayers;

    [SerializeField]
    private vTagMask _tagMask;

    private void OnTriggerEnter(Collider other)
    {
        if (_targetLayers.ContainsLayer(other.gameObject.layer)
            && _tagMask.Contains(other.gameObject.tag))
        {
            if (other.TryGetComponent(out vIControlAI controlAI))
            {
                controlAI.SetCurrentTarget(transform);
            }
        }
    }
}
