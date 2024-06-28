using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sprimate
{
    public class GlobalFadeManager : MonoSingleton<GlobalFadeManager> {

        protected override bool ShouldDestroyOnLoad
        {
            get
            {
                return true;
            }
        }
        public float fadeTime = 0.3f;
        public string fadeLayer;
        CanvasGroupAlphaCrossFader fader;
        protected override void Awake()
        {
            base.Awake();
            GameObject camObj = new GameObject("Fade Camera");
            camObj.transform.SetParent(transform);
            var cam = camObj.AddComponent<Camera>();
            cam.cullingMask = LayerMask.GetMask(fadeLayer);
            cam.depth = 99f;//(maxCamDepth - 1) allowed in inspector
            cam.clearFlags = CameraClearFlags.Nothing;
            GameObject faderCanvas = new GameObject("Fader Canvas");
            Canvas canvas = faderCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = cam;
            faderCanvas.transform.SetParent(camObj.transform);
            GameObject imageGo = new GameObject("Fader Image");
            imageGo.transform.SetParent(faderCanvas.transform);
            var canvasGroup = imageGo.GetOrAddComponent<CanvasGroup>();
            fader = imageGo.GetOrAddComponent<CanvasGroupAlphaCrossFader>();
            InitializeFader();
            var image = imageGo.AddComponent<Image>();
            image.color = Color.black;
            image.transform.localPosition = Vector3.zero;
            gameObject.SetLayerRecursive(LayerMask.NameToLayer(fadeLayer));
            faderCanvas.transform.localPosition = new Vector3(0, 0, cam.nearClipPlane + 0.001f);
        }

        public void Fade(bool toBlack, Action callback = null, int? _fadeTime = null)
        {
            if (_fadeTime.HasValue)
            {
                InitializeFader(_fadeTime.Value);
            }

            fader.Crossfade(toBlack, callback);
        }

        void InitializeFader(float? _fadeTime = null)
        {
            if (_fadeTime.HasValue)
            {
                fadeTime = _fadeTime.Value;
            }

            fader.Init(false, fadeTime);
        }
    }
}
