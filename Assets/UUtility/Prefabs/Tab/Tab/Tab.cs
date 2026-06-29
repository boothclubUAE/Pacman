using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

using UTool.Utility;

using Newtonsoft.Json;

namespace UTool.TabSystem
{
    public class Tab : MonoBehaviour
    {
        [BeginGroup]
        [ShowIf(nameof(controlledByAttribute))][NewLabel("Tab Name")][Disable] public string attTabName;
        [HideIf(nameof(controlledByAttribute))][NewLabel("Tab Name")][SearchableEnum] public TabName tTabName;

        [Disable] public bool controlledByAttribute = false;
        [EndGroup]
        [Disable] public bool usedByAttribute = false;

        public string tabName => controlledByAttribute ? attTabName : tTabName.ToString();

        [SpaceArea]

        [SerializeField]
        [LabelByChild("tVariableName")]
        [ReorderableList(ListStyle.Round, Foldable = true)]
        public List<TVariable> tabVariables = new List<TVariable>();

        [SpaceArea]

        [SerializeField] public UnityEvent OnLoaded = new UnityEvent();
        [SpaceArea]
        [SerializeField] public UnityEvent OnSaved = new UnityEvent();
        [SpaceArea]
        [SerializeField] public UnityEvent<VariableUpdateType, TVariable> OnAnyVariableUpdate = new UnityEvent<VariableUpdateType, TVariable>();

        [HideInInspector] public TabContent content;

        private string fileName => $"{tabName}Config.json";
        private string filePath => $@"{UT.dataPath}\TabConfigData\{fileName}";

        //[Button]
        //private void UpdateTabVariableData()
        //{
        //    TriggerTabVariableApplyDefault();
        //    SaveDataToFile();
        //}

        public void EarlyAwake()
        {

        }

        public void SetupTab()
        {
            foreach (TVariable variable in tabVariables)
            {
                variable.OnValueUpdate.RemoveAllListeners();
                variable.OnValueUpdate.AddListener(InvokeOnAnyVariableUpdate);
            }

            ApplyDefaultValue();
            LoadDataFromFile();
        }

        private void OnValidate()
        {
            ValidateTabVariables();
        }

        public TVariable GetVariable(TabVariableName variableName)
        {
            TVariable variable = tabVariables.Find(x => x.tVariableName == variableName);

            if (variable == null)
                Debug.LogError($"Variable '{variableName}' Could Not Be Found In Tab '{tabName}'");

            return variable;
        }

        private void ValidateTabVariables()
        {
            List<string> variableNames = new List<string>();

            string messagePart = "";
            string seperator = $"<color=#ff0000>||</color>";

            for (int i = 0; i < tabVariables.Count; i++)
            {
                TVariable variable = tabVariables[i];

                if (variableNames.Contains(variable.variableName))
                    messagePart += $" {variable.variableName} - Index {variableNames.IndexOf(variable.variableName)}, {i} {seperator}";
                else
                    variableNames.Add(variable.variableName);
            }

            if (messagePart != "")
                Debug.LogError($"Dupicate Entires {seperator}{messagePart} - At Tab : {tabName}");
        }

        private void Start()
        {
            OnLoaded?.Invoke();
        }

        public void ApplyTabVariables()
        {
            SaveDataToFile();

            foreach (TVariable variable in tabVariables)
                variable.TriggerValueUpdate(VariableUpdateType.Applied);

            OnSaved?.Invoke();
        }

        public void LoadDataFromFile()
        {
            if (UUtility.CheckAndCreateFile(filePath))
            {
                SaveDataToFile();
            }
            else
            {
                string data = File.ReadAllText(filePath);
                List<TVariable> fileTabVariables = JsonConvert.DeserializeObject<List<TVariable>>(data);

                foreach (TVariable var in tabVariables)
                {
                    TVariable varCopy = fileTabVariables.Find(x => x.variableName == var.variableName);

                    if (varCopy != null)
                    {
                        varCopy.LoadVectorData();
                        var.Copy(varCopy);
                    }
                }

                LoadTabVariableVectorData();
            }

            foreach (TVariable variable in tabVariables)
                variable.TriggerValueUpdate(VariableUpdateType.Ready);
        }

        public void SaveDataToFile()
        {
            StoreTabVariableVectorData();

            UUtility.CheckAndCreateFile(filePath);

            string data = JsonConvert.SerializeObject(tabVariables, Formatting.Indented);
            File.WriteAllText(filePath, data);
        }

        private void StoreTabVariableVectorData()
        {
            foreach (TVariable variable in tabVariables)
                variable.StoreVectorData();
        }

        private void LoadTabVariableVectorData()
        {
            foreach (TVariable variable in tabVariables)
                variable.LoadVectorData();
        }

        public void ApplyDefaultValue()
        {
            foreach (TVariable variable in tabVariables)
                variable.ApplyDefault();
        }

        private void InvokeOnAnyVariableUpdate(VariableUpdateType updateType, TVariable variable)
        {
            OnAnyVariableUpdate?.Invoke(updateType, variable);
        }
    }
}