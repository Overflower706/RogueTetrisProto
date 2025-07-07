using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIDebugger : MonoBehaviour
{
    private GraphicRaycaster raycaster;

    private void Awake()
    {
        raycaster = GetComponent<GraphicRaycaster>();
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            DetectTopUIElement();
        }
    }

    private void DetectTopUIElement()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerData, results);

        if (results.Count > 0)
        {
            var bumin = results[0].gameObject;
            Debug.Log($"Top UI Element: {bumin.name} in {bumin.gameObject.transform.parent.name}");
        }
        else
        {
            Debug.Log("No UI Element detected.");
        }
    }
}