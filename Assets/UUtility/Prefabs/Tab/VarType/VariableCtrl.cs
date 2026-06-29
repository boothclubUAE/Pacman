using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

namespace UTool.TabSystem
{
    public class VariableCtrl : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameDisplay;
        [SpaceArea]
        [SerializeField] private TextMeshProUGUI valueDisplay;
        [SerializeField][ReorderableList(Foldable = true)] private List<TMP_InputField> inputFields = new List<TMP_InputField>();

        private TVariable tVariable;

        bool updatingDisplay = false;

        public void AssignVariable(TVariable tVariable)
        {
            this.tVariable = tVariable;

            string displayName = tVariable.variableName.Insert(1, "</uppercase>").Insert(0, "<uppercase>"); ;
            nameDisplay.text = displayName;

            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            updatingDisplay = true;

            switch (tVariable.variableType)
            {
                case TabVariableType.String:
                    inputFields[0].text = tVariable.stringValue;
                    break;

                case TabVariableType.Float:
                    inputFields[0].text = tVariable.floatValue + "";
                    break;

                case TabVariableType.Int:
                    inputFields[0].text = tVariable.intValue + "";
                    break;

                case TabVariableType.Bool:
                    valueDisplay.text = GetBoolValue();
                    break;

                case TabVariableType.Vector3:
                    inputFields[0].text = tVariable.splitVector3Value[0] + "";
                    inputFields[1].text = tVariable.splitVector3Value[1] + "";
                    inputFields[2].text = tVariable.splitVector3Value[2] + "";
                    break;

                case TabVariableType.Vector2:
                    inputFields[0].text = tVariable.splitVector2Value[0] + "";
                    inputFields[1].text = tVariable.splitVector2Value[1] + "";
                    break;

                case TabVariableType.Trigger:
                    break;
            }

            updatingDisplay = false;
        }

        private void SetInputCaretToEnd()
        {
            foreach (TMP_InputField inputField in inputFields)
                inputField.MoveTextEnd(false);
        }

        public void UpdateValue()
        {
            if (updatingDisplay)
                return;

            switch (tVariable.variableType)
            {
                case TabVariableType.String:
                    tVariable.stringValue = inputFields[0].text;
                    break;

                case TabVariableType.Float:
                    tVariable.floatValue = GetInputTextAsFloat(0);
                    break;

                case TabVariableType.Int:
                    tVariable.intValue = GetInputTextAsInt(0);
                    break;

                case TabVariableType.Bool:
                    tVariable.boolValue = !tVariable.boolValue;
                    break;

                case TabVariableType.Vector3:
                    tVariable.vector3Value = new Vector3(GetInputTextAsFloat(0), GetInputTextAsFloat(1), GetInputTextAsFloat(2));
                    tVariable.StoreVectorData();
                    break;

                case TabVariableType.Vector2:
                    tVariable.vector2Value = new Vector2(GetInputTextAsFloat(0), GetInputTextAsFloat(1));
                    tVariable.StoreVectorData();
                    break;

                case TabVariableType.Trigger:
                    tVariable.Trigger();
                    break;
            }

            //UpdateDisplay();

            tVariable.TriggerValueUpdate(VariableUpdateType.Changed);
        }

        private int GetInputTextAsInt(int id)
        {
            if (long.TryParse(inputFields[id].text, out long value))
            {
                if (value > int.MaxValue)
                    return int.MaxValue;
                else if (value < int.MinValue)
                    return int.MinValue;

                return (int)value;
            }
            else
                return 0;
        }

        private float GetInputTextAsFloat(int id)
        {
            if (float.TryParse(inputFields[id].text, out float value))
                return value;
            else
                return 0;
        }

        private string GetBoolValue()
        {
            string value = tVariable.boolValue ? tVariable.trueValue : tVariable.falseValue;
            return value == "" ? tVariable.boolValue.ToString() : value;
        }
    }
}