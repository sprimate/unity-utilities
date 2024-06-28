using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugHelperCanvas : MonoBehaviour
{
    public TextMeshProUGUI textTemplate;
    void OnEnable()
    {
        DebugHelper.instance.textTemplate = textTemplate;
    }

    private void OnDisable() {
        if (DebugHelper.instance.textTemplate == textTemplate)
        {
            DebugHelper.instance.textTemplate = null;
        }
    }
}