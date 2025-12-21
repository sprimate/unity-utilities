using UnityEngine;
using TMPro;
using UnityEngine.UI;
using HitTrax.CoreUtilities;

namespace HitTrax.UnityUtilities
{
    public static class UIUtilities
    {
        public static Safe<Button> SetTMProText(this Safe<Button> button, string text)
            => button.IfSome(b => b.SetTMProText(text));

        public static Button SetTMProText(this Button button, string text)
        {
            button
                .MaybeInChild<TextMeshProUGUI>()
                .IfSome(textMesh => textMesh.text = text);

            return button;
        }

        public static Button SetTMProColor(this Button button, Color color)
        {
            button
                .MaybeInChild<TextMeshProUGUI>()
                .IfSome(textMesh => textMesh.color = color);

            return button;
        }        
    }
}
