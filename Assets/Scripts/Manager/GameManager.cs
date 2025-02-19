using Invector.vCharacterController;
using Invector.vCover;
using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private CoverCharacter[] _coverCharacters;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.1f);
        foreach (var coverChar in _coverCharacters)
        {
            coverChar.vCoverController.ForceEnterCover(coverChar.targetCoverPoint);
        }
        
    }

    [Serializable]
    public struct CoverCharacter
    {
        public vCoverController vCoverController;
        public vCoverPoint targetCoverPoint;
    }
}
