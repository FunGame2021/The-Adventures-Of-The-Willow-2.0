using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelsManager : MonoBehaviour
{
    [SerializeField] private Animator RightAnimator;
    [SerializeField] private Animator LeftAnimator;
    [SerializeField] private Animator BottomAnimator;
    [SerializeField] private Animator InfoAnimator;
    [SerializeField] private Animator TopAnimator;
    private bool isRightHide;
    private bool isLeftHide;
    private bool isBottomHide;
    private bool isInfoHide;
    private bool isTopHide;


    void Start()
    {
        
    }

    void Update()
    {
        if (UserInput.instance.playerMoveAndExtraActions.UI.LeftEditorPanel.WasPressedThisFrame())
        {
            isLeftHide = !isLeftHide;
            LeftAnimator.SetBool("HideLeftPanel", isLeftHide ? true : false);
        }
        if (UserInput.instance.playerMoveAndExtraActions.UI.BottomEditorPanel.WasPressedThisFrame())
        {
            isBottomHide = !isBottomHide;
            BottomAnimator.SetBool("HideBottomPanel", isBottomHide ? true : false);
        }
        if (UserInput.instance.playerMoveAndExtraActions.UI.InfoEditorPanel.WasPressedThisFrame())
        {
            isInfoHide = !isInfoHide;
            InfoAnimator.SetBool("HideInfoPanel", isInfoHide ? true : false);
        }
        if (UserInput.instance.playerMoveAndExtraActions.UI.RightEditorPanel.WasPressedThisFrame())
        {
            isRightHide = !isRightHide;
            RightAnimator.SetBool("HideRightPanel", isRightHide ? true : false);
        }
        if (UserInput.instance.playerMoveAndExtraActions.UI.TopEditorPanel.WasPressedThisFrame())
        {
            isTopHide = !isTopHide;
            TopAnimator.SetBool("HideTopPanel", isTopHide ? true : false);
        }
    }
}
