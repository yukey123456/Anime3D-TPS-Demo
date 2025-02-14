using Kalagaan;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateModeConverter : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    [SerializeField]
    private AnimatorUpdateMode animatorUpdateMode;

    [SerializeField]
    private Kalagaan.UpdateMode vertExUpdateMode;

    [SerializeField]
    private DynamicBone.UpdateMode dynamicBoneUpdateMode;

    private void Start()
    {
        CovertAnimatorUpdateMode();
        CovertVertExmotionUpdateMode();
        CovertDynamicBoneUpdateMode();
    }

    private void CovertAnimatorUpdateMode()
    {
        var animators = target.GetComponentsInChildren<Animator>();
        foreach (var animator in animators)
        {
            animator.updateMode = animatorUpdateMode;
        }
    }

    private void CovertVertExmotionUpdateMode()
    {
        var vertExmotions = target.GetComponentsInChildren<VertExmotionBase>();
        foreach (var vertExmotion in vertExmotions)
        {
            vertExmotion.UpdateMode = vertExUpdateMode;
        }

        var vertExSensors = target.GetComponentsInChildren<VertExmotionSensorBase>();
        foreach (var sensor in vertExSensors)
        {
            sensor.UpdateMode = vertExUpdateMode;
        }
    }

    private void CovertDynamicBoneUpdateMode()
    {
        var dynamicBones = target.GetComponentsInChildren<DynamicBone>();
        foreach (var dynamicBone in dynamicBones)
        {
            dynamicBone.m_UpdateMode = dynamicBoneUpdateMode;
        }
    }
}
