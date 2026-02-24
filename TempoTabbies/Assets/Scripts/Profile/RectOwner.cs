using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RectOwner : MonoBehaviour
{
    /// <summary>
    /// The scrollRect attached to this component
    /// </summary>
    ScrollRect _scrollRect;
    /// <summary>
    /// All selectable items inside the scrollrect
    /// </summary>
    Selectable[] _selectables;

    private void Start()
    {
        _scrollRect = GetComponent<ScrollRect>(); Debug.Log(_scrollRect);
        _selectables = GetComponentsInChildren<Selectable>();
        ScrollToSelection();
    }

    void ScrollToSelection()
    {
        GameObject selection = EventSystem.current.currentSelectedGameObject;
        if (!selection) return;
        if (!selection.transform.IsChildOf(transform)) return;
        if (!selection.TryGetComponent(out RectTransform rectTransform)) return;
        _scrollRect.ScrollToCenter(rectTransform);
        int index = Array.IndexOf(_selectables, rectTransform);
        if (index < 0) return;
        float target = 1f - (float)index / Mathf.Max(_selectables.Length - 2, 1);
        _scrollRect.verticalNormalizedPosition = target;
    }
    public void Update()
    {
        ScrollToSelection();
    }
}
