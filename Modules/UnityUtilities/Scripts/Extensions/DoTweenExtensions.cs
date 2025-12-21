using UnityEngine;

namespace HitTrax.UnityUtilities
{
    using UnityEngine;
    using UnityEngine.Rendering;
    using DG.Tweening;

    public static class DoTweenExtensions
    {
        public static Tween DOWeight(this Volume volume, float endValue, float duration)
        {
            return DOTween.To(
                () => volume.weight,
                x => volume.weight = x,
                endValue,
                duration
            );
        }

        public static Tween DOFadeOut(this Volume volume, float duration)
        {
            return volume.DOWeight(0f, duration);
        }

        public static Tween DOFadeIn(this Volume volume, float duration)
        {
            return volume.DOWeight(1f, duration);
        }

        public static Tween DOHeight(this RectTransform rectTransform, float endValue, float duration)
        {
            return DOTween.To(
                () => rectTransform.sizeDelta.y,
                y => rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, y),
                endValue,
                duration
            );
        }
    }
}
