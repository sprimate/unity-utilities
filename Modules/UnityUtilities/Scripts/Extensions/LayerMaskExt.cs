using UnityEngine;

public static class LayerMaskExt
{
    /// <summary>
    /// Extension method to check if a layer is in a layermask
    /// </summary>
    /// <param name="mask"></param>
    /// <param name="layer"></param>
    /// <returns></returns>
    public static bool Contains(this LayerMask mask, int layer)
    {
        return mask == (mask | LayerToLayerMask(layer));//(1 << layer));
    }


    /// <summary>
    /// Inverts a LayerMask.
    /// </summary>
    public static LayerMask Inverse(this LayerMask original)
    {
        return ~original;
    }

    /// <summary>
    /// Adds a number of layer names to an existing LayerMask.
    /// </summary>
    public static LayerMask Combine(this LayerMask original, LayerMask otherLayerMask)
    {
        return original | otherLayerMask;
    }

    public static LayerMask LayerToLayerMask(int layer)
    {
        return 1 << layer;
    }
}