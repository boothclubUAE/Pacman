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

    [Header("Input Fields (page 2)")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField phoneInput;

    [Header("Keyboard Keys (page 2)")]
    [SerializeField] private List<Button> keyButtons = new List<Button>();
    [SerializeField] private Button spaceButton;
    [SerializeField] private Button backButton;

    private readonly List<GameObject> pages = new List<GameObject>();
    private readonly List<KeyBinding> keyboardBindings = new List<KeyBinding>();

    [Header("Validation")]
    [SerializeField] private float shakeStrength = 10f;
    [SerializeField] private float shakeDuration = 0.3f;


    private PageState currentState = PageState.Page1;
    private TMP_InputField activeInput;
    internal bool waitingForStartButton;
    internal bool waitingForEndButton;

    private class KeyBinding
    {
        public Button Button;
        public string Value;
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
    private GameObject GetValidationObject(TMP_InputField field)
    {
        if (field == null) return null;
        return field.transform.Find("Validation")?.gameObject;
    }
    private bool IsValidName(string name)
    {
        return !string.IsNullOrWhiteSpace(name);
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return System.Text.RegularExpressions.Regex.IsMatch(email, pattern);
    }

    private bool IsValidPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        return System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\d+$");
    }
    private void ShakeField(TMP_InputField field)
    {
        if (field == null) return;

        RectTransform rt = field.GetComponent<RectTransform>();
        if (rt == null) return;

        rt.DOComplete();
        rt.DOShakeAnchorPos(shakeDuration, shakeStrength);
    }
    private bool ValidateInputs()
    {
        bool valid = true;

        // NAME
        bool nameValid = IsValidName(nameInput.text);
        ToggleValidationUI(nameInput, nameValid);
        if (!nameValid)
        {
            ShakeField(nameInput);
            valid = false;
        }

        // EMAIL
        bool emailValid = IsValidEmail(emailInput.text);
        ToggleValidationUI(emailInput, emailValid);
        if (!emailValid)
        {
            ShakeField(emailInput);
            valid = false;
        }

        // PHONE
        bool phoneValid = IsValidPhone(phoneInput.text);
        ToggleValidationUI(phoneInput, phoneValid);
        if (!phoneValid)
        {
            ShakeField(phoneInput);
            valid = false;
        }

        return valid;
    }
    private void ToggleValidationUI(TMP_InputField field, bool isValid)
    {
        GameObject validation = GetValidationObject(field);
        if (validation != null)
            validation.SetActive(!isValid);
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
        bool isValid = ValidateInputs();

        if (!isValid)
            return;

        SaveRegistrationToCsv();
        ShowPage(PageState.Page3);
    }

    public void OnEnd()
    {
        Debug.Log("ON END " + waitingForEndButton);
        if (waitingForEndButton)
        {
            waitingForEndButton = false;

            ShowPage(PageState.Page4GameOver);

            DOVirtual.DelayedCall(5, ResetGame);
        }
    }

    private void ResetGame()
    {
        if (nameInput != null) nameInput.text = "";
        if (emailInput != null) emailInput.text = "";
        if (phoneInput != null) phoneInput.text = "";

        activeInput = null;

        ShowPage(PageState.Page1);


        waitingForEndButton = false;
    }

    private void SaveRegistrationToCsv()
    {
        string filePath = Path.Combine(Application.dataPath, "registrations.csv");

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

    internal void OnGameStarted()
    {
        waitingForStartButton = false;
        waitingForEndButton = true;
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

        if (state == PageState.Page3)
        {
            waitingForStartButton = true;
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

    }

    private void OnKeyboardKeyPressed(KeyBinding binding)
    {
        HandleCharacterKey(binding.Value);
    }

    private string ResolveKeyValue(KeyBinding binding)
    {
        if (binding == null)
            return string.Empty;

        return binding.Value;
    }



    private void BuildKeyboardBindings()
    {
        keyboardBindings.Clear();

        foreach (Button button in keyButtons)
        {
            TryAddKeyBinding(button);
        }
    }

    private void TryAddKeyBinding(Button button)
    {
        if (button == null)
            return;

        string value = GetBaseKeyValue(button);

        if (string.IsNullOrEmpty(value))
            return;

        keyboardBindings.Add(new KeyBinding
        {
            Button = button,
            Value = value
        });
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
}