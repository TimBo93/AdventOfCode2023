using Xunit.Abstractions;

namespace AdventOfCode2023.Day_07;

public class Day7(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void DemoInputPart1()
    {
        var data = @"32T3K 765
T55J5 684
KK677 28
KTJJT 220
QQQJA 483".Split("\n");

        var result = Day7SilverCalc(data);

        Assert.Equal(6440, result);
    }

    [Fact]
    public void DemoInputPart2()
    {
        var data = @"32T3K 765
T55J5 684
KK677 28
KTJJT 220
QQQJA 483".Split("\n");

        var result = Day7GoldCalc(data);

        Assert.Equal(5905, result);
    }

    [Fact]
    public async Task Day7Silver()
    {
        var data = await File.ReadAllLinesAsync("Day 07/Day7.txt");
        var result = Day7SilverCalc(data);
        testOutputHelper.WriteLine(result.ToString());
    }

    [Fact]
    public async Task Day7Gold()
    {
        var data = await File.ReadAllLinesAsync("Day 07/Day7.txt");
        var result = Day7GoldCalc(data);
        testOutputHelper.WriteLine(result.ToString());
    }

    private static decimal Day7SilverCalc(string[] data)
    {
        var hands = data.Select(x =>
        {
            var split = x.Split(' ');
            var cards = split[0];
            var bid = decimal.Parse(split[1]);
            return new Hand(cards, bid, false);
        }).ToList();

        hands.Sort();

        return hands.Select((h, i) => h.Bid * (i + 1)).Sum();
    }

    private static decimal Day7GoldCalc(string[] data)
    {
        var hands = data.Select(x =>
        {
            var split = x.Split(' ');
            var cards = split[0];
            var bid = decimal.Parse(split[1]);
            return new Hand(cards, bid, true);
        }).ToList();

        hands.Sort();

        return hands.Select((h, i) => h.Bid * (i + 1)).Sum();
    }
}

internal class Hand : IComparable<Hand>
{
    private readonly List<(char card, int amount)> _groups;
    private readonly string _hand;
    private readonly bool _applyJoker;

    public Hand(string hand, decimal bid, bool applyJoker)
    {
        Bid = bid;
        _applyJoker = applyJoker;
        _hand = hand;
        _groups = _hand.GroupBy(x => x).Select(x => (x.Key, x.Count())).OrderByDescending(x => x.Item2).ToList();

        if (applyJoker)
        {
            var jokerGroup = _groups.Find(x => x.card == 'J');
            if (jokerGroup.amount != 0)
            {
                _groups.Remove(jokerGroup);
                if (_groups.Count == 0)
                {
                    _groups.Add(('A', 5));
                }
                else
                {
                    _groups[0] = (_groups[0].card, _groups[0].amount + jokerGroup.amount);
                }
            }
        }
    }

    public decimal Bid { get; }

    internal static readonly char[] CardValuePart1 = { 'A', 'K', 'Q', 'J', 'T', '9', '8', '7', '6', '5', '4', '3', '2' };
    internal static readonly char[] CardValuePart2 = { 'A', 'K', 'Q', 'T', '9', '8', '7', '6', '5', '4', '3', '2', 'J' };

    public int CompareTo(Hand? other)
    {
        if (other == null) throw new NotImplementedException();

        var primaryScoreSelf = PrimaryScore();
        var primaryScoreOther = other.PrimaryScore();

        if (primaryScoreSelf < primaryScoreOther) return -1;
        if (primaryScoreSelf > primaryScoreOther) return 1;

        for (var i = 0; i < _hand.Length; i++)
        {
            var cardSelf = _hand[i];
            var cardOther = other._hand[i];
            var diff = CardToRank(cardSelf) - CardToRank(cardOther);
            if (diff != 0) return diff;
        }

        return 0;
    }

    private int PrimaryScore()
    {
        if (FiveOfAKind()) return 6;

        if (FourOfAKind()) return 5;

        if (FullHouse()) return 4;

        if (ThreeOfAKind()) return 3;

        if (TwoPair()) return 2;

        if (OnePair()) return 1;

        return 0;
    }

    private bool FiveOfAKind()
    {
        return _groups.Count == 1;
    }

    private bool FourOfAKind()
    {
        return _groups[0].amount == 4;
    }

    private bool FullHouse()
    {
        return _groups[0].amount == 3 && _groups[1].amount == 2;
    }

    private bool ThreeOfAKind()
    {
        return _groups[0].amount == 3;
    }

    private bool TwoPair()
    {
        return _groups[0].amount == 2 && _groups[1].amount == 2;
    }

    private bool OnePair()
    {
        return _groups[0].amount == 2;
    }

    private int CardToRank(char card)
    {
        var rankToUse = _applyJoker ? CardValuePart2 : CardValuePart1;
        return rankToUse.Reverse().ToList()
            .IndexOf(card);
    }
}