using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using UTool.Utility;
using UTool.Tweening;
using UTool.TabSystem;

using DG.Tweening;

namespace UTool
{
    public class UT : MonoBehaviour
    {
        public static UT _instance;

        public static readonly string devProjectName = "UUtility";
        public static string projectName = "EmptyProjectName";

        public static string workingPath => $@"{System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)}\UTool";
        public static string environmentDataPath => $@"{workingPath}\Softwares\{(isUUtilityProject ? devProjectName : projectName)}";
        public static string dataPath => isAndroidPlatform ? $@"{Application.persistentDataPath}\{projectName}" : environmentDataPath;
        public static string tempDataPath => $@"{(isAndroidPlatform? Application.persistentDataPath : workingPath)}\temp";

        public static bool isUUtilityProject => Application.productName == devProjectName;
        public static bool isStandardProject => !isUUtilityProject;
        public static bool isAndroidPlatform => Application.platform == RuntimePlatform.Android;

        public static Color primaryColor => UT._instance._primaryColor;
        public static Color secondaryColor => UT._instance._secondaryColor;
        public static Color tertiaryColor => UT._instance._tertiaryColor;

        [SerializeField][BeginGroup][DisableIf(nameof(isStandardProject))] public string version;
        [SerializeField][EndGroup] private string _projectName;

        [SpaceArea]

        [SerializeField][BeginGroup][EndGroup] private HiddenButtonAvoidTouchState avoidTouchState;

        [SpaceArea]

        [SerializeField][BeginGroup] private bool activateMultiDisplay;
        [SerializeField][EndGroup] private int multiDisplayCount;

        [SpaceArea]

        [SerializeField][BeginGroup] public UTSceneHelper utSceneHelperPrefab;
        [SerializeField][EndGroup] public TabManager tabManager;

        [SpaceArea]

        [SerializeField][BeginGroup] private GameObject graphy;
        [SerializeField] private GameObject singletonHolder;
        [SerializeField] private TweenElement toolPanelTE;
        [SerializeField] private HiddenButtonPanel hiddenButtonPanel;
        [SerializeField][EndGroup] private CanvasGroup debugConsoleCG;

        [SpaceArea]

        [SerializeField][BeginGroup] private Color _primaryColor;
        [SerializeField] private Color _secondaryColor;
        [SerializeField][EndGroup] private Color _tertiaryColor;

        [SpaceArea]

        [SerializeField][ReorderableList] public List<Camera> sceneCameras = new List<Camera>();


        bool isToolPanelOpen = false;
        bool isDebugConsoleOpen = false;

        float toolButtonDownTimer = -1;

        public void SetProjectName(string newName)
        {     
            projectName = _projectName = newName;
        }

        public void EarlyAwake()
        {
            Application.runInBackground = true;
            if (isAndroidPlatform)
                Application.targetFrameRate = 60;
            else
                Application.targetFrameRate = 120;

            if (activateMultiDisplay)
                for (int i = 1; i < multiDisplayCount; i++)
                        if (i < Display.displays.Length)
                            Display.displays[i].Activate();

            if (isStandardProject)
            {
                if (string.IsNullOrWhiteSpace(_projectName))
                    Debug.LogError("Add Project Name", gameObject);
                else
                    projectName = _projectName;
            }

            
            if (UUtility.DirectoryExists(tempDataPath))
                UUtility.DeleteDirectory(tempDataPath, true);

            UUtility.CreateDirectory(tempDataPath);
        }

        public void MidAwake()
        {

        }

        public void LateAwake()
        {

        }

        public void OnSceneAwake()
        {
            tabManager.ReloadManager();

            BroadcastMessage("SceneAwake", SendMessageOptions.DontRequireReceiver);
        }

        public void OnSceneStart()
        {
            BroadcastMessage("SceneStart", SendMessageOptions.DontRequireReceiver);
        }

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;

            singletonHolder.SetActive(true);
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKeyDown(KeyCode.D))
                    toolButtonDownTimer = 0;

                if (Input.GetKey(KeyCode.D))
                {
                    if (toolButtonDownTimer >= 0)
                        toolButtonDownTimer += Time.deltaTime;

                    if (toolButtonDownTimer >= 0.3f)
                    {
                        toolButtonDownTimer = -1;
                        hiddenButtonPanel.OpenRing(true);
                    }
                }

                if (Input.GetKeyUp(KeyCode.D))
                    if (hiddenButtonPanel.isOpen)
                        hiddenButtonPanel.OpenRing(false);
                    else
                        ToggleToolPanel();
            }
            else
            {
                if (Input.GetKey(KeyCode.D))
                {
                    if (hiddenButtonPanel.isOpen)
                        hiddenButtonPanel.OpenRing(false);
                }
            }

            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.R))
            {
                if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                    RestartLevel();
                else
                    RestartLevelTo(0);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            OnAfterRestart();
        }

        public void ToggleToolPanel() => ShowToolPanel(!isToolPanelOpen);
        public void ShowToolPanel(bool state)
        {
            isToolPanelOpen = state;
            toolPanelTE.Show(isToolPanelOpen);
        }

        public void ToggleDebugConsole() => ShowDebugConsole(!isDebugConsoleOpen);
        public void ShowDebugConsole(bool state)
        {
            isDebugConsoleOpen = state;
            debugConsoleCG.FadeCanvasGroup(isDebugConsoleOpen);
        }

        public void OpenDataFolder()
        {
            Application.OpenURL(dataPath);
        }

        public static void RestartLevel()
        {
            OnBeforeRestart();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public static void RestartLevelTo(int sceneIndex)
        {
            OnBeforeRestart();
            SceneManager.LoadScene(sceneIndex);
        }

        private static void OnBeforeRestart()
        {
            DOTween.Clear(true);
            DOTween.KillAll();
        }

        private static void OnAfterRestart()
        {
            Instantiate(_instance.utSceneHelperPrefab);
        }

        public void OnValueChange(VariableUpdateType updateType, TVariable variable)
        {
            if (variable.tVariableName == TabVariableName.PerformanceStats)
                graphy.SetActive(variable.boolValue);
        }

        public void EarlyQuit()
        {

        }

        public void MidQuit()
        {

        }

        public void LateQuit()
        {

        }

        #region BroadcastEvent

        public void Awake()
        {
            if (_instance)
            {
                DestroyImmediate(gameObject);
                return;
            }
            else
            {
                _instance = this;
                DontDestroyOnLoad(this);
            }

            switch (avoidTouchState)
            {
                case HiddenButtonAvoidTouchState.OnlyInBuild:
#if !UNITY_EDITOR
                    hiddenButtonPanel.DisableTouch(true);
#endif
                    break;
                case HiddenButtonAvoidTouchState.InEditorAndBuild:
                    hiddenButtonPanel.DisableTouch(true);
                    break;
            }

            BroadcastMessage("EarlyAwake", SendMessageOptions.DontRequireReceiver);
            BroadcastMessage("MidAwake", SendMessageOptions.DontRequireReceiver);
            BroadcastMessage("LateAwake", SendMessageOptions.DontRequireReceiver);
        }

        private void OnApplicationQuit()
        {
            BroadcastMessage("EarlyQuit", SendMessageOptions.DontRequireReceiver);
            BroadcastMessage("MidQuit", SendMessageOptions.DontRequireReceiver);
            BroadcastMessage("LateQuit", SendMessageOptions.DontRequireReceiver);
        }

        #endregion

        private enum HiddenButtonAvoidTouchState
        {
            None,
            OnlyInBuild,
            InEditorAndBuild
        }
    }
}