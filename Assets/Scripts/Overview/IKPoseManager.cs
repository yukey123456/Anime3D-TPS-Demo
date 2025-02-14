using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKPoseManager : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;

    [SerializeField] 
    private float _lookWeight;

    [SerializeField]
    private float _bodyWeight;

    [SerializeField]
    private bool _ikActive;

    [SerializeField]
    private Transform _target;

    private void OnAnimatorIK()
    {
        if (_animator == null)
            return;

        if (_ikActive && _target)
        {
            Debug.LogError(_target.position);
            _animator.SetLookAtWeight(_lookWeight, _bodyWeight);
            _animator.SetLookAtPosition(_target.position);
        }
        else
        {
            _animator.SetLookAtWeight(0f);
        }
    }
}
