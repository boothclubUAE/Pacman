using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UTool.TabSystem
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TabButtonAttribute : Attribute
    {
        public object parent;

        public bool vaild => parent != null;

        public TabName tab;
        public string variableName;

        public TabVariableType variableType;
        public Type vType;

        public TVariable tVariable;
        public MethodInfo methodInfo;

        public TabButtonAttribute(TabName tabName = TabName.None, string fieldName = "")
        {
            tab = tabName;
            variableName = fieldName;
        }

        public void Setup(MethodInfo methodInfo, object parent)
        {
            this.methodInfo = methodInfo;
            this.parent = parent;

            if (variableName == "")
                variableName = methodInfo.Name;

            variableType = TabVariableType.Trigger;
        }

        public void Trigger()
        {
            if (methodInfo == null)
                return;

            methodInfo.Invoke(parent, null);
        }
    }
}