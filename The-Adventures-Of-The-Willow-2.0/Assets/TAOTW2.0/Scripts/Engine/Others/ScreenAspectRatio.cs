using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class ScreenAspectRatio : MonoBehaviour
{
    public static ScreenAspectRatio instance;
    public bool m_isStarting;
    public bool isLevelStarting;

    public GameObject m_target;
    public Image m_maskTransition;
    public RectTransform m_canvas;

    float m_screen_h = 0;
    float m_screen_w = 0;

    float m_radius = 0;

    float m_counter;

    private Vector3 previousPlayerPosition;

    //LevelInfo
    [SerializeField] private Animator LevelInfoAnimator;
    [SerializeField] private GameObject TransitionMaterial1;
    [SerializeField] private GameObject TransitionLevelMaterial1;

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
        if (m_target != null)
        {
            GetCharacterPosition();
        }
    }

    void Update()
    {
        m_target = GameObject.FindGameObjectWithTag("Player");
        if (!isLevelStarting)
        {
            m_counter += Time.deltaTime;

            if (previousPlayerPosition != m_target.transform.position)
            {
                GetCharacterPosition();
            }
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
    }
    public void OpenTransition()
    {
        Debug.Log("Open");

        // World
        Material worldMaterial = TransitionMaterial1.GetComponent<Image>().material;
        Vector2 worldTransition1Value = new Vector2(-0.5f, 0f);
        Vector2 worldTransition2Value = new Vector2(0.5f, 0.05f);
        worldMaterial.SetVector("Transition1", worldTransition1Value);
        worldMaterial.SetVector("Transition2", worldTransition2Value);

        // Level
        Material levelMaterial = TransitionLevelMaterial1.GetComponent<Image>().material;
        Vector2 levelTransition1Value = new Vector2(-0.5f, 0f);
        Vector2 levelTransition2Value = new Vector2(0.5f, 0.05f);
        levelMaterial.SetVector("Transition1", levelTransition1Value);
        levelMaterial.SetVector("Transition2", levelTransition2Value);
    }

    public void CloseTransition()
    {
        Debug.Log("Close");

        // World
        Material worldMaterial = TransitionMaterial1.GetComponent<Image>().material;
        Vector2 worldTransition1Value = new Vector2(0.07f, 0f);
        Vector2 worldTransition2Value = new Vector2(-0.07f, 0.05f);
        worldMaterial.SetVector("Transition1", worldTransition1Value);
        worldMaterial.SetVector("Transition2", worldTransition2Value);

        // Level
        Material levelMaterial = TransitionLevelMaterial1.GetComponent<Image>().material;
        Vector2 levelTransition1Value = new Vector2(0.07f, 0f);
        Vector2 levelTransition2Value = new Vector2(-0.07f, 0.05f);
        levelMaterial.SetVector("Transition1", levelTransition1Value);
        levelMaterial.SetVector("Transition2", levelTransition2Value);
    }

    public void StartTransitionNow()
    {
        if(isLevelStarting)
        {
            if (LevelInfoAnimator != null)
            {
                LevelInfoAnimator.SetBool("StartedLevel", true);
            }
            StartCoroutine(ToAnimation());
        }
    }
    IEnumerator ToAnimation()
    {
        if(previousPlayerPosition != m_target.transform.position)
        {
            GetCharacterPosition();
        }
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
        yield return new WaitForSeconds(3f);
    }
    public void GetCharacterPosition()
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
}
