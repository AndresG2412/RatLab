using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using RatLab.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class GameView : MonoBehaviour
{
    private const int HandSize = 5;

    private static readonly Color PlayerColor = new Color32(55, 110, 200, 255);
    private static readonly Color EnemyColor = new Color32(190, 65, 65, 255);
    private static readonly Color EmptyCellColor = new Color32(125, 130, 135, 255);
    private static readonly Color BoardCardTextColor = Color.white;

    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private Board board = new Board();
    private readonly System.Random random = new System.Random();
    private readonly Card[] playerHand = new Card[HandSize];
    private readonly Card[] enemyHand = new Card[HandSize];
    private readonly bool[] usedPlayerCards = new bool[HandSize];
    private readonly bool[] usedEnemyCards = new bool[HandSize];
    private readonly HandCardView[] playerCardViews = new HandCardView[HandSize];
    private readonly HandCardView[] enemyCardViews = new HandCardView[HandSize];
    private readonly BoardCellView[] boardCellViews = new BoardCellView[Board.CellCount];

    private TextMeshProUGUI turnText;
    private Owner currentTurn = Owner.Player;
    private int selectedCardIndex = -1;
    private Card selectedCard;
    private bool gameOver;
    private bool enemyThinking;
    private Coroutine enemyTurnCoroutine;

    public void Build(Transform canvasTransform)
    {
        GenerateHands();
        CreateBackground(canvasTransform);
        CreateBoard(canvasTransform);
        CreateHands(canvasTransform);
        CreateTurnText(canvasTransform);
        CreateBackButton(canvasTransform);
        CreateRestartButton(canvasTransform);
        turnText.text = "Turno: Jugador - elige una carta";
        RefreshView();
    }

    private void GenerateHands()
    {
        for (int index = 0; index < HandSize; index++)
        {
            playerHand[index] = Card.CreateRandom(random);
            enemyHand[index] = Card.CreateRandom(random);
        }
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
            typeof(Button));

        cellObject.transform.SetParent(boardTransform, false);

        Image image = cellObject.GetComponent<Image>();
        Button button = cellObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;

        int capturedCellIndex = cellIndex;
        button.onClick.AddListener(() => PlaceSelectedCard(capturedCellIndex));

        TextMeshProUGUI[] labels = CreateCardLabels(cellObject.transform, 22f);
        return new BoardCellView(button, image, labels);
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

    private HandCardView CreateHandCard(Transform handTransform, Owner owner, int cardIndex, Card card)
    {
        GameObject cardObject = new GameObject(
            $"{owner}Card_{cardIndex + 1}",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button));

        cardObject.transform.SetParent(handTransform, false);

        RectTransform cardRect = cardObject.GetComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(150f, 190f);

        Image image = cardObject.GetComponent<Image>();
        Button button = cardObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;

        int capturedCardIndex = cardIndex;
        button.onClick.AddListener(() => SelectCard(owner, capturedCardIndex));

        TextMeshProUGUI[] labels = CreateCardLabels(cardObject.transform, 24f);
        SetCardLabels(labels, card);
        return new HandCardView(button, image, labels);
    }

    private TextMeshProUGUI[] CreateCardLabels(Transform cardTransform, float fontSize)
    {
        return new[]
        {
            CreateSideLabel(cardTransform, "North", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(70f, 30f), fontSize),
            CreateSideLabel(cardTransform, "East", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-10f, 0f), new Vector2(55f, 30f), fontSize),
            CreateSideLabel(cardTransform, "South", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 12f), new Vector2(70f, 30f), fontSize),
            CreateSideLabel(cardTransform, "West", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(10f, 0f), new Vector2(55f, 30f), fontSize)
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

    private void SelectCard(Owner owner, int cardIndex)
    {
        if (gameOver || enemyThinking || owner != Owner.Player || currentTurn != Owner.Player || IsCardUsed(owner, cardIndex))
        {
            return;
        }

        selectedCardIndex = cardIndex;
        Card[] hand = owner == Owner.Player ? playerHand : enemyHand;
        selectedCard = hand[cardIndex];
        turnText.text = $"Turno: {GetOwnerLabel(currentTurn)} - elige una casilla vacía";
        RefreshView();
    }

    private void PlaceSelectedCard(int cellIndex)
    {
        if (gameOver || enemyThinking || currentTurn != Owner.Player || selectedCardIndex < 0 || selectedCard == null || !board.IsEmpty(cellIndex))
        {
            return;
        }

        var flippedCellIndices = BoardLogic.PlaceCard(board, cellIndex, selectedCard, Owner.Player);
        MarkCardAsUsed(Owner.Player, selectedCardIndex);
        selectedCardIndex = -1;
        selectedCard = null;

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
                availableCards.Add(enemyHand[index]);
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

        var flippedCellIndices = BoardLogic.PlaceCard(
            board,
            selectedMove.CellIndex,
            selectedMove.Card,
            Owner.Enemy);
        int usedCardIndex = Array.IndexOf(enemyHand, selectedMove.Card);
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

    private void RefreshView()
    {
        RefreshHand(Owner.Player, playerHand, usedPlayerCards, playerCardViews);
        RefreshHand(Owner.Enemy, enemyHand, usedEnemyCards, enemyCardViews);

        for (int index = 0; index < Board.CellCount; index++)
        {
            BoardCellView cellView = boardCellViews[index];

            if (board.IsEmpty(index))
            {
                cellView.Image.color = EmptyCellColor;
                SetCardLabels(cellView.Labels, null);
                cellView.Button.interactable = !gameOver
                    && !enemyThinking
                    && currentTurn == Owner.Player
                    && selectedCardIndex >= 0;
                continue;
            }

            cellView.Image.color = GetOwnerColor(board.GetOwner(index));
            SetCardLabels(cellView.Labels, board.GetCard(index));
            cellView.Button.interactable = false;
        }
    }

    private void RefreshHand(Owner owner, Card[] hand, bool[] usedCards, HandCardView[] views)
    {
        for (int index = 0; index < HandSize; index++)
        {
            Color color = GetOwnerColor(owner);

            if (usedCards[index])
            {
                color = Color.Lerp(color, Color.black, 0.45f);
            }
            else if (owner == currentTurn && selectedCardIndex == index)
            {
                color = Color.Lerp(color, Color.white, 0.35f);
            }

            SetCardLabels(views[index].Labels, hand[index]);
            views[index].Image.color = color;
            views[index].Button.interactable = !gameOver
                && !enemyThinking
                && owner == Owner.Player
                && currentTurn == Owner.Player
                && !usedCards[index];
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

        labels[0].text = $"N {card.North}";
        labels[1].text = $"E {card.East}";
        labels[2].text = $"S {card.South}";
        labels[3].text = $"O {card.West}";
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

        board = new Board();
        Array.Clear(usedPlayerCards, 0, usedPlayerCards.Length);
        Array.Clear(usedEnemyCards, 0, usedEnemyCards.Length);
        currentTurn = Owner.Player;
        selectedCardIndex = -1;
        selectedCard = null;
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

    private static string GetOwnerLabel(Owner owner)
    {
        return owner == Owner.Player ? "Jugador" : "Enemigo";
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
        public HandCardView(Button button, Image image, TextMeshProUGUI[] labels)
        {
            Button = button;
            Image = image;
            Labels = labels;
        }

        public Button Button { get; }
        public Image Image { get; }
        public TextMeshProUGUI[] Labels { get; }
    }

    private sealed class BoardCellView
    {
        public BoardCellView(Button button, Image image, TextMeshProUGUI[] labels)
        {
            Button = button;
            Image = image;
            Labels = labels;
        }

        public Button Button { get; }
        public Image Image { get; }
        public TextMeshProUGUI[] Labels { get; }
    }
}
