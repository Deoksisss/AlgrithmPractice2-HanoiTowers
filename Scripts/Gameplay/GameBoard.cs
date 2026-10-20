using System;
using System.Collections.Generic;
using Godot;
using HanoiTowers.Core;

namespace HanoiTowers.Gameplay;

public partial class GameBoard : Control
{
    [Signal]
    public delegate void MoveMadeEventHandler(int currentMoves, int optimalMoves, int stars);

    [Signal]
    public delegate void GameWonEventHandler(int moves, int optimalMoves, int stars, float timeSeconds);

    [Signal]
    public delegate void AutoPlayStateChangedEventHandler(bool isPlaying, bool isFinished, int currentStep, int totalSteps);

    public int DiskCount { get; private set; } = 3;
    public bool IsAutoMode { get; private set; } = false;
    public float AutoSpeedMultiplier { get; set; } = 1.0f;

    private HanoiEngine _engine = null!;
    private readonly RodView[] _rods = new RodView[3];
    private readonly Dictionary<int, DiskView> _disksBySize = new();
    private DiskView? _activeDraggedDisk;
    private int _hoveredRodIndex = -1;

    // Click-to-move support
    private int _selectedRodForClick = -1;
    private DiskView? _selectedDiskForClick;

    // Auto mode state
    private List<(int from, int to)> _autoSolution = new();
    private int _autoStepIndex = 0;
    private bool _isAutoPlaying = false;
    private bool _isAnimating = false;
    private float _timerSeconds = 0f;
    private bool _timerRunning = false;

    private Control _rodsContainer = null!;
    private Control _disksContainer = null!;

    public override void _Ready()
    {
        _rodsContainer = new Control { Name = "RodsContainer", AnchorRight = 1.0f, AnchorBottom = 1.0f };
        _disksContainer = new Control { Name = "DisksContainer", AnchorRight = 1.0f, AnchorBottom = 1.0f };
        AddChild(_rodsContainer);
        AddChild(_disksContainer);

        SetupRods();
        Resized += OnBoardResized;
    }

    public override void _Process(double delta)
    {
        if (_timerRunning)
        {
            _timerSeconds += (float)delta;
        }

        if (IsAutoMode && _isAutoPlaying && !_isAnimating)
        {
            ExecuteNextAutoStep();
        }
    }

    private void SetupRods()
    {
        string[] labels = { "Башня A (Старт)", "Башня B (Буфер)", "Башня C (Финиш)" };
        for (int i = 0; i < 3; i++)
        {
            var rod = new RodView
            {
                Name = $"Rod_{i}",
                RodIndex = i,
                RodName = labels[i]
            };
            rod.RodClicked += OnRodClicked;
            _rods[i] = rod;
            _rodsContainer.AddChild(rod);
        }
    }

    private void OnBoardResized()
    {
        LayoutRods();
        RepositionAllDisksInstant();
    }

    private void LayoutRods()
    {
        float totalWidth = Size.X;
        float boardHeight = Size.Y;

        // Position rods evenly across width
        float[] xCenters = { totalWidth * 0.20f, totalWidth * 0.50f, totalWidth * 0.80f };

        for (int i = 0; i < 3; i++)
        {
            var rod = _rods[i];
            float rodW = rod.BaseWidth;
            float rodH = rod.PillarHeight + rod.BaseHeight + 50f;
            float x = xCenters[i] - (rodW / 2f);
            float y = Math.Max(40f, boardHeight - rodH - 30f);
            rod.Position = new Vector2(x, y);
        }
    }

    public void StartManualGame(int diskCount)
    {
        IsAutoMode = false;
        _isAutoPlaying = false;
        DiskCount = Math.Clamp(diskCount, 3, 10);
        _engine = new HanoiEngine(DiskCount);

        _timerSeconds = 0f;
        _timerRunning = true;
        _selectedRodForClick = -1;
        _selectedDiskForClick = null;

        RebuildDisks();
        UpdateDraggableState();
        LayoutRods();
        RepositionAllDisksInstant();

        EmitSignal(SignalName.MoveMade, 0, StarCalculator.GetOptimalMoves(DiskCount), 3);
    }

    public void StartAutoGame(int diskCount)
    {
        IsAutoMode = true;
        _isAutoPlaying = false;
        DiskCount = Math.Clamp(diskCount, 3, 10);
        _engine = new HanoiEngine(DiskCount);

        _timerSeconds = 0f;
        _timerRunning = false;
        _selectedRodForClick = -1;
        _selectedDiskForClick = null;

        _autoSolution = HanoiEngine.GenerateOptimalSolution(DiskCount);
        _autoStepIndex = 0;

        RebuildDisks();
        UpdateDraggableState(); // In auto mode, disks are not draggable by user
        LayoutRods();
        RepositionAllDisksInstant();

        EmitSignal(SignalName.AutoPlayStateChanged, _isAutoPlaying, false, _autoStepIndex, _autoSolution.Count);
        EmitSignal(SignalName.MoveMade, 0, _autoSolution.Count, 3);
    }

    private void RebuildDisks()
    {
        foreach (var disk in _disksBySize.Values)
        {
            disk.QueueFree();
        }
        _disksBySize.Clear();

        float diskHeight = DiskCount switch
        {
            <= 4 => 32f,
            <= 6 => 28f,
            <= 8 => 24f,
            _ => 20f
        };

        for (int size = 1; size <= DiskCount; size++)
        {
            var disk = new DiskView();
            disk.Setup(size, DiskCount, _rods[0].BaseWidth, diskHeight);
            disk.CurrentRod = 0;

            disk.DragStarted += OnDiskDragStarted;
            disk.DragUpdated += OnDiskDragUpdated;
            disk.DragEnded += OnDiskDragEnded;

            _disksContainer.AddChild(disk);
            _disksBySize[size] = disk;
        }
    }

    private void RepositionAllDisksInstant()
    {
        if (_engine == null) return;

        float diskHeight = DiskCount switch
        {
            <= 4 => 32f,
            <= 6 => 28f,
            <= 8 => 24f,
            _ => 20f
        };

        for (int rodIdx = 0; rodIdx < 3; rodIdx++)
        {
            var rodDisks = _engine.GetRodDisks(rodIdx);
            var rod = _rods[rodIdx];

            for (int slot = 0; slot < rodDisks.Count; slot++)
            {
                int diskSize = rodDisks[slot];
                if (_disksBySize.TryGetValue(diskSize, out var diskView))
                {
                    Vector2 localPosOnRod = rod.GetDiskSpawnPosition(slot, diskHeight);
                    // Disk origin is centered horizontally
                    Vector2 globalPos = rod.Position + localPosOnRod - new Vector2(diskView.Size.X / 2f, 0);
                    diskView.Position = globalPos;
                    diskView.SetHomePosition(globalPos);
                    diskView.CurrentRod = rodIdx;
                }
            }
        }
    }

    private void UpdateDraggableState()
    {
        foreach (var disk in _disksBySize.Values)
        {
            disk.IsDraggable = false;
        }

        if (IsAutoMode) return;

        for (int rodIdx = 0; rodIdx < 3; rodIdx++)
        {
            int topDiskSize = _engine.GetTopDisk(rodIdx);
            if (topDiskSize > 0 && _disksBySize.TryGetValue(topDiskSize, out var diskView))
            {
                diskView.IsDraggable = true;
            }
        }
    }

    #region Drag-and-Drop Handling

    private void OnDiskDragStarted(DiskView disk)
    {
        _activeDraggedDisk = disk;
        _hoveredRodIndex = -1;
        SoundManager.Instance?.PlayPickup();

        // Cancel any click selection if in progress
        if (_selectedDiskForClick != null && _selectedDiskForClick != disk)
        {
            _selectedDiskForClick.Position = _selectedDiskForClick.GetHomePosition();
            _selectedDiskForClick = null;
            _selectedRodForClick = -1;
        }
    }

    private void OnDiskDragUpdated(DiskView disk, Vector2 globalMousePos)
    {
        int bestRod = FindClosestRod(globalMousePos);
        if (bestRod != _hoveredRodIndex)
        {
            ClearAllRodHighlights();
            _hoveredRodIndex = bestRod;
            if (_hoveredRodIndex >= 0)
            {
                bool isValid = _engine.CanMove(disk.CurrentRod, _hoveredRodIndex);
                _rods[_hoveredRodIndex].SetDropHighlight(true, isValid);
            }
        }
    }

    private void OnDiskDragEnded(DiskView disk, Vector2 globalMousePos)
    {
        ClearAllRodHighlights();
        int targetRod = FindClosestRod(globalMousePos);

        if (targetRod >= 0 && targetRod != disk.CurrentRod && _engine.CanMove(disk.CurrentRod, targetRod))
        {
            // Valid Move!
            ExecuteManualMove(disk.CurrentRod, targetRod);
        }
        else
        {
            // Invalid Move or cancelled
            SoundManager.Instance?.PlayError();
            disk.ShakeAndReturn(disk.GetHomePosition());
        }

        _activeDraggedDisk = null;
        _hoveredRodIndex = -1;
    }

    private int FindClosestRod(Vector2 globalPos)
    {
        int closest = -1;
        float minDistance = float.MaxValue;

        for (int i = 0; i < 3; i++)
        {
            var rod = _rods[i];
            float rodCenterX = rod.Position.X + (rod.Size.X / 2f);
            float dist = MathF.Abs(globalPos.X - rodCenterX);

            // Accept drop if horizontal distance within rod base width + 40px
            if (dist < (rod.BaseWidth / 2f + 40f))
            {
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = i;
                }
            }
        }

        return closest;
    }

    private void ClearAllRodHighlights()
    {
        for (int i = 0; i < 3; i++)
        {
            _rods[i].ClearHighlight();
        }
    }

    #endregion

    #region Click-to-Move Alternative

    private void OnRodClicked(int rodIndex)
    {
        if (IsAutoMode || _isAnimating) return;

        if (_selectedRodForClick == -1)
        {
            // First click: select source rod
            int topDiskSize = _engine.GetTopDisk(rodIndex);
            if (topDiskSize > 0 && _disksBySize.TryGetValue(topDiskSize, out var disk))
            {
                _selectedRodForClick = rodIndex;
                _selectedDiskForClick = disk;
                SoundManager.Instance?.PlayPickup();
                // Lift disk slightly up the rod
                var liftedPos = disk.GetHomePosition() - new Vector2(0, 40f);
                disk.AnimateTo(liftedPos, 0.15f);
                _rods[rodIndex].SetDropHighlight(true, true);
            }
        }
        else
        {
            // Second click: target rod
            int fromRod = _selectedRodForClick;
            int toRod = rodIndex;
            var disk = _selectedDiskForClick!;

            ClearAllRodHighlights();
            _selectedRodForClick = -1;
            _selectedDiskForClick = null;

            if (toRod != fromRod && _engine.CanMove(fromRod, toRod))
            {
                ExecuteManualMove(fromRod, toRod);
            }
            else
            {
                // Return to base position
                if (toRod != fromRod)
                {
                    SoundManager.Instance?.PlayError();
                }
                disk.AnimateTo(disk.GetHomePosition(), 0.15f);
            }
        }
    }

    #endregion

    private void ExecuteManualMove(int fromRod, int toRod)
    {
        _isAnimating = true;
        int diskSize = _engine.GetTopDisk(fromRod);
        var diskView = _disksBySize[diskSize];

        _engine.TryMove(fromRod, toRod);
        SoundManager.Instance?.PlayDrop();

        // Calculate landing position on target rod
        int newSlot = _engine.GetRodDisks(toRod).Count - 1;
        float diskHeight = diskView.Size.Y;
        var targetRodView = _rods[toRod];
        Vector2 localSlotPos = targetRodView.GetDiskSpawnPosition(newSlot, diskHeight);
        Vector2 targetGlobalPos = targetRodView.Position + localSlotPos - new Vector2(diskView.Size.X / 2f, 0);

        diskView.CurrentRod = toRod;
        diskView.AnimateTo(targetGlobalPos, 0.2f, () =>
        {
            _isAnimating = false;
            UpdateDraggableState();

            int optimal = StarCalculator.GetOptimalMoves(DiskCount);
            int stars = StarCalculator.CalculateStars(_engine.MovesCount, DiskCount);
            EmitSignal(SignalName.MoveMade, _engine.MovesCount, optimal, stars);

            if (_engine.IsSolved(2))
            {
                _timerRunning = false;
                SoundManager.Instance?.PlayVictory();
                EmitSignal(SignalName.GameWon, _engine.MovesCount, optimal, stars, _timerSeconds);
            }
        });
    }

    #region Auto Mode Operations

    public void ToggleAutoPlay()
    {
        if (!IsAutoMode) return;
        _isAutoPlaying = !_isAutoPlaying;
        EmitSignal(SignalName.AutoPlayStateChanged, _isAutoPlaying, _autoStepIndex >= _autoSolution.Count, _autoStepIndex, _autoSolution.Count);
    }

    public void StepForwardAuto()
    {
        if (!IsAutoMode || _isAnimating || _autoStepIndex >= _autoSolution.Count) return;
        ExecuteNextAutoStep();
    }

    public void ResetAuto()
    {
        if (!IsAutoMode) return;
        _isAutoPlaying = false;
        _isAnimating = false;
        _autoStepIndex = 0;
        _engine.Reset(DiskCount);
        RepositionAllDisksInstant();
        EmitSignal(SignalName.AutoPlayStateChanged, false, false, 0, _autoSolution.Count);
        EmitSignal(SignalName.MoveMade, 0, _autoSolution.Count, 3);
    }

    private void ExecuteNextAutoStep()
    {
        if (_autoStepIndex >= _autoSolution.Count)
        {
            _isAutoPlaying = false;
            EmitSignal(SignalName.AutoPlayStateChanged, false, true, _autoStepIndex, _autoSolution.Count);
            return;
        }

        var (fromRod, toRod) = _autoSolution[_autoStepIndex];
        int diskSize = _engine.GetTopDisk(fromRod);
        if (diskSize <= 0 || !_disksBySize.TryGetValue(diskSize, out var diskView))
            return;

        _isAnimating = true;
        _engine.TryMove(fromRod, toRod);
        _autoStepIndex++;

        // Calculate positions
        var fromRodView = _rods[fromRod];
        var toRodView = _rods[toRod];
        int targetSlot = _engine.GetRodDisks(toRod).Count - 1;
        float diskHeight = diskView.Size.Y;

        Vector2 localSlotPos = toRodView.GetDiskSpawnPosition(targetSlot, diskHeight);
        Vector2 targetGlobalPos = toRodView.Position + localSlotPos - new Vector2(diskView.Size.X / 2f, 0);

        // Peak point above the rods for graceful arc movement
        float peakY = MathF.Min(fromRodView.Position.Y, toRodView.Position.Y) - 30f;
        float midX = (diskView.Position.X + targetGlobalPos.X) / 2f;
        Vector2 peakPos = new Vector2(midX, peakY);

        float baseDuration = 0.35f / MathF.Max(0.1f, AutoSpeedMultiplier);
        // Instant speed bypass
        if (AutoSpeedMultiplier >= 15f)
        {
            diskView.Position = targetGlobalPos;
            diskView.SetHomePosition(targetGlobalPos);
            diskView.CurrentRod = toRod;
            _isAnimating = false;
            OnAutoStepFinished();
        }
        else
        {
            diskView.CurrentRod = toRod;
            SoundManager.Instance?.PlayDrop();
            diskView.AnimateArcTo(peakPos, targetGlobalPos, baseDuration, () =>
            {
                _isAnimating = false;
                OnAutoStepFinished();
            });
        }
    }

    private void OnAutoStepFinished()
    {
        int optimal = _autoSolution.Count;
        int stars = StarCalculator.CalculateStars(_engine.MovesCount, DiskCount);
        EmitSignal(SignalName.MoveMade, _engine.MovesCount, optimal, stars);

        bool isFinished = _autoStepIndex >= _autoSolution.Count;
        EmitSignal(SignalName.AutoPlayStateChanged, _isAutoPlaying, isFinished, _autoStepIndex, optimal);

        if (isFinished)
        {
            _isAutoPlaying = false;
            SoundManager.Instance?.PlayVictory();
            EmitSignal(SignalName.GameWon, _engine.MovesCount, optimal, 3, _timerSeconds);
        }
    }

    #endregion

    public float GetElapsedTime() => _timerSeconds;
}
