using MilkShake;
using UnityEngine;

public class vShakingHelper : MonoBehaviour
{
    [SerializeField]
    private vCameraHandler vCameraHandler;

    [SerializeField]
    private ShakeParameters[] shakingPresets;

    private Shaker _shakeTarget;

    public void Shake(int presetIndex)
    {
        if (vCameraHandler == null || !vCameraHandler.IsCameraValid())
            return;

        if (_shakeTarget == null)
        {
            _shakeTarget = vCameraHandler.Camera.GetComponent<Shaker>();
        }

        if (_shakeTarget == null || shakingPresets == null || shakingPresets.Length == 0)
            return;

        int index = Mathf.Clamp(presetIndex, 0, shakingPresets.Length);
        _shakeTarget.Shake(shakingPresets[index]);
    }
}
