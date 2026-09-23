using System;
using System.Collections.Generic;
using RatLab.Core;
using UnityEngine;

namespace RatLab.Data
{
    [CreateAssetMenu(fileName = "CardCatalog", menuName = "RatLab/Cards/Card Catalog")]
    public sealed class CardCatalog : ScriptableObject
    {
        [SerializeField] private Sprite enemyBack;
        [SerializeField] private List<CardVariantRecord> cards = new List<CardVariantRecord>();

        public int Count => cards == null ? 0 : cards.Count;

        public CardVisualData CreateRandomCard(System.Random random)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            if (cards == null || cards.Count == 0)
            {
                throw new InvalidOperationException("The card catalog is empty.");
            }

            return cards[random.Next(cards.Count)].CreateRuntimeCard(enemyBack);
        }

        public List<CardVisualData> CreateRandomHand(System.Random random, int handSize)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            if (handSize < 0 || handSize > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(handSize));
            }

            List<CardVisualData> hand = new List<CardVisualData>(handSize);
            HashSet<int> selectedIndices = new HashSet<int>();

            while (hand.Count < handSize)
            {
                int cardIndex = random.Next(cards.Count);
                if (selectedIndices.Add(cardIndex))
                {
                    hand.Add(cards[cardIndex].CreateRuntimeCard(enemyBack));
                }
            }

            return hand;
        }

        public void SetData(List<CardVariantRecord> newCards, Sprite newEnemyBack)
        {
            cards = newCards ?? new List<CardVariantRecord>();
            enemyBack = newEnemyBack;
        }
    }

    [Serializable]
    public sealed class CardVariantRecord
    {
        [SerializeField] private string id;
        [SerializeField] private int level;
        [SerializeField] private CardLevelData data;

        public CardVariantRecord(string cardId, int cardLevel, CardLevelData levelData)
        {
            id = cardId;
            level = cardLevel;
            data = levelData;
        }

        public CardVisualData CreateRuntimeCard(Sprite enemyBack)
        {
            return new CardVisualData(
                id,
                level,
                data.ToCard(),
                data.Image,
                enemyBack);
        }
    }

    [Serializable]
    public sealed class CardLevelData
    {
        [SerializeField] private Sprite image;
        [SerializeField, Range(1, 9)] private int north = 1;
        [SerializeField, Range(1, 9)] private int east = 1;
        [SerializeField, Range(1, 9)] private int south = 1;
        [SerializeField, Range(1, 9)] private int west = 1;

        public CardLevelData(Sprite sprite, int northValue, int eastValue, int southValue, int westValue)
        {
            image = sprite;
            north = northValue;
            east = eastValue;
            south = southValue;
            west = westValue;
        }

        public Sprite Image => image;

        public Card ToCard()
        {
            return new Card(north, east, south, west);
        }
    }

    public sealed class CardVisualData
    {
        public CardVisualData(string cardId, int level, Card values, Sprite frontSprite, Sprite backSprite)
        {
            Id = cardId;
            Level = level;
            Values = values;
            FrontSprite = frontSprite;
            BackSprite = backSprite;
        }

        public string Id { get; }
        public int Level { get; }
        public Card Values { get; }
        public Sprite FrontSprite { get; }
        public Sprite BackSprite { get; }
    }
}
