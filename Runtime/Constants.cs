

using UnityEngine;

public class Constants : MonoSingleton<Constants>
{
    #region Layers
    public const int UI_LAYER = 5;
    public const int IGNORE_RAYCAST_LAYER = 2;
    public const int HURTBOX_LAYER = 7;
    public const int GRINDABLE_LAYER = 22;
    public const int HURTBOX_SCANNER_LAYER = 24;
    public const int HITBOX_LAYER = 17;
    public const int ACTIVE_PLAYER_LAYER = 8;
    public const int OTHER_PLAYER_LAYER = 14;
    public const int PLAYER_TRIGGER_AREA_LAYER = 23;
    public const int LIGHT_SHIELD_LAYER = 19;
    public const int ACTIVE_ONE_WAY_PLATFORM_LAYER = 25;
    public const int INACTIVE_ONE_WAY_PLATFORM_LAYER = 26;
    public const int ITEM_LAYER = 16;

    #endregion

    [Header("Debug Properties")]
    public KeyCode debugKeyCode = KeyCode.LeftControl;

    [SerializeField] bool visualizeHitboxesBuild = false;
    [SerializeField] bool visualizeHitboxesEditor = true;
    public bool visualizeHitboxes
    {
        get
        {
            return Application.isEditor ? visualizeHitboxesEditor : visualizeHitboxesBuild;
        }

        set
        {
            if (Application.isEditor)
            {
                visualizeHitboxesEditor = value;
            }
            else
            {
                visualizeHitboxesBuild = value;
            }
        }
    }
    [Header("Materials")]
    public Material uiOverlayMaterial;
    public Material textMeshProOverlayMaterial;

    [Header("Melee")]
    public Material hitboxVisualizationMaterial;
    public float displayMeleeFrameDataSeconds;


    [Header("Layer Masks")]
    public LayerMask environmentLayerMask;
    public LayerMask hardEnvironmentLayerMask => environmentLayerMask ^ (1 << LIGHT_SHIELD_LAYER);
    public LayerMask targetableLayerMask;
    public LayerMask decalLayerMask => hardEnvironmentLayerMask;
}
