

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Responsible for showing and hiding loading animation. 
/// Also ensures all other UI is hidden.
/// </summary>
public class LoadingAnimationManager : AConnectionManager<LoadingAnimationManager, LoadingAnimationManagerConfiguration>
{
    private Dictionary<Func<bool>, bool> completionActions;
    public bool copyPositionTonstanceOnDestroy = true;

    protected override bool ShouldCreateIfNull { get { return false; } }
    protected override bool ShouldWarnOfExistingInstance { get { return false; } }

    protected override bool ShouldDestroyOnLoad => false;
    [SerializeField]
    private Canvas loadingAnimationCanvas;

    private Camera uiCamera { get => configuration.uiCamera; }

    [SerializeField]
    private bool setActiveOnAwake;

    [SerializeField]
    private Image progressBar;

    int mask = 0;

    public Action OnLoadingComplete;
    public Action OnNextLoadingComplete;
    public bool loading
    {
        get
        {
            return loadingAnimationCanvas?.enabled == true;
        }
    }
    protected override void Awake()
    {
        base.Awake();
        OnConfigurationSwap += (newConfig) =>
        {
            loadingAnimationCanvas.worldCamera = uiCamera;
            bool shouldHideUi = false;
            foreach (var l in completionActions)
            {
                if (l.Value)
                {
                    shouldHideUi = true;
                    break;
                }
            }

            if (completionActions.Count > 0)
            {
                SetActive(true, shouldHideUi);
            }
        };

        mask = uiCamera.cullingMask;
        if (setActiveOnAwake)
            SetActive(true);

        SetProgress(0);
    }

    protected override void DestroyThisComponentImmediately()
    {
        if (copyPositionTonstanceOnDestroy)
        {
            instance.transform.position = transform.position;
        }

        base.DestroyThisComponentImmediately();
    }

    private void Update()
    {
        if (completionActions != null)
        {
            int totalActions = completionActions.Count;
            if (totalActions > 0)
            {
                int completedActions = 0;

                bool shouldHideUI = false;
                foreach (KeyValuePair<Func<bool>, bool> pair in completionActions)
                {
                    Func<bool> action = pair.Key;
                    if (action != null && action())
                    {
                        completedActions++;
                    }

                    shouldHideUI |= pair.Value;
                }

                if (!loading || (shouldHideUI && uiCamera && uiCamera.cullingMask == mask))
                {
                    SetActive(true, shouldHideUI);
                }

                // Calculate the current progress.
                float progress = ((float)completedActions) / totalActions;

                // Update the current progress.
                SetProgress(progress);

                // Remove all old actions.
                if (progress == 1)
                {
                    ForceStopLoading();
                }
            }
        }
    }

    /// <summary>
    /// Add a check for whether loading is completed
    /// </summary>
    /// <param name="loadingCompletedAction">A function that represents a completed loaded action. When this function returns true, it will be removed from the loading queue</param>
    public static void AddProgressCheck(Func<bool> loadingCompletedAction, bool shouldHideUI = false)
    {
        if (instance.completionActions == null)
        {
            instance.completionActions = new Dictionary<Func<bool>, bool>();
        }

        instance.completionActions[loadingCompletedAction] = shouldHideUI;
    }


    public void SetProgress(float ratio)
    {
        if (ratio == 0f)
        {
            progressBar.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            progressBar.transform.parent.gameObject.SetActive(true);
        }

        if (progressBar)
            progressBar.fillAmount = ratio;
    }

    public void ForceStopLoading()
    {
        completionActions.Clear();
        SetActive(false);
    }

    /// <summary>
    /// Wrapper used for more obvious naming
    /// </summary>
    public void Load(Func<bool> onLoadingCompleteAction, bool shouldHideAllUi = true)
    {
        AddProgressCheck(onLoadingCompleteAction, shouldHideAllUi);
    }

    /// <summary>
    /// Wrapper used for more obvious naming
    /// </summary>
    public void Load(bool shouldHideAllUi = true)
    {
        SetActive(true, shouldHideAllUi);
    }

    void SetActive(bool active, bool shouldHideAllUi = true)
    {
        if (loadingAnimationCanvas)
        {
            loadingAnimationCanvas.enabled = active;
            var anim = loadingAnimationCanvas.gameObject.GetComponentInChildren<Animator>();
            if (anim)
                anim.enabled = active;
            gameObject.SetChildrenActive(active);
        }
        else
        {
            Debug.LogError("Unassigned loadingAnimationCanvas ref!!", this);
            return;
        }

        if (shouldHideAllUi)
        {
            if (active)
            {
                if (uiCamera != null)
                {
                    uiCamera.cullingMask = (1 << LayerMask.NameToLayer("UI_Preloader"));
                    Debug.LogWarningFormat(
                        "Changing culling mask on camera: {0} to 'UI_Preloader'.",
                        uiCamera.name);
                }
            }
            else
            {
                if (uiCamera != null)
                {
                    Debug.LogWarningFormat(
                    "Restoring culling mask on camera: {0}.",
                    uiCamera.name);
                    uiCamera.cullingMask = mask;
                }
            }
        }

        if (!active)
        {
            OnLoadingComplete?.SafeInvoke();
            OnNextLoadingComplete?.SafeInvoke();
            OnNextLoadingComplete = null;
        }
    }
}
