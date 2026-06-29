using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UTMPManager : MonoBehaviour
{
    [EditorButton(nameof(UpdateAllUTMP))]
    [SerializeField][InLineEditor] public UTMPFontLookUp fontLookup;

    private void UpdateAllUTMP()
    {
        Debug.Log("aa");

    }
}
