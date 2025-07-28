using UnityEngine;

public class AnimationEventsHandler : MonoBehaviour
{
    public void OnIdleAnimationEnd()
    {
        PlayerAnimatorController.instance.OnIdleAnimationEnd();
    }
}