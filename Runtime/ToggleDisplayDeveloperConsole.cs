using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleDisplayDeveloperConsole : MonoSingleton<ToggleDisplayDeveloperConsole>
{
    protected override bool ShouldDestroyOnLoad => false;
    public bool displayDevConsole;
    void Start()
    {
        if (Debug.isDebugBuild)
        {
            Debug.developerConsoleVisible = displayDevConsole;
        }
    }
    // Update is called once per frame
    void LateUpdate()
    {
        if (!Debug.isDebugBuild)
        {
            return;
        }

        if (DebugHelper.CheckDebugMode(KeyCode.G, "Toggle Developer Console: " + (!displayDevConsole)))
        {
            displayDevConsole = !displayDevConsole;
            if (displayDevConsole)
            {
                Debug.developerConsoleVisible = true;
            }
        }

        if (!displayDevConsole)
        {
            Debug.developerConsoleVisible = false;
        }
    }

    void Update()
    {
        if (!displayDevConsole)
        {
            Debug.developerConsoleVisible = false;
        }
    }
}
