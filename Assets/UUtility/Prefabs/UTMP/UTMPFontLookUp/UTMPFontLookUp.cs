using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "UTMPFontLookUp", menuName = "Create Font Look Up Table")]
public class UTMPFontLookUp : ScriptableObject
{
    [SerializeField] public TMP_FontAsset primary;
    [SerializeField] public TMP_FontAsset secondary;
    [SerializeField] public TMP_FontAsset tertiary;
}
