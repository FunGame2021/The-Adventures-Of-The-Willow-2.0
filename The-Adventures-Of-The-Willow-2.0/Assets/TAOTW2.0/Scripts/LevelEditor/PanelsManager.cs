using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    #region UI
    [Header("UI")]
    [SerializeField] private Button RightBTN;
    [SerializeField] private Button LeftBTN;
    [SerializeField] private Button BottomBTN;
    [SerializeField] private Button InfoBTN;
    [SerializeField] private Button TopBTN;
    #endregion


    void Start()
    {
        RightBTN.onClick.AddListener(OnRightButtonClick);
        LeftBTN.onClick.AddListener(OnLeftButtonClick);
        BottomBTN.onClick.AddListener(OnBottomButtonClick);
        InfoBTN.onClick.AddListener(OnInfoButtonClick);
        TopBTN.onClick.AddListener(OnTopButtonClick);
    }
    private void OnRightButtonClick()
    {
        if (!isRightHide)
        {
            // Oculta o painel e gira para 90°
            RightAnimator.SetBool("HideRightPanel", true);
            RightBTN.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        }
        else
        {
            // Mostra o painel e gira para -90°
            RightAnimator.SetBool("HideRightPanel", false);
            RightBTN.transform.rotation = Quaternion.Euler(0f, 0f, -90f);
        }

        isRightHide = !isRightHide;
    }
    private void OnBottomButtonClick()
    {
        if (!isBottomHide)
        {
            // Oculta o painel e gira para 90°
            BottomAnimator.SetBool("HideBottomPanel", true);
            BottomBTN.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else
        {
            // Mostra o painel e gira para -90°
            BottomAnimator.SetBool("HideBottomPanel", false);
            BottomBTN.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
        }

        isBottomHide = !isBottomHide;
    }
    private void OnInfoButtonClick()
    {
        if (!isInfoHide)
        {
            InfoAnimator.SetBool("HideInfoPanel", true);
            InfoBTN.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else
        {
            InfoAnimator.SetBool("HideInfoPanel", false);
            InfoBTN.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
        }

        isInfoHide = !isInfoHide;
    }
    private void OnLeftButtonClick()
    {
        if (!isLeftHide)
        {
            // Oculta o painel e gira para 90°
            LeftAnimator.SetBool("HideLeftPanel", true);
            LeftBTN.transform.rotation = Quaternion.Euler(0f, 0f, -90f);
        }
        else
        {
            // Mostra o painel e gira para -90°
            LeftAnimator.SetBool("HideLeftPanel", false);
            LeftBTN.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        }

        isLeftHide = !isLeftHide;
    }
    private void OnTopButtonClick()
    {
        if (!isTopHide)
        {
            TopAnimator.SetBool("HideTopPanel", true);
            TopBTN.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
        }
        else
        {
            TopAnimator.SetBool("HideTopPanel", false);
            TopBTN.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }

        isTopHide = !isTopHide;
    }
    void Update()
    {
        if (UserInput.instance.playerMoveAndExtraActions.UI.LeftCTRL.IsPressed() && UserInput.instance.playerMoveAndExtraActions.UI.LeftEditorPanel.WasPressedThisFrame())
        {
            OnLeftButtonClick();
        }
        if (UserInput.instance.playerMoveAndExtraActions.UI.LeftCTRL.IsPressed() && UserInput.instance.playerMoveAndExtraActions.UI.BottomEditorPanel.WasPressedThisFrame())
        {
            OnBottomButtonClick();
        }
        if (UserInput.instance.playerMoveAndExtraActions.UI.LeftCTRL.IsPressed() && UserInput.instance.playerMoveAndExtraActions.UI.InfoEditorPanel.WasPressedThisFrame())
        {
            OnInfoButtonClick();
        }
        if (UserInput.instance.playerMoveAndExtraActions.UI.LeftCTRL.IsPressed() && UserInput.instance.playerMoveAndExtraActions.UI.RightEditorPanel.WasPressedThisFrame())
        {
            OnRightButtonClick();
        }
        if (UserInput.instance.playerMoveAndExtraActions.UI.LeftCTRL.IsPressed() && UserInput.instance.playerMoveAndExtraActions.UI.TopEditorPanel.WasPressedThisFrame())
        {
            OnTopButtonClick();
        }
    }
}
