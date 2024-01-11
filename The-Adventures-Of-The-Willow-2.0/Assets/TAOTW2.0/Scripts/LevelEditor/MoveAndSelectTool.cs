using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class MoveAndSelectTool : MonoBehaviour
{
    public static MoveAndSelectTool instance;

    [SerializeField] private Color selectedColor; // Variável para armazenar a cor escolhida no Inspetor
    [SerializeField] private Color originalColor; // Variável para armazenar a cor escolhida no Inspetor
    private SpriteRenderer selectedEnemySpriteRenderer;
    private Color originalEnemyColor;

    // Move Decor without collider / Duple prefab one with script for in-game editor another for in-game play
    public delegate void MouseClick();
    public static event MouseClick OnMouseClick;
    public delegate void MouseClick2();
    public static event MouseClick2 OnMouseClick2;

    public static Vector3 mousePosition;
    public static GameObject selectedDecorObject;
    Vector3 DecorOffset;

    //Decor 2
    public static GameObject selectedDecor2Object;
    Vector3 Decor2Offset;

    private MoveableObject currentMoveableObject;
    private MoveableObjectDecor2 currentMoveable2Object;

    //Move GameObjects with colliders
    public Transform selectedGameObjectSprite;
    public Transform GameObjectParent;

    //Move objects and enemies with colliders
    public Transform selectedEnemySprite;
    public Transform selectedObjectSprite;
    public Transform enemyParent;
    public Transform objectParent;
    public Vector3 offset;
    private bool isDragging = false;

    [SerializeField] private TMP_Dropdown dropdownSelectType;

    public bool isEnemy;
    public bool isObject;
    public bool isGameObject;
    public bool isDecor;
    public bool isDecor2;


    // Change z-pos and shortLayer
    public GameObject PanelToHideValues;

    private int shortLayer = 0;

    public TMP_InputField zPosInput;
    public TMP_InputField shortLayerPosInput;
    public TMP_Dropdown dropdownShortLayerList;


    //For level editor info
    public string stringInfo;

    //Hide option decor shortlayer pos
    [SerializeField] private GameObject shortLayerPosUI;
    [SerializeField] private GameObject shortLayerPosTextUI;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }

        // Adiciona um listener para o evento onValueChanged
        dropdownSelectType.onValueChanged.AddListener(OnDropdownValueChanged);

    }

    void Update()
    {
        if (Mouse.current != null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            if (mousePos != null)
            {
                mousePosition = LevelEditorManager.instance.mainCamera.ScreenToWorldPoint(mousePos);
            }
        }

        if (LevelEditorManager.instance.isActiveSelectPoint)
        {
            // Verifica se o clique foi realizado em um elemento do UI
            if (EventSystem.current.IsPointerOverGameObject())
            {
                selectedEnemySprite = null;
                selectedObjectSprite = null;
                selectedGameObjectSprite = null;
                selectedDecorObject = null;
                selectedDecor2Object = null;
                return; // Sai da função se o clique foi no UI
            }

            if (isEnemy)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()), Vector2.zero);
                    if (hit.collider != null)
                    {
                        GameObject hitObject = hit.collider.gameObject;

                        // Verifique se o objeto atingido é um sprite
                        if (hitObject.CompareTag("Enemy"))
                        {
                            // Desmarcar o objeto selecionado anterior, se houver
                            if (selectedEnemySpriteRenderer != null)
                            {
                                // Restaurar a cor original de todos os objetos parentes
                                ChangeParentColors(selectedEnemySpriteRenderer.transform, originalEnemyColor);
                            }

                            selectedEnemySprite = hitObject.transform;
                            stringInfo = selectedEnemySprite.name;
                            enemyParent = GetEnemyParent(selectedEnemySprite);
                            offset = enemyParent.position - Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                            isDragging = true;

                            // Salvar o novo objeto selecionado e alterar sua cor
                            selectedEnemySpriteRenderer = hitObject.GetComponent<SpriteRenderer>();
                            if (selectedEnemySpriteRenderer != null)
                            {
                                // Armazenar a cor original antes de alterá-la
                                originalEnemyColor = GetOriginalColor(selectedEnemySpriteRenderer.transform);

                                // Aplicar a cor selecionada em todos os objetos parentes
                                ChangeParentColors(selectedEnemySpriteRenderer.transform, selectedColor);
                            }
                        }
                    }
                    else
                    {
                        // Aqui também, ao invés de setar para null apenas o renderer do objeto, precisamos descolorir todos os objetos pai
                        ChangeParentColors(selectedEnemySpriteRenderer.transform, originalEnemyColor);
                        selectedEnemySprite = null;
                    }
                }
            }


            if (isGameObject)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()), Vector2.zero);
                    if (hit.collider != null)
                    {
                        GameObject hitObject = hit.collider.gameObject;

                        // Verifique se o objeto atingido é um sprite
                        if (hitObject.CompareTag("GameObject") || hitObject.CompareTag("LevelDot"))
                        {
                            selectedGameObjectSprite = hitObject.transform;
                            stringInfo = selectedGameObjectSprite.name;
                            if(hitObject.CompareTag("LevelDot"))
                            {
                                GameObjectParent = hitObject.transform;
                            }
                            else
                            {
                                GameObjectParent = GetGameObjectParent(selectedGameObjectSprite);
                            }
                            offset = GameObjectParent.position - Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

                            isDragging = true;
                        }
                    }
                    else
                    {
                        selectedGameObjectSprite = null;
                    }
                }
            }

            if (isObject)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()), Vector2.zero);
                    if (hit.collider != null)
                    {
                        GameObject hitObject = hit.collider.gameObject;

                        // Verifique se o objeto atingido é um sprite de objeto
                        if (hitObject.CompareTag("ObjectObject"))
                        {
                            selectedObjectSprite = hitObject.transform;
                            stringInfo = selectedObjectSprite.name;
                            objectParent = GetObjectParent(selectedObjectSprite);
                            offset = objectParent.position - Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                            isDragging = true;
                        }
                    }
                    else
                    {
                        selectedObjectSprite = null;
                    }
                }
            }

            if (isDecor)
            {
                if (OnMouseClick != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    OnMouseClick();
                    if (selectedDecorObject)
                    {
                        DecorOffset = selectedDecorObject.transform.position - mousePosition;
                        stringInfo = selectedDecorObject.name;
                        UpdateUIWithSelectedObjectData();
                    }
                }
                else if (OnMouseClick != null && Mouse.current.leftButton.isPressed)
                {
                    if (selectedDecorObject)
                    {
                        Vector3 newPosition = mousePosition + DecorOffset;
                        newPosition.z = selectedDecorObject.transform.position.z; // Mantém o valor do ZPos
                        selectedDecorObject.transform.position = newPosition;
                        UpdateUIWithSelectedObjectData();
                    }
                }
            }
            else
            {
                selectedDecorObject = null;
                currentMoveableObject = null;
            }

            if (isDecor2)
            {
                if (OnMouseClick2 != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    OnMouseClick2();
                    if (selectedDecor2Object)
                    {
                        Decor2Offset = selectedDecor2Object.transform.position - mousePosition;
                        stringInfo = selectedDecor2Object.name;
                        UpdateUIWithSelectedObjectData2();
                    }
                }
                else if (OnMouseClick2 != null && Mouse.current.leftButton.isPressed)
                {
                    if (selectedDecor2Object)
                    {
                        Vector3 newPosition = mousePosition + Decor2Offset;
                        newPosition.z = selectedDecor2Object.transform.position.z; // Mantém o valor do ZPos
                        selectedDecor2Object.transform.position = newPosition;
                        UpdateUIWithSelectedObjectData2();
                    }
                }
                if(OnMouseClick2 != null && Mouse.current.rightButton.wasPressedThisFrame)
                {
                    selectedDecor2Object = null;
                }
            }
            else
            {
                selectedDecor2Object = null;
                currentMoveable2Object = null;
            }
        }
        else
        {
            selectedEnemySprite = null;
            selectedObjectSprite = null;
            selectedGameObjectSprite = null;
            selectedDecorObject = null;
            selectedDecor2Object = null;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
            //selectedDecorObject = null;
            //selectedDecor2Object = null;

            // Desmarcar o objeto selecionado e voltar à cor original
            if (selectedEnemySpriteRenderer != null)
            {
                selectedEnemySpriteRenderer.color = originalColor; // Volta à cor original
                selectedEnemySpriteRenderer = null;
            }
        }


        // Verifica se a tecla "Delete" foi pressionada
        if (Keyboard.current != null && UserInput.instance.playerMoveAndExtraActions.UI.Delete.WasPerformedThisFrame())
        {
            // Verifica se há algum objeto selecionado e o apaga
            if (isEnemy && selectedEnemySprite != null)
            {
                Destroy(selectedEnemySprite.gameObject); // Destrua o objeto selecionado diretamente
                selectedEnemySprite = null;
            }
            else if (isGameObject && selectedGameObjectSprite != null)
            {
                Destroy(selectedGameObjectSprite.gameObject); // Destrua o objeto selecionado diretamente
                selectedGameObjectSprite = null;
            }
            else if (isObject && selectedObjectSprite != null)
            {
                Destroy(selectedObjectSprite.gameObject); // Destrua o objeto selecionado diretamente
                selectedObjectSprite = null;
            }
            else if (isDecor && selectedDecorObject != null)
            {
                Destroy(selectedDecorObject.gameObject); // Destrua o objeto selecionado diretamente
                selectedDecorObject = null;
            }
            else if (isDecor2 && selectedDecor2Object != null)
            {
                Destroy(selectedDecor2Object.gameObject); // Destrua o objeto selecionado diretamente
                selectedDecor2Object = null;
            }
        }

    }



    private void ChangeParentColors(Transform objTransform, Color color)
    {
        if (objTransform == null)
        {
            return;
        }

        SpriteRenderer objRenderer = objTransform.GetComponent<SpriteRenderer>();
        if (objRenderer != null)
        {
            objRenderer.color = color;
        }

        // Recursivamente chama a função para os objetos pai até chegar ao objeto raiz
        ChangeParentColors(objTransform.parent, color);
    }

    private Color GetOriginalColor(Transform objTransform)
    {
        SpriteRenderer objRenderer = objTransform.GetComponent<SpriteRenderer>();
        if (objRenderer != null)
        {
            return objRenderer.color;
        }
        return Color.white; // Caso não encontre o componente SpriteRenderer, retorna a cor branca como padrão.
    }

    private void LateUpdate()
    {
        Vector3 mousePosition = Mouse.current.position.ReadValue();

        // Verifica se o clique foi realizado em um elemento do UI
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return; // Sai da função se o clique foi no UI
        }

        if (isEnemy)
        {
            if (isDragging && enemyParent != null)
            {
                Vector3 newPosition = Camera.main.ScreenToWorldPoint(mousePosition) + offset;
                enemyParent.position = new Vector3(newPosition.x, newPosition.y, enemyParent.position.z);
            }
        }


        if (isGameObject)
        {
            if (isDragging && GameObjectParent != null)
            {
                Vector3 newPosition = Camera.main.ScreenToWorldPoint(mousePosition) + offset;
                GameObjectParent.position = new Vector3(newPosition.x, newPosition.y, GameObjectParent.position.z);
            }
        }

        if (isObject)
        {
            if (isDragging && objectParent != null)
            {
                Vector3 newPosition = Camera.main.ScreenToWorldPoint(mousePosition) + offset;
                objectParent.position = new Vector3(newPosition.x, newPosition.y, objectParent.position.z);
            }
        }

        if(isDecor)
        {
            if (selectedDecorObject != null && Mouse.current.leftButton.isPressed)
            {
                Vector3 newPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) + DecorOffset;
                newPosition.z = selectedDecorObject.transform.position.z; // Mantém o valor do ZPos
                selectedDecorObject.transform.position = newPosition;

                // Obtém o componente MoveableObject do objeto selecionado
                MoveableObject moveableObject = selectedDecorObject.GetComponent<MoveableObject>();

                // Verifica se o componente MoveableObject existe no objeto selecionado
                if (moveableObject != null)
                {
                    // Atualiza as propriedades ZPos e ShortLayer do MoveableObject
                    moveableObject.ZPos = newPosition.z;
                    moveableObject.ShortLayer = selectedDecorObject.GetComponent<SpriteRenderer>().sortingOrder;
                }

                UpdateUIWithSelectedObjectData();
            }
        }

        if (isDecor2)
        {
            if (selectedDecor2Object != null && Mouse.current.leftButton.isPressed)
            {
                Vector3 newPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) + Decor2Offset;
                newPosition.z = selectedDecor2Object.transform.position.z; // Mantém o valor do ZPos
                selectedDecor2Object.transform.position = newPosition;

                // Obtém o componente MoveableObject do objeto selecionado
                MoveableObjectDecor2 moveableObject2 = selectedDecor2Object.GetComponent<MoveableObjectDecor2>();

                UpdateUIWithSelectedObjectData2();
                // Verifica se o componente MoveableObject existe no objeto selecionado
                if (moveableObject2 != null)
                {
                    moveableObject2.ZPos = newPosition.z;
                    moveableObject2.ShortLayer = moveableObject2.GetComponentInChildren<SpriteRenderer>().sortingOrder;
                }
            }
        }
    }

    private Transform GetEnemyParent(Transform spriteTransform)
    {
        Transform parent = spriteTransform.parent;

        while (parent != null)
        {
            if (parent.CompareTag("EnemyObject"))
            {
                return parent;
            }

            parent = parent.parent;
        }

        return null;
    }

    private Transform GetGameObjectParent(Transform GameObjectSpriteTransform)
    {
        Transform parent = GameObjectSpriteTransform;

        while (parent != null)
        {
            if (parent.CompareTag("GameObject"))
            {
                return parent;
            }

            parent = parent.parent;
        }

        return null;
    }

    private Transform GetObjectParent(Transform spriteObjectTransform)
    {
        Transform parent = spriteObjectTransform;

        // Verifica se o objeto atual ou algum de seus pais tem a tag "ObjectObject"
        while (parent != null)
        {
            if (parent.CompareTag("ObjectObject"))
            {
                return parent;
            }

            parent = parent.parent;
        }

        return null;
    }



    private void HideUIInfo()
    {
        PanelToHideValues.SetActive(true);
    }
    private void UpdateUIWithSelectedObjectData()
    {
        if (selectedDecorObject != null)
        {
            shortLayerPosUI.SetActive(true);
            shortLayerPosTextUI.SetActive(true);
            // Desabilitar a interação com os campos de entrada
            zPosInput.interactable = true;
            shortLayerPosInput.interactable = true;

            // Desabilitar a edição do texto
            zPosInput.textComponent.raycastTarget = true;
            shortLayerPosInput.textComponent.raycastTarget = true;

            //sprite Renderer Decor
            string[] ExcludedLayers = { "Trees1", "Trees2", "Trees3", "FTrees1", "FTrees2", "FTrees3", 
                "Default", "Player", "Enemies", "Powerups", "Lasers", "LampUI", "Back" };

            // Obter as opções de short layers excluindo aqueles que estão na lista de exclusão
            List<string> shortLayerOptions = SortingLayer.layers
                .Where(layer => !ExcludedLayers.Contains(layer.name))
                .Select(layer => layer.name)
                .ToList();

            // Limpa as opções existentes no Dropdown (se houver)
            dropdownShortLayerList.ClearOptions();

            // Adiciona as opções ao Dropdown
            dropdownShortLayerList.AddOptions(shortLayerOptions);

            // Atualiza o valor do Dropdown para corresponder ao shortLayer atual
            dropdownShortLayerList.value = shortLayer;


            // Obtém o componente MoveableObject do objeto selecionado
            MoveableObject moveableObject = selectedDecorObject.GetComponent<MoveableObject>();

            currentMoveableObject = moveableObject;

            // Verifica se o componente MoveableObject existe no objeto selecionado
            if (moveableObject != null)
            {
                dropdownShortLayerList.value = GetDropdownIndex(moveableObject.ShortLayerName);

                // Atualiza os campos de entrada de texto e o Dropdown com as informações obtidas
                zPosInput.text = moveableObject.ZPos.ToString("F2");
                shortLayerPosInput.text = moveableObject.ShortLayer.ToString();
            }
        }
        else
        {
            // Objeto desselecionado, redefinir os campos de entrada de texto e o valor do Dropdown
            currentMoveableObject = null;
            dropdownShortLayerList.value = 0;
            zPosInput.text = "";
            shortLayerPosInput.text = "";
        }
    }
    private void UpdateUIWithSelectedObjectData2()
    {
        if (selectedDecor2Object != null)
        {
            shortLayerPosUI.SetActive(false);
            shortLayerPosTextUI.SetActive(false);

            // Desabilitar a interação com os campos de entrada
            zPosInput.interactable = true;
            shortLayerPosInput.interactable = false;

            // Desabilitar a edição do texto
            zPosInput.textComponent.raycastTarget = true;
            shortLayerPosInput.textComponent.raycastTarget = false;

            //sprite Renderer Decor

            // Lista com os sorting layers que serão incluídos
            string[] includedLayers = { "Trees1", "Trees2", "Trees3", "FTrees1", "FTrees2", "FTrees3" };

            // Obter as opções de short layers que estão na lista de inclusão
            List<string> shortLayerOptions = SortingLayer.layers
                .Where(layer => includedLayers.Contains(layer.name))
                .Select(layer => layer.name)
                .ToList();

            // Limpa as opções existentes no Dropdown (se houver)
            dropdownShortLayerList.ClearOptions();

            // Adiciona as opções ao Dropdown
            dropdownShortLayerList.AddOptions(shortLayerOptions);

            // Atualiza o valor do Dropdown para corresponder ao shortLayer atual
            dropdownShortLayerList.value = shortLayer;



            // Obtém o componente MoveableObject do objeto selecionado
            MoveableObjectDecor2 moveableObject2 = selectedDecor2Object.GetComponent<MoveableObjectDecor2>();

            currentMoveable2Object = moveableObject2;

            // Verifica se o componente MoveableObject existe no objeto selecionado
            if (moveableObject2 != null)
            {
                dropdownShortLayerList.value = GetDropdownIndex(moveableObject2.ShortLayerName);

                zPosInput.text = moveableObject2.ZPos.ToString("F2");

            }
        }
        else
        {
            // Objeto desselecionado, redefinir os campos de entrada de texto e o valor do Dropdown
            currentMoveableObject = null;
            dropdownShortLayerList.value = 0;
            zPosInput.text = "";
            shortLayerPosInput.text = "";
        }
    }
    private int GetDropdownIndex(string shortLayerName)
    {
        // Encontre o índice correspondente ao nome do short layer no Dropdown
        for (int i = 0; i < dropdownShortLayerList.options.Count; i++)
        {
            if (dropdownShortLayerList.options[i].text == shortLayerName)
            {
                return i;
            }
        }

        // Retorna 0 como índice padrão se não encontrar correspondência
        return 0;
    }

    public void ApplyChangesToSelectedObject()
    {
        if (currentMoveableObject != null)
        {
            int shortLayer;
            if (!string.IsNullOrEmpty(shortLayerPosInput.text) && int.TryParse(shortLayerPosInput.text, out shortLayer))
            {
                currentMoveableObject.ShortLayer = shortLayer;
            }

            float zPos;
            if (!string.IsNullOrEmpty(zPosInput.text) && float.TryParse(zPosInput.text, out zPos))
            {
                currentMoveableObject.ZPos = zPos;
            }

            currentMoveableObject.ShortLayerName = dropdownShortLayerList.options[dropdownShortLayerList.value].text;

            currentMoveableObject.ApplyChanges();

        }

        if (currentMoveable2Object != null)
        {

            float zPos;
            if (!string.IsNullOrEmpty(zPosInput.text) && float.TryParse(zPosInput.text, out zPos))
            {
                currentMoveable2Object.ZPos = zPos;
            }
            currentMoveable2Object.ShortLayerName = dropdownShortLayerList.options[dropdownShortLayerList.value].text;

            currentMoveable2Object.ApplyChanges();

        }
    }

    public void OnDropdownValueChanged(int value)
    {
        // Limpar todos os objetos selecionados
        selectedEnemySprite = null;
        selectedObjectSprite = null;
        selectedGameObjectSprite = null;
        selectedDecorObject = null;
        selectedDecor2Object = null;
        currentMoveableObject = null;
        currentMoveable2Object = null;

        switch (value)
        {
            case 0: // Valor da Opção 1 selecionada
                isEnemy = true;
                isGameObject = false;
                isObject = false;
                isDecor = false;
                isDecor2 = false;
                break;

            case 1: // Valor da Opção 2 selecionada
                isEnemy = false;
                isGameObject = true;
                isObject = false;
                isDecor = false;
                isDecor2 = false;
                break;

            case 2: // Valor da Opção 3 selecionada
                isEnemy = false;
                isGameObject = false;
                isObject = true;
                isDecor = false;
                isDecor2 = false;
                break;

            case 3: // Valor da Opção 4 selecionada
                isEnemy = false;
                isGameObject = false;
                isObject = false;
                isDecor = true;
                isDecor2 = false;
                break;

            case 4: // Valor da Opção 5 selecionada
                isEnemy = false;
                isGameObject = false;
                isObject = false;
                isDecor = false;
                isDecor2 = true;
                break;

            default: // Valor inválido selecionado
                isEnemy = false;
                isGameObject = false;
                isObject = false;
                isDecor = false;
                isDecor2 = false;
                break;
        }
    }

}