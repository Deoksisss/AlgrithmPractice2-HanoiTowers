using System;
using System.Collections.Generic;

namespace HanoiTowers.Core;

public class HanoiEngine
{
    public const int RodCount = 3;
    private readonly List<int>[] _rods = new List<int>[RodCount];

    public int DiskCount { get; private set; }
    public int MovesCount { get; private set; }
    public IReadOnlyList<int> GetRodDisks(int rodIndex) => _rods[rodIndex].AsReadOnly();

    public HanoiEngine(int diskCount)
    {
        for (int i = 0; i < RodCount; i++)
        {
            _rods[i] = new List<int>();
        }
        Reset(diskCount);
    }

    public void Reset(int diskCount)
    {
        DiskCount = Math.Clamp(diskCount, 3, 10);
        MovesCount = 0;

        for (int i = 0; i < RodCount; i++)
        {
            _rods[i].Clear();
        }

        // Rod 0 (A) gets all disks from largest (DiskCount) to smallest (1)
        for (int size = DiskCount; size >= 1; size--)
        {
            _rods[0].Add(size);
        }
    }

    public int GetTopDisk(int rodIndex)
    {
        if (rodIndex < 0 || rodIndex >= RodCount) return -1;
        var rod = _rods[rodIndex];
        return rod.Count > 0 ? rod[^1] : -1;
    }

    public bool CanMove(int fromRod, int toRod)
    {
        if (fromRod < 0 || fromRod >= RodCount || toRod < 0 || toRod >= RodCount)
            return false;
        if (fromRod == toRod)
            return false;

        var source = _rods[fromRod];
        if (source.Count == 0)
            return false;

        var target = _rods[toRod];
        if (target.Count == 0)
            return true;

        int diskToMove = source[^1];
        int targetTopDisk = target[^1];

        // Valid only if moving smaller disk onto larger disk
        return diskToMove < targetTopDisk;
    }

    public bool TryMove(int fromRod, int toRod)
    {
        if (!CanMove(fromRod, toRod))
            return false;

        int disk = _rods[fromRod][^1];
        _rods[fromRod].RemoveAt(_rods[fromRod].Count - 1);
        _rods[toRod].Add(disk);
        MovesCount++;
        return true;
    }

    public bool IsSolved(int targetRod = 2)
    {
        if (targetRod < 0 || targetRod >= RodCount) return false;
        var rod = _rods[targetRod];
        if (rod.Count != DiskCount) return false;

        // Verify correct order (bottom should be DiskCount down to 1 at top)
        for (int i = 0; i < DiskCount; i++)
        {
            if (rod[i] != DiskCount - i)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Classic recursive algorithm for optimal solution in minimum moves (2^n - 1)
    /// </summary>
    public static List<(int from, int to)> GenerateOptimalSolution(int diskCount, int fromRod = 0, int toRod = 2, int auxRod = 1)
    {
        var moves = new List<(int from, int to)>((1 << diskCount) - 1);
        SolveRecursive(diskCount, fromRod, toRod, auxRod, moves);
        return moves;
    }

    private static void SolveRecursive(int n, int from, int to, int aux, List<(int from, int to)> moves)
    {
        if (n <= 0) return;
        // 1. Move n-1 disks from source to auxiliary
        SolveRecursive(n - 1, from, aux, to, moves);
        // 2. Move largest disk from source to target
        moves.Add((from, to));
        // 3. Move n-1 disks from auxiliary to target
        SolveRecursive(n - 1, aux, to, from, moves);
    }
}
