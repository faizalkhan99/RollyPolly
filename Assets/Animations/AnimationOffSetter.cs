using UnityEngine;

public class AnimationOffsetter : MonoBehaviour
{
    // Make sure this matches the name of your animation state in the Animator window.
    // This is the box that your animation clip is inside.
    private const string AnimationStateName = "coinFloat-anim";

    void Start()
    {
        // Get the Animator component attached to this coin
        Animator animator = GetComponent<Animator>();

        if (animator != null)
        {
            // Play the animation state from a random starting point (0.0 to 1.0)
            animator.Play(AnimationStateName, 0, Random.Range(0f, 1f));
        }
    }
}