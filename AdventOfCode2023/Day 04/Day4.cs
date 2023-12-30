using Xunit.Abstractions;

namespace AdventOfCode2023.Day_04;

public class Day4(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void DemoInputPart1()
    {
        var data = @"Card 1: 41 48 83 86 17 | 83 86  6 31 17  9 48 53
Card 2: 13 32 20 16 61 | 61 30 68 82 17 32 24 19
Card 3:  1 21 53 59 44 | 69 82 63 72 16 21 14  1
Card 4: 41 92 73 84 69 | 59 84 76 51 58  5 54 83
Card 5: 87 83 26 28 32 | 88 30 70 12 93 22 82 36
Card 6: 31 18 13 56 72 | 74 77 10 23 35 67 36 11".Split("\n");

        var result = Day4SilverCalc(data);


        Assert.Equal(13, result);
    }

    [Fact]
    public void DemoInputPart2()
    {
        var data = @"Card 1: 41 48 83 86 17 | 83 86  6 31 17  9 48 53
Card 2: 13 32 20 16 61 | 61 30 68 82 17 32 24 19
Card 3:  1 21 53 59 44 | 69 82 63 72 16 21 14  1
Card 4: 41 92 73 84 69 | 59 84 76 51 58  5 54 83
Card 5: 87 83 26 28 32 | 88 30 70 12 93 22 82 36
Card 6: 31 18 13 56 72 | 74 77 10 23 35 67 36 11".Split("\n");

        var result = Day4GoldCalc(data);
        Assert.Equal(30, result);
    }

    [Fact]
    public async Task Day4Silver()
    {
        var data = await File.ReadAllLinesAsync("Day 04/Day4.txt");
        var result = Day4SilverCalc(data);
        testOutputHelper.WriteLine(result.ToString());
    }

    [Fact]
    public async Task Day4Gold()
    {
        var data = await File.ReadAllLinesAsync("Day 04/Day4.txt");
        var result = Day4GoldCalc(data);
        testOutputHelper.WriteLine(result.ToString());
    }

    private decimal Day4SilverCalc(string[] data)
    {
        var cards = data.Select(x => new Card(x));
        return cards.Sum(x => x.Score());
    }

    private decimal Day4GoldCalc(string[] data)
    {
        var cards = data.Select(x => new Card(x)).ToList();
        var cardDeck = new CardDeck(cards);

        for (var i = cards.Min(x => x.CardNumber); i <= cards.Max(x => x.CardNumber); i++)
        {
            var card = cardDeck.Cards[i];
            var amount = card.Amount;

            var score = card.Card.NumMatching();
            for (var ii = i + 1; ii <= i + score; ii++)
            {
                if (cardDeck.Cards.TryGetValue(ii, out var cardToIncrease))
                {
                    cardToIncrease.Amount += amount;
                }
            }
        }

        return cardDeck.Cards.Values.Sum(x => x.Amount);
    }
}

internal class CardDeck
{
    public CardDeck(IEnumerable<Card> cards)
    {
        Cards = cards.Select(x => new CardAmount(x)
        {
            Amount = 1
        }).ToDictionary(x => x.Card.CardNumber);
    }

    public IReadOnlyDictionary<int, CardAmount> Cards { get; set; }
}

internal class CardAmount(Card card)
{
    public decimal Amount { get; set; }
    public Card Card { get; } = card;
}

internal class Card
{
    private readonly IReadOnlyList<int> _myNumbers;
    private readonly IReadOnlyList<int> _winningNumbers;

    public Card(string line)
    {
        var cardSplit = line.Split(":");
        CardNumber = int.Parse(cardSplit[0].Split(" ", StringSplitOptions.RemoveEmptyEntries).Last());

        var dataSplit = cardSplit[1].Split("|");

        _myNumbers = dataSplit[0].Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
        _winningNumbers = dataSplit[1].Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
    }

    public int CardNumber { get; }

    public int Score()
    {
        var numMatching = NumMatching();
        if (numMatching == 0) return 0;

        return 1 << (numMatching - 1);
    }

    public int NumMatching()
    {
        return _myNumbers.Count(x => _winningNumbers.Contains(x));
    }
}