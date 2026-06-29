using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UTool;

public class UTSceneHelper : MonoBehaviour
{
    private void Awake()
    {
        UT._instance.OnSceneAwake();
    }

    void Start()
    {
        UT._instance.OnSceneStart();
    }
}
