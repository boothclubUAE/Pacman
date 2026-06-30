using System;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Registration : MonoBehaviour
{
    public static Registration Instance;

    private enum PageState
    {
        Page1 = 0,
        Page2 = 1,
        Page3 = 2,
        Page4GameOver = 3
    }

    [Header("Pages (set from scene)")]
    [SerializeField] private GameObject page1;
    [SerializeField] private GameObject page2;
    [SerializeField] private GameObject page3;
    [SerializeField] private GameObject page4GameOver;

    [Header("Navigation Buttons")]
    [SerializeField] private Button page1NextButton;
    [SerializeField] private Button page2DoneButton;
    [SerializeField] private Button playButton;

    [Header("Input Fields (page 2)")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField phoneInput;

    [Header("Keyboard Keys (page 2)")]
    [SerializeField] private List<Button> keyButtons = new List<Button>();
    [SerializeField] private Button spaceButton;
    [SerializeField] private Button backButton;
    [SerializeField] private List<Button> lowercaseButtons = new List<Button>();
    [SerializeField] private List<Button> uppercaseButtons = new List<Button>();
    [SerializeField] private List<Button> numbersButtons = new List<Button>();
    [SerializeField] private GameObject lowercasePage;
    [SerializeField] private GameObject uppercasePage;
    [SerializeField] private GameObject numbersPage;

    private readonly List<GameObject> pages = new List<GameObject>();
    private readonly List<KeyBinding> keyboardBindings = new List<KeyBinding>();

    private enum KeyboardPage
    {
        Lowercase,
        Uppercase,
        Numbers
    }

    private PageState currentState = PageState.Page1;
    private TMP_InputField activeInput;
    private bool waitingForYellowButton;
    private KeyboardPage currentKeyboardPage = KeyboardPage.Lowercase;

    private bool IsSingleKeyboardLayout =>
        lowercasePage == null && uppercasePage == null && numbersPage == null;

    private class KeyBinding
    {
        public Button Button;
        public string Value;
        public KeyboardPage Page;
    }

    private void Awake()
    {
        Instance = this;
        AutoWireFromScene();
        BuildPageList();
        WireButtons();
        RegisterInputSelection();
        ShowPage(PageState.Page1);
    }

    private void HandleCharacterKey(string character)
    {
        if (currentState != PageState.Page2 || activeInput == null)
        {
            return;
        }

        activeInput.text += character;
        activeInput.caretPosition = activeInput.text.Length;
    }

    private void HandleBackspace()
    {
        if (currentState != PageState.Page2 || activeInput == null || string.IsNullOrEmpty(activeInput.text))
        {
            return;
        }

        activeInput.text = activeInput.text.Substring(0, activeInput.text.Length - 1);
        activeInput.caretPosition = activeInput.text.Length;
    }

    private void OnPage2Done()
    {
        SaveRegistrationToCsv();
        ShowPage(PageState.Page3);
    }

    public void OnEnd()
    {
        if (!waitingForYellowButton)
            return;

        waitingForYellowButton = false;

        ShowPage(PageState.Page4GameOver);

        DOVirtual.DelayedCall(5, ResetGame);
    }

    private void ResetGame()
    {
        if (nameInput != null) nameInput.text = "";
        if (emailInput != null) emailInput.text = "";
        if (phoneInput != null) phoneInput.text = "";

        activeInput = null;

        ShowPage(PageState.Page1);

        SetKeyboardPage(KeyboardPage.Lowercase);

        if (playButton != null)
            playButton.interactable = true;

        waitingForYellowButton = false;
    }

    private void SaveRegistrationToCsv()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "registrations.csv");

        bool fileExists = File.Exists(filePath);

        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            if (!fileExists)
            {
                writer.WriteLine("DateTime,Name,Email,Phone");
            }

            string dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            string name = EscapeCsv(nameInput != null ? nameInput.text : "");
            string email = EscapeCsv(emailInput != null ? emailInput.text : "");
            string phone = EscapeCsv(phoneInput != null ? phoneInput.text : "");

            writer.WriteLine($"{dateTime},{name},{email},{phone}");
        }

        Debug.Log("CSV Saved To: " + filePath);
    }

    private string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        value = value.Replace("\"", "\"\"");

        if (value.Contains(",") || value.Contains("\n"))
        {
            value = $"\"{value}\"";
        }

        return value;
    }

    private void OnPlayPressed()
    {
        if (currentState != PageState.Page3)
            return;

        waitingForYellowButton = true;
        playButton.interactable = false;
    }

    private void SetActiveInput(TMP_InputField input)
    {
        activeInput = input;
        input.ActivateInputField();
    }

    private void ShowPage(PageState state)
    {
        currentState = state;

        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].SetActive(i == (int)state);
        }

        if (state == PageState.Page3 && playButton != null)
        {
            playButton.interactable = true;
        }
    }

    private void BuildPageList()
    {
        pages.Clear();
        pages.Add(page1);
        pages.Add(page2);
        pages.Add(page3);
        pages.Add(page4GameOver);
    }

    private void WireButtons()
    {
        AutoCollectKeyboardKeys();
        BuildKeyboardBindings();
        UpdateKeyboardPageObjects();

        if (page1NextButton != null)
        {
            page1NextButton.onClick.RemoveAllListeners();
            page1NextButton.onClick.AddListener(() => ShowPage(PageState.Page2));
        }

        if (page2DoneButton != null)
        {
            page2DoneButton.onClick.RemoveAllListeners();
            page2DoneButton.onClick.AddListener(OnPage2Done);
        }

        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(OnPlayPressed);
        }

        for (int i = 0; i < keyboardBindings.Count; i++)
        {
            KeyBinding binding = keyboardBindings[i];

            if (binding == null || binding.Button == null)
            {
                continue;
            }

            binding.Button.onClick.RemoveAllListeners();
            binding.Button.onClick.AddListener(() => OnKeyboardKeyPressed(binding));
        }

        if (!IsSingleKeyboardLayout)
        {
            WireModeButtons(lowercaseButtons, KeyboardPage.Lowercase);
            WireModeButtons(uppercaseButtons, KeyboardPage.Uppercase);
            WireModeButtons(numbersButtons, KeyboardPage.Numbers);
        }

        if (spaceButton != null)
        {
            spaceButton.onClick.RemoveAllListeners();
            spaceButton.onClick.AddListener(() => HandleCharacterKey(" "));
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(HandleBackspace);
        }

        ApplyKeyboardState();
    }

    private void OnKeyboardKeyPressed(KeyBinding binding)
    {
        string value = ResolveKeyValue(binding);

        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        HandleCharacterKey(value);
    }

    private string ResolveKeyValue(KeyBinding binding)
    {
        if (binding == null)
        {
            return string.Empty;
        }

        if (!IsSingleKeyboardLayout && binding.Page != currentKeyboardPage)
        {
            return string.Empty;
        }

        return binding.Value;
    }

    private void SetKeyboardPage(KeyboardPage keyboardPage)
    {
        currentKeyboardPage = keyboardPage;
        UpdateKeyboardPageObjects();
        ApplyKeyboardState();
    }

    private void UpdateKeyboardPageObjects()
    {
        if (IsSingleKeyboardLayout)
        {
            return;
        }

        if (lowercasePage != null)
            lowercasePage.SetActive(currentKeyboardPage == KeyboardPage.Lowercase);

        if (uppercasePage != null)
            uppercasePage.SetActive(currentKeyboardPage == KeyboardPage.Uppercase);

        if (numbersPage != null)
            numbersPage.SetActive(currentKeyboardPage == KeyboardPage.Numbers);
    }

    private void ApplyKeyboardState()
    {
        for (int i = 0; i < keyboardBindings.Count; i++)
        {
            KeyBinding binding = keyboardBindings[i];

            if (binding == null || binding.Button == null)
            {
                continue;
            }

            if (IsSingleKeyboardLayout)
            {
                binding.Button.gameObject.SetActive(true);
                binding.Button.interactable = true;
                continue;
            }

            bool isVisible = binding.Page == currentKeyboardPage;

            binding.Button.gameObject.SetActive(isVisible);
            binding.Button.interactable = isVisible;
        }
    }

    private void BuildKeyboardBindings()
    {
        keyboardBindings.Clear();

        AddBindingsFromPage(lowercasePage, KeyboardPage.Lowercase);
        AddBindingsFromPage(uppercasePage, KeyboardPage.Uppercase);
        AddBindingsFromPage(numbersPage, KeyboardPage.Numbers);

        if (keyboardBindings.Count == 0)
        {
            AddBindingsFromSingleKeyboardLayout();
        }
    }

    private void AddBindingsFromSingleKeyboardLayout()
    {
        Transform root = null;

        var kbGo = GameObject.Find("KeyboardRoot");

        if (kbGo != null)
        {
            root = kbGo.transform;
        }
        else if (page2 != null)
        {
            root = page2.transform.Find("KeyboardRoot");
        }

        if (root == null)
        {
            foreach (Button button in keyButtons)
            {
                TryAddKeyBinding(button, KeyboardPage.Lowercase);
            }

            return;
        }

        foreach (Button button in root.GetComponentsInChildren<Button>(true))
        {
            TryAddKeyBinding(button, KeyboardPage.Lowercase);
        }
    }

    private void TryAddKeyBinding(Button button, KeyboardPage page)
    {
        if (button == null || !button.gameObject.name.StartsWith("Key_"))
        {
            return;
        }

        string value = GetBaseKeyValue(button);

        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        keyboardBindings.Add(new KeyBinding
        {
            Button = button,
            Value = value,
            Page = page
        });
    }

    private void AddBindingsFromPage(GameObject page, KeyboardPage keyboardPage)
    {
        if (page == null)
        {
            return;
        }

        Button[] pageButtons = page.GetComponentsInChildren<Button>(true);

        foreach (Button button in pageButtons)
        {
            if (button == null || !button.gameObject.name.StartsWith("Key_"))
            {
                continue;
            }

            string value = GetBaseKeyValue(button);

            if (string.IsNullOrEmpty(value))
            {
                continue;
            }

            keyboardBindings.Add(new KeyBinding
            {
                Button = button,
                Value = value,
                Page = keyboardPage
            });
        }
    }

    private void RegisterInputSelection()
    {
        var inputFields = new List<TMP_InputField>();

        if (nameInput != null) inputFields.Add(nameInput);
        if (emailInput != null) inputFields.Add(emailInput);
        if (phoneInput != null) inputFields.Add(phoneInput);

        foreach (TMP_InputField field in inputFields)
        {
            if (field == null)
            {
                continue;
            }

            field.onSelect.RemoveAllListeners();
            field.onSelect.AddListener(_ => SetActiveInput(field));
        }

        if (inputFields.Count > 0 && inputFields[0] != null)
        {
            SetActiveInput(inputFields[0]);
        }
    }

    private void AutoWireFromScene()
    {
        if (page1 == null) page1 = GameObject.Find("Page1");
        if (page2 == null) page2 = GameObject.Find("Page2");
        if (page3 == null) page3 = GameObject.Find("Page3");
        if (page4GameOver == null) page4GameOver = GameObject.Find("Page4");

        if (page1NextButton == null)
        {
            var go = GameObject.Find("NextButton");
            if (go != null) page1NextButton = go.GetComponent<Button>();
        }

        if (page2DoneButton == null)
        {
            var go = GameObject.Find("DoneButton");
            if (go != null) page2DoneButton = go.GetComponent<Button>();
        }

        if (playButton == null)
        {
            var go = GameObject.Find("PlayButton");
            if (go != null) playButton = go.GetComponent<Button>();
        }

        if (spaceButton == null)
        {
            var go = GameObject.Find("SpaceButton");
            if (go != null) spaceButton = go.GetComponent<Button>();
        }

        if (backButton == null)
        {
            var go = GameObject.Find("BackButton");
            if (go != null) backButton = go.GetComponent<Button>();
        }

        if (nameInput == null)
        {
            var go = GameObject.Find("NameInput");
            if (go != null) nameInput = go.GetComponent<TMP_InputField>();
        }

        if (emailInput == null)
        {
            var go = GameObject.Find("EmailInput");
            if (go != null) emailInput = go.GetComponent<TMP_InputField>();
        }

        if (phoneInput == null)
        {
            var go = GameObject.Find("PhoneInput");
            if (go != null) phoneInput = go.GetComponent<TMP_InputField>();
        }

        if (lowercasePage == null)
        {
            var go = GameObject.Find("LowercasePage");
            if (go != null) lowercasePage = go;
        }

        if (uppercasePage == null)
        {
            var go = GameObject.Find("UppercasePage");
            if (go != null) uppercasePage = go;
        }

        if (numbersPage == null)
        {
            var go = GameObject.Find("NumbersPage");
            if (go != null) numbersPage = go;
        }
    }

    private void AutoCollectKeyboardKeys()
    {
        if (keyButtons.Count > 0)
        {
            return;
        }

        keyButtons.Clear();

        Button[] allButtons = FindObjectsOfType<Button>(true);

        foreach (Button button in allButtons)
        {
            if (button != null && button.gameObject.name.StartsWith("Key_"))
            {
                keyButtons.Add(button);
            }
        }
    }

    private static string GetBaseKeyValue(Button button)
    {
        if (button == null)
        {
            return string.Empty;
        }

        string objectName = button.gameObject.name;

        const string prefix = "Key_";

        if (objectName.StartsWith(prefix) && objectName.Length > prefix.Length)
        {
            return objectName.Substring(prefix.Length);
        }

        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);

        return label != null ? label.text : string.Empty;
    }

    private void WireModeButtons(List<Button> buttons, KeyboardPage page)
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            Button button = buttons[i];

            if (button == null)
            {
                continue;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => SetKeyboardPage(page));
        }
    }
}