using Invector.vCamera;
using Invector.vCharacterController;
using System.Collections;
using UnityEngine;

public class PlayerSwitchingManager : MonoBehaviour
{
    [SerializeField]
    private vThirdPersonCamera _tpCamera;

    [SerializeField]
    private vThirdPersonInput[] _vInputs;

    private int _currentIdx;
    private vThirdPersonInput _currentInput;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.2f);

        foreach (var input in _vInputs)
        {
            EnableInput(input, false);
        }
        Switch(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            AutoSwitchTarget();
        }
    }

    public void AutoSwitchTarget()
    {
        int targetIdx = _currentIdx ++;
        if (_currentIdx >= _vInputs.Length)
            _currentIdx = 0;

        Switch(targetIdx);
    }

    public void Switch(int index)
    {
        if (_currentInput)
        {
            EnableInput(_currentInput, false);
        }

        _currentInput = _vInputs[Mathf.Clamp(index, 0, _vInputs.Length - 1)];
        EnableInput(_currentInput, true);
    }

    private void EnableInput(vThirdPersonInput input, bool isOn)
    {
        input.SetLockAllInput(!isOn);

        if (isOn)
        {
            _tpCamera.SetMainTarget(input.transform);
        }
        else
        {
            if (input.TryGetComponent<vCameraHandler>(out var handler)) 
                handler.ClearCamera();
        }
    }
}
