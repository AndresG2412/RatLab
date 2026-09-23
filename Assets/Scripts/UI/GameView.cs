using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using RatLab.Core;
using RatLab.Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class GameView : MonoBehaviour
{
    private const int HandSize = 5;

    private static readonly Color PlayerColor = new Color32(55, 110, 200, 255);
    private static readonly Color EnemyColor = new Color32(190, 65, 65, 255);
    private static readonly Color EmptyCellColor = new Color32(125, 130, 135, 255);
    private static readonly Color HighlightCellColor = new Color32(190, 195, 200, 255);
    private static readonly Color BoardCardTextColor = Color.white;

    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private Board board = new Board();
    private readonly System.Random random = new System.Random();
    private readonly CardVisualData[] playerHand = new CardVisualData[HandSize];
    private readonly CardVisualData[] enemyHand = new CardVisualData[HandSize];
    private readonly bool[] usedPlayerCards = new bool[HandSize];
    private readonly bool[] usedEnemyCards = new bool[HandSize];
    private readonly HandCardView[] playerCardViews = new HandCardView[HandSize];
    private readonly HandCardView[] enemyCardViews = new HandCardView[HandSize];
    private readonly BoardCellView[] boardCellViews = new BoardCellView[Board.CellCount];
    private readonly Sprite[] boardSprites = new Sprite[Board.CellCount];

    private CardCatalog cardCatalog;
    private bool missingCatalogWarningLogged;
    private TextMeshProUGUI turnText;
    private RectTransform dragLayer;
    private Owner currentTurn = Owner.Player;
    private bool gameOver;
    private bool enemyThinking;
    private Coroutine enemyTurnCoroutine;
    private Coroutine cardReturnCoroutine;
    private DraggableCardView activePlayerDrag;
    private DraggableCardView returningPlayerCard;
    private GameObject activeEnemyAnimationObject;
    private int highlightedCellIndex = -1;

    internal RectTransform DragLayer => dragLayer;

    public void Build(Transform canvasTransform, CardCatalog catalog)
    {
        cardCatalog = catalog;
        GenerateHands();
        CreateBackground(canvasTransform);
        CreateBoard(canvasTransform);
        CreateHands(canvasTransform);
        CreateTurnText(canvasTransform);
        CreateBackButton(canvasTransform);
        CreateRestartButton(canvasTransform);
        CreateDragLayer(canvasTransform);
        turnText.text = "Turno: Jugador - elige una carta";
        RefreshView();
    }

    private void GenerateHands()
    {
        if (cardCatalog != null && cardCatalog.Count >= HandSize)
        {
            List<CardVisualData> playerCards = cardCatalog.CreateRandomHand(random, HandSize);
            List<CardVisualData> enemyCards = cardCatalog.CreateRandomHand(random, HandSize);

            for (int index = 0; index < HandSize; index++)
            {
                playerHand[index] = playerCards[index];
                enemyHand[index] = enemyCards[index];
            }

            return;
        }

        for (int index = 0; index < HandSize; index++)
        {
            playerHand[index] = CreateRandomVisualCard();
            enemyHand[index] = CreateRandomVisualCard();
        }
    }

    private CardVisualData CreateRandomVisualCard()
    {
        if (cardCatalog != null && cardCatalog.Count > 0)
        {
            return cardCatalog.CreateRandomCard(random);
        }

        if (!missingCatalogWarningLogged)
        {
            Debug.LogWarning("RatLab: no CardCatalog found. Using temporary random cards without artwork.");
            missingCatalogWarningLogged = true;
        }
        return new CardVisualData(
            "Generated",
            1,
            Card.CreateRandom(random),
            null,
            null);
    }

    private void CreateBackground(Transform canvasTransform)
    {
        GameObject backgroundObject = new GameObject(
            "Background",
            typeof(RectTransform),
            typeof(Image));

        backgroundObject.transform.SetParent(canvasTransform, false);

        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        Image background = backgroundObject.GetComponent<Image>();
        background.color = new Color32(24, 38, 48, 255);
    }

    private void CreateBoard(Transform canvasTransform)
    {
        GameObject boardObject = new GameObject(
            "Board",
            typeof(RectTransform),
            typeof(GridLayoutGroup));

        boardObject.transform.SetParent(canvasTransform, false);

        RectTransform boardRect = boardObject.GetComponent<RectTransform>();
        boardRect.anchorMin = new Vector2(0.5f, 0.5f);
        boardRect.anchorMax = new Vector2(0.5f, 0.5f);
        boardRect.pivot = new Vector2(0.5f, 0.5f);
        boardRect.sizeDelta = new Vector2(500f, 500f);
        boardRect.anchoredPosition = new Vector2(0f, 30f);

        GridLayoutGroup grid = boardObject.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(156f, 156f);
        grid.spacing = new Vector2(16f, 16f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        grid.childAlignment = TextAnchor.MiddleCenter;

        for (int index = 0; index < Board.CellCount; index++)
        {
            boardCellViews[index] = CreateBoardCell(boardObject.transform, index);
        }
    }

    private BoardCellView CreateBoardCell(Transform boardTransform, int cellIndex)
    {
        GameObject cellObject = new GameObject(
            $"Cell_{cellIndex + 1}",
            typeof(RectTransform),
            typeof(Image),
            typeof(BoardDropCellView));

        cellObject.transform.SetParent(boardTransform, false);

        Image image = cellObject.GetComponent<Image>();
        BoardDropCellView dropCell = cellObject.GetComponent<BoardDropCellView>();
        dropCell.Initialize(this, cellIndex);

        Image artwork = CreateArtworkImage(cellObject.transform);
        TextMeshProUGUI[] labels = CreateCardLabels(cellObject.transform, 22f);
        return new BoardCellView(image, artwork, labels);
    }

    private void CreateHands(Transform canvasTransform)
    {
        Transform enemyHandTransform = CreateHandRow(
            canvasTransform,
            "EnemyHand",
            new Vector2(0f, -32f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f));

        Transform playerHandTransform = CreateHandRow(
            canvasTransform,
            "PlayerHand",
            new Vector2(0f, 32f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f));

        for (int index = 0; index < HandSize; index++)
        {
            enemyCardViews[index] = CreateHandCard(enemyHandTransform, Owner.Enemy, index, enemyHand[index]);
            playerCardViews[index] = CreateHandCard(playerHandTransform, Owner.Player, index, playerHand[index]);
        }
    }

    private Transform CreateHandRow(
        Transform canvasTransform,
        string name,
        Vector2 anchoredPosition,
        Vector2 anchor,
        Vector2 pivot)
    {
        GameObject handObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(HorizontalLayoutGroup));

        handObject.transform.SetParent(canvasTransform, false);

        RectTransform handRect = handObject.GetComponent<RectTransform>();
        handRect.anchorMin = anchor;
        handRect.anchorMax = anchor;
        handRect.pivot = pivot;
        handRect.sizeDelta = new Vector2(900f, 190f);
        handRect.anchoredPosition = anchoredPosition;

        HorizontalLayoutGroup layout = handObject.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 15f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        return handObject.transform;
    }

    private HandCardView CreateHandCard(Transform handTransform, Owner owner, int cardIndex, CardVisualData data)
    {
        GameObject cardObject = new GameObject(
            $"{owner}Card_{cardIndex + 1}",
            typeof(RectTransform),
            typeof(Image),
            typeof(CanvasGroup),
            typeof(DraggableCardView));

        cardObject.transform.SetParent(handTransform, false);

        RectTransform cardRect = cardObject.GetComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(150f, 190f);

        Image image = cardObject.GetComponent<Image>();
        DraggableCardView draggable = cardObject.GetComponent<DraggableCardView>();
        draggable.Initialize(this, owner, cardIndex);

        Image artwork = CreateArtworkImage(cardObject.transform);
        SetArtworkOwnerColor(artwork, GetOwnerColor(owner));
        TextMeshProUGUI[] labels = CreateCardLabels(cardObject.transform, 24f);
        return new HandCardView(draggable, image, artwork, labels);
    }

    private Image CreateArtworkImage(Transform parent)
    {
        GameObject artworkObject = new GameObject(
            "Artwork",
            typeof(RectTransform),
            typeof(Image));

        artworkObject.transform.SetParent(parent, false);

        RectTransform artworkRect = artworkObject.GetComponent<RectTransform>();
        artworkRect.anchorMin = Vector2.zero;
        artworkRect.anchorMax = Vector2.one;
        artworkRect.offsetMin = Vector2.zero;
        artworkRect.offsetMax = Vector2.zero;

        Image artwork = artworkObject.GetComponent<Image>();
        artwork.preserveAspect = false;
        artwork.raycastTarget = false;
        return artwork;
    }

    private void SetArtworkOwnerColor(Image artwork, Color ownerColor)
    {
        Outline outline = artwork.GetComponent<Outline>();
        if (outline == null)
        {
            outline = artwork.gameObject.AddComponent<Outline>();
        }

        outline.effectColor = ownerColor;
        outline.effectDistance = new Vector2(3f, 3f);
        outline.useGraphicAlpha = false;
    }

    private TextMeshProUGUI[] CreateCardLabels(Transform cardTransform, float fontSize)
    {
        return new[]
        {
            CreateSideLabel(cardTransform, "North", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -4f), new Vector2(70f, 24f), fontSize),
            CreateSideLabel(cardTransform, "East", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-3f, 0f), new Vector2(28f, 24f), fontSize),
            CreateSideLabel(cardTransform, "South", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 4f), new Vector2(70f, 24f), fontSize),
            CreateSideLabel(cardTransform, "West", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(3f, 0f), new Vector2(28f, 24f), fontSize)
        };
    }

    private TextMeshProUGUI CreateSideLabel(
        Transform parent,
        string name,
        Vector2 anchor,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 size,
        float fontSize)
    {
        GameObject labelObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(TextMeshProUGUI));

        labelObject.transform.SetParent(parent, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = anchor;
        labelRect.anchorMax = anchor;
        labelRect.pivot = pivot;
        labelRect.sizeDelta = size;
        labelRect.anchoredPosition = anchoredPosition;

        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.font = TMP_Settings.defaultFontAsset;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = BoardCardTextColor;
        label.raycastTarget = false;
        return label;
    }

    private void CreateTurnText(Transform canvasTransform)
    {
        GameObject statusObject = new GameObject(
            "TurnText",
            typeof(RectTransform),
            typeof(TextMeshProUGUI));

        statusObject.transform.SetParent(canvasTransform, false);

        RectTransform statusRect = statusObject.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0.5f, 0.5f);
        statusRect.anchorMax = new Vector2(0.5f, 0.5f);
        statusRect.pivot = new Vector2(0.5f, 0.5f);
        statusRect.sizeDelta = new Vector2(900f, 50f);
        statusRect.anchoredPosition = new Vector2(0f, -255f);

        turnText = statusObject.GetComponent<TextMeshProUGUI>();
        turnText.font = TMP_Settings.defaultFontAsset;
        turnText.fontSize = 28f;
        turnText.alignment = TextAlignmentOptions.Center;
        turnText.color = Color.white;
    }

    private void CreateBackButton(Transform canvasTransform)
    {
        GameObject buttonObject = new GameObject(
            "BackButton",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button));

        buttonObject.transform.SetParent(canvasTransform, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = Vector2.zero;
        buttonRect.anchorMax = Vector2.zero;
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(220f, 64f);
        buttonRect.anchoredPosition = new Vector2(140f, 60f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = PlayerColor;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(LoadMainMenuScene);

        TextMeshProUGUI label = CreateSideLabel(
            buttonObject.transform,
            "Label",
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            Vector2.zero,
            24f);

        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.pivot = new Vector2(0.5f, 0.5f);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        label.text = "Volver al menú";
    }

    private void CreateRestartButton(Transform canvasTransform)
    {
        GameObject buttonObject = new GameObject(
            "RestartButton",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button));

        buttonObject.transform.SetParent(canvasTransform, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1f, 0f);
        buttonRect.anchorMax = new Vector2(1f, 0f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(220f, 64f);
        buttonRect.anchoredPosition = new Vector2(-140f, 60f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = EnemyColor;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(RestartGame);

        TextMeshProUGUI label = CreateSideLabel(
            buttonObject.transform,
            "Label",
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            Vector2.zero,
            24f);

        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.pivot = new Vector2(0.5f, 0.5f);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        label.text = "Jugar de nuevo";
    }

    private void CreateDragLayer(Transform canvasTransform)
    {
        GameObject dragLayerObject = new GameObject(
            "DragLayer",
            typeof(RectTransform),
            typeof(CanvasGroup));

        dragLayerObject.transform.SetParent(canvasTransform, false);

        dragLayer = dragLayerObject.GetComponent<RectTransform>();
        dragLayer.anchorMin = Vector2.zero;
        dragLayer.anchorMax = Vector2.one;
        dragLayer.offsetMin = Vector2.zero;
        dragLayer.offsetMax = Vector2.zero;
        dragLayer.SetAsLastSibling();

        CanvasGroup canvasGroup = dragLayerObject.GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    internal bool CanBeginDrag(DraggableCardView card)
    {
        return !gameOver
            && !enemyThinking
            && currentTurn == Owner.Player
            && activePlayerDrag == null
            && card.Owner == Owner.Player
            && !IsCardUsed(Owner.Player, card.CardIndex);
    }

    internal void NotifyDragStarted(DraggableCardView card)
    {
        activePlayerDrag = card;
        highlightedCellIndex = -1;
        RefreshBoardView();
    }

    internal void NotifyDragEnded(DraggableCardView card)
    {
        if (activePlayerDrag != card)
        {
            return;
        }

        activePlayerDrag = null;
        highlightedCellIndex = -1;
        card.SetCanDrag(false);
        returningPlayerCard = card;
        cardReturnCoroutine = StartCoroutine(ReturnCardToHand(card));
        RefreshBoardView();
    }

    internal void HandleCardDrop(DraggableCardView card, int cellIndex)
    {
        if (activePlayerDrag != card
            || gameOver
            || enemyThinking
            || currentTurn != Owner.Player
            || !board.IsEmpty(cellIndex))
        {
            return;
        }

        CardVisualData placedVisual = playerHand[card.CardIndex];
        var flippedCellIndices = BoardLogic.PlaceCard(board, cellIndex, placedVisual.Values, Owner.Player);
        boardSprites[cellIndex] = placedVisual.FrontSprite;
        MarkCardAsUsed(Owner.Player, card.CardIndex);
        activePlayerDrag = null;
        highlightedCellIndex = -1;
        card.CompleteDrop();

        if (board.IsFull)
        {
            ShowGameOver();
            RefreshView();
            return;
        }

        currentTurn = Owner.Enemy;
        enemyThinking = true;
        turnText.text = $"Turno: Enemigo pensando... volteadas: {flippedCellIndices.Count}";
        RefreshView();
        enemyTurnCoroutine = StartCoroutine(PlayEnemyTurnAfterDelay());
    }

    internal void HandleCellPointerEnter(int cellIndex)
    {
        if (activePlayerDrag == null || gameOver || enemyThinking || !board.IsEmpty(cellIndex))
        {
            return;
        }

        highlightedCellIndex = cellIndex;
        RefreshBoardView();
    }

    internal void HandleCellPointerExit(int cellIndex)
    {
        if (highlightedCellIndex != cellIndex)
        {
            return;
        }

        highlightedCellIndex = -1;
        RefreshBoardView();
    }

    private IEnumerator ReturnCardToHand(DraggableCardView card)
    {
        yield return card.ReturnToHand(0.2f);
        cardReturnCoroutine = null;
        returningPlayerCard = null;
        RefreshView();
    }

    private IEnumerator PlayEnemyTurnAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        enemyTurnCoroutine = null;

        if (gameOver)
        {
            yield break;
        }

        List<Card> availableCards = new List<Card>();

        for (int index = 0; index < HandSize; index++)
        {
            if (!usedEnemyCards[index])
            {
                availableCards.Add(enemyHand[index].Values);
            }
        }

        EnemyMove selectedMove = BoardLogic.ChooseBestMove(
            board,
            availableCards,
            Owner.Enemy,
            random);

        if (selectedMove == null)
        {
            currentTurn = Owner.Player;
            enemyThinking = false;
            turnText.text = "Turno: Jugador - elige una carta";
            RefreshView();
            yield break;
        }

        int usedCardIndex = FindEnemyCardIndex(selectedMove.Card);
        CardVisualData selectedVisual = enemyHand[usedCardIndex];
        enemyCardViews[usedCardIndex].Draggable.SetVisible(false);
        activeEnemyAnimationObject = CreateEnemyAnimationCard(selectedVisual);
        yield return AnimateEnemyCardToCell(
            activeEnemyAnimationObject,
            enemyCardViews[usedCardIndex].Draggable.GetComponent<RectTransform>(),
            boardCellViews[selectedMove.CellIndex].Image.rectTransform,
            0.5f);

        if (activeEnemyAnimationObject != null)
        {
            activeEnemyAnimationObject.SetActive(false);
            Destroy(activeEnemyAnimationObject);
            activeEnemyAnimationObject = null;
        }

        var flippedCellIndices = BoardLogic.PlaceCard(
            board,
            selectedMove.CellIndex,
            selectedMove.Card,
            Owner.Enemy);
        boardSprites[selectedMove.CellIndex] = selectedVisual.FrontSprite;
        MarkCardAsUsed(Owner.Enemy, usedCardIndex);

        if (board.IsFull)
        {
            ShowGameOver();
            RefreshView();
            yield break;
        }

        currentTurn = Owner.Player;
        enemyThinking = false;
        turnText.text = $"Turno: Jugador - volteadas: {flippedCellIndices.Count}. Elige una carta";
        RefreshView();
    }

    private GameObject CreateEnemyAnimationCard(CardVisualData data)
    {
        GameObject cardObject = new GameObject(
            "EnemyCardAnimation",
            typeof(RectTransform),
            typeof(Image));

        cardObject.transform.SetParent(dragLayer, false);

        RectTransform cardRect = cardObject.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = boardCellViews[0].Image.rectTransform.rect.size;

        Image image = cardObject.GetComponent<Image>();
        image.color = EnemyColor;
        image.raycastTarget = false;

        Image artwork = CreateArtworkImage(cardObject.transform);
        artwork.sprite = data.FrontSprite;
        SetArtworkOwnerColor(artwork, EnemyColor);
        SetCardLabels(CreateCardLabels(cardObject.transform, 22f), data.Values);
        cardObject.transform.SetAsLastSibling();
        return cardObject;
    }

    private int FindEnemyCardIndex(Card card)
    {
        for (int index = 0; index < HandSize; index++)
        {
            if (enemyHand[index].Values == card)
            {
                return index;
            }
        }

        return -1;
    }

    private IEnumerator AnimateEnemyCardToCell(
        GameObject cardObject,
        RectTransform sourceRect,
        RectTransform targetRect,
        float duration)
    {
        RectTransform animationRect = cardObject.GetComponent<RectTransform>();
        Vector2 sourceScreenPosition = RectTransformUtility.WorldToScreenPoint(null, sourceRect.position);
        Vector2 targetScreenPosition = RectTransformUtility.WorldToScreenPoint(null, targetRect.position);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dragLayer,
            sourceScreenPosition,
            null,
            out Vector2 sourcePosition);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dragLayer,
            targetScreenPosition,
            null,
            out Vector2 targetPosition);

        animationRect.anchoredPosition = sourcePosition;
        AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / duration);
            animationRect.anchoredPosition = Vector2.LerpUnclamped(
                sourcePosition,
                targetPosition,
                curve.Evaluate(normalizedTime));
            yield return null;
        }

        animationRect.anchoredPosition = targetPosition;
    }

    private void RefreshView()
    {
        RefreshHand(Owner.Player, playerHand, usedPlayerCards, playerCardViews);
        RefreshHand(Owner.Enemy, enemyHand, usedEnemyCards, enemyCardViews);
        RefreshBoardView();
    }

    private void RefreshBoardView()
    {
        for (int index = 0; index < Board.CellCount; index++)
        {
            BoardCellView cellView = boardCellViews[index];

            if (board.IsEmpty(index))
            {
                cellView.Image.color = highlightedCellIndex == index
                    ? HighlightCellColor
                    : EmptyCellColor;
                cellView.Artwork.sprite = null;
                cellView.Artwork.enabled = false;
                SetCardLabels(cellView.Labels, null);
                continue;
            }

            cellView.Image.color = GetOwnerColor(board.GetOwner(index));
            cellView.Artwork.sprite = boardSprites[index];
            cellView.Artwork.enabled = boardSprites[index] != null;
            SetArtworkOwnerColor(cellView.Artwork, GetOwnerColor(board.GetOwner(index)));
            SetCardLabels(cellView.Labels, board.GetCard(index));
        }
    }

    private void RefreshHand(Owner owner, CardVisualData[] hand, bool[] usedCards, HandCardView[] views)
    {
        for (int index = 0; index < HandSize; index++)
        {
            Color color = GetOwnerColor(owner);

            if (usedCards[index])
            {
                color = Color.Lerp(color, Color.black, 0.45f);
            }

            views[index].Draggable.SetVisible(true);
            CardVisualData data = hand[index];
            bool showFace = owner == Owner.Player;
            views[index].Artwork.sprite = showFace ? data.FrontSprite : data.BackSprite;
            views[index].Artwork.enabled = views[index].Artwork.sprite != null;
            SetArtworkOwnerColor(views[index].Artwork, GetOwnerColor(owner));
            SetCardLabels(views[index].Labels, showFace ? data.Values : null);
            views[index].Image.color = color;
            views[index].Draggable.SetCanDrag(!gameOver
                && !enemyThinking
                && owner == Owner.Player
                && currentTurn == Owner.Player
                && !usedCards[index]);
        }
    }

    private void SetCardLabels(TextMeshProUGUI[] labels, Card card)
    {
        if (card == null)
        {
            for (int index = 0; index < labels.Length; index++)
            {
                labels[index].text = string.Empty;
            }

            return;
        }

        labels[0].text = card.North.ToString();
        labels[1].text = card.East.ToString();
        labels[2].text = card.South.ToString();
        labels[3].text = card.West.ToString();
    }

    private bool IsCardUsed(Owner owner, int cardIndex)
    {
        return owner == Owner.Player ? usedPlayerCards[cardIndex] : usedEnemyCards[cardIndex];
    }

    private void MarkCardAsUsed(Owner owner, int cardIndex)
    {
        if (owner == Owner.Player)
        {
            usedPlayerCards[cardIndex] = true;
        }
        else
        {
            usedEnemyCards[cardIndex] = true;
        }
    }

    private void RestartGame()
    {
        if (enemyTurnCoroutine != null)
        {
            StopCoroutine(enemyTurnCoroutine);
            enemyTurnCoroutine = null;
        }

        if (cardReturnCoroutine != null)
        {
            StopCoroutine(cardReturnCoroutine);
            cardReturnCoroutine = null;
        }

        if (returningPlayerCard != null)
        {
            returningPlayerCard.CancelImmediately();
            returningPlayerCard = null;
        }

        if (activePlayerDrag != null)
        {
            activePlayerDrag.CancelImmediately();
            activePlayerDrag = null;
        }

        if (activeEnemyAnimationObject != null)
        {
            activeEnemyAnimationObject.SetActive(false);
            Destroy(activeEnemyAnimationObject);
            activeEnemyAnimationObject = null;
        }

        board = new Board();
        Array.Clear(boardSprites, 0, boardSprites.Length);
        Array.Clear(usedPlayerCards, 0, usedPlayerCards.Length);
        Array.Clear(usedEnemyCards, 0, usedEnemyCards.Length);
        currentTurn = Owner.Player;
        highlightedCellIndex = -1;
        gameOver = false;
        enemyThinking = false;
        GenerateHands();
        turnText.text = "Turno: Jugador - elige una carta";
        RefreshView();
    }

    private void ShowGameOver()
    {
        gameOver = true;
        enemyThinking = false;

        Owner winner = BoardLogic.GetWinner(board);
        int playerCount = board.CountOwnedBy(Owner.Player);
        int enemyCount = board.CountOwnedBy(Owner.Enemy);
        turnText.text = $"{GetWinnerLabel(winner)} | Jugador: {playerCount} | Enemigo: {enemyCount}";
    }

    private void LoadMainMenuScene()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private static string GetWinnerLabel(Owner owner)
    {
        if (owner == Owner.Player)
        {
            return "Gana el jugador";
        }

        if (owner == Owner.Enemy)
        {
            return "Gana el enemigo";
        }

        return "Empate";
    }

    private static Color GetOwnerColor(Owner owner)
    {
        if (owner == Owner.Player)
        {
            return PlayerColor;
        }

        if (owner == Owner.Enemy)
        {
            return EnemyColor;
        }

        return EmptyCellColor;
    }

    private sealed class HandCardView
    {
        public HandCardView(DraggableCardView draggable, Image image, Image artwork, TextMeshProUGUI[] labels)
        {
            Draggable = draggable;
            Image = image;
            Artwork = artwork;
            Labels = labels;
        }

        public DraggableCardView Draggable { get; }
        public Image Image { get; }
        public Image Artwork { get; }
        public TextMeshProUGUI[] Labels { get; }
    }

    private sealed class BoardCellView
    {
        public BoardCellView(Image image, Image artwork, TextMeshProUGUI[] labels)
        {
            Image = image;
            Artwork = artwork;
            Labels = labels;
        }

        public Image Image { get; }
        public Image Artwork { get; }
        public TextMeshProUGUI[] Labels { get; }
    }
}
