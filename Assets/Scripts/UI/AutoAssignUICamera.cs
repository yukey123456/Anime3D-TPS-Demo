using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class AutoAssignUICamera : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;

    private void Awake()
    {
        if (!canvas) return;

        var cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        canvas.worldCamera = cameras.First(x => x.gameObject.CompareTag("UICamera"));
    }

    private void OnValidate()
    {
        if (canvas)
            return;

        canvas = GetComponent<Canvas>();
    }
}
