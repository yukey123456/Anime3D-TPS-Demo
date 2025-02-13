using System.Collections.Generic;
using UnityEngine;

public class CompositeAttackReceiver : MonoBehaviour, ICompositeAttackReceiver
{
    [SerializeField]
    private bool _warningWhenOwnerNotFound;

    public IAttackReceiverContainer Owner { private get; set; }

    public bool CanReceiveAttack(AttackValidationData data)
    {
        if (Owner == null)
        {
            if (_warningWhenOwnerNotFound)
            {
                Debug.LogWarningFormat("Cannot be found owner of receiver attack {0}", name);
            }
            
            return true;
        }

        return Owner.CanReceiveAttack(data);
    }
}

public interface IAttackReceiverContainer
{
    GameObject gameObject { get; }

    bool CanReceiveAttack(AttackValidationData data);
}

public interface ICompositeAttackReceiver
{
    IAttackReceiverContainer Owner { set; }

    bool CanReceiveAttack(AttackValidationData data);
}

public struct AttackValidationData
{
    public List<string> ignoreTags;
}