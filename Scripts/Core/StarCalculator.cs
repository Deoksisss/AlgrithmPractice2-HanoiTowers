using System;

namespace HanoiTowers.Core;

public static class StarCalculator
{
    public static int GetOptimalMoves(int diskCount)
    {
        if (diskCount <= 0) return 0;
        // Formula: 2^n - 1
        return (1 << diskCount) - 1;
    }

    public static int CalculateStars(int moves, int diskCount)
    {
        int optimal = GetOptimalMoves(diskCount);
        if (moves <= optimal)
        {
            return 3;
        }

        int twoStarThreshold = optimal + Math.Max(2, (int)Math.Ceiling(optimal * 0.35));
        if (moves <= twoStarThreshold)
        {
            return 2;
        }

        int oneStarThreshold = optimal * 2;
        if (moves <= oneStarThreshold)
        {
            return 1;
        }

        return 0;
    }

    public static (int threeStarMax, int twoStarMax, int oneStarMax) GetStarThresholds(int diskCount)
    {
        int optimal = GetOptimalMoves(diskCount);
        int twoStar = optimal + Math.Max(2, (int)Math.Ceiling(optimal * 0.35));
        int oneStar = optimal * 2;
        return (optimal, twoStar, oneStar);
    }
}
