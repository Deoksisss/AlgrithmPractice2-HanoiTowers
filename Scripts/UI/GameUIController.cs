using System;
using Godot;
using HanoiTowers.Core;
using HanoiTowers.Gameplay;

namespace HanoiTowers.UI;

public partial class GameUIController : Control
{
    [Signal]
    public delegate void BackToMenuRequestedEventHandler();

    private GameBoard _gameBoard = null!;
    private VictoryModalController _victoryModal = null!;

    // Top bar elements
    private Label _titleLabel = null!;
    private Label _movesLabel = null!;
    private Label _optimalLabel = null!;
    private Label _starsPreviewLabel = null!;
    private Label _timerLabel = null!;
    private Button _soundBtn = null!;
    private Button _restartBtn = null!;
    private Button _backBtn = null!;

    // Auto mode bar elements
    private PanelContainer _autoBarPanel = null!;
    private Button _playPauseBtn = null!;
    private Button _stepBtn = null!;
    private Button _resetAutoBtn = null!;
    private Label _autoProgressLabel = null!;
    private OptionButton _speedOptionBtn = null!;

    private int _currentLevel = -1; // -1 if Free Play
    private int _currentDiskCount = 3;
    private bool _isAutoMode = false;

    public override void _Ready()
    {
        SetupUI();
    }

    public void InitializeManualGame(int diskCount, int levelNumber = -1)
    {
        _currentLevel = levelNumber;
        _currentDiskCount = diskCount;
        _isAutoMode = false;

        _autoBarPanel.Visible = false;
        if (_currentLevel > 0)
        {
            _titleLabel.Text = $"Кампания: Уровень {_currentLevel} ({diskCount} дисков)";
        }
        else
        {
            _titleLabel.Text = $"Свободная игра ({diskCount} дисков)";
        }

        _gameBoard.StartManualGame(diskCount);
    }

    public void InitializeAutoGame(int diskCount)
    {
        _currentLevel = -1;
        _currentDiskCount = diskCount;
        _isAutoMode = true;

        _autoBarPanel.Visible = true;
        _titleLabel.Text = $"Компьютерное решение ({diskCount} дисков)";
        _playPauseBtn.Text = "▶ Запуск";

        _gameBoard.StartAutoGame(diskCount);
    }

    private void SetupUI()
    {
        AnchorRight = 1.0f;
        AnchorBottom = 1.0f;

        // Background
        var bg = new ColorRect
        {
            Color = new Color("090d16"), // Very dark slate / navy
            AnchorRight = 1.0f,
            AnchorBottom = 1.0f
        };
        AddChild(bg);

        // GameBoard
        _gameBoard = new GameBoard
        {
            AnchorRight = 1.0f,
            AnchorBottom = 1.0f,
            OffsetTop = 75,
            OffsetBottom = -90
        };
        _gameBoard.MoveMade += OnMoveMade;
        _gameBoard.GameWon += OnGameWon;
        _gameBoard.AutoPlayStateChanged += OnAutoPlayStateChanged;
        AddChild(_gameBoard);

        // Top Navigation & Stats Bar
        CreateTopBar();

        // Bottom Auto Controls Bar
        CreateAutoBar();

        // Victory Modal
        _victoryModal = new VictoryModalController();
        _victoryModal.NextLevelClicked += OnNextLevelClicked;
        _victoryModal.RetryClicked += OnRestartClicked;
        _victoryModal.MenuClicked += () => EmitSignal(SignalName.BackToMenuRequested);
        AddChild(_victoryModal);
    }

    private void CreateTopBar()
    {
        var topPanel = new PanelContainer
        {
            AnchorRight = 1.0f,
            CustomMinimumSize = new Vector2(0, 68)
        };
        var topStyle = new StyleBoxFlat
        {
            BgColor = new Color("0f172a"),
            BorderWidthBottom = 2,
            BorderColor = new Color("1e293b"),
            ContentMarginLeft = 20,
            ContentMarginRight = 20
        };
        topPanel.AddThemeStyleboxOverride("panel", topStyle);
        AddChild(topPanel);

        var hbox = new HBoxContainer
        {
            Alignment = BoxContainer.AlignmentMode.Begin
        };
        hbox.AddThemeConstantOverride("separation", 16);
        topPanel.AddChild(hbox);

        // Back button
        _backBtn = new Button { Text = "← В меню", CustomMinimumSize = new Vector2(100, 38) };
        _backBtn.Pressed += () => EmitSignal(SignalName.BackToMenuRequested);
        hbox.AddChild(_backBtn);

        // Title
        _titleLabel = new Label
        {
            Text = "Ханойские башни",
            VerticalAlignment = VerticalAlignment.Center
        };
        _titleLabel.AddThemeFontSizeOverride("font_size", 18);
        _titleLabel.AddThemeColorOverride("font_color", new Color("38bdf8"));
        hbox.AddChild(_titleLabel);

        // Spacer
        var spacer = new Control { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        hbox.AddChild(spacer);

        // Stats Container
        var statsHBox = new HBoxContainer();
        statsHBox.AddThemeConstantOverride("separation", 18);

        _movesLabel = new Label { Text = "Ходы: 0", VerticalAlignment = VerticalAlignment.Center };
        _movesLabel.AddThemeFontSizeOverride("font_size", 16);
        _movesLabel.AddThemeColorOverride("font_color", Colors.White);
        statsHBox.AddChild(_movesLabel);

        _optimalLabel = new Label { Text = "Идеал: 7", VerticalAlignment = VerticalAlignment.Center };
        _optimalLabel.AddThemeFontSizeOverride("font_size", 14);
        _optimalLabel.AddThemeColorOverride("font_color", new Color("94a3b8"));
        statsHBox.AddChild(_optimalLabel);

        _starsPreviewLabel = new Label { Text = "⭐⭐⭐", VerticalAlignment = VerticalAlignment.Center };
        _starsPreviewLabel.AddThemeFontSizeOverride("font_size", 16);
        statsHBox.AddChild(_starsPreviewLabel);

        _timerLabel = new Label { Text = "⏱ 00:00", VerticalAlignment = VerticalAlignment.Center };
        _timerLabel.AddThemeFontSizeOverride("font_size", 15);
        _timerLabel.AddThemeColorOverride("font_color", new Color("cbd5e1"));
        statsHBox.AddChild(_timerLabel);

        hbox.AddChild(statsHBox);

        // Spacer 2
        var spacer2 = new Control { CustomMinimumSize = new Vector2(10, 0) };
        hbox.AddChild(spacer2);

        // Restart button
        _restartBtn = new Button { Text = "↺ Заново", CustomMinimumSize = new Vector2(85, 38) };
        _restartBtn.Pressed += OnRestartClicked;
        hbox.AddChild(_restartBtn);

        // Sound toggle
        _soundBtn = new Button
        {
            Text = GameProgress.Current.SoundEnabled ? "🔊 Звук: Вкл" : "🔇 Звук: Выкл",
            CustomMinimumSize = new Vector2(110, 38)
        };
        _soundBtn.Pressed += OnSoundToggleClicked;
        hbox.AddChild(_soundBtn);
    }

    private void CreateAutoBar()
    {
        _autoBarPanel = new PanelContainer
        {
            AnchorTop = 1.0f,
            AnchorRight = 1.0f,
            AnchorBottom = 1.0f,
            OffsetTop = -75,
            CustomMinimumSize = new Vector2(0, 75)
        };
        var bottomStyle = new StyleBoxFlat
        {
            BgColor = new Color("0f172a"),
            BorderWidthTop = 2,
            BorderColor = new Color("1e293b"),
            ContentMarginLeft = 24,
            ContentMarginRight = 24
        };
        _autoBarPanel.AddThemeStyleboxOverride("panel", bottomStyle);
        AddChild(_autoBarPanel);

        var hbox = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        hbox.AddThemeConstantOverride("separation", 16);
        _autoBarPanel.AddChild(hbox);

        _playPauseBtn = new Button { Text = "▶ Старт", CustomMinimumSize = new Vector2(120, 42) };
        _playPauseBtn.Pressed += () => _gameBoard.ToggleAutoPlay();
        hbox.AddChild(_playPauseBtn);

        _stepBtn = new Button { Text = "⏭ Шаг вперёд", CustomMinimumSize = new Vector2(130, 42) };
        _stepBtn.Pressed += () => _gameBoard.StepForwardAuto();
        hbox.AddChild(_stepBtn);

        _resetAutoBtn = new Button { Text = "↺ Сброс", CustomMinimumSize = new Vector2(90, 42) };
        _resetAutoBtn.Pressed += () => _gameBoard.ResetAuto();
        hbox.AddChild(_resetAutoBtn);

        _autoProgressLabel = new Label { Text = "Шаг 0 из 0", VerticalAlignment = VerticalAlignment.Center };
        _autoProgressLabel.AddThemeFontSizeOverride("font_size", 16);
        _autoProgressLabel.AddThemeColorOverride("font_color", new Color("38bdf8"));
        hbox.AddChild(_autoProgressLabel);

        // Speed Dropdown
        var speedLabel = new Label { Text = "Скорость:", VerticalAlignment = VerticalAlignment.Center };
        speedLabel.AddThemeFontSizeOverride("font_size", 14);
        hbox.AddChild(speedLabel);

        _speedOptionBtn = new OptionButton { CustomMinimumSize = new Vector2(120, 38) };
        _speedOptionBtn.AddItem("0.5x (Медленно)", 0);
        _speedOptionBtn.AddItem("1x (Обычная)", 1);
        _speedOptionBtn.AddItem("2x (Быстро)", 2);
        _speedOptionBtn.AddItem("5x (Турбо)", 3);
        _speedOptionBtn.AddItem("Мгновенно", 4);
        _speedOptionBtn.Select(1);
        _speedOptionBtn.ItemSelected += OnSpeedSelected;
        hbox.AddChild(_speedOptionBtn);
    }

    public override void _Process(double delta)
    {
        float elapsed = _gameBoard.GetElapsedTime();
        int mins = (int)(elapsed / 60);
        int secs = (int)(elapsed % 60);
        _timerLabel.Text = $"⏱ {mins:D2}:{secs:D2}";
    }

    private void OnSpeedSelected(long index)
    {
        float speed = index switch
        {
            0 => 0.5f,
            1 => 1.0f,
            2 => 2.0f,
            3 => 5.0f,
            4 => 20.0f,
            _ => 1.0f
        };
        _gameBoard.AutoSpeedMultiplier = speed;
    }

    private void OnMoveMade(int currentMoves, int optimalMoves, int stars)
    {
        _movesLabel.Text = $"Ходы: {currentMoves}";
        _optimalLabel.Text = $"Идеал: {optimalMoves}";
        _starsPreviewLabel.Text = stars switch
        {
            3 => "⭐⭐⭐",
            2 => "⭐⭐",
            1 => "⭐",
            _ => "0 звезд"
        };
    }

    private void OnAutoPlayStateChanged(bool isPlaying, bool isFinished, int currentStep, int totalSteps)
    {
        _playPauseBtn.Text = isPlaying ? "⏸ Пауза" : (isFinished ? "✓ Завершено" : "▶ Старт");
        _autoProgressLabel.Text = $"Шаг {currentStep} из {totalSteps}";
        _stepBtn.Disabled = isPlaying || isFinished;
    }

    private void OnGameWon(int moves, int optimal, int stars, float timeSeconds)
    {
        if (_currentLevel > 0)
        {
            GameProgress.RecordLevelResult(_currentLevel, moves, timeSeconds);
        }
        else if (!_isAutoMode)
        {
            GameProgress.RecordFreePlayResult(_currentDiskCount, moves);
        }

        bool hasNextLevel = (_currentLevel > 0 && _currentLevel < 8);
        _victoryModal.ShowVictory(moves, optimal, stars, timeSeconds, hasNextLevel, _isAutoMode);
    }

    private void OnRestartClicked()
    {
        _victoryModal.Visible = false;
        if (_isAutoMode)
        {
            InitializeAutoGame(_currentDiskCount);
        }
        else
        {
            InitializeManualGame(_currentDiskCount, _currentLevel);
        }
    }

    private void OnNextLevelClicked()
    {
        _victoryModal.Visible = false;
        if (_currentLevel > 0 && _currentLevel < 8)
        {
            int nextLvl = _currentLevel + 1;
            int nextDisks = nextLvl + 2;
            InitializeManualGame(nextDisks, nextLvl);
        }
    }

    private void OnSoundToggleClicked()
    {
        GameProgress.Current.SoundEnabled = !GameProgress.Current.SoundEnabled;
        GameProgress.Save();
        _soundBtn.Text = GameProgress.Current.SoundEnabled ? "🔊 Звук: Вкл" : "🔇 Звук: Выкл";
    }
}
