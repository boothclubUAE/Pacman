using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    object current;

    string path => Application.persistentDataPath + "/settings.txt";

    public RectTransform panel;
    public float showX = 10f;
    public float duration = 0.3f;

    bool isOpen;

    public Button invisibleButton;
    public float holdTime = 2f;

    float holdTimer;
    bool holding;

    public Button DebugButton;
    public GameObject InGameDebugMenu;

    void Start()
    {
        DebugButton.onClick.AddListener(() =>
            InGameDebugMenu.SetActive(!InGameDebugMenu.activeSelf));

        SetupHoldButton();

        LoadClasses();

        LoadFromFile();

        if (objects.Count > 0)
            LoadDetails(objects[0]);

        close.onClick.AddListener(HidePanel);
        apply.onClick.AddListener(SaveToFile);
        reset.onClick.AddListener(ResetValues);
    }

    void SaveToFile()
    {
        List<string> lines = new();

        foreach (object o in objects)
        {
            Type type = o.GetType();
            string prefix = type.Name + ".";

            foreach (FieldInfo f in type.GetFields())
            {
                if (!f.IsDefined(typeof(TabField), true))
                    continue;

                string key = prefix + f.Name;
                object value = f.GetValue(o);

                lines.Add($"{key}={value}");
            }
        }

        File.WriteAllLines(path, lines);
    }

    void LoadFromFile()
    {
        if (!File.Exists(path)) return;

        var lines = File.ReadAllLines(path);
        Dictionary<string, string> data = new();

        foreach (var line in lines)
        {
            if (!line.Contains("=")) continue;

            var sp = line.Split('=');
            data[sp[0]] = sp[1];
        }

        foreach (object o in objects)
        {
            Type type = o.GetType();
            string prefix = type.Name + ".";

            foreach (FieldInfo f in type.GetFields())
            {
                if (!f.IsDefined(typeof(TabField), true))
                    continue;

                string key = prefix + f.Name;
                if (!data.ContainsKey(key)) continue;

                string v = data[key];

                try
                {
                    if (f.FieldType == typeof(int))
                        f.SetValue(o, int.Parse(v));
                    else if (f.FieldType == typeof(float))
                        f.SetValue(o, float.Parse(v));
                    else if (f.FieldType == typeof(bool))
                        f.SetValue(o, bool.Parse(v));
                    else if (f.FieldType == typeof(string))
                        f.SetValue(o, v);
                }
                catch { }
            }
        }
    }

    void LoadClasses()
    {
        foreach (Transform t in modulesContent)
            Destroy(t.gameObject);

        objects.Clear();

        var sceneObjects =
            FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (var mb in sceneObjects)
        {
            Type type = mb.GetType();

            if (!type.IsDefined(typeof(HasTabField), true))
                continue;

            objects.Add(mb);

            GameObject b = Instantiate(moduleButton, modulesContent);
            b.GetComponentInChildren<TMP_Text>().text = type.Name;

            b.GetComponent<Button>().onClick.AddListener(() =>
                LoadDetails(mb));
        }
    }

    void Clear()
    {
        foreach (Transform t in detailsContent)
            Destroy(t.gameObject);
    }

    void LoadDetails(object obj)
    {
        current = obj;

        Clear();

        Type type = obj.GetType();

        foreach (FieldInfo f in type.GetFields())
        {
            if (!f.IsDefined(typeof(TabField), true))
                continue;

            CreateField(f, obj);
        }

        foreach (MethodInfo m in type.GetMethods())
        {
            if (!m.IsDefined(typeof(TabButton), true))
                continue;

            GameObject go = Instantiate(triggerPrefab, detailsContent);
            go.GetComponentInChildren<TMP_Text>().text = m.Name;
            go.GetComponentInChildren<Button>().onClick.AddListener(() => m.Invoke(obj, null));
        }
    }

    void CreateField(FieldInfo f, object obj)
    {
        GameObject prefab = null;

        if (f.FieldType == typeof(string)) prefab = stringPrefab;
        else if (f.FieldType == typeof(int)) prefab = integerPrefab;
        else if (f.FieldType == typeof(float)) prefab = decimalPrefab;
        else if (f.FieldType == typeof(bool)) prefab = booleanPrefab;

        if (prefab == null) return;

        GameObject go = Instantiate(prefab, detailsContent);
        go.GetComponentInChildren<TMP_Text>().text = f.Name;

        string key = obj.GetType().Name + "." + f.Name;

        if (f.FieldType == typeof(string))
        {
            var input = go.GetComponentInChildren<TMP_InputField>();
            input.text = f.GetValue(obj)?.ToString() ?? "";
            input.onEndEdit.AddListener(v =>
            {
                f.SetValue(obj, v);
                SaveToFile();
            });
        }

        if (f.FieldType == typeof(float))
        {
            var input = go.GetComponentInChildren<TMP_InputField>();
            Button[] buttons = go.GetComponentsInChildren<Button>();

            void Refresh(float v)
            {
                f.SetValue(obj, v);
                input.text = v.ToString("0.00");
            }

            float v0 = (float)f.GetValue(obj);
            Refresh(v0);

            input.onEndEdit.AddListener(v =>
            {
                if (float.TryParse(v, out float p))
                {
                    Refresh(p);
                    SaveToFile();
                }
            });

            buttons[0].onClick.AddListener(() =>
            {
                Refresh((float)f.GetValue(obj) + 0.1f);
                SaveToFile();
            });

            buttons[1].onClick.AddListener(() =>
            {
                Refresh((float)f.GetValue(obj) - 0.1f);
                SaveToFile();
            });
        }

        if (f.FieldType == typeof(int))
        {
            var input = go.GetComponentInChildren<TMP_InputField>();
            Button[] buttons = go.GetComponentsInChildren<Button>();

            void Refresh(int v)
            {
                f.SetValue(obj, v);
                input.text = v.ToString();
            }

            int v0 = (int)f.GetValue(obj);
            Refresh(v0);

            input.onEndEdit.AddListener(v =>
            {
                if (int.TryParse(v, out int p))
                {
                    Refresh(p);
                    SaveToFile();
                }
            });

            buttons[0].onClick.AddListener(() =>
            {
                Refresh((int)f.GetValue(obj) + 1);
                SaveToFile();
            });

            buttons[1].onClick.AddListener(() =>
            {
                Refresh((int)f.GetValue(obj) - 1);
                SaveToFile();
            });
        }

        if (f.FieldType == typeof(bool))
        {
            var button = go.GetComponentInChildren<Button>();
            var text = button.GetComponentInChildren<TMP_Text>();

            void Refresh()
            {
                text.text = ((bool)f.GetValue(obj)).ToString();
            }

            button.onClick.AddListener(() =>
            {
                f.SetValue(obj, !(bool)f.GetValue(obj));
                Refresh();
                SaveToFile();
            });

            Refresh();
        }
    }

    void ResetValues()
    {
        foreach (object o in objects)
        {
            Type type = o.GetType();
            object fresh = Activator.CreateInstance(type);

            foreach (FieldInfo f in type.GetFields())
            {
                if (!f.IsDefined(typeof(TabField), true)) continue;
                f.SetValue(o, f.GetValue(fresh));
            }
        }

        SaveToFile();

        if (current != null)
            LoadDetails(current);
    }

    void SetupHoldButton()
    {
        var trigger = invisibleButton.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        var down = new UnityEngine.EventSystems.EventTrigger.Entry
        {
            eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown
        };

        down.callback.AddListener(e =>
        {
            holding = true;
            holdTimer = 0;
        });

        var up = new UnityEngine.EventSystems.EventTrigger.Entry
        {
            eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp
        };

        up.callback.AddListener(e =>
        {
            holding = false;
            holdTimer = 0;
        });

        trigger.triggers.Add(down);
        trigger.triggers.Add(up);
    }

    void ShowPanel()
    {
        if (isOpen) return;
        isOpen = true;
        panel.gameObject.SetActive(true);
        float w = panel.rect.width;
        panel.anchoredPosition = new Vector2(-(w + showX), panel.anchoredPosition.y);
        panel.DOAnchorPosX(showX, duration);
    }

    void HidePanel()
    {
        if (!isOpen) return;
        isOpen = false;
        float w = panel.rect.width;
        panel.DOAnchorPosX(-(showX + w), duration)
            .OnComplete(() => panel.gameObject.SetActive(false));
    }

    void Update()
    {
        if (!holding) return;

        holdTimer += Time.unscaledDeltaTime;

        if (holdTimer >= holdTime)
        {
            holding = false;
            ShowPanel();
        }
    }
}