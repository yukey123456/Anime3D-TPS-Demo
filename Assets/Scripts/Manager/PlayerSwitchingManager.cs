using Invector.vCamera;
using Invector.vCharacterController;
using Invector.vItemManager;
using System.Collections;
using UnityEngine;

public class PlayerSwitchingManager : MonoBehaviour
{
    [SerializeField]
    private vThirdPersonCamera _tpCamera;

    [SerializeField]
    private vAllieShooterInput[] _vInputs;

    private int _currentIdx;
    private vAllieShooterInput _currentInput;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.2f);

        foreach (var input in _vInputs)
        {
            EnableInput(input, false);
        }
        Switch(0);
    }

    private void LateUpdate()
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

    private void EnableInput(vAllieShooterInput input, bool isOn)
    {
        input.SetLockAllInput(!isOn);
        input.TogglePlayerControl(isOn);
        
        if (input.TryGetComponent(out vItemManager itemManager))
        {
            itemManager.inventory.SetLockInventoryInput(!isOn);
        }

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
