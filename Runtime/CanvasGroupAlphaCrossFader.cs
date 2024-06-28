


using UnityEngine;
using System.Collections;
using System;

namespace Sprimate
{
	[RequireComponent(typeof(CanvasGroup))]
	public class CanvasGroupAlphaCrossFader : MonoBehaviour
	{
		public enum State { Off, FadeIn, On, FadeOut  }
		private State state = State.Off;

		CanvasGroup canvasGroup;
		float fadeDuration = 1;

		public void Init(bool on, float fadeDuration = 0.25f)
		{
			canvasGroup = gameObject.GetComponent<CanvasGroup>();
			if (canvasGroup == null)
				canvasGroup = gameObject.AddComponent<CanvasGroup>();

            SetCanvasGroup(on);
			canvasGroup.alpha = on ? 1 : 0;
			alphaStart = canvasGroup.alpha;
			this.fadeDuration = fadeDuration;
		}

		public void Crossfade(bool on, Action _callback = null)
		{
            if (canvasGroup == null)
            {
                Init(!on);
            }
            callback = _callback;
			state = on ? State.FadeIn : State.FadeOut;
		}

		void SetCanvasGroup(bool on)
		{
			canvasGroup.interactable = on;
			canvasGroup.blocksRaycasts = on;
			state = on ? State.On : State.Off;
		}

        Action callback;
		State lastState;
		float timer;
		float tRatio;
		float alpha;
		float alphaStart;
		void Update()
		{
			if (state != lastState)
			{
				alphaStart = canvasGroup.alpha;
				timer = Time.unscaledTime;
				if (state == State.FadeIn)
				{
					SetCanvasGroup(true);
					state = State.FadeIn;
				}
			}
			tRatio = fadeDuration == 0 ? 1f : (Time.unscaledTime - timer) / fadeDuration;

			switch (state)
			{
				case State.Off:
				case State.On:
					break;
				case State.FadeIn:
					alpha = Mathf.Lerp(alphaStart, 1, tRatio);
					if (tRatio >= 1)
					{
						state = State.On;
						alpha = 1;
                        GenericCoroutineManager.RunInFrames(1, callback, this);
					}

					canvasGroup.alpha = alpha;
					break;
				
				case State.FadeOut:
					alpha = Mathf.Lerp(alphaStart, 0, tRatio);
					if (tRatio >= 1)
					{
						state = State.Off;
						alpha = 0;
						SetCanvasGroup(false);
                        GenericCoroutineManager.RunInFrames(1, callback, this);
                    }

                    canvasGroup.alpha = alpha;
					break;
				default:
					break;
			}

			lastState = state;
		}
	}
}