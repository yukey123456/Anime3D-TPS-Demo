using MilkShake;
using UnityEngine;

public class ShakingHelper : MonoBehaviour
{
    [SerializeField]
    private Shaker shakeTarget;

    [SerializeField]
    private ShakeParameters[] shakingPresets;

    public void Shake(int presetIndex)
    {
        if (shakeTarget == null || shakingPresets == null || shakingPresets.Length == 0)
            return;

        int index = Mathf.Clamp(presetIndex, 0, shakingPresets.Length);
        shakeTarget.Shake(shakingPresets[index]);
    }
}
