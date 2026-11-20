using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SurvivalGame.Core;
using SurvivalGame.Inventory;
using SurvivalGame.Player;

namespace SurvivalGame.UI
{
    /// <summary>
    /// Builds the entire interface at runtime using uGUI primitives: main menu, HUD,
    /// inventory + crafting, pause, death and victory screens. Works offline and on touch.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class GameUI : MonoBehaviour
    {
        private static readonly Color Dark = new Color(0.04f, 0.05f, 0.07f, 0.92f);
        private static readonly Color PanelColor = new Color(0.08f, 0.1f, 0.14f, 0.88f);
        private static readonly Color Accent = new Color(0.36f, 0.72f, 0.42f, 1f);

        public static GameUI Instance { get; private set; }

        private Canvas _canvas;
        private GameObject _mainMenu;
        private GameObject _controlsPanel;
        private GameObject _pausePanel;
        private GameObject _gameOverPanel;
        private GameObject _victoryPanel;
        private GameObject _inventoryPanel;

        private Text _dayText;
        private Text _goalText;
        private Text _temperatureText;
        private Text _promptText;
        private Text _gameOverText;
        private Text _victoryText;
        private UnityEngine.UI.Image _healthFill;
        private UnityEngine.UI.Image _staminaFill;
        private UnityEngine.UI.Image _hungerFill;
        private UnityEngine.UI.Image _thirstFill;

        private Transform _slotContainer;
        private Transform _recipeContainer;

        private PlayerSurvivalStats _stats;
        private bool _inventoryOpen;
        private bool _subscribed;

        public static void Create(Transform parent)
        {
            GameObject go = new GameObject("GameUI");
            go.transform.SetParent(parent, false);
            go.AddComponent<GameUI>();
        }

        private void Awake()
        {
            Instance = this;
            _stats = FindFirstObjectByType<PlayerSurvivalStats>();
            if (EventSystem.current == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
            }
        }

        private void Start()
        {
            BuildCanvas();
            BuildHud();
            BuildMainMenu();
            BuildControls();
            BuildPause();
            BuildEndScreen(false);
            BuildEndScreen(true);
            BuildInventory();
            Subscribe();
            ShowMainMenu();
            RefreshInventory();
        }

        private void OnDestroy()
        {
            Unsubscribe();
            if (Instance == this) Instance = null;
        }

        private void Subscribe()
        {
            if (_subscribed) return;
            GameLoop.OnStateChanged += OnStateChanged;
            GameLoop.OnDayProgressChanged += OnDayProgress;

            if (_stats != null)
            {
                _stats.OnHealthChanged += (current, max) => SetFill(_healthFill, current / max);
                _stats.OnStaminaChanged += (current, max) => SetFill(_staminaFill, current / max);
                _stats.OnHungerChanged += (current, max) => SetFill(_hungerFill, current / max);
                _stats.OnThirstChanged += (current, max) => SetFill(_thirstFill, current / max);
                _stats.OnPlayerDied += () => SetFill(_healthFill, 0f);
            }

            PlayerInteraction interaction = FindFirstObjectByType<PlayerInteraction>();
            if (interaction != null) interaction.OnPromptUpdated += prompt => _promptText.text = prompt;

            if (InventorySystem.Instance != null) InventorySystem.Instance.OnInventoryChanged += RefreshInventory;
            if (CraftingSystem.Instance != null) CraftingSystem.Instance.OnItemCrafted += OnCrafted;

            _subscribed = true;

            if (_stats != null)
            {
                SetFill(_healthFill, _stats.HealthPercent);
                SetFill(_staminaFill, _stats.StaminaPercent);
                SetFill(_hungerFill, _stats.HungerPercent);
                SetFill(_thirstFill, _stats.ThirstPercent);
            }
        }

        private void Unsubscribe()
        {
            if (!_subscribed) return;
            GameLoop.OnStateChanged -= OnStateChanged;
            GameLoop.OnDayProgressChanged -= OnDayProgress;
            if (InventorySystem.Instance != null) InventorySystem.Instance.OnInventoryChanged -= RefreshInventory;
            if (CraftingSystem.Instance != null) CraftingSystem.Instance.OnItemCrafted -= OnCrafted;
            _subscribed = false;
        }

        private void Update()
        {
            GameInput.Poll();

            if (_temperatureText != null && _stats != null)
            {
                _temperatureText.text = _stats.CurrentTemperature.ToString("F1") + " C";
            }

            RunState state = GameLoop.Instance == null ? RunState.Ready : GameLoop.Instance.State;

            if (GameInput.PausePressed && (state == RunState.Running || state == RunState.Paused))
            {
                GameLoop.Instance?.TogglePause();
            }

            if (GameInput.InventoryPressed && state == RunState.Running)
            {
                ToggleInventory();
            }
        }

        private void OnCrafted(ItemData item)
        {
            RefreshInventory();
        }

        private void OnStateChanged(RunState state)
        {
            HideAllPanels();

            switch (state)
            {
                case RunState.Ready:
                    ShowMainMenu();
                    break;
                case RunState.Paused:
                    _pausePanel?.SetActive(true);
                    break;
                case RunState.Lost:
                    if (_gameOverText != null && GameLoop.Instance != null)
                    {
                        _gameOverText.text = "You succumbed to the wilderness.\n\nDays survived: " + GameLoop.Instance.DaysSurvived;
                    }
                    _gameOverPanel?.SetActive(true);
                    break;
                case RunState.Won:
                    if (_victoryText != null && GameLoop.Instance != null)
                    {
                        _victoryText.text = "You survived the wild!\n\nDays survived: " + GameLoop.Instance.DaysSurvived;
                    }
                    _victoryPanel?.SetActive(true);
                    break;
            }
        }

        private void OnDayProgress(int current, int target)
        {
            if (_dayText != null) _dayText.text = "DAY " + (current + 1);
            if (_goalText != null)
            {
                _goalText.text = target > 0
                    ? "Survive " + target + " days   (" + current + "/" + target + ")"
                    : "Survive as long as possible";
            }
        }

        private void BuildCanvas()
        {
            GameObject canvasObject = new GameObject("HUD_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            _canvas = canvasObject.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 10;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void BuildHud()
        {
            Transform parent = _canvas.transform;

            RectTransform stats = Panel("Vitals", parent, new Vector2(0f, 1f), new Vector2(370f, 250f), new Vector2(28f, -28f), new Vector2(0f, 1f), PanelColor);
            _healthFill = StatRow(stats, "HEALTH", 0, new Color(0.88f, 0.24f, 0.24f));
            _staminaFill = StatRow(stats, "STAMINA", 1, new Color(0.92f, 0.78f, 0.2f));
            _hungerFill = StatRow(stats, "HUNGER", 1, new Color(0.92f, 0.48f, 0.16f));
            _thirstFill = StatRow(stats, "THIRST", 2, new Color(0.24f, 0.62f, 0.92f));

            _dayText = Label("Day", parent, "DAY 1", new Vector2(600f, 80f), new Vector2(0.5f, 1f), new Vector2(0f, -34f), 46, TextAnchor.MiddleCenter, Color.white);
            _goalText = Label("Goal", parent, "", new Vector2(760f, 48f), new Vector2(0.5f, 1f), new Vector2(0f, -90f), 26, TextAnchor.MiddleCenter, new Color(0.85f, 0.9f, 0.8f));
            _temperatureText = Label("Temperature", parent, "37.0 C", new Vector2(260f, 60f), new Vector2(1f, 1f), new Vector2(-30f, -36f), 30, TextAnchor.MiddleRight, Color.white);

            RectTransform crosshair = Image("Crosshair", parent, new Vector2(8f, 8f), new Vector2(0.5f, 0.5f), Vector2.zero, new Color(1f, 1f, 1f, 0.85f));
            crosshair.gameObject.name = "Crosshair";
            _promptText = Label("Prompt", parent, "", new Vector2(900f, 60f), new Vector2(0.5f, 0f), new Vector2(0f, 150f), 28, TextAnchor.MiddleCenter, Color.white);
            Label("Hint", parent, "WASD move  |  Shift run  |  LMB attack  |  E interact  |  TAB inventory  |  ESC pause",
                new Vector2(1500f, 40f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), 20, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.6f));
        }

        private void BuildMainMenu()
        {
            _mainMenu = FullScreen("MainMenu", "UNITY SURVIVAL");
            Transform root = _mainMenu.transform;

            Label("Tagline", root, "Survive the wilderness. Build. Craft. Endure.", new Vector2(1000f, 60f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), 32, TextAnchor.MiddleCenter, new Color(0.8f, 0.86f, 0.78f));

            Button("PlayButton", root, "PLAY", new Vector2(0f, 30f), new Vector2(420f, 88f), StartGame);
            Button("ControlsButton", root, "CONTROLS", new Vector2(0f, -80f), new Vector2(420f, 76f), () =>
            {
                _mainMenu.SetActive(false);
                _controlsPanel.SetActive(true);
            });
            Button("QuitButton", root, "QUIT", new Vector2(0f, -180f), new Vector2(420f, 76f), () => GameLoop.Instance?.QuitGame());
        }

        private void BuildControls()
        {
            _controlsPanel = FullScreen("Controls", "CONTROLS");
            string controls =
                "WASD / Left Stick - Move\n" +
                "Mouse / Right Pad - Look\n" +
                "Shift / RUN - Sprint\n" +
                "Space / JUMP - Jump\n" +
                "Left Click / ATTACK - Attack & harvest\n" +
                "Right Click / BLOCK - Block\n" +
                "E / USE - Interact & pick up\n" +
                "TAB / BAG - Inventory & crafting\n" +
                "ESC / MENU - Pause\n\n" +
                "Survive 5 days. Gather wood, stone, fibre and food.\n" +
                "Stay warm at the campfire during cold nights.";
            Label("ControlList", _controlsPanel.transform, controls, new Vector2(1200f, 640f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), 32, TextAnchor.MiddleCenter, Color.white);
            Button("BackButton", _controlsPanel.transform, "BACK", new Vector2(0f, -390f), new Vector2(320f, 76f), () =>
            {
                _controlsPanel.SetActive(false);
                _mainMenu.SetActive(true);
            });
        }

        private void BuildPause()
        {
            _pausePanel = CenterPanel("PausePanel", "PAUSED", new Vector2(560f, 620f));
            Transform root = _pausePanel.transform;
            Button("ResumeButton", root, "RESUME", new Vector2(0f, 90f), new Vector2(420f, 80f), () => GameLoop.Instance?.TogglePause());
            Button("RestartButton", root, "RESTART", new Vector2(0f, -10f), new Vector2(420f, 80f), () => GameLoop.Instance?.RestartGame());
            Button("MenuButton", root, "MAIN MENU", new Vector2(0f, -110f), new Vector2(420f, 80f), () => GameLoop.Instance?.RestartGame());
            Button("QuitButton", root, "QUIT", new Vector2(0f, -210f), new Vector2(420f, 80f), () => GameLoop.Instance?.QuitGame());
        }

        private void BuildEndScreen(bool victory)
        {
            GameObject panel = CenterPanel(victory ? "VictoryPanel" : "GameOverPanel",
                victory ? "VICTORY" : "GAME OVER", new Vector2(720f, 560f));
            Text text = Label("ResultText", panel.transform, "", new Vector2(620f, 220f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), 34, TextAnchor.MiddleCenter, Color.white);
            Button("AgainButton", panel.transform, victory ? "PLAY AGAIN" : "RETRY", new Vector2(0f, -110f),
                new Vector2(420f, 80f), () => GameLoop.Instance?.RestartGame());
            Button("MenuButton", panel.transform, "MAIN MENU", new Vector2(0f, -210f),
                new Vector2(420f, 80f), () => GameLoop.Instance?.RestartGame());

            if (victory)
            {
                _victoryPanel = panel;
                _victoryText = text;
            }
            else
            {
                _gameOverPanel = panel;
                _gameOverText = text;
            }
        }

        private void BuildInventory()
        {
            _inventoryPanel = CenterPanel("InventoryPanel", "INVENTORY & CRAFTING", new Vector2(1240f, 720f));
            Transform root = _inventoryPanel.transform;

            Label("InventoryTitle", root, "PACK", new Vector2(500f, 60f), new Vector2(0.5f, 0.5f),
                new Vector2(-280f, 290f), 30, TextAnchor.MiddleCenter, Accent);
            Label("CraftingTitle", root, "CRAFTING", new Vector2(500f, 60f), new Vector2(0.5f, 0.5f),
                new Vector2(300f, 290f), 30, TextAnchor.MiddleCenter, Accent);

            RectTransform slotsPanel = Panel("SlotsPanel", root, new Vector2(0.5f, 0.5f), new Vector2(560f, 540f), new Vector2(-280f, -20f), new Vector2(0.5f, 0.5f), PanelColor);
            GameObject slotsObject = new GameObject("Slots", typeof(RectTransform), typeof(GridLayoutGroup));
            RectTransform slotsRect = slotsObject.GetComponent<RectTransform>();
            slotsRect.SetParent(slotsPanel, false);
            slotsRect.anchorMin = Vector2.zero;
            slotsRect.anchorMax = Vector2.one;
            slotsRect.offsetMin = new Vector2(12f, 12f);
            slotsRect.offsetMax = new Vector2(-12f, -12f);
            GridLayoutGroup grid = slotsObject.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(82f, 82f);
            grid.spacing = new Vector2(8f, 8f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 6;
            grid.childAlignment = TextAnchor.UpperCenter;
            _slotContainer = slotsRect;

            RectTransform recipesPanel = Panel("RecipesPanel", root, new Vector2(0.5f, 0.5f), new Vector2(540f, 540f), new Vector2(300f, -20f), new Vector2(0.5f, 0.5f), PanelColor);
            GameObject recipesObject = new GameObject("Recipes", typeof(RectTransform), typeof(VerticalLayoutGroup));
            RectTransform recipesRect = recipesObject.GetComponent<RectTransform>();
            recipesRect.SetParent(recipesPanel, false);
            recipesRect.anchorMin = Vector2.zero;
            recipesRect.anchorMax = Vector2.one;
            recipesRect.offsetMin = new Vector2(16f, 16f);
            recipesRect.offsetMax = new Vector2(-16f, -16f);
            VerticalLayoutGroup layout = recipesObject.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            _recipeContainer = recipesRect;

            Button("CloseButton", root, "CLOSE", new Vector2(0f, -320f), new Vector2(320f, 70f), ToggleInventory);
            _inventoryPanel.SetActive(false);
        }

        private void RefreshInventory()
        {
            if (_slotContainer == null || InventorySystem.Instance == null) return;

            foreach (Transform child in _slotContainer) Destroy(child.gameObject);

            IReadOnlyList<InventorySlot> slots = InventorySystem.Instance.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlot slot = slots[i];
                int index = i;
                RectTransform cell = Panel("Slot" + i, _slotContainer, new Vector2(0.5f, 0.5f), new Vector2(82f, 82f), Vector2.zero, new Vector2(0.5f, 0.5f), PanelColor);

                if (!slot.IsEmpty)
                {
                    UnityEngine.UI.Image image = cell.GetComponent<UnityEngine.UI.Image>();
                    image.color = ColorFor(slot.item.itemType);
                    Label("Name", cell, slot.item.itemName, new Vector2(78f, 50f), new Vector2(0.5f, 0.5f),
                        Vector2.zero, 17, TextAnchor.MiddleCenter, Color.white);
                    if (slot.count > 1)
                    {
                        Label("Count", cell, "x" + slot.count, new Vector2(76f, 26f), new Vector2(0.5f, 0f),
                            new Vector2(0f, 4f), 18, TextAnchor.MiddleRight, Color.white);
                    }

                    UnityEngine.UI.Button button = cell.gameObject.AddComponent<UnityEngine.UI.Button>();
                    button.onClick.AddListener(() =>
                    {
                        InventorySystem.Instance.UseItem(index, _stats);
                        RefreshInventory();
                    });
                }
            }

            if (_recipeContainer == null || CraftingSystem.Instance == null) return;
            foreach (Transform child in _recipeContainer) Destroy(child.gameObject);

            foreach (ItemData recipe in CraftingSystem.Instance.Recipes)
            {
                RectTransform buttonRect = Panel("Recipe_" + recipe.itemId, _recipeContainer, new Vector2(0.5f, 0.5f),
                    new Vector2(500f, 84f), Vector2.zero, new Vector2(0.5f, 0.5f), PanelColor);
                buttonRect.GetComponent<UnityEngine.UI.Image>().color = CraftingSystem.Instance.CanCraft(recipe)
                    ? new Color(0.2f, 0.4f, 0.24f, 0.95f)
                    : new Color(0.22f, 0.22f, 0.24f, 0.9f);
                Label("Label", buttonRect, recipe.itemName + "  -  " + Cost(recipe), new Vector2(480f, 78f),
                    new Vector2(0.5f, 0.5f), Vector2.zero, 20, TextAnchor.MiddleCenter, Color.white);
                UnityEngine.UI.Button button = buttonRect.gameObject.AddComponent<UnityEngine.UI.Button>();
                button.onClick.AddListener(() =>
                {
                    CraftingSystem.Instance.Craft(recipe);
                    RefreshInventory();
                });
            }
        }

        private static string Cost(ItemData recipe)
        {
            string result = "";
            foreach (CraftingCost cost in recipe.recipeRequirements)
            {
                if (cost.item == null) continue;
                if (result.Length > 0) result += ", ";
                result += cost.item.itemName + " x" + cost.amount;
            }
            return result.Length == 0 ? "free" : result;
        }

        private static Color ColorFor(ItemType type)
        {
            switch (type)
            {
                case ItemType.Food: return new Color(0.7f, 0.28f, 0.3f, 0.95f);
                case ItemType.Drink: return new Color(0.2f, 0.5f, 0.8f, 0.95f);
                case ItemType.Weapon: return new Color(0.55f, 0.35f, 0.65f, 0.95f);
                case ItemType.Tool:
                case ItemType.Equipment: return new Color(0.45f, 0.45f, 0.5f, 0.95f);
                default: return new Color(0.32f, 0.4f, 0.28f, 0.95f);
            }
        }

        private void ToggleInventory()
        {
            _inventoryOpen = !_inventoryOpen;
            _inventoryPanel?.SetActive(_inventoryOpen);
            if (_inventoryOpen) RefreshInventory();
        }

        private void StartGame()
        {
            HideAllPanels();
            GameLoop.Instance?.StartRun();
        }

        private void ShowMainMenu()
        {
            HideAllPanels();
            _mainMenu?.SetActive(true);
        }

        private void HideAllPanels()
        {
            if (_mainMenu != null) _mainMenu.SetActive(false);
            if (_controlsPanel != null) _controlsPanel.SetActive(false);
            if (_pausePanel != null) _pausePanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            if (_victoryPanel != null) _victoryPanel.SetActive(false);
            if (_inventoryPanel != null)
            {
                _inventoryPanel.SetActive(false);
                _inventoryOpen = false;
            }
        }

        private static void SetFill(UnityEngine.UI.Image image, float value)
        {
            if (image == null) return;
            image.fillAmount = Mathf.Clamp01(value);
        }

        private UnityEngine.UI.Image StatRow(Transform parent, string label, int row, Color color)
        {
            float y = -70f - row * 52f;
            Label(label + "Label", parent, label, new Vector2(120f, 34f), new Vector2(0f, 1f), new Vector2(20f, y), 20, TextAnchor.MiddleLeft, Color.white);
            RectTransform background = Image(label + "Bar", parent, new Vector2(220f, 26f), new Vector2(0f, 1f), new Vector2(132f, y), new Color(0f, 0f, 0f, 0.55f));
            RectTransform fill = Image(label + "Fill", background, new Vector2(220f, 26f), new Vector2(0f, 0.5f), new Vector2(0f, 0f), color);
            UnityEngine.UI.Image image = fill.GetComponent<UnityEngine.UI.Image>();
            image.type = UnityEngine.UI.Image.Type.Filled;
            image.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            image.fillAmount = 1f;
            return image;
        }

        private GameObject FullScreen(string name, string title)
        {
            RectTransform rect = Panel(name, _canvas.transform, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), Dark);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Label("Title", rect, title, new Vector2(1400f, 140f), new Vector2(0.5f, 0.5f), new Vector2(0f, 300f), 86, TextAnchor.MiddleCenter, Accent);
            return rect.gameObject;
        }

        private GameObject CenterPanel(string name, string title, Vector2 size)
        {
            RectTransform rect = Panel(name, _canvas.transform, new Vector2(0.5f, 0.5f), size, Vector2.zero, new Vector2(0.5f, 0.5f), PanelColor);
            Label("Title", rect, title, new Vector2(size.x - 40f, 100f), new Vector2(0.5f, 1f), new Vector2(0f, -56f), 56, TextAnchor.MiddleCenter, Accent);
            return rect.gameObject;
        }

        private static RectTransform Panel(string name, Transform parent, Vector2 anchor, Vector2 size, Vector2 position, Vector2 pivot, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            go.GetComponent<UnityEngine.UI.Image>().color = color;
            return rect;
        }

        private static RectTransform Image(string name, Transform parent, Vector2 size, Vector2 anchor, Vector2 position, Color color)
        {
            RectTransform rect = Panel(name, parent, anchor, size, position, new Vector2(0f, 0.5f), color);
            return rect;
        }

        private static Text Label(string name, Transform parent, string value, Vector2 size, Vector2 anchor, Vector2 position,
            int fontSize, TextAnchor alignment, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            Text text = go.GetComponent<Text>();
            text.text = value;
            text.font = FontForUi();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static void Button(string name, Transform parent, string value, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action)
        {
            RectTransform rect = Panel(name, parent, new Vector2(0.5f, 0.5f), size, position, new Vector2(0.5f, 0.5f),
                new Color(0.19f, 0.34f, 0.22f, 0.98f));
            Label("Label", rect, value, new Vector2(size.x - 10f, size.y - 6f), new Vector2(0.5f, 0.5f),
                Vector2.zero, 34, TextAnchor.MiddleCenter, Color.white);
            UnityEngine.UI.Button button = rect.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = rect.GetComponent<UnityEngine.UI.Image>();
            button.onClick.AddListener(action);
        }

        private static Font _uiFont;

        private static Font FontForUi()
        {
            if (_uiFont != null) return _uiFont;
            _uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_uiFont == null) _uiFont = Font.CreateDynamicFontFromOSFont("Arial", 16);
            return _uiFont;
        }
    }
}
