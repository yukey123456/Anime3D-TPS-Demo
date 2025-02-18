using Invector;
using Invector.vCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSwitchingManager : MonoBehaviour
{
    [SerializeField]
    private vThirdPersonInput[] _vInputs;

    [SerializeField]
    private int _targetIndex;

    private vThirdPersonInput _currentInput;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);

        foreach (var input in _vInputs)
        {
            EnableInput(input, false);
        }
        Switch(0);
    }

    [ContextMenu(nameof(SwitchToTargetIndex))]
    public void SwitchToTargetIndex()
    {
        Switch(_targetIndex);
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
        //input.tpCamera.gameObject.SetActive(isOn);
    }
}
