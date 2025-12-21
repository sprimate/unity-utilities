using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    public int averageOver = 5;
    float[] _samples;
    int _index;
    bool ShouldShow => Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.F);
    void Start() => _samples = new float[averageOver];

    void Update()
    {
        _samples[_index] = 1f / Time.unscaledDeltaTime;
        _index = (_index + 1) % _samples.Length;
    }

    void OnGUI()
    {
        if (!ShouldShow)
        {
            return;
        }
        
        float avg = 0;
        for (int i = 0; i < _samples.Length; i++)
        {
            avg += _samples[i];
        }
        avg /= _samples.Length;

        int w = 120, h = 30;
        Rect bg = new(Screen.width - w - 4, 4, w, h);
        GUI.color = Color.black;
        GUI.DrawTexture(bg, Texture2D.whiteTexture);

        GUI.color = Color.green;
        GUI.Label(bg, $"FPS: {avg:F1}");
    }
}
