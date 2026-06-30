using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;

public class ControlPanel : MonoBehaviour
{
    public Transform modulesContent;
    public Transform detailsContent;

    public GameObject moduleButton;
    public GameObject stringPrefab;
    public GameObject integerPrefab;
    public GameObject decimalPrefab;
    public GameObject booleanPrefab;
    public GameObject triggerPrefab;

    public Button close;
    public Button apply;
    public Button reset;

    List<object> objects = new();
    Dictionary<object, object> defaults = new();

    object current;

    string path => Application.persistentDataPath + "/settings.json";

    public RectTransform panel;
    public float showX = 10f;
    public float duration = 0.3f;

    bool isOpen;
    Coroutine holdRoutine;
    public Button invisibleButton;
    public float holdTime = 2f;
    float holdTimer;
    bool holding;

    [Serializable]
    public class SaveData
    {
        public List<string> values = new();
    }
    void Start()
    {
        invisibleButton.onClick.AddListener(() => { }); // dummy to ensure it's active

        var eventTrigger = invisibleButton.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        var pointerDown = new UnityEngine.EventSystems.EventTrigger.Entry
        {
            eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown
        };

        pointerDown.callback.AddListener((e) =>
        {
            holding = true;
            holdTimer = 0f;
        });

        var pointerUp = new UnityEngine.EventSystems.EventTrigger.Entry
        {
            eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp
        };

        pointerUp.callback.AddListener((e) =>
        {
            holding = false;
        });

        eventTrigger.triggers.Add(pointerDown);
        eventTrigger.triggers.Add(pointerUp);

        LoadClasses();
        Load();
        close.onClick.AddListener(HidePanel);
        apply.onClick.AddListener(Save);
        reset.onClick.AddListener(ResetValues);
    }
    public void ShowPanel()
    {
        if (isOpen) return;
        isOpen = true;

        panel.gameObject.SetActive(true);

        float width = panel.rect.width;
        panel.anchoredPosition = new Vector2(-(width + showX), panel.anchoredPosition.y);

        panel.DOAnchorPosX(showX, duration)
            .SetEase(Ease.InOutQuad);
    }

    public void HidePanel()
    {
        if (!isOpen) return;
        isOpen = false;

        float width = panel.rect.width;

        panel.DOAnchorPosX(-(showX + width), duration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => panel.gameObject.SetActive(false));
    }
    void Update()
    {
        // Hold-to-open
        if (holding)
        {
            holdTimer += Time.unscaledDeltaTime;

            if (holdTimer >= holdTime)
            {
                holding = false;
                holdTimer = 0f;
                ShowPanel();
            }
        }

        // Ctrl + D shortcut
        bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        if (ctrl && Input.GetKeyDown(KeyCode.D))
        {
            if (isOpen)
                HidePanel();
            else
                ShowPanel();
        }
    }
    void LoadClasses()
    {
        foreach (Transform c in modulesContent)
            Destroy(c.gameObject);

        objects.Clear();
        defaults.Clear();

        var sceneObjects = FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (var mb in sceneObjects)
        {
            var type = mb.GetType();

            if (!type.IsDefined(typeof(HasTabField), true))
                continue;


            object obj = mb;

            objects.Add(obj);


            var copy = Activator.CreateInstance(type);

            JsonUtility.FromJsonOverwrite(
                JsonUtility.ToJson(obj),
                copy
            );

            defaults[obj] = copy;


            var b = Instantiate(moduleButton, modulesContent);

            b.GetComponentInChildren<TMP_Text>()
            .text = type.Name;


            b.GetComponent<Button>()
            .onClick.AddListener(() => LoadDetails(obj));
        }
    }

    void Clear()
    {
        foreach (Transform c in detailsContent)
            Destroy(c.gameObject);
    }

    void LoadDetails(object obj)
    {
        current = obj;
        Clear();

        var type = obj.GetType();

        foreach (var f in type.GetFields())
        {
            if (!f.IsDefined(typeof(TabField), true))
                continue;

            CreateField(f, obj);
        }

        foreach (var m in type.GetMethods())
        {
            if (!m.IsDefined(typeof(TabButton), true))
                continue;

            var go = Instantiate(triggerPrefab, detailsContent);
            go.transform.Find("Title").GetComponent<TMP_Text>().text = m.Name;

            go.GetComponentInChildren<Button>()
            .onClick.AddListener(() => m.Invoke(obj, null));
        }
    }


    void CreateField(FieldInfo f, object obj)
    {
        GameObject prefab = null;

        if (f.FieldType == typeof(string))
            prefab = stringPrefab;

        else if (f.FieldType == typeof(int))
            prefab = integerPrefab;

        else if (f.FieldType == typeof(float) ||
                f.FieldType == typeof(double))
            prefab = decimalPrefab;

        else if (f.FieldType == typeof(bool))
            prefab = booleanPrefab;


        if (prefab == null) return;


        var go = Instantiate(prefab, detailsContent);

        go.transform.Find("Title")
        .GetComponent<TMP_Text>().text = f.Name;


        if (f.FieldType == typeof(string))
        {
            var input = go.GetComponentInChildren<TMP_InputField>();
            input.text = (string)f.GetValue(obj);
            input.onEndEdit.AddListener(v => f.SetValue(obj, v));
        }

        if (f.FieldType == typeof(int))
        {
            var input = go.GetComponentInChildren<TMP_InputField>();
            input.text = f.GetValue(obj).ToString();

            var buttons = go.GetComponentsInChildren<Button>();

            buttons[0].onClick.AddListener(() => {
                int v = (int)f.GetValue(obj) - 1;
                f.SetValue(obj, v);
                input.text = v.ToString();
            });

            buttons[1].onClick.AddListener(() => {
                int v = (int)f.GetValue(obj) + 1;
                f.SetValue(obj, v);
                input.text = v.ToString();
            });
        }


        if (f.FieldType == typeof(float))
        {
            var input = go.GetComponentInChildren<TMP_InputField>();
            input.text = f.GetValue(obj).ToString();

            var buttons = go.GetComponentsInChildren<Button>();

            buttons[0].onClick.AddListener(() => {
                float v = (float)f.GetValue(obj) - 0.1f;
                f.SetValue(obj, v);
                input.text = v.ToString("0.0");
            });

            buttons[1].onClick.AddListener(() => {
                float v = (float)f.GetValue(obj) + 0.1f;
                f.SetValue(obj, v);
                input.text = v.ToString("0.0");
            });
        }


        if (f.FieldType == typeof(bool))
        {
            var b = go.GetComponentInChildren<Button>();
            var txt = b.GetComponentInChildren<TMP_Text>();

            Action refresh = () => {
                bool v = (bool)f.GetValue(obj);
                txt.text = v.ToString();
            };

            b.onClick.AddListener(() => {
                f.SetValue(obj, !(bool)f.GetValue(obj));
                refresh();
            });

            refresh();
        }
    }


    void Save()
    {
        SaveData data = new();

        foreach (var o in objects)
        {
            data.values.Add(JsonUtility.ToJson(o));
        }

        File.WriteAllText(path, JsonUtility.ToJson(data, true));

        Debug.Log("Saved: " + path);
    }

    void Load()
    {
        if (!File.Exists(path))
            return;

        SaveData data = JsonUtility.FromJson<SaveData>(
            File.ReadAllText(path)
        );

        for (int i = 0; i < data.values.Count && i < objects.Count; i++)
        {
            JsonUtility.FromJsonOverwrite(
                data.values[i],
                objects[i]
            );
        }
    }
    void ResetValues()
    {
        foreach (var o in objects)
        {
            var d = defaults[o];

            foreach (var f in o.GetType().GetFields())
                f.SetValue(o, f.GetValue(d));
        }

        if (current != null)
            LoadDetails(current);
    }
}