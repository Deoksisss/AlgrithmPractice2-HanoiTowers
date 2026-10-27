using System;
using Godot;
using HanoiTowers.Core;

namespace HanoiTowers.UI;

public partial class MainMenuController : Control
{
    private Control _menuPanel = null!;
    private Control _freePlayDialog = null!;
    private Control _rulesDialog = null!;

    private LevelSelectController _levelSelect = null!;
    private GameUIController _gameUI = null!;

    // Free Play Config UI
    private HSlider _diskSlider = null!;
    private Label _diskCountValLabel = null!;
    private Label _optimalValLabel = null!;
    private CheckBox _manualModeCheck = null!;
    private CheckBox _autoModeCheck = null!;
    private Button _soundBtn = null!;

    public override void _Ready()
    {
        if (Array.Exists(OS.GetCmdlineArgs(), a => a == "--run-tests"))
        {
            Tests.HanoiTests.RunAllTests();
            GetTree().Quit(0);
            return;
        }

        // Add SoundManager if not present
        if (SoundManager.Instance == null)
        {
            var sm = new SoundManager { Name = "SoundManager" };
            AddChild(sm);
        }

        SetupUI();
    }

    private void SetupUI()
    {
        AnchorRight = 1.0f;
        AnchorBottom = 1.0f;

        // Background
        var bg = new ColorRect
        {
            Color = new Color("090d16"),
            AnchorRight = 1.0f,
            AnchorBottom = 1.0f
        };
        AddChild(bg);

        // Menu View
        CreateMainMenuView();

        // Dialogs
        CreateFreePlayDialog();
        CreateRulesDialog();

        // Level Select View
        _levelSelect = new LevelSelectController { Visible = false };
        _levelSelect.BackToMenu += ShowMainMenu;
        _levelSelect.LevelSelected += OnLevelSelected;
        AddChild(_levelSelect);

        // Game UI View
        _gameUI = new GameUIController { Visible = false };
        _gameUI.BackToMenuRequested += ShowMainMenu;
        AddChild(_gameUI);
    }

    private void CreateMainMenuView()
    {
        _menuPanel = new Control
        {
            AnchorRight = 1.0f,
            AnchorBottom = 1.0f
        };
        AddChild(_menuPanel);

        var center = new CenterContainer
        {
            AnchorRight = 1.0f,
            AnchorBottom = 1.0f
        };
        _menuPanel.AddChild(center);

        var vbox = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        vbox.AddThemeConstantOverride("separation", 24);
        center.AddChild(vbox);

        // Title
        var titleLabel = new Label
        {
            Text = "ХАНОЙСКИЕ БАШНИ",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        titleLabel.AddThemeFontSizeOverride("font_size", 44);
        titleLabel.AddThemeColorOverride("font_color", new Color("38bdf8"));
        vbox.AddChild(titleLabel);

        var subtitleLabel = new Label
        {
            Text = "Классическая математическая головоломка на C#",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        subtitleLabel.AddThemeFontSizeOverride("font_size", 16);
        subtitleLabel.AddThemeColorOverride("font_color", new Color("94a3b8"));
        vbox.AddChild(subtitleLabel);

        vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 16) });

        // Menu buttons
        var btnVBox = new VBoxContainer();
        btnVBox.AddThemeConstantOverride("separation", 14);

        var freePlayBtn = CreateMenuButton("🎮  Свободная игра", new Color("0284c7"));
        freePlayBtn.Pressed += () => _freePlayDialog.Visible = true;
        btnVBox.AddChild(freePlayBtn);

        var campaignBtn = CreateMenuButton("🏆  Кампания (Уровни 1-8)", new Color("0f766e"));
        campaignBtn.Pressed += ShowLevelSelect;
        btnVBox.AddChild(campaignBtn);

        var rulesBtn = CreateMenuButton("📖  Правила и подсказки", new Color("334155"));
        rulesBtn.Pressed += () => _rulesDialog.Visible = true;
        btnVBox.AddChild(rulesBtn);

        var bottomHBox = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        bottomHBox.AddThemeConstantOverride("separation", 10);

        _soundBtn = new Button
        {
            Text = GameProgress.Current.SoundEnabled ? "🔊 Звук: Вкл" : "🔇 Звук: Выкл",
            CustomMinimumSize = new Vector2(130, 40)
        };
        _soundBtn.Pressed += OnSoundToggle;
        bottomHBox.AddChild(_soundBtn);

        var quitBtn = new Button
        {
            Text = "Выход",
            CustomMinimumSize = new Vector2(110, 40)
        };
        quitBtn.Pressed += () => GetTree().Quit();
        bottomHBox.AddChild(quitBtn);

        btnVBox.AddChild(bottomHBox);
        vbox.AddChild(btnVBox);
    }

    private Button CreateMenuButton(string text, Color bgColor)
    {
        var btn = new Button
        {
            Text = text,
            CustomMinimumSize = new Vector2(300, 50)
        };
        btn.AddThemeFontSizeOverride("font_size", 17);

        var normalStyle = new StyleBoxFlat
        {
            BgColor = bgColor,
            CornerRadiusTopLeft = 10,
            CornerRadiusTopRight = 10,
            CornerRadiusBottomLeft = 10,
            CornerRadiusBottomRight = 10,
            ShadowColor = new Color(0, 0, 0, 0.35f),
            ShadowSize = 6,
            ShadowOffset = new Vector2(0, 2)
        };
        var hoverStyle = new StyleBoxFlat
        {
            BgColor = bgColor.Lightened(0.18f),
            CornerRadiusTopLeft = 10,
            CornerRadiusTopRight = 10,
            CornerRadiusBottomLeft = 10,
            CornerRadiusBottomRight = 10,
            ShadowColor = new Color(0, 0, 0, 0.45f),
            ShadowSize = 8,
            ShadowOffset = new Vector2(0, 3)
        };

        btn.AddThemeStyleboxOverride("normal", normalStyle);
        btn.AddThemeStyleboxOverride("hover", hoverStyle);
        btn.AddThemeStyleboxOverride("pressed", hoverStyle);
        return btn;
    }

    private void CreateFreePlayDialog()
    {
        _freePlayDialog = new Control
        {
            AnchorRight = 1.0f,
            AnchorBottom = 1.0f,
            Visible = false
        };
        AddChild(_freePlayDialog);

        var dimBg = new ColorRect
        {
            Color = new Color(0, 0, 0, 0.65f),
            AnchorRight = 1.0f,
            AnchorBottom = 1.0f
        };
        _freePlayDialog.AddChild(dimBg);

        var panel = new PanelContainer
        {
            CustomMinimumSize = new Vector2(460, 400),
            AnchorLeft = 0.5f,
            AnchorTop = 0.5f,
            AnchorRight = 0.5f,
            AnchorBottom = 0.5f,
            OffsetLeft = -230,
            OffsetTop = -200,
            OffsetRight = 230,
            OffsetBottom = 200
        };
        var panelStyle = new StyleBoxFlat
        {
            BgColor = new Color("0f172a"),
            CornerRadiusTopLeft = 14,
            CornerRadiusTopRight = 14,
            CornerRadiusBottomLeft = 14,
            CornerRadiusBottomRight = 14,
            BorderWidthLeft = 2,
            BorderWidthRight = 2,
            BorderWidthTop = 2,
            BorderWidthBottom = 2,
            BorderColor = new Color("38bdf8"),
            ContentMarginLeft = 24,
            ContentMarginRight = 24,
            ContentMarginTop = 20,
            ContentMarginBottom = 20
        };
        panel.AddThemeStyleboxOverride("panel", panelStyle);
        _freePlayDialog.AddChild(panel);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 16);
        panel.AddChild(vbox);

        var title = new Label
        {
            Text = "Параметры свободной игры",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        title.AddThemeFontSizeOverride("font_size", 22);
        title.AddThemeColorOverride("font_color", new Color("38bdf8"));
        vbox.AddChild(title);

        // Disks slider row
        var diskRow = new HBoxContainer();
        var diskTitle = new Label { Text = "Количество дисков: ", VerticalAlignment = VerticalAlignment.Center };
        diskTitle.AddThemeFontSizeOverride("font_size", 16);
        diskRow.AddChild(diskTitle);

        _diskCountValLabel = new Label { Text = "3", VerticalAlignment = VerticalAlignment.Center };
        _diskCountValLabel.AddThemeFontSizeOverride("font_size", 18);
        _diskCountValLabel.AddThemeColorOverride("font_color", new Color("fbbf24"));
        diskRow.AddChild(_diskCountValLabel);
        vbox.AddChild(diskRow);

        _diskSlider = new HSlider
        {
            MinValue = 3,
            MaxValue = 10,
            Step = 1,
            Value = 3
        };
        _diskSlider.ValueChanged += (val) =>
        {
            int disks = (int)val;
            _diskCountValLabel.Text = disks.ToString();
            _optimalValLabel.Text = $"Идеально: 2^{disks} - 1 = {StarCalculator.GetOptimalMoves(disks)} ходов";
        };
        vbox.AddChild(_diskSlider);

        _optimalValLabel = new Label
        {
            Text = $"Идеально: 2^3 - 1 = 7 ходов",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _optimalValLabel.AddThemeFontSizeOverride("font_size", 14);
        _optimalValLabel.AddThemeColorOverride("font_color", new Color("94a3b8"));
        vbox.AddChild(_optimalValLabel);

        vbox.AddChild(new HSeparator());

        // Mode Selection
        var modeGroupLabel = new Label { Text = "Режим прохождения:" };
        modeGroupLabel.AddThemeFontSizeOverride("font_size", 16);
        vbox.AddChild(modeGroupLabel);

        var radioGroup = new ButtonGroup();

        _manualModeCheck = new CheckBox
        {
            Text = "Ручной (Drag-and-Drop, оценка 0-3 звезды)",
            ButtonPressed = true,
            ButtonGroup = radioGroup
        };
        _manualModeCheck.AddThemeFontSizeOverride("font_size", 14);
        vbox.AddChild(_manualModeCheck);

        _autoModeCheck = new CheckBox
        {
            Text = "Автоматический (Решение компьютером)",
            ButtonGroup = radioGroup
        };
        _autoModeCheck.AddThemeFontSizeOverride("font_size", 14);
        vbox.AddChild(_autoModeCheck);

        vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 8) });

        // Action Buttons
        var actionRow = new HBoxContainer();
        actionRow.AddThemeConstantOverride("separation", 12);

        var cancelBtn = new Button { Text = "Отмена", CustomMinimumSize = new Vector2(0, 40) };
        cancelBtn.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        cancelBtn.Pressed += () => _freePlayDialog.Visible = false;
        actionRow.AddChild(cancelBtn);

        var startBtn = new Button { Text = "Начать игру", CustomMinimumSize = new Vector2(0, 40) };
        startBtn.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        startBtn.Pressed += OnStartFreePlay;
        actionRow.AddChild(startBtn);

        vbox.AddChild(actionRow);
    }

    private void CreateRulesDialog()
    {
        _rulesDialog = new Control
        {
            AnchorRight = 1.0f,
            AnchorBottom = 1.0f,
            Visible = false
        };
        AddChild(_rulesDialog);

        var dimBg = new ColorRect
        {
            Color = new Color(0, 0, 0, 0.65f),
            AnchorRight = 1.0f,
            AnchorBottom = 1.0f
        };
        _rulesDialog.AddChild(dimBg);

        var panel = new PanelContainer
        {
            CustomMinimumSize = new Vector2(560, 480),
            AnchorLeft = 0.5f,
            AnchorTop = 0.5f,
            AnchorRight = 0.5f,
            AnchorBottom = 0.5f,
            OffsetLeft = -280,
            OffsetTop = -240,
            OffsetRight = 280,
            OffsetBottom = 240
        };
        var panelStyle = new StyleBoxFlat
        {
            BgColor = new Color("0f172a"),
            CornerRadiusTopLeft = 14,
            CornerRadiusTopRight = 14,
            CornerRadiusBottomLeft = 14,
            CornerRadiusBottomRight = 14,
            BorderWidthLeft = 2,
            BorderWidthRight = 2,
            BorderWidthTop = 2,
            BorderWidthBottom = 2,
            BorderColor = new Color("38bdf8"),
            ContentMarginLeft = 24,
            ContentMarginRight = 24,
            ContentMarginTop = 20,
            ContentMarginBottom = 20
        };
        panel.AddThemeStyleboxOverride("panel", panelStyle);
        _rulesDialog.AddChild(panel);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 14);
        panel.AddChild(vbox);

        var title = new Label
        {
            Text = "Правила игры «Ханойские башни»",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        title.AddThemeFontSizeOverride("font_size", 22);
        title.AddThemeColorOverride("font_color", new Color("38bdf8"));
        vbox.AddChild(title);

        var rulesText = new RichTextLabel
        {
            BbcodeEnabled = true,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            Text = "[b]Цель игры:[/b]\nПереместить стопку дисков со стартовой Башни A на любую другую: [color=#38bdf8]Башню B[/color] или [color=#38bdf8]Башню C[/color].\n\n" +
                   "[b]Правила:[/b]\n" +
                   "1. За один ход разрешено перекладывать только [color=#38bdf8]один верхний диск[/color].\n" +
                   "2. [color=#ef4444]Нельзя класть больший диск на меньший[/color]. Диск меньшего размера всегда должен лежать выше.\n" +
                   "3. Игрок может совершать неограниченное число ходов.\n\n" +
                   "[b]Рейтинг звёзд (0-3 звезды):[/b]\n" +
                   "• [color=#fbbf24]⭐⭐⭐ 3 звезды[/color] — идеальное решение за минимально возможное число ходов: [b]2^n - 1[/b].\n" +
                   "• [color=#fbbf24]⭐⭐ 2 звезды[/color] — отличное прохождение с незначительным отклонением от идеала.\n" +
                   "• [color=#fbbf24]⭐ 1 звезда[/color] — головоломка решена (до 2х от оптимума).\n" +
                   "• [color=#94a3b8]0 звёзд[/color] — превышено двойное количество ходов.\n\n" +
                   "[b]Управление:[/b]\n" +
                   "• Перетаскивайте диски мышью (Drag-and-Drop) или кликайте сначала на стержень-источник, затем на стержень-цель."
        };
        rulesText.AddThemeFontSizeOverride("normal_font_size", 14);
        vbox.AddChild(rulesText);

        var closeBtn = new Button
        {
            Text = "Понятно",
            CustomMinimumSize = new Vector2(0, 40)
        };
        closeBtn.Pressed += () => _rulesDialog.Visible = false;
        vbox.AddChild(closeBtn);
    }

    private void OnStartFreePlay()
    {
        _freePlayDialog.Visible = false;
        int diskCount = (int)_diskSlider.Value;
        bool isAuto = _autoModeCheck.ButtonPressed;

        _menuPanel.Visible = false;
        _levelSelect.Visible = false;
        _gameUI.Visible = true;

        if (isAuto)
        {
            _gameUI.InitializeAutoGame(diskCount);
        }
        else
        {
            _gameUI.InitializeManualGame(diskCount, -1);
        }
    }

    private void ShowLevelSelect()
    {
        _menuPanel.Visible = false;
        _gameUI.Visible = false;
        _levelSelect.Visible = true;
        _levelSelect.RefreshLevels();
    }

    private void ShowMainMenu()
    {
        _freePlayDialog.Visible = false;
        _rulesDialog.Visible = false;
        _levelSelect.Visible = false;
        _gameUI.Visible = false;
        _menuPanel.Visible = true;
    }

    private void OnLevelSelected(int levelNumber, int diskCount)
    {
        _levelSelect.Visible = false;
        _gameUI.Visible = true;
        _gameUI.InitializeManualGame(diskCount, levelNumber);
    }

    private void OnSoundToggle()
    {
        GameProgress.Current.SoundEnabled = !GameProgress.Current.SoundEnabled;
        GameProgress.Save();
        _soundBtn.Text = GameProgress.Current.SoundEnabled ? "🔊 Звук: Вкл" : "🔇 Звук: Выкл";
    }
}
