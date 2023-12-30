using System.Numerics;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace AdventOfCode2023.Day_24;

public class Day24(ITestOutputHelper testOutputHelper)
{
    private readonly string _demoInput = @"19, 13, 30 @ -2,  1, -2
18, 19, 22 @ -1, -1, -2
20, 25, 34 @ -2, -2, -4
12, 31, 28 @ -1, -2, -1
20, 19, 15 @  1, -5, -3";


    [Fact]
    public async Task DemoInputTest()
    {
        var minValue = BigInteger.Parse("7");
        var maxValue = BigInteger.Parse("27");

        Assert.Equal(2, FindAllCollisionsXy(Parse(_demoInput.Split("\n")), minValue, maxValue));
    }

    [Fact]
    public async Task DemoInputTest2()
    {
        var a = new Hailstone("19, 13, 30 @ -2, 1, -2");
        var b = new Hailstone("18, 19, 22 @ -1, -1, -2");

        var minValue = BigInteger.Parse("7");
        var maxValue = BigInteger.Parse("27");

        Assert.True(a.CheckForCollisionInArea(b, minValue, maxValue));
    }

    [Fact]
    public async Task Silver1Test()
    {
        var data = await File.ReadAllLinesAsync("Day 24/Day24.txt");
        var sum = FindAllCollisionsXy(Parse(data), BigInteger.Parse("200000000000000"), BigInteger.Parse("400000000000000"));
        Assert.Equal(sum, 19976);
        testOutputHelper.WriteLine(sum.ToString());
    }

    [Fact]
    public async Task DemoInputTestPart2()
    {
        var hailstones = Parse(_demoInput.Split("\n"));
        var parallels = FindGoldenThrowOrigin(hailstones);
    }


    [Fact]
    public async Task GoldTest()
    {
        var data = await File.ReadAllLinesAsync("Day 24/Day24.txt");
        var hailstones = Parse(data);
        var sum = FindGoldenThrowOrigin(hailstones);
        testOutputHelper.WriteLine(sum.ToString());
    }


    private object FindGoldenThrowOrigin(IReadOnlyList<Hailstone> hailstones)
    {

        var hailstonesToXprojection =
            hailstones.Select(x => new VelocityPos() { Position = x.PX, Velocity = x.VX }).ToList();

        var speedX = FindVelocity(hailstonesToXprojection);
        // there are some hailstones having (thankfully) exact my speed, so I can locate X-Position
        var posX = hailstonesToXprojection.First(x => x.Velocity == speedX).Position;

        var collisionsX = hailstones.Where(x => x.VX != speedX && x.PX != posX).Take(2).ToList();
        var timeOfCollision0 = GetTimeToCollide(posX, speedX, new VelocityPos() { Position = collisionsX[0].PX, Velocity = collisionsX[0].VX });
        var timeOfCollision1 = GetTimeToCollide(posX, speedX, new VelocityPos() { Position = collisionsX[1].PX, Velocity = collisionsX[1].VX });
        
        var speedY = (collisionsX[1].GetPositionForTime(timeOfCollision1).y - collisionsX[0].GetPositionForTime(timeOfCollision0).y) / (timeOfCollision1 - timeOfCollision0);
        var posY = collisionsX[0].GetPositionForTime(timeOfCollision0).y - timeOfCollision0 * speedY;


        var speedZ = (collisionsX[1].GetPositionForTime(timeOfCollision1).z - collisionsX[0].GetPositionForTime(timeOfCollision0).z) / (timeOfCollision1 - timeOfCollision0);
        var posZ = collisionsX[0].GetPositionForTime(timeOfCollision0).z - timeOfCollision0 * speedZ;

        return posX + posY + posZ;
    }

    private double GetTimeToCollide(double posX, int speedX, VelocityPos velocityPos)
    {
        return (velocityPos.Position - posX) / (speedX - velocityPos.Velocity);
    }

    private int FindVelocity(List<VelocityPos> axisProjection)
    {
        for (int velocity = 1; velocity < 100_000_000; velocity++)
        {
            foreach (var hailstone in axisProjection)
            {
                VelocityPos spaceShuttlePositionPos = hailstone.CalculateSpaceShuttlePosition(velocity);
                if (CheckThatAllWillBeHit(axisProjection, spaceShuttlePositionPos))
                {
                    return velocity;
                }
                var spaceShuttlePositionPosNegativeSpeed = hailstone.CalculateSpaceShuttlePosition(-velocity);
                if (CheckThatAllWillBeHit(axisProjection, spaceShuttlePositionPosNegativeSpeed))
                {
                    return -velocity ;
                }
            }
        }

        throw new NotSupportedException("nothing found");
    }

    private bool CheckThatAllWillBeHit(IEnumerable<VelocityPos> hailstonesToCheck, VelocityPos velocityPos)
    {
        return hailstonesToCheck.All(x => x.CollidesWith(velocityPos));
    }

    private IReadOnlyList<Hailstone> Parse(string[] input)
    {
        return input.Select(x => new Hailstone(x.Trim())).ToList();
    }

    private int FindAllCollisionsXy(IReadOnlyList<Hailstone> hailstones, BigInteger minValue, BigInteger maxValue)
    {
        var collisions = 0;
        for (int i = 0; i < hailstones.Count - 1; i++)
        {
            for (int ii = i + 1; ii < hailstones.Count; ii++)
            {
                var a = hailstones[i];
                var b = hailstones[ii];
                if (a.CheckForCollisionInArea(b, minValue, maxValue))
                {
                    collisions++;
                }
            }
        }
        return collisions;
    }
}

internal class VelocityPos
{
    public required double Velocity { get; init; }
    public required double Position { get; init; }

    public bool CollidesWith(VelocityPos velocityPos)
    {
        if (Math.Abs(Velocity - velocityPos.Velocity) < 0.0001)
        {
            return true;
        }

        // var timeOfCollision = (velocityPos.Position - Position) / (Velocity - velocityPos.Velocity);
        var exactCollision = (velocityPos.Position - Position) % (Velocity - velocityPos.Velocity) == 0;

        return exactCollision; // && timeOfCollision >= 0
    }

    public VelocityPos CalculateSpaceShuttlePosition(double assumedVelocityOfSpaceShuttle)
    {
        return new VelocityPos()
        {
            Velocity = assumedVelocityOfSpaceShuttle,
            Position = Position + Velocity - assumedVelocityOfSpaceShuttle
        };
    }
}

internal class Hailstone
{
    public double PX { get; }
    public double PY { get; }
    public double PZ { get; }

    public double VX { get; }
    public double VY { get; }
    public double VZ { get; }

    public Hailstone(string line)
    {
        string pattern = @"-?\d+";
        var matches = Regex.Matches(line, pattern);
        PX = double.Parse(matches[0].Value);
        PY = double.Parse(matches[1].Value);
        PZ = double.Parse(matches[2].Value);
        VX = int.Parse(matches[3].Value);
        VY = int.Parse(matches[4].Value);
        VZ = int.Parse(matches[5].Value);
    }

    public bool CheckForCollisionInArea(Hailstone other, BigInteger minValue, BigInteger maxValue)
    {
        var collision = CollidesWithXy(other);
        if (collision == null)
        {
            return false;
        }

        if (collision.TimeHailstoneAReachCollision < 0 || collision.TimeHailstoneBReachCollision < 0)
        {
            return false;
        }

        return (double)minValue <= collision.CollisionPointX && collision.CollisionPointX <= (double)maxValue &&
               (double)minValue <= collision.CollisionPointY && collision.CollisionPointY <= (double)maxValue;
    }

    public Collision2d? CollidesWithXy(Hailstone other)
    {
        if (VX == 0)
        {
            throw new NotSupportedException();
        }

        var dYdx1 = VY / VX;
        var dYdX2 = other.VY / other.VX;

        if (dYdx1 == dYdX2)
        {
            return null;
        }

        var b1 = PY - dYdx1 * PX;
        var b2 = other.PY - dYdX2 * other.PX;

        var collisionPointX = (b2 - b1) / (dYdx1 - dYdX2);
        var timeHailstoneAReachCollision = (collisionPointX - PX) / VX;
        var timeHailstoneBReachCollision = (collisionPointX - other.PX) / other.VX;


        return new Collision2d()
        {
            TimeHailstoneAReachCollision = timeHailstoneAReachCollision,
            TimeHailstoneBReachCollision = timeHailstoneBReachCollision,
            CollisionPointX = collisionPointX,
            CollisionPointY = GetPositionForTime(timeHailstoneAReachCollision).Item2,
        };
    }

    public (double  x, double y, double z) GetPositionForTime(double time)
    {
        return (PX + time * VX, PY + time * VY, PZ + time * VZ);
    }

    public double GetDistanceTo(Hailstone other)
    {
        return Math.Sqrt(Math.Pow(PX - other.PX, 2) + Math.Pow(PY - other.PY, 2) + Math.Pow(PZ - other.PZ, 2));
    }
}

class Collision2d
{
    public required double TimeHailstoneAReachCollision { get; init; }
    public required double TimeHailstoneBReachCollision { get; init; }
    public required double CollisionPointX { get; init; }
    public required double CollisionPointY { get; init; }
}
