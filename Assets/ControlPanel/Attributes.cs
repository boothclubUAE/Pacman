using System;

[AttributeUsage(AttributeTargets.Class)]
public class HasTabField : Attribute { }

[AttributeUsage(AttributeTargets.Field)]
public class TabField : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public class TabButton : Attribute { }