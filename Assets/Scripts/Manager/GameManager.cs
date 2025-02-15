using Invector.vCharacterController;
using Invector.vCover;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private vCoverController _vCoverController;

    [SerializeField]
    private vCoverPoint _targetCoverPoint;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.2f);
        _vCoverController.ForceEnterCover(_targetCoverPoint);
    }
}
