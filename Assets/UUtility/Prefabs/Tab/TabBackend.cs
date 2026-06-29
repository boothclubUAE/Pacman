using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTool.TabSystem
{
    public class TabBackend : MonoBehaviour
    {
        public void OnValueChange(VariableUpdateType updateType, TVariable variable)
        {
            if (updateType == VariableUpdateType.Changed)
                return;

            //if (variable.variableName == TabBackend.VariableName.)
        }

        public static System.Type GetType(TabVariableType variableType)
        {
            switch (variableType)
            {
                case TabVariableType.String:
                    return typeof(string);

                case TabVariableType.Float:
                    return typeof(float);

                case TabVariableType.Int:
                    return typeof(int);

                case TabVariableType.Bool:
                    return typeof(bool);

                case TabVariableType.Vector3:
                    return typeof(Vector3);

                case TabVariableType.Vector2:
                    return typeof(Vector2);

                default: return null;
            }
        }

        public static TabVariableType GetVariableType(object type)
        {
            switch (type)
            {
                case string:
                    return TabVariableType.String;

                case float:
                    return TabVariableType.Float;

                case int:
                    return TabVariableType.Int;

                case bool:
                    return TabVariableType.Bool;

                case Vector3:
                    return TabVariableType.Vector3;

                case Vector2:
                    return TabVariableType.Vector2;

                default:
                    return TabVariableType.None;
            }
        }
    }

    public enum TabName
    {
        Variables = 0,
        Settings = 100,

        UNetComm = 200,
        UNetComm2 = 201,
        UNetComm3 = 202,
        UNetComm4 = 203,

        SerialComms1 = 300,
        SerialComms2 = 301,
        SerialComms3 = 302,
        SerialComms4 = 303,

        None = 1000
    }

    public enum TabVariableName
    {
        V1,
        V2,
        V3,
        V4,

        OpenDataFolder = 100,
        RestartScene = 101,
        ShowDebugConsole = 102,
        DebugLog = 103,
        PerformanceStats = 105,

        IP = 200,
        Port = 201,
        Protocol = 202,
        ServiceMode = 203,
        ServiceType = 204,
        GetServerIP = 205,

        COM_ID = 300,
        BaudRate = 301,
        ReconnectionDelay = 302,
        MaxUnreadQueue = 303,
        ProccessAtRate = 304,
        WaitWhileTerminating = 305
    }

    public enum TabVariableType
    {
        String = 0,
        Float = 1,
        Int = 2,
        Bool = 3,
        Vector3 = 4,
        Vector2 = 5,
        Trigger = 6,

        None = 100
    }

    public enum VariableUpdateType
    {
        Ready,
        Changed,
        Applied
    }

    public enum FieldUpdateMode
    {
        Live,
        Applied
    }
}