using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EditorInfoText : MonoBehaviour
{
    public TMP_Text editorInfoText;


    void Update()
    {
        if (LevelEditorManager.instance.isActiveSelectPoint)
        {
            if (MoveAndSelectTool.instance.isDecor2 || MoveAndSelectTool.instance.isDecor || MoveAndSelectTool.instance.isEnemy || MoveAndSelectTool.instance.isGameObject
                || MoveAndSelectTool.instance.isObject)
            {
                editorInfoText.text = "Selected: " + MoveAndSelectTool.instance.stringInfo;
            }
        }
        else if (!string.IsNullOrEmpty(LevelEditorManager.instance.selectedEnemyName) || !string.IsNullOrEmpty(LevelEditorManager.instance.selectedDecorName)
        || !string.IsNullOrEmpty(LevelEditorManager.instance.selectedDecor2Name) || !string.IsNullOrEmpty(LevelEditorManager.instance.selectedObjectName)
        || !string.IsNullOrEmpty(LevelEditorManager.instance.selectedGameObjectName))
        {
            editorInfoText.text = "Selected To Add: " + LevelEditorManager.instance.selectedStringInfo;
        }
        else
        {
            // Se nenhum objeto estiver selecionado, limpe o texto
            editorInfoText.text = "";
        }
    }

}
