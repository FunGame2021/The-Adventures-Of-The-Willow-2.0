using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

public class MoveAndSelectTool : MonoBehaviour
{
    public static MoveAndSelectTool instance;

    [SerializeField] private Color selectedColor;
    [SerializeField] private Color originalColor;

    private SpriteRenderer selectedEnemySpriteRenderer;
    private Color originalEnemyColor;

    public delegate void MouseClick();
    public static event MouseClick OnMouseClick;
    public delegate void MouseClick2();
    public static event MouseClick2 OnMouseClick2;

    public static Vector3 mousePosition;
    public static GameObject selectedDecorObject;
    Vector3 DecorOffset;

    public static GameObject selectedDecor2Object;
    Vector3 Decor2Offset;

    private MoveableObject currentMoveableObject;
    private MoveableObjectDecor2 currentMoveable2Object;

    // Objetos selecionados para gameobjects, enemies e objects
    public Transform selectedGameObjectSprite;
    public Transform GameObjectParent;

    public Transform selectedEnemySprite;
    public Transform selectedObjectSprite;
    public Transform enemyParent;
    public Transform objectParent;

    // Offset usado no drag
    public Vector3 offset;
    private bool isDragging = false;

    [SerializeField] private TMP_Dropdown dropdownSelectType;

    public bool isEnemy;
    public bool isObject;
    public bool isGameObject;
    public bool isDecor;
    public bool isDecor2;

    // UI
    public GameObject PanelToHideValues;

    private int shortLayer = 0;

    public TMP_InputField zPosInput;
    public TMP_InputField shortLayerPosInput;
    public TMP_Dropdown dropdownShortLayerList;
    public TMP_InputField scaleInput;

    public string stringInfo;

    [SerializeField] private GameObject shortLayerPosUI;
    [SerializeField] private GameObject shortLayerPosTextUI;

    private void Start()
    {
        if (instance == null) instance = this;

        dropdownSelectType.onValueChanged.AddListener(OnDropdownValueChanged);
        isEnemy = true;
        OnDropdownValueChanged(0);
    }

    void Update()
    {
        Vector2 inputPosition = Vector2.zero;
        bool inputStarted = false;
        bool inputHeld = false;
        bool inputEnded = false;

        // Detecta input mouse
        if (Mouse.current != null)
        {
            inputPosition = Mouse.current.position.ReadValue();
            inputStarted = Mouse.current.leftButton.wasPressedThisFrame;
            inputHeld = Mouse.current.leftButton.isPressed;
            inputEnded = Mouse.current.leftButton.wasReleasedThisFrame;
        }

        // Detecta input touch, sobrepõe mouse se houver toque
        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
        {
            var touch = Touchscreen.current.touches[0];
            inputPosition = touch.position.ReadValue();

            var phase = touch.phase.ReadValue();

            if (phase == UnityEngine.InputSystem.TouchPhase.Began) inputStarted = true;
            else if (phase == UnityEngine.InputSystem.TouchPhase.Moved || phase == UnityEngine.InputSystem.TouchPhase.Stationary) inputHeld = true;
            else if (phase == UnityEngine.InputSystem.TouchPhase.Ended || phase == UnityEngine.InputSystem.TouchPhase.Canceled) inputEnded = true;
        }

        // Atualiza posição do input no mundo
        Vector3 inputWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(inputPosition.x, inputPosition.y, 0));
        mousePosition = inputWorldPos;

        if (LevelEditorManager.instance.isActiveSelectPoint)
        {
            // Ignora clique se for na UI
            if (EventSystem.current.IsPointerOverGameObject())
            {
                ClearSelections();
                return;
            }

            // -- SELEÇÃO ENEMY --
            if (isEnemy && inputStarted)
            {
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(inputPosition), Vector2.zero);
                if (hit.collider != null && hit.collider.gameObject.CompareTag("Enemy"))
                {
                    SelectEnemy(hit.collider.gameObject.transform, inputPosition);
                }
                else
                {
                    DeselectEnemy();
                }
            }

            // -- SELEÇÃO GAMEOBJECT --
            if (isGameObject && inputStarted)
            {
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(inputPosition), Vector2.zero);
                if (hit.collider != null && (hit.collider.gameObject.CompareTag("GameObject") || hit.collider.gameObject.CompareTag("LevelDot")))
                {
                    SelectGameObject(hit.collider.gameObject.transform, inputPosition);
                }
                else
                {
                    selectedGameObjectSprite = null;
                    isDragging = false;
                }
            }

            // -- SELEÇÃO OBJECT --
            if (isObject && inputStarted)
            {
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(inputPosition), Vector2.zero);
                if (hit.collider != null && hit.collider.gameObject.CompareTag("ObjectObject"))
                {
                    SelectObject(hit.collider.gameObject.transform, inputPosition);
                }
                else
                {
                    selectedObjectSprite = null;
                    isDragging = false;
                }
            }

            // -- SELEÇÃO DECOR --
            if (isDecor)
            {
                if (OnMouseClick != null && inputStarted)
                {
                    OnMouseClick();
                    if (selectedDecorObject != null)
                    {
                        DecorOffset = selectedDecorObject.transform.position - mousePosition;
                        stringInfo = selectedDecorObject.name;
                        UpdateUIWithSelectedObjectData();
                    }
                }
                else if (OnMouseClick != null && inputHeld)
                {
                    if (selectedDecorObject != null)
                    {
                        Vector3 newPos = mousePosition + DecorOffset;
                        newPos.z = selectedDecorObject.transform.position.z;
                        selectedDecorObject.transform.position = newPos;
                        UpdateUIWithSelectedObjectData();
                    }
                }
            }
            else
            {
                selectedDecorObject = null;
                currentMoveableObject = null;
            }

            // -- SELEÇÃO DECOR2 --
            if (isDecor2)
            {
                if (OnMouseClick2 != null && inputStarted)
                {
                    OnMouseClick2();
                    if (selectedDecor2Object != null)
                    {
                        Decor2Offset = selectedDecor2Object.transform.position - mousePosition;
                        stringInfo = selectedDecor2Object.name;
                        UpdateUIWithSelectedObjectData2();
                    }
                }
                else if (OnMouseClick2 != null && inputHeld)
                {
                    if (selectedDecor2Object != null)
                    {
                        Vector3 newPos = mousePosition + Decor2Offset;
                        newPos.z = selectedDecor2Object.transform.position.z;
                        selectedDecor2Object.transform.position = newPos;
                        UpdateUIWithSelectedObjectData2();
                    }
                }

                if (OnMouseClick2 != null && Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
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
            ClearSelections();
        }

        if (inputEnded)
        {
            isDragging = false;
            if (selectedEnemySpriteRenderer != null)
            {
                selectedEnemySpriteRenderer.color = originalColor;
                selectedEnemySpriteRenderer = null;
            }
        }

        // Deletar com Delete
        if (Keyboard.current != null && UserInput.instance.playerMoveAndExtraActions.UI.Delete.WasPerformedThisFrame())
        {
            DeleteSelected();
        }
    }

    private void LateUpdate()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Vector3 inputPos = Vector3.zero;

        if (Mouse.current != null)
            inputPos = Mouse.current.position.ReadValue();
        else if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
            inputPos = Touchscreen.current.touches[0].position.ReadValue();
        else return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(inputPos.x, inputPos.y, 0));

        // Aplica o offset para movimento suave e correto

        if (isEnemy && isDragging && enemyParent != null)
        {
            enemyParent.position = new Vector3(worldPos.x + offset.x, worldPos.y + offset.y, enemyParent.position.z);
        }

        if (isGameObject && isDragging && GameObjectParent != null)
        {
            GameObjectParent.position = new Vector3(worldPos.x + offset.x, worldPos.y + offset.y, GameObjectParent.position.z);
        }

        if (isObject && isDragging && objectParent != null)
        {
            objectParent.position = new Vector3(worldPos.x + offset.x, worldPos.y + offset.y, objectParent.position.z);
        }

        if (isDecor && selectedDecorObject != null && Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            Vector3 newPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) + DecorOffset;
            newPos.z = selectedDecorObject.transform.position.z;
            selectedDecorObject.transform.position = newPos;

            var mo = selectedDecorObject.GetComponent<MoveableObject>();
            if (mo != null)
            {
                mo.ZPos = newPos.z;
                mo.ShortLayer = selectedDecorObject.GetComponent<SpriteRenderer>().sortingOrder;
                mo.floatScale = selectedDecorObject.transform.localScale.x;
            }

            UpdateUIWithSelectedObjectData();
        }

        if (isDecor2 && selectedDecor2Object != null && Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            Vector3 newPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) + Decor2Offset;
            newPos.z = selectedDecor2Object.transform.position.z;
            selectedDecor2Object.transform.position = newPos;

            var mo2 = selectedDecor2Object.GetComponent<MoveableObjectDecor2>();
            if (mo2 != null)
            {
                mo2.ZPos = newPos.z;
                mo2.ShortLayer = mo2.GetComponentInChildren<SpriteRenderer>().sortingOrder;
                mo2.floatScale = mo2.transform.localScale.x;
            }

            UpdateUIWithSelectedObjectData2();
        }
    }

    private void SelectEnemy(Transform enemyTransform, Vector2 inputPosition)
    {
        if (selectedEnemySpriteRenderer != null)
            ChangeParentColors(selectedEnemySpriteRenderer.transform, originalEnemyColor);

        selectedEnemySprite = enemyTransform;
        stringInfo = selectedEnemySprite.name;

        enemyParent = GetEnemyParent(selectedEnemySprite);

        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(new Vector3(inputPosition.x, inputPosition.y, enemyParent.position.z));
        offset = enemyParent.position - worldPoint;

        isDragging = true;

        selectedEnemySpriteRenderer = enemyTransform.GetComponent<SpriteRenderer>();
        if (selectedEnemySpriteRenderer != null)
        {
            originalEnemyColor = GetOriginalColor(selectedEnemySpriteRenderer.transform);
            ChangeParentColors(selectedEnemySpriteRenderer.transform, selectedColor);
        }
    }

    private void DeselectEnemy()
    {
        if (selectedEnemySpriteRenderer != null)
            ChangeParentColors(selectedEnemySpriteRenderer.transform, originalEnemyColor);

        selectedEnemySprite = null;
        isDragging = false;
    }

    private void SelectGameObject(Transform gameObjectTransform, Vector2 inputPosition)
    {
        selectedGameObjectSprite = gameObjectTransform;
        stringInfo = selectedGameObjectSprite.name;

        if (gameObjectTransform.CompareTag("LevelDot"))
            GameObjectParent = gameObjectTransform;
        else
            GameObjectParent = GetGameObjectParent(selectedGameObjectSprite);

        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(new Vector3(inputPosition.x, inputPosition.y, GameObjectParent.position.z));
        offset = GameObjectParent.position - worldPoint;

        isDragging = true;
    }

    private void SelectObject(Transform objectTransform, Vector2 inputPosition)
    {
        selectedObjectSprite = objectTransform;
        stringInfo = selectedObjectSprite.name;

        objectParent = GetObjectParent(selectedObjectSprite);

        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(new Vector3(inputPosition.x, inputPosition.y, objectParent.position.z));
        offset = objectParent.position - worldPoint;

        isDragging = true;
    }

    private void DeleteSelected()
    {
        if (isEnemy && selectedEnemySprite != null)
        {
            Destroy(selectedEnemySprite.gameObject);
            selectedEnemySprite = null;
        }
        else if (isGameObject && selectedGameObjectSprite != null)
        {
            Destroy(selectedGameObjectSprite.gameObject);
            selectedGameObjectSprite = null;
        }
        else if (isObject && selectedObjectSprite != null)
        {
            Destroy(selectedObjectSprite.gameObject);
            selectedObjectSprite = null;
        }
        else if (isDecor && selectedDecorObject != null)
        {
            Destroy(selectedDecorObject.gameObject);
            selectedDecorObject = null;
        }
        else if (isDecor2 && selectedDecor2Object != null)
        {
            Destroy(selectedDecor2Object.gameObject);
            selectedDecor2Object = null;
        }
    }

    private void ClearSelections()
    {
        selectedEnemySprite = null;
        selectedObjectSprite = null;
        selectedGameObjectSprite = null;
        selectedDecorObject = null;
        selectedDecor2Object = null;
        isDragging = false;

        if (selectedEnemySpriteRenderer != null)
        {
            ChangeParentColors(selectedEnemySpriteRenderer.transform, originalEnemyColor);
            selectedEnemySpriteRenderer = null;
        }
    }

    private void ChangeParentColors(Transform objTransform, Color color)
    {
        if (objTransform == null) return;

        SpriteRenderer sr = objTransform.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = color;

        ChangeParentColors(objTransform.parent, color);
    }

    private Color GetOriginalColor(Transform objTransform)
    {
        SpriteRenderer sr = objTransform.GetComponent<SpriteRenderer>();
        return sr != null ? sr.color : Color.white;
    }

    private Transform GetEnemyParent(Transform spriteTransform)
    {
        Transform parent = spriteTransform.parent;
        while (parent != null)
        {
            if (parent.CompareTag("EnemyObject")) return parent;
            parent = parent.parent;
        }
        return null;
    }

    private Transform GetGameObjectParent(Transform spriteTransform)
    {
        Transform parent = spriteTransform;
        while (parent != null)
        {
            if (parent.CompareTag("GameObject")) return parent;
            parent = parent.parent;
        }
        return null;
    }

    private Transform GetObjectParent(Transform spriteTransform)
    {
        Transform parent = spriteTransform;
        while (parent != null)
        {
            if (parent.CompareTag("ObjectObject")) return parent;
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
                scaleInput.text = moveableObject.floatScale.ToString();
            }
        }
        else
        {
            // Objeto desselecionado, redefinir os campos de entrada de texto e o valor do Dropdown
            currentMoveableObject = null;
            dropdownShortLayerList.value = 0;
            zPosInput.text = "";
            shortLayerPosInput.text = "";
            scaleInput.text = "";
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
                scaleInput.text = moveableObject2.floatScale.ToString();

            }
        }
        else
        {
            // Objeto desselecionado, redefinir os campos de entrada de texto e o valor do Dropdown
            currentMoveableObject = null;
            dropdownShortLayerList.value = 0;
            zPosInput.text = "";
            shortLayerPosInput.text = "";
            scaleInput.text = "";
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
            float scale;
            if (!string.IsNullOrEmpty(scaleInput.text) && float.TryParse(scaleInput.text, out scale))
            {
                currentMoveableObject.floatScale = scale;
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
            float scale;
            if (!string.IsNullOrEmpty(scaleInput.text) && float.TryParse(scaleInput.text, out scale))
            {
                currentMoveable2Object.floatScale = scale;
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