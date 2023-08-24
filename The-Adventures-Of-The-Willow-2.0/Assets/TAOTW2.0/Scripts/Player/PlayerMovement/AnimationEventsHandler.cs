using UnityEngine;

public class AnimationEventsHandler : MonoBehaviour
{
    public PlayerAnimatorController playerAnimatorController;

    // Evento chamado no final da animação Idle 1
    public void OnIdle1AnimationEnd()
    {
        playerAnimatorController.OnIdle1AnimationEnd();
    }

    // Evento chamado no final da animação Idle 2
    public void OnIdle2AnimationEnd()
    {
        playerAnimatorController.OnIdle2AnimationEnd();
    }

    // Evento chamado no final da animação Idle 3
    public void OnIdle3AnimationEnd()
    {
        playerAnimatorController.OnIdle3AnimationEnd();
    }
}
