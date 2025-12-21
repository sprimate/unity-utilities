using UnityEngine;

namespace HitTrax.UnityUtilities
{
    [RequireComponent(typeof(Camera))]

    /// <summary>
    /// While it may seem unnecessary at first, this makes it easier to manage Cameras
    /// in which you want to render all but one (or a few) layers.
    /// Otherwise, each time you add a new layer, you need to go back to the Camera and add that layer
    /// In this case, you just set the Camera to render everything, but then exclude what you
    /// don't want at runtime.
    /// </summary>
    public class ExcludeCullingMaskOnStart : MonoBehaviour
    {
        public string[] excludeLayers;

        private void Start()
        {
            Camera camera = this.GetComponent<Camera>();
            if (camera != null)
            {
                foreach (var layer in excludeLayers)
                {
                    camera.RemoveFromCullingMask(layer);
                }
            }     
        }

    }
}
