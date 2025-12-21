using HitTrax.CoreUtilities;
using UnityEngine;
using UnityEngine.Splines;

namespace HitTrax.UnityUtilities
{
    public static class SplineAnimatorUtilities
    {
        static public Safe<GameObject> AssignToSplineContainer(this Safe<GameObject> gameObject, Safe<SplineContainer> container)
        {
            gameObject.MaybeComponent<SplineAnimate>().AssignToSplineContainer(container);
            return gameObject;
        }
        static public Safe<SplineAnimate> AssignToSplineContainer(this Safe<Component> comp, Safe<SplineContainer> container)
           => comp.MaybeComponent<SplineAnimate>().AssignToSplineContainer(container);

        static public Safe<SplineAnimate> AssignToSplineContainer(this Safe<SplineAnimate> animator, Safe<SplineContainer> container)
            => animator.IfSome(anim => container.IfSome(cont => anim.AssignToSplineContainer(cont)));

        static public SplineAnimate AssignToSplineContainer(this SplineAnimate animator, SplineContainer container)
        {
            if (animator == null || container == null)
            {
                return animator;
            }

            animator.Container = container;
            return animator;
        }

        static public Safe<GameObject> PlaySplineAnimation(this Safe<GameObject> gameObject)
            => gameObject
                .MaybeComponent<SplineAnimate>()
                .Select(
                    animator => {
                        animator.Play();
                        return animator.gameObject;
                    });

        static public float GetSplineAnimatorElapsedTime(this GameObject gameObject)
            => gameObject.Safe().GetSplineAnimatorElapsedTime();

        static public float GetSplineAnimatorElapsedTime(this Safe<GameObject> gameObject)
            => gameObject
                .MaybeInChild<SplineAnimate>(true)
                .SelectOut(animator => animator.ElapsedTime,
                () => 0f.LogError($"No SplineAnimator found on {gameObject.Name()} or children"));

        static public float GetSplineAnimatorNormalizedTime(this GameObject gameObject)
           => gameObject.Safe().GetSplineAnimatorNormalizedTime();

        static public float GetSplineAnimatorNormalizedTime(this Safe<GameObject> gameObject)
            => gameObject
                .MaybeInChild<SplineAnimate>(true)
                .SelectOut(animator => animator.NormalizedTime,
                () => 0f.LogError($"No SplineAnimator found on {gameObject.Name()} or children"));

    }
}
