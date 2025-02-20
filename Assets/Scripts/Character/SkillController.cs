using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;

public class SkillController : MonoBehaviour
{
    [SerializeField]
    private PlayableDirector _playableDirector;

    [SerializeField]
    private GameObject _content;

    private Camera _mainCamera;

    private void OnEnable()
    {
        _playableDirector.stopped += OnEndSkillCutIn;
        _mainCamera = Camera.main;
        _content.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            UseSkill();
        }
    }

    public void UseSkill()
    {
        _playableDirector.Play();
        OnStartSkillCutIn();
    }

    public void OnStartSkillCutIn()
    {
        _content.SetActive(true);
        _mainCamera.enabled = false;
        Time.timeScale = 0f;
    }

    public void OnEndSkillCutIn(PlayableDirector director)
    {
        _content.SetActive(false);
        _mainCamera.enabled = true;
        Time.timeScale = 1f;
    }

    private void OnDisable()
    {
        _playableDirector.stopped -= OnEndSkillCutIn;
    }
}
