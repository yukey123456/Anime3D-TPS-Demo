using Invector;
using Invector.vCharacterController;
using Invector.vEventSystems;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class OnReceiveAttack : UnityEvent<vDamage, vIMeleeFighter, bool>
{
}

public class DamageReceiverContainer : MonoBehaviour, IAttackReceiverContainer, vIAttackReceiver
{
    [SerializeField]
    private GameObject[] _targets;

    [SerializeField] 
    private OnReceiveDamage _onStartReceiveDamage = new OnReceiveDamage();

    [SerializeField] 
    private OnReceiveDamage _onReceiveDamage = new OnReceiveDamage();

    [SerializeField]
    private OnReceiveAttack _onReceiveAttack = new OnReceiveAttack();

    private vIHealthController _healthController;

    private List<vDamageReceiver> _damageReceivers = new List<vDamageReceiver>();

    public OnReceiveDamage onStartReceiveDamage 
    { 
        get => _onStartReceiveDamage; 
        protected set { _onStartReceiveDamage = value; }
    }

    public OnReceiveDamage onReceiveDamage 
    { 
        get => _onReceiveDamage; 
        protected set { _onReceiveDamage = value; } 
    }

    public OnReceiveAttack onReceiveAttack 
    { 
        get => _onReceiveAttack;
        protected set { _onReceiveAttack = value; }
    }

    private void Awake()
    {
        foreach (var attackReceiver in _targets)
        {
            if (attackReceiver.TryGetComponent(out vDamageReceiver damageReceiver))
            {
                damageReceiver.targetReceiver = gameObject;
                _damageReceivers.Add(damageReceiver);
            }

            if (attackReceiver.TryGetComponent(out ICompositeAttackReceiver compositeAttackReceiver))
            {
                compositeAttackReceiver.Owner = this;
            }
        }
    }

    public bool CanReceiveAttack(AttackValidationData data)
    {
        return !data.ignoreTags.Contains(gameObject.tag);
    }

    public void OnReceiveAttack(vDamage damage, vIMeleeFighter attacker)
    {
        if (_healthController == null)
            _healthController = GetComponentInParent<vIHealthController>();

        bool isDead = false;

        if (_healthController != null)
        {
            isDead = _healthController.isDead;

            onStartReceiveDamage.Invoke(damage);
            _healthController.TakeDamage(damage);
            onReceiveDamage.Invoke(damage);

            if (!isDead && _healthController.isDead)
            {
                isDead = true;
            }
        }

        onReceiveAttack.Invoke(damage, attacker, isDead);
    }

    [ContextMenu(nameof(FindAllAttackReceivers))]
    private void FindAllAttackReceivers()
    {
        var attackReceivers = new List<GameObject>();
        FindAttackReceiversInChildren(transform, attackReceivers);
        _targets = attackReceivers.ToArray();
    }

    private void FindAttackReceiversInChildren(Transform transform, List<GameObject> attackReceivers)
    {
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out vDamageReceiver attackReceiver))
            {
                attackReceivers.Add(child.gameObject);
            }

            FindAttackReceiversInChildren(child, attackReceivers);
        }
    }
}
