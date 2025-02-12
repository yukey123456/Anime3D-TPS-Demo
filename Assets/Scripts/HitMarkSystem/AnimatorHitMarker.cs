using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorHitMarker : MonoBehaviour, IHitMarker
{
    [SerializeField]
    private string _onHitState;

    [SerializeField]
    private int _onHitLayer;

    [SerializeField]
    private string _onKillState;

    [SerializeField]
    private int _onKillLayer;

    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void PlayOnHitFeedback()
    {
        _animator.Play(_onHitState, _onHitLayer, 0f);
    }

    public void PlayOnKillFeedback()
    {
        _animator.Play(_onKillState, _onKillLayer, 0f);
    }
}
