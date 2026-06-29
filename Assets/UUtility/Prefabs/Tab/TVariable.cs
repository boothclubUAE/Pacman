using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Newtonsoft.Json;

namespace UTool.TabSystem
{
    [Serializable]
    public class TVariable
    {
        [SpaceArea]

        [BeginGroup]
        [ShowIf(nameof(controlledByAttribute))][NewLabel("Variable Name")][Disable] public string attVariableName;
        [HideIf(nameof(controlledByAttribute))][NewLabel("Variable Name")][SearchableEnum] public TabVariableName tVariableName;
        [SearchableEnum] public TabVariableType variableType;

        [JsonIgnore] public string variableName => controlledByAttribute ? attVariableName : tVariableName.ToString();

        [SpaceArea]

        [JsonIgnore][NewLabel("Default Value"), ShowIf(nameof(isStringVar))] public string defaultStringValue;
        [JsonIgnore][NewLabel("Default Value"), ShowIf(nameof(isFloatVar))] public float defaultFloatValue;
        [JsonIgnore][NewLabel("Default Value"), ShowIf(nameof(isIntVar))] public int defaultIntValue;
        [JsonIgnore][NewLabel("Default Value"), ShowIf(nameof(isBoolVar))] public bool defaultBoolValue;
        [JsonIgnore][NewLabel("Default Value"), ShowIf(nameof(isVector3Var))] public Vector3 defaultVector3Value;
        [JsonIgnore][NewLabel("Default Value"), ShowIf(nameof(isVector2Var))] public Vector2 defaultVector2Value;

        [HideInInspector] public float[] defaultSplitVector3Value = new float[3];
        [HideInInspector] public float[] defaultSplitVector2Value = new float[2];

        [ShowIf(nameof(isBoolVar), true)] public string trueValue = "true";
        [ShowIf(nameof(isBoolVar), true)] public string falseValue = "false";

        [NewLabel("Value"), ShowDisabledIf(nameof(isStringVar))] public string stringValue;
        [NewLabel("Value"), ShowDisabledIf(nameof(isFloatVar))] public float floatValue;
        [NewLabel("Value"), ShowDisabledIf(nameof(isIntVar))] public int intValue;
        [NewLabel("Value"), ShowDisabledIf(nameof(isBoolVar))] public bool boolValue;
        [JsonIgnore][NewLabel("Value"), ShowDisabledIf(nameof(isVector3Var))] public Vector3 vector3Value;
        [JsonIgnore][NewLabel("Value"), ShowDisabledIf(nameof(isVector2Var))] public Vector2 vector2Value;

        [HideInInspector] public float[] splitVector3Value = new float[3];
        [HideInInspector] public float[] splitVector2Value = new float[2];

        [SpaceArea]

        [JsonIgnore][ShowIf(nameof(isTriggerVar))] public UnityEvent OnValueTrigger = new UnityEvent();
        [SpaceArea]
        [EndGroup]
        [JsonIgnore][HideIf(nameof(isTriggerVar))] public UnityEvent<VariableUpdateType, TVariable> OnValueUpdate = new UnityEvent<VariableUpdateType, TVariable>();
        
        [SpaceArea]

        [Disable] public bool controlledByAttribute = false;
        [JsonIgnore][HideInInspector] public bool usedByAttribute = false;

        private bool isStringVar => variableType == TabVariableType.String;
        private bool isFloatVar => variableType == TabVariableType.Float;
        private bool isIntVar => variableType == TabVariableType.Int;
        private bool isBoolVar => variableType == TabVariableType.Bool;
        private bool isVector3Var => variableType == TabVariableType.Vector3;
        private bool isVector2Var => variableType == TabVariableType.Vector2;
        private bool isTriggerVar => variableType == TabVariableType.Trigger;

        [JsonIgnore][HideInInspector] public List<TabFieldAttribute> tabFieldAttributes = new List<TabFieldAttribute>();
        [JsonIgnore][HideInInspector] public List<TabButtonAttribute> tabButtonAttributes = new List<TabButtonAttribute>();

        public void StoreDefaultValue(object value)
        {
            switch (value)
            {
                case string:
                    defaultStringValue = (string)value;
                    break;

                case float:
                    defaultFloatValue = (float)value;
                    break;

                case int:
                    defaultIntValue = (int)value;
                    break;

                case bool:
                    defaultBoolValue = (bool)value;
                    break;

                case Vector3:
                    defaultVector3Value = (Vector3)value;
                    break;

                case Vector2:
                    defaultVector2Value = (Vector2)value;
                    break;

                default:
                    Debug.LogWarning($"TVariable - Non-Supported Type : '{value.GetType()}'");
                    break;
            }

            StoreVectorData();
        }

        public object GetValue()
        {
            switch (variableType)
            {
                case TabVariableType.String:
                    return stringValue;

                case TabVariableType.Float:
                    return floatValue;

                case TabVariableType.Int:
                    return intValue;

                case TabVariableType.Bool:
                    return boolValue;

                case TabVariableType.Vector3:
                    return vector3Value;

                case TabVariableType.Vector2:
                    return vector2Value;

                default: return null;
            }
        }

        public void ApplyDefault()
        {
            stringValue = defaultStringValue;
            floatValue = defaultFloatValue;
            intValue = defaultIntValue;
            boolValue = defaultBoolValue;
            vector3Value = new Vector3(defaultVector3Value.x, defaultVector3Value.y, defaultVector3Value.z);
            vector2Value = new Vector2(defaultVector2Value.x, defaultVector2Value.y);

            StoreVectorData();
        }

        public void Copy(TVariable variable)
        {
            stringValue = variable.stringValue;
            floatValue = variable.floatValue;
            intValue = variable.intValue;
            boolValue = variable.boolValue;
            vector3Value = variable.vector3Value;
            vector2Value = variable.vector2Value;

            StoreVectorData();
        }

        public void StoreVectorData()
        {
            CopyVector3(defaultVector3Value, ref defaultSplitVector3Value);
            CopyVector2(defaultVector2Value, ref defaultSplitVector2Value);

            CopyVector3(vector3Value, ref splitVector3Value);
            CopyVector2(vector2Value, ref splitVector2Value);
        }

        public void LoadVectorData()
        {
            LoadVector3(defaultSplitVector3Value, ref defaultVector3Value);
            LoadVector2(defaultSplitVector2Value, ref defaultVector2Value);

            LoadVector3(splitVector3Value, ref vector3Value);
            LoadVector2(splitVector2Value, ref vector2Value);
        }

        private void CopyVector3(Vector3 fromVector, ref float[] toSplitVector)
        {
            toSplitVector = new float[3];

            toSplitVector[0] = fromVector.x;
            toSplitVector[1] = fromVector.y;
            toSplitVector[2] = fromVector.z;
        }

        private void CopyVector2(Vector2 vector, ref float[] toSplitVector)
        {
            toSplitVector = new float[2];

            toSplitVector[0] = vector.x;
            toSplitVector[1] = vector.y;
        }

        private void LoadVector3(float[] fromSplitVector, ref Vector3 toVector)
        {
            if (fromSplitVector.Length != 3)
                return;

            toVector = new Vector3(fromSplitVector[0], fromSplitVector[1], fromSplitVector[2]);
        }

        private void LoadVector2(float[] fromSplitVector, ref Vector2 toVector)
        {
            if (fromSplitVector.Length != 2)
                return;

            toVector = new Vector2(fromSplitVector[0], fromSplitVector[1]);
        }

        public void TriggerValueUpdate(VariableUpdateType updateType)
        {
            OnValueUpdate?.Invoke(updateType, this);

            foreach (TabFieldAttribute tabFieldAttribute in tabFieldAttributes)
                tabFieldAttribute.UpdateValue(GetValue(), updateType);
        }

        public void Trigger()
        {
            OnValueTrigger?.Invoke();

            foreach (TabButtonAttribute tabButtonAttribute in tabButtonAttributes)
                tabButtonAttribute.Trigger();
        }
    }
}