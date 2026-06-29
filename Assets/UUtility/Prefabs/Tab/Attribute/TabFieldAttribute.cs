using System;
using System.Reflection;
using UnityEngine;

namespace UTool.TabSystem
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class TabFieldAttribute : Attribute
    {
        public object parent;

        public bool vaild => parent != null;

        public TabName tab;
        public string variableName;
        public FieldUpdateMode fieldUpdateModel;
        public string callbackMethodName;

        public TabVariableType variableType;
        public Type vType;
        public object defaultValue;

        public TVariable tVariable;
        public FieldInfo fieldInfo;

        public TabFieldAttribute(FieldUpdateMode fieldUpdateModel, string callbackMethodName = null) : this(TabName.None, fieldUpdateModel: fieldUpdateModel, callbackMethodName: callbackMethodName)
        {
        }

        public TabFieldAttribute(string callbackMethodName) : this(TabName.None, callbackMethodName: callbackMethodName)
        {
        }

        public TabFieldAttribute(TabName tabName = TabName.None, string fieldName = "", FieldUpdateMode fieldUpdateModel = FieldUpdateMode.Applied, string callbackMethodName = null)
        {
            tab = tabName;
            variableName = fieldName;
            this.fieldUpdateModel = fieldUpdateModel;
            this.callbackMethodName = callbackMethodName;
        }

        public void Setup(FieldInfo fieldInfo, object parent)
        {
            this.fieldInfo = fieldInfo;
            this.parent = parent;

            if (variableName == "")
                variableName = fieldInfo.Name;

            defaultValue = fieldInfo.GetValue(parent);

            variableType = TabBackend.GetVariableType(defaultValue);
            vType = TabBackend.GetType(variableType);

            if (variableType == TabVariableType.None)
                Debug.LogWarning($"Tab Field Attribute - Variable '{variableName}' Is Unsupported Type : {defaultValue.GetType()}");
        }

        public void UpdateValue(object value, VariableUpdateType updateType)
        {
            if (updateType == VariableUpdateType.Changed && fieldUpdateModel == FieldUpdateMode.Applied)
                return;

            fieldInfo.SetValue(parent, value);

            if (string.IsNullOrEmpty(callbackMethodName))
                return;

            MethodInfo methodInfo = parent.GetType().GetMethod(callbackMethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (methodInfo == null)
                return;

            switch (methodInfo.GetParameters().Length)
            {
                case 0:
                    methodInfo.Invoke(parent, null);
                    break;

                case 1:
                    methodInfo.Invoke(parent, new object[] { updateType });
                    break;

                case 2:
                    methodInfo.Invoke(parent, new object[] { value, updateType });
                    break;

                case 3:
                    methodInfo.Invoke(parent, new object[] { value.GetType(), value, updateType });
                    break;

                default:
                    break;

            }
        }
    }
}