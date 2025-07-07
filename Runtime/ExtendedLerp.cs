using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtendedLerp  {

	public static Vector3 EaseOut(Vector3 start, Vector3 end, float t){
		t = Mathf.Sin(t * Mathf.PI * 0.5f);
		return Vector3.Lerp(start,end,t);
	}
	public static Vector3 EaseIn(Vector3 start, Vector3 end, float t){
		t = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
		return Vector3.Lerp(start,end,t);
	}

	public static Vector3 Smoothstep(Vector3 start, Vector3 end, float t){
		t = t*t * (3f - 2f*t);
		return Vector3.Lerp(start,end,t);
	}

	public static Vector3 Smootherstep(Vector3 start, Vector3 end, float t){
		t = t*t*t * (t * (6f*t - 15f) + 10f);
		return Vector3.Lerp(start,end,t);
	}

	public static float Smoothstep(float start, float end, float t){
		t = t*t * (3f - 2f*t);
		return Mathf.Lerp(start,end,t);
	}

	public static float Smootherstep(float start, float end, float t){
		t = t*t*t * (t * (6f*t - 15f) + 10f);
		return Mathf.Lerp(start,end,t);
	}
}