using System.Collections;
using RatLab.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class DraggableCardView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private GameView gameView;
    private Transform originalParent;
    private RectTransform placeholder;
    private RectTransform dragLayer;
    private Vector2 pointerOffset;
    private int originalSiblingIndex;
    private Owner owner;
    private int cardIndex;
    private bool canDrag;
    private bool isDragging;

    public Owner Owner => owner;
    public int CardIndex => cardIndex;
    public bool IsDragging => isDragging;

    public void Initialize(GameView view, Owner cardOwner, int index)
    {
        gameView = view;
        owner = cardOwner;
        cardIndex = index;
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetCanDrag(bool value)
    {
        canDrag = value;
    }

    public void SetVisible(bool value)
    {
        canvasGroup.alpha = value ? 1f : 0f;

        if (!isDragging)
        {
            canvasGroup.blocksRaycasts = value;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!canDrag || gameView == null || !gameView.CanBeginDrag(this))
        {
            return;
        }

        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        dragLayer = gameView.DragLayer;
        Vector2 cardScreenPosition = RectTransformUtility.WorldToScreenPoint(
            eventData.pressEventCamera,
            rectTransform.position);

        CreatePlaceholder();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dragLayer,
            cardScreenPosition,
            eventData.pressEventCamera,
            out Vector2 cardLocalPosition);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dragLayer,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 pointerLocalPosition);

        pointerOffset = cardLocalPosition - pointerLocalPosition;

        transform.SetParent(dragLayer, false);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        isDragging = true;
        UpdatePosition(eventData);
        gameView.NotifyDragStarted(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            UpdatePosition(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isDragging && gameView != null)
        {
            gameView.NotifyDragEnded(this);
        }
    }

    public void CompleteDrop()
    {
        if (!isDragging)
        {
            return;
        }

        isDragging = false;
        RestoreToHand();
    }

    public IEnumerator ReturnToHand(float duration)
    {
        if (!isDragging)
        {
            yield break;
        }

        isDragging = false;
        Vector2 startPosition = rectTransform.anchoredPosition;
        Vector2 targetPosition = GetPlaceholderPositionInDragLayer();
        AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / duration);
            rectTransform.anchoredPosition = Vector2.LerpUnclamped(
                startPosition,
                targetPosition,
                curve.Evaluate(normalizedTime));
            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;
        RestoreToHand();
    }

    public void CancelImmediately()
    {
        isDragging = false;
        RestoreToHand();
    }

    private void UpdatePosition(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dragLayer,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 pointerLocalPosition);

        rectTransform.anchoredPosition = pointerLocalPosition + pointerOffset;
    }

    private void CreatePlaceholder()
    {
        GameObject placeholderObject = new GameObject(
            "DragPlaceholder",
            typeof(RectTransform),
            typeof(LayoutElement));

        placeholderObject.transform.SetParent(originalParent, false);
        placeholder = placeholderObject.GetComponent<RectTransform>();
        placeholder.sizeDelta = rectTransform.sizeDelta;
        placeholder.SetSiblingIndex(originalSiblingIndex);

        LayoutElement layoutElement = placeholderObject.GetComponent<LayoutElement>();
        layoutElement.preferredWidth = rectTransform.rect.width;
        layoutElement.preferredHeight = rectTransform.rect.height;
    }

    private Vector2 GetPlaceholderPositionInDragLayer()
    {
        Vector2 placeholderScreenPosition = RectTransformUtility.WorldToScreenPoint(null, placeholder.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dragLayer,
            placeholderScreenPosition,
            null,
            out Vector2 placeholderLocalPosition);
        return placeholderLocalPosition;
    }

    private void RestoreToHand()
    {
        if (placeholder != null && originalParent != null)
        {
            int siblingIndex = placeholder.GetSiblingIndex();
            transform.SetParent(originalParent, false);
            transform.SetSiblingIndex(Mathf.Clamp(siblingIndex, 0, originalParent.childCount - 1));
            Destroy(placeholder.gameObject);
        }

        placeholder = null;
        dragLayer = null;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }
}
