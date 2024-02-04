using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using static System.TimeZoneInfo;

public class ScreenAspectRatio : MonoBehaviour
{
    public static ScreenAspectRatio instance;
    public bool m_isStarting;
    public bool isLevelStarting = false;

    public GameObject m_target;
    public Image m_maskTransition;
    public RectTransform m_canvas;

    float m_screen_h = 0;
    float m_screen_w = 0;

    float m_radius = 0;

    float m_counter;

    private Vector3 previousPlayerPosition;

    public Material transitionMaterial;
    private Vector2 startTransition1;
    private Vector2 startTransition2;
    private Vector2 endTransition1;
    private Vector2 endTransition2;
    private float transitionDuration = 1.0f; // Duração da transição em segundos
    private float transitionTime = 0f;
    private bool isTransitioning = false;



    //LevelInfo
    [SerializeField] private Animator LevelInfoAnimator;

    private void Awake()
    {
        if(instance == null)
        { 
            instance = this;
        }
    }
    void Start()
    {
        m_target = GameObject.FindGameObjectWithTag("Player");
        if(m_target == null)
        {
            m_target = new GameObject(); // Crie um objeto vazio (0,0,0) como m_target
        }
        GetStartCharacterPosition();
       
    }

    void Update()
    {
        if (isLevelStarting)
        {
            m_counter += Time.deltaTime;

            if (m_counter > 0.5)
            {
                if (m_isStarting)
                {
                    if (m_radius < 1)
                    {
                        m_radius += Time.deltaTime;
                        m_maskTransition.material.SetFloat("Radius", m_radius);
                    }
                }
                else
                {
                    if (m_radius > 0)
                    {
                        m_radius -= Time.deltaTime;
                        m_maskTransition.material.SetFloat("Radius", m_radius);
                    }
                }
            }
        }
        if (isTransitioning)
        {
            transitionTime += Time.deltaTime;
            float t = Mathf.Clamp01(transitionTime / transitionDuration);

            // Interpola os valores suavemente
            Vector2 currentTransition1 = Vector2.Lerp(startTransition1, endTransition1, t);
            Vector2 currentTransition2 = Vector2.Lerp(startTransition2, endTransition2, t);

            transitionMaterial.SetVector("_Transition1", currentTransition1);
            transitionMaterial.SetVector("_Transition2", currentTransition2);

            if (t >= 1.0f)
            {
                // A transição está completa
                isTransitioning = false;

                // Verifique se as transições terminaram com Vector2 (0, 0)
                if (endTransition1 == new Vector2(0.1f, 0f) && endTransition2 ==  new Vector2(-0.1f, 0.05f))
                {
                    // Modifique o canal _Color do material para alpha 0
                    Color currentColor = transitionMaterial.GetColor("_Color");
                    currentColor.a = 0f;
                    transitionMaterial.SetColor("_Color", currentColor);
                    isLevelStarting = true;
                }
            }
        }
    }
    public void OpenTransition()
    {
        if (!isTransitioning)
        {
            // Defina os valores iniciais e finais
            startTransition1 = new Vector2(-0.5f, 0f);
            startTransition2 = new Vector2(0.5f, 0.05f);
            endTransition1 = new Vector2(0.1f, 0f);
            endTransition2 = new Vector2(-0.1f, 0.05f);

            isTransitioning = true;
            transitionTime = 0f;
        }
    }

    public void CloseTransition()
    {
        if (!isTransitioning)
        {
            // Defina os valores iniciais e finais
            startTransition1 = new Vector2(0f, 0f);
            startTransition2 = new Vector2(0f, 0.05f);
            endTransition1 = new Vector2(-0.5f, 0f);
            endTransition2 = new Vector2(0.5f, 0.05f);

            // Modifique o canal _Color do material para alpha 0
            Color currentColor = transitionMaterial.GetColor("_Color");
            currentColor.a = 1f;
            transitionMaterial.SetColor("_Color", currentColor);

            isTransitioning = true;
            isLevelStarting = false;
            transitionTime = 0f;
        }
    }

    public void StartTransitionNow()
    {
        if (LevelInfoAnimator != null)
        {
            LevelInfoAnimator.SetBool("StartedLevel", true);
        }

    }
    public void OpenRadiusTransition()
    {
        m_isStarting = true; 
        m_target = GameObject.FindGameObjectWithTag("Player");
        if (m_target == null)
        {
            m_target = new GameObject(); // Crie um objeto vazio (0,0,0) como m_target
        }
    }
    public void CloseRadiusTransition()
    {
        m_isStarting = false; 
        m_target = GameObject.FindGameObjectWithTag("Player");
        if (m_target == null)
        {
            m_target = new GameObject(); // Crie um objeto vazio (0,0,0) como m_target
        }
    }
    public void GetStartCharacterPosition()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(m_target.transform.position);

        float characterScreen_w = 0;
        float characterScreen_h = 0;

        if (m_isStarting)
        {
            m_radius = 0;
            m_maskTransition.material.SetFloat("Radius", m_radius);
        }
        else
        {
            m_radius = 1;
            m_maskTransition.material.SetFloat("Radius", m_radius);
        }

        m_screen_h = Screen.height;
        m_screen_w = Screen.width;

        if (m_screen_w < m_screen_h) //Portrait
        {
            m_maskTransition.rectTransform.sizeDelta = new Vector2(m_canvas.rect.height, m_canvas.rect.height);
            float newScreenPos_x = screenPos.x + (m_screen_h - m_screen_w) / 2;

            characterScreen_w = (newScreenPos_x * 100) / m_screen_h;
            characterScreen_w /= 100;

            characterScreen_h = (screenPos.y * 100) / m_screen_h;
            characterScreen_h /= 100;
        }
        else  //Landscape
        {
            m_maskTransition.rectTransform.sizeDelta = new Vector2(m_canvas.rect.width, m_canvas.rect.width);
            float newScreenPos_y = screenPos.y + (m_screen_w - m_screen_h) / 2;

            characterScreen_w = (screenPos.x * 100) / m_screen_w;
            characterScreen_w /= 100;

            characterScreen_h = (newScreenPos_y * 100) / m_screen_w;
            characterScreen_h /= 100;
        }

        m_maskTransition.material.SetFloat("Center_X", characterScreen_w);
        m_maskTransition.material.SetFloat("Center_Y", characterScreen_h);
    }
    public void GetCharacterPosition()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(m_target.transform.position);

        float characterScreen_w = 0;
        float characterScreen_h = 0;

        //if (m_isStarting)
        //{
        //    m_radius = 0;
        //    m_maskTransition.material.SetFloat("Radius", m_radius);
        //}
        //else
        //{
        //    m_radius = 1;
        //    m_maskTransition.material.SetFloat("Radius", m_radius);
        //}

        m_screen_h = Screen.height;
        m_screen_w = Screen.width;

        if (m_screen_w < m_screen_h) //Portrait
        {
            m_maskTransition.rectTransform.sizeDelta = new Vector2(m_canvas.rect.height, m_canvas.rect.height);
            float newScreenPos_x = screenPos.x + (m_screen_h - m_screen_w) / 2;

            characterScreen_w = (newScreenPos_x * 100) / m_screen_h;
            characterScreen_w /= 100;

            characterScreen_h = (screenPos.y * 100) / m_screen_h;
            characterScreen_h /= 100;
        }
        else  //Landscape
        {
            m_maskTransition.rectTransform.sizeDelta = new Vector2(m_canvas.rect.width, m_canvas.rect.width);
            float newScreenPos_y = screenPos.y + (m_screen_w - m_screen_h) / 2;

            characterScreen_w = (screenPos.x * 100) / m_screen_w;
            characterScreen_w /= 100;

            characterScreen_h = (newScreenPos_y * 100) / m_screen_w;
            characterScreen_h /= 100;
        }

        m_maskTransition.material.SetFloat("Center_X", characterScreen_w);
        m_maskTransition.material.SetFloat("Center_Y", characterScreen_h);
    }


}
