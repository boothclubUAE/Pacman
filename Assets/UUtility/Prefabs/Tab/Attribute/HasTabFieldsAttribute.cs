using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTool.TabSystem
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class HasTabFieldAttribute : Attribute
    {
        public object parent;
        public Type parentType;

        public bool active = false;
    }
}