using System;
using System.Diagnostics;
using HanoiTowers.Core;

namespace HanoiTowers.Tests;

public static class HanoiTests
{
    public static void RunAllTests()
    {
        Console.WriteLine("=== Running Hanoi Towers Test Suite ===");
        TestOptimalMovesCalculation();
        TestRecursiveSolverAllDiskCounts();
        TestIllegalMoves();
        TestStarRatingRules();
        TestVictoryOnBothRods();
        Console.WriteLine("=== All Tests Passed Successfully! ===");
    }

    private static void TestVictoryOnBothRods()
    {
        var engine = new HanoiEngine(3);
        // Solve to Rod 1
        var moves = HanoiEngine.GenerateOptimalSolution(3, fromRod: 0, toRod: 1, auxRod: 2);
        foreach (var (from, to) in moves)
        {
            engine.TryMove(from, to);
        }
        Debug.Assert(engine.IsSolved(1), "Should be solved on rod 1");
        Debug.Assert(engine.IsSolved(1, 2), "Should be solved on rod 1 or 2");
        Debug.Assert(!engine.IsSolved(2), "Should not be solved on rod 2");
        Console.WriteLine("✓ TestVictoryOnBothRods passed");
    }

    private static void TestOptimalMovesCalculation()
    {
        // Formula: 2^n - 1
        Debug.Assert(StarCalculator.GetOptimalMoves(3) == 7, "3 disks optimal should be 7");
        Debug.Assert(StarCalculator.GetOptimalMoves(4) == 15, "4 disks optimal should be 15");
        Debug.Assert(StarCalculator.GetOptimalMoves(5) == 31, "5 disks optimal should be 31");
        Debug.Assert(StarCalculator.GetOptimalMoves(10) == 1023, "10 disks optimal should be 1023");
        Console.WriteLine("✓ TestOptimalMovesCalculation passed");
    }

    private static void TestRecursiveSolverAllDiskCounts()
    {
        for (int n = 3; n <= 10; n++)
        {
            var engine = new HanoiEngine(n);
            var moves = HanoiEngine.GenerateOptimalSolution(n);
            int expectedMoves = (1 << n) - 1;

            Debug.Assert(moves.Count == expectedMoves, $"Optimal move count mismatch for {n} disks: expected {expectedMoves}, got {moves.Count}");

            foreach (var (from, to) in moves)
            {
                bool canMove = engine.CanMove(from, to);
                Debug.Assert(canMove, $"Illegal move generated in solution for {n} disks: {from} -> {to}");
                bool success = engine.TryMove(from, to);
                Debug.Assert(success, $"Failed to execute move {from} -> {to}");
            }

            Debug.Assert(engine.IsSolved(2), $"Tower was not solved after executing recursive solution for {n} disks");
            Debug.Assert(engine.MovesCount == expectedMoves, "Move count mismatch");
        }
        Console.WriteLine("✓ TestRecursiveSolverAllDiskCounts (disks 3-10) passed");
    }

    private static void TestIllegalMoves()
    {
        var engine = new HanoiEngine(3);
        // Start: Rod 0 has [3, 2, 1], Rod 1 is empty, Rod 2 is empty

        // Cannot move from empty rod
        Debug.Assert(!engine.CanMove(1, 0), "Should not move from empty rod");
        Debug.Assert(!engine.CanMove(2, 0), "Should not move from empty rod");

        // Cannot move to same rod
        Debug.Assert(!engine.CanMove(0, 0), "Cannot move to same rod");

        // Legal move: disk 1 to rod 1
        Debug.Assert(engine.CanMove(0, 1), "Should allow moving disk 1 to rod 1");
        engine.TryMove(0, 1);

        // Now Rod 0 has [3, 2], Rod 1 has [1]
        // Illegal move: moving disk 2 onto disk 1
        Debug.Assert(!engine.CanMove(0, 1), "Should NOT allow moving disk 2 onto smaller disk 1");
        Debug.Assert(!engine.TryMove(0, 1), "TryMove should reject larger onto smaller");

        Console.WriteLine("✓ TestIllegalMoves passed");
    }

    private static void TestStarRatingRules()
    {
        // 3 disks -> optimal = 7
        Debug.Assert(StarCalculator.CalculateStars(7, 3) == 3, "Exact optimal should get 3 stars");
        Debug.Assert(StarCalculator.CalculateStars(9, 3) == 2, "Small deviation should get 2 stars");
        Debug.Assert(StarCalculator.CalculateStars(14, 3) == 1, "Double moves should get 1 star");
        Debug.Assert(StarCalculator.CalculateStars(15, 3) == 0, "> Double moves should get 0 stars");

        Console.WriteLine("✓ TestStarRatingRules passed");
    }
}
