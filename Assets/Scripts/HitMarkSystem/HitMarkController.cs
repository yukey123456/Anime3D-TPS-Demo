using UnityEngine;

public class HitMarkController : MonoBehaviour
{
    [SerializeField]
    private GameObject _hitMarkerGO;

    private IHitMarker _hitMarker;

    private void Awake()
    {
        _hitMarker = _hitMarkerGO.GetComponent<IHitMarker>();
    }

    public void PlayOnHit()
    {
        _hitMarker.PlayOnHitFeedback();
    }

    public void PlayOnKill()
    {
        _hitMarker.PlayOnKillFeedback();
    }
}