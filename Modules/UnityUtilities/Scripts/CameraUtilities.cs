using System.Collections.Generic;
using System.Linq;
using HitTrax.CoreUtilities;
using UnityEngine;

namespace HitTrax.UnityUtilities
{
    public static class CameraUtilities
    {
        public static bool IsCulled(this Camera camera, string maskName)
            => camera.IsCulled(maskName.NameToLayer());

        public static bool AreCullingAny(this Camera camera, IEnumerable<string> maskNames)
            => maskNames.Any(layerMask => camera.IsCulled(layerMask)); 

        public static bool IsCulled(this Camera camera, LayerMask layerMask)
            => (camera.cullingMask & (1 << layerMask)) != 0;
        public static Safe<Camera> AddToCullingMask(this Safe<Camera> camera, string maskName)
            => camera.IfSome(c => c.AddToCullingMask(maskName.NameToLayer()));

        public static Camera AddToCullingMask(this Camera camera, string maskName)
            => camera.AddToCullingMask(maskName.NameToLayer());

        public static Camera AddToCullingMask(this Camera camera, LayerMask layerMask)
        {
            camera.cullingMask |= (1 << layerMask);
            return camera;
        }

        public static void AddToCullingMask(this IEnumerable<Camera> cameras, string maskName)
            => cameras.AddToCullingMask(maskName.NameToLayer());           

        public static IEnumerable<Camera> AddToCullingMask(this IEnumerable<Camera> cameras, LayerMask layerMask)
        {            
            foreach (var camera in cameras)
            {
                camera.AddToCullingMask(layerMask);
            }

            return cameras;
        }

        public static IEnumerable<Camera> RemoveFromCullingMask(this IEnumerable<Camera> cameras, string maskName)
            => cameras.RemoveFromCullingMask(maskName.NameToLayer());

        public static IEnumerable<Camera> RemoveFromCullingMask(this IEnumerable<Camera> cameras, LayerMask layerMask)
        {            
            foreach (var camera in cameras)
            {
                camera.RemoveFromCullingMask(layerMask);
            }

            return cameras;
        }

        public static Safe<Camera> RemoveFromCullingMask(this Safe<Camera> camera, string maskName)
            => camera.IfSome(c => c.RemoveFromCullingMask(maskName.NameToLayer()));
        
        public static Camera RemoveFromCullingMask(this Camera camera, string maskName)
            => camera.RemoveFromCullingMask(maskName.NameToLayer());        

        public static Camera RemoveFromCullingMask(this Camera camera, LayerMask layerMask)
        {           
            camera.cullingMask &= ~(1 << layerMask);
            return camera;
        }        
    }
}
