using UnityEngine;
using UnityEngine.EventSystems;

public sealed class BoardDropCellView : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    private GameView gameView;
    private int cellIndex;

    public void Initialize(GameView view, int index)
    {
        gameView = view;
        cellIndex = index;
    }

    public void OnDrop(PointerEventData eventData)
    {
        DraggableCardView draggedCard = eventData.pointerDrag == null
            ? null
            : eventData.pointerDrag.GetComponent<DraggableCardView>();

        if (draggedCard != null)
        {
            gameView.HandleCardDrop(draggedCard, cellIndex);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        gameView.HandleCellPointerEnter(cellIndex);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        gameView.HandleCellPointerExit(cellIndex);
    }
}
