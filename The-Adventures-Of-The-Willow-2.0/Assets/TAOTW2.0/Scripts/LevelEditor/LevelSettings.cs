using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using FMODUnity;


public class LevelSettings : MonoBehaviour
{
    public static LevelSettings instance;

    #region MusicSettings
    public MusicData musicData; // Refer�ncia ao MusicData ScriptableObject
    public Transform biomeButtonContainer; // Container para os bot�es de Biomas
    public Transform musicButtonContainer; // Container para os bot�es de m�sicas

    // Refer�ncia ao bot�o de Bioma selecionado
    private Button selectedBiomeButton;

    // Refer�ncia ao bot�o de m�sica selecionado
    private Button selectedMusicButton;
    private Button lastSelectedBiomeButton;
    [SerializeField] private Button buttonPrefab;

    private int selectedMusicID;
    public int MusicIDToSave;

    // Vari�vel para armazenar o sprite do bioma selecionado
    private Sprite selectedBiomeSprite;

    // Vari�vel para armazenar o ID da m�sica atualmente em reprodu��o
    private int currentPlayingMusicID = -1;

    // Vari�veis para armazenar os eventos FMOD
    private FMOD.Studio.EventInstance musicEventInstance; // Evento da m�sica selecionada

    #endregion
    #region TimeManager
    public int levelTime;
    public int newLevelTime;
    [SerializeField] private TMP_InputField TimeInput;
    [SerializeField] private TextMeshProUGUI currentSavedTime;
    #endregion

    #region Background
    public BackgroundData backgroundData;
    [SerializeField] private Transform biomeBackgroundButtonContainer; // Container para os bot�es de Biomas
    [SerializeField] private Transform backgroundButtonContainer; // Container para os bot�es de m�sicas
    private Button selectedBackgroundBiomeButton;
    private Button selectedBackgroundButton;
    [SerializeField] private Button buttonBackgroundPrefab;
    private string selectedBackgroundName;
    private Sprite selectedBackgroundBiomeSprite;
    private Button lastSelectedBackgroundBiomeButton;
    [SerializeField] private TMP_InputField offsetInput;

    public string BackgroundToSave;
    public float BackgroundOffsetToSave = 1.07f;

    public Transform backgroundLocal;
    private GameObject currentBackgroundInstance;

    #endregion

    #region TimeWeather
    public string volumeNameTimeWeather;
    public TimeWeatherData ScriptableTimeWeatherData;
    [SerializeField] private Transform TimeWeatherLocal;
    private GameObject currentTimeWeatherInstance;
    #endregion

    #region Weather
    public string particleNameWeather;
    public WeatherData ScriptableWeatherData;
    [SerializeField] private Transform weatherLocal;
    private GameObject currentWeatherInstance;
    #endregion

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    private void Update()
    {
        currentSavedTime.text = levelTime.ToString();
    }

    #region MusicSettings
    // Fun��o para instanciar os bot�es de Biomas
    public void CreateBiomeButtons()
    {
        // Limpar os bot�es de Biomas existentes
        foreach (Transform child in biomeButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // Instanciar os bot�es para cada Bioma
        foreach (MusicData.BiomeCategory biome in musicData.biomeCategories)
        {
            Button biomeButton = InstantiateButton(biome.biomeSprite, biomeButtonContainer);
            biomeButton.onClick.AddListener(() => OnBiomeButtonClicked(biomeButton, biome));
        }

    }

    // Fun��o para instanciar os bot�es de M�sicas para um determinado Bioma
    public void CreateMusicButtons(MusicData.BiomeCategory biome)
    {
        // Limpar os bot�es de M�sicas existentes
        foreach (Transform child in musicButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // Instanciar os bot�es para cada M�sica do Bioma
        foreach (MusicData.Music music in biome.musicList)
        {
            Button musicButton = InstantiateButton(music.musicSprite, musicButtonContainer);
            musicButton.onClick.AddListener(() => OnMusicButtonClicked(musicButton, music));

            // Verificar se o bot�o atual corresponde ao bot�o da m�sica selecionada
            if (music.musicID == selectedMusicID)
            {
                // Marcar o bot�o como selecionado
                selectedMusicButton = musicButton;
                // (se necess�rio, adicione aqui a l�gica para marcar o bot�o, por exemplo, mudando sua cor ou adicionando um indicador visual)
            }

            // Configurar a sprite da m�sica no bot�o (se voc� deseja adicionar sprites ou �cones espec�ficos �s m�sicas)
            Image buttonImage = musicButton.GetComponentInChildren<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = music.musicSprite;
            }
        }

        // Selecionar automaticamente o bot�o do Bioma da m�sica atual
        if (selectedMusicButton != null)
        {
            // Encontrar o Bioma da m�sica atual
            MusicData.BiomeCategory biomeOfSelectedMusic = GetBiomeOfSelectedMusic();

            // Se encontrarmos o Bioma, selecionar o bot�o correspondente
            if (biomeOfSelectedMusic != null)
            {
                selectedBiomeButton = biomeButtonContainer.Find(biomeOfSelectedMusic.biomeName).GetComponent<Button>();
                selectedBiomeButton.interactable = false;
            }
        }
    }

    // Fun��o chamada quando um bot�o de Bioma � clicado
    public void OnBiomeButtonClicked(Button biomeButton, MusicData.BiomeCategory biome)
    {
        // Desmarcar o �ltimo bioma selecionado, se houver um
        if (lastSelectedBiomeButton != null)
        {
            // Resetar a cor do bot�o anterior
            var colors = lastSelectedBiomeButton.colors;
            colors.normalColor = Color.white;
            lastSelectedBiomeButton.colors = colors;
            // Tornar o �ltimo bot�o de bioma selecionado iterativo novamente
            lastSelectedBiomeButton.interactable = true;
        }

        // Marcar o bot�o do bioma selecionado
        selectedBiomeButton = biomeButton;
        lastSelectedBiomeButton = selectedBiomeButton; // Atualizar a refer�ncia para o �ltimo bioma selecionado

        selectedBiomeButton.interactable = false;

        // Armazenar o sprite do bioma selecionado
        selectedBiomeSprite = biome.biomeSprite;

        // Alterar a cor do bot�o selecionado
        var selectedColors = selectedBiomeButton.colors;
        selectedColors.normalColor = Color.green; // Defina a cor do bot�o selecionado
        selectedBiomeButton.colors = selectedColors;

        // Criar e exibir o subpainel de m�sicas para o Bioma selecionado
        CreateMusicButtons(biome);
        // (se necess�rio, adicione aqui a l�gica para exibir o subpainel)
    }

    // Fun��o chamada quando um bot�o de M�sica � clicado
    public void OnMusicButtonClicked(Button musicButton, MusicData.Music music)
    {
        // Se a m�sica atual for a mesma que a m�sica selecionada, n�o � necess�rio fazer nada
        if (music.musicID == currentPlayingMusicID)
        {
            return;
        }

        // Parar a m�sica atualmente em reprodu��o, se houver
        StopMusic();

        // Marcar o bot�o da m�sica selecionada
        selectedMusicButton = musicButton;

        // Armazenar o ID da m�sica selecionada
        selectedMusicID = music.musicID;

        MusicIDToSave = selectedMusicID;
        // Reproduzir a m�sica atual
        PlayMusic();
    }

    // Fun��o para instanciar um bot�o com um sprite espec�fico e adicion�-lo a um container
    private Button InstantiateButton(Sprite buttonSprite, Transform container)
    {
        Button button = Instantiate<Button>(buttonPrefab, container);

        // Configurar a imagem do bot�o com o sprite fornecido
        Image buttonImage = button.GetComponentInChildren<Image>();
        if (buttonImage != null)
        {
            buttonImage.sprite = buttonSprite;
        }

        return button;
    }

    // Fun��o para encontrar o Bioma da m�sica selecionada
    private MusicData.BiomeCategory GetBiomeOfSelectedMusic()
    {
        // Verificar se h� um bot�o de m�sica selecionado
        if (selectedMusicButton != null)
        {
            // Obter a m�sica selecionada usando o ID
            MusicData.Music selectedMusic = musicData.biomeCategories
                .SelectMany(biome => biome.musicList)
                .FirstOrDefault(music => music.musicID == selectedMusicID);

            if (selectedMusic != null)
            {
                // Obter o nome da m�sica
                string selectedMusicName = selectedMusic.musicName;

                // Procurar o bioma que cont�m a m�sica com o nome selecionado
                foreach (MusicData.BiomeCategory biome in musicData.biomeCategories)
                {
                    foreach (MusicData.Music music in biome.musicList)
                    {
                        if (music.musicName == selectedMusicName)
                        {
                            return biome;
                        }
                    }
                }
            }
        }

        return null;
    }


    public void SetMusicID(int musicID)
    {
        selectedMusicID = musicID;
        MusicIDToSave = selectedMusicID;
        // Aqui voc� pode implementar qualquer l�gica adicional para lidar com a mudan�a de MusicID, se necess�rio.
    }

    // Fun��o para reproduzir a m�sica selecionada
    private void PlayMusic()
    {
        // Carregar o evento FMOD associado � m�sica selecionada
        if (FMODEvents.instance != null && FMODEvents.instance.musicList.ContainsKey((FMODEvents.MusicID)selectedMusicID))
        {
            EventReference musicEvent = FMODEvents.instance.musicList[(FMODEvents.MusicID)selectedMusicID];

            musicEventInstance = RuntimeManager.CreateInstance(musicEvent);

            // Tocar a m�sica a partir do in�cio
            if (musicEventInstance.isValid())
            {
                musicEventInstance.start();
                // Armazenar o ID da m�sica atualmente em reprodu��o
                currentPlayingMusicID = selectedMusicID;
            }
        }
    }

    // Fun��o para parar a m�sica em reprodu��o
    public void StopMusic()
    {
        // Verificar se h� uma m�sica em reprodu��o e se � v�lida antes de parar
        if (musicEventInstance.isValid() && currentPlayingMusicID != -1)
        {
            musicEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicEventInstance.release();
        }
    }
    #endregion

    #region LevelTimeManager
    public void UpdateValues()
    {
        levelTime = newLevelTime;
        TimeInput.text = newLevelTime.ToString();
    }

    public void UpdateLevelTime()
    {
        if (int.TryParse(TimeInput.text, out int timeValue))
        {
            // O valor � um n�mero inteiro v�lido
            levelTime = timeValue;
        }
        else
        {
            // O valor n�o � um n�mero inteiro v�lido, fa�a algo para lidar com isso
            Debug.LogWarning("Invalid integer input!");
        }
    }
    #endregion

    #region Background

    public void CreateBackgroundBiomeButtons()
    {
        // Limpar os bot�es de Biomas de fundo existentes
        foreach (Transform child in biomeBackgroundButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // Instanciar os bot�es para cada Bioma de fundo
        foreach (BackgroundData.BiomeCategory biome in backgroundData.biomeCategories)
        {
            Button biomeButton = InstantiateButton(biome.biomeSprite, biomeBackgroundButtonContainer);
            biomeButton.onClick.AddListener(() => OnBiomeBackgroundButtonClicked(biomeButton, biome));

            Image buttonImage = biomeButton.GetComponentInChildren<Image>();

            // Verificar se o nome do bioma atual corresponde ao selectedBackgroundName
            if (biome.biomeName == selectedBackgroundName)
            {
                // Marcar o bot�o como selecionado
                selectedBackgroundBiomeButton = biomeButton;
                if (buttonImage != null)
                {
                    buttonImage.sprite = biome.biomeSprite;
                    buttonImage.color = Color.red;
                }
            }
            else
            {
                if (buttonImage != null)
                {
                    buttonImage.sprite = biome.biomeSprite;
                    buttonImage.color = Color.white;
                }
            }
        }
    }

    public void CreateBackgroundButtons(BackgroundData.BiomeCategory biome)
    {
        // Limpar os bot�es de fundo existentes
        foreach (Transform child in backgroundButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // Instanciar os bot�es para cada fundo do Bioma de fundo
        foreach (BackgroundData.Background background in biome.BackgroundList)
        {
            Button backgroundButton = InstantiateButton(background.backgroundSprite, backgroundButtonContainer);
            backgroundButton.onClick.AddListener(() => OnBackgroundButtonClicked(backgroundButton, background));

            Image buttonImage = backgroundButton.GetComponentInChildren<Image>();
            // Verificar se o bot�o atual corresponde ao bot�o de fundo selecionado
            if (background.backgroundName == selectedBackgroundName)
            {
                // Marcar o bot�o como selecionado
                selectedBackgroundButton = backgroundButton;
                if (buttonImage != null)
                {
                    buttonImage.sprite = background.backgroundSprite;
                    buttonImage.color = Color.red;
                }
            }
            else
            {
                if (buttonImage != null)
                {
                    buttonImage.sprite = background.backgroundSprite;
                    buttonImage.color = Color.white;
                }
            }
        }
    }
    public void OnBiomeBackgroundButtonClicked(Button biomeButton, BackgroundData.BiomeCategory biome)
    {
        // Desmarcar o �ltimo bioma de background selecionado, se houver um
        if (lastSelectedBackgroundBiomeButton != null)
        {
            // Resetar a cor do bot�o anterior
            var colors = lastSelectedBackgroundBiomeButton.colors;
            colors.normalColor = Color.white;
            lastSelectedBackgroundBiomeButton.colors = colors;
            lastSelectedBackgroundBiomeButton.interactable = true;
        }

        // Marcar o bot�o do bioma de background selecionado
        selectedBackgroundBiomeButton = biomeButton;
        lastSelectedBackgroundBiomeButton = selectedBackgroundBiomeButton; // Atualizar a refer�ncia para o �ltimo bioma de background selecionado

        selectedBackgroundBiomeButton.interactable = false;

        // Armazenar o sprite do Bioma de fundo selecionado
        selectedBackgroundBiomeSprite = biome.biomeSprite;

        // Criar e exibir o subpainel de fundos para o Bioma de fundo selecionado
        CreateBackgroundButtons(biome);
    }
    public void OnBackgroundButtonClicked(Button backgroundButton, BackgroundData.Background background)
    {
        UpdateOffset();
        // Atualize o nome do fundo selecionado
        selectedBackgroundName = background.backgroundName;

        // Salve os detalhes do fundo, se necess�rio
        BackgroundToSave = selectedBackgroundName;

        // Verifique se h� um prefab de fundo associado ao BackgroundData
        if (background.backgroundPrefab != null)
        {
            // Apague todos os filhos de backgroundLocal, se houver algum
            foreach (Transform child in backgroundLocal)
            {
                Destroy(child.gameObject);
            }

            // Encontre o prefab do fundo correspondente com base no nome
            GameObject backgroundPrefab = null;
            foreach (BackgroundData.BiomeCategory biomeCategory in backgroundData.biomeCategories)
            {
                foreach (BackgroundData.Background backgroundInfo in biomeCategory.BackgroundList)
                {
                    if (backgroundInfo.backgroundName == selectedBackgroundName)
                    {
                        backgroundPrefab = backgroundInfo.backgroundPrefab;
                        break;
                    }
                }
                if (backgroundPrefab != null)
                    break;
            }

            if (backgroundPrefab != null)
            {
                // Instancie o novo prefab de fundo no local com base no offset
                float offset = BackgroundOffsetToSave; // Supondo que BackgroundOffsetToSave foi definido previamente
                Vector3 spawnPosition = new Vector3(backgroundLocal.position.x, offset, backgroundLocal.position.z);
                currentBackgroundInstance = Instantiate(backgroundPrefab, spawnPosition, Quaternion.identity, backgroundLocal);
            }
            else
            {
                Debug.LogWarning("Prefab not found for background: " + selectedBackgroundName);
            }
        }
    }
    public void UpdateOffset()
    {
        // Verifique se o campo de entrada de texto n�o est� vazio
        if (!string.IsNullOrEmpty(offsetInput.text))
        {
            // Tente converter o texto para um valor float
            if (float.TryParse(offsetInput.text, out float offset))
            {
                // A convers�o foi bem-sucedida, atualize BackgroundOffsetToSave
                BackgroundOffsetToSave = offset;

                // Verifique se o backgroundLocal tem pelo menos um filho
                if (backgroundLocal.childCount > 0)
                {
                    // Obtenha o primeiro filho
                    Transform firstChild = backgroundLocal.GetChild(0);

                    // Atualize a posi��o Y do primeiro filho com o novo offset
                    Vector3 newPosition = firstChild.transform.position;
                    newPosition.y = offset;
                    firstChild.transform.position = newPosition;

                    // Encontre o script dentro do prefab
                    ParallaxBackground[] parallaxBackgroundScript = firstChild.GetComponentsInChildren<ParallaxBackground>();

                    // Verifique se pelo menos um script foi encontrado
                    if (parallaxBackgroundScript != null && parallaxBackgroundScript.Length > 0)
                    {
                        // Chame o m�todo EditorUpdatePos() em cada script ParallaxBackground
                        foreach (ParallaxBackground script in parallaxBackgroundScript)
                        {
                            script.EditorUpdatePos();
                        }
                    }
                    else
                    {
                        Debug.LogError("ParallaxBackground script not found on the prefab.");
                    }
                }
            }
            else
            {
                // A convers�o falhou, voc� pode lidar com isso de acordo com suas necessidades (exemplo: exibir uma mensagem de erro).
                Debug.LogError("Erro na convers�o do valor de offsetInput para float.");
            }
        }
    }

    public void UpdateBackground(string backgroundName, float offset)
    {
        selectedBackgroundName = backgroundName;
        BackgroundToSave = selectedBackgroundName;
        BackgroundOffsetToSave = offset;
        foreach (Transform child in backgroundLocal)
        {
            Destroy(child.gameObject);
        }

        // Encontre o prefab do fundo correspondente com base no nome
        GameObject backgroundPrefab = null;
        foreach (BackgroundData.BiomeCategory biomeCategory in backgroundData.biomeCategories)
        {
            foreach (BackgroundData.Background backgroundInfo in biomeCategory.BackgroundList)
            {
                if (backgroundInfo.backgroundName == selectedBackgroundName)
                {
                    backgroundPrefab = backgroundInfo.backgroundPrefab;
                    break;
                }
            }
            if (backgroundPrefab != null)
                break;
        }

        if (backgroundPrefab != null)
        {
            // Instancie o novo prefab de fundo no local com base no offset
            Vector3 spawnPosition = new Vector3(backgroundLocal.position.x, offset, backgroundLocal.position.z);
            currentBackgroundInstance = Instantiate(backgroundPrefab, spawnPosition, Quaternion.identity, backgroundLocal);

            // Encontre o script ParallaxBackground no prefab
            ParallaxBackground[] backgroundScript = backgroundPrefab.GetComponentsInChildren<ParallaxBackground>();

            // Verifique se pelo menos um script foi encontrado
            if (backgroundScript != null && backgroundScript.Length > 0)
            {
                // Chame o m�todo EditorUpdatePos() em cada script ParallaxBackground
                foreach (ParallaxBackground script in backgroundScript)
                {
                    script.EditorUpdatePos();
                }
            }
            else
            {
                Debug.LogError("ParallaxBackground script not found on the prefab.");
            }
        }
        else
        {
            Debug.LogWarning("Prefab not found for background: " + selectedBackgroundName);
        }

    }

    #endregion

    #region TimeWeather

    public void UpdateTimeWeather(string timeWeather)
    {
        volumeNameTimeWeather = timeWeather;
        foreach (Transform child in TimeWeatherLocal)
        {
            Destroy(child.gameObject);
        }
        GameObject TimeWeatherPrefab = null;
        foreach (TimeWeatherData.TimeWeatherCategory timeWeatherCategory in ScriptableTimeWeatherData.timeWeatherCategories)
        {
            foreach (TimeWeatherData.TimeWeather timeWeatherInfo in timeWeatherCategory.TimeWeatherList)
            {
                if (timeWeatherInfo.TimeWeatherName == volumeNameTimeWeather)
                {
                    TimeWeatherPrefab = timeWeatherInfo.TimeWeatherPrefab;
                    break;
                }
            }
            if (TimeWeatherPrefab != null)
                break;
        }
        if (TimeWeatherPrefab != null)
        {
            // Instancie o novo prefab de fundo no local com base no offset
            Vector3 spawnPosition = new Vector3(TimeWeatherLocal.position.x, TimeWeatherLocal.position.z);
            currentTimeWeatherInstance = Instantiate(TimeWeatherPrefab, spawnPosition, Quaternion.identity, TimeWeatherLocal);
        }
        else
        {
            Debug.LogWarning("Prefab not found for TimeWeather: " + volumeNameTimeWeather);
        }
    }

    public void MorningSelected()
    {
        volumeNameTimeWeather = "MorningVolume";
        UpdateTimeWeather("MorningVolume");
    }
    public void MiddaySelected()
    {
        volumeNameTimeWeather = "MiddayVolume";
        UpdateTimeWeather("MiddayVolume");
    }
    public void AfternoonSelected()
    {

        volumeNameTimeWeather = "AfternoonVolume";
        UpdateTimeWeather("AfternoonVolume");
    }
    public void NightSelected()
    {
        volumeNameTimeWeather = "NightVolume";
        UpdateTimeWeather("NightVolume");
    }
    public void CaveSelected()
    {
        volumeNameTimeWeather = "CaveVolume";
        UpdateTimeWeather("CaveVolume");
    }
    public void DarkSelected()
    {
        volumeNameTimeWeather = "DarkVolume";
        UpdateTimeWeather("DarkVolume");
    }
    #endregion

    #region Weather
    public void UpdateWeather(string weather)
    {
        particleNameWeather = weather;

        foreach (Transform child in weatherLocal)
        {
            Destroy(child.gameObject);
        }
        GameObject WeatherPrefab = null;
        foreach (WeatherData.WeatherCategory WeatherCategory in ScriptableWeatherData.WeatherCategories)
        {
            foreach (WeatherData.Weather WeatherInfo in WeatherCategory.WeatherList)
            {
                if (WeatherInfo.WeatherName == particleNameWeather)
                {
                    WeatherPrefab = WeatherInfo.WeatherPrefab;
                    break;
                }
            }
            if (WeatherPrefab != null)
                break;
        }
        if (WeatherPrefab != null)
        {
            // Instancie o novo prefab de fundo no local com base no offset
            Vector3 spawnPosition = new Vector3(7, 10, 10);
            currentWeatherInstance = Instantiate(WeatherPrefab, spawnPosition, Quaternion.identity, weatherLocal);
        }
        else
        {
            Debug.LogWarning("Prefab not found for Weather: " + particleNameWeather);
        }
    }

    public void RainSelected()
    {
        particleNameWeather = "Rain";
        UpdateWeather("Rain");
    }
    public void SnowSelected()
    {
        particleNameWeather = "Snow";
        UpdateWeather("Snow");
    }
    public void StormRain()
    {
        particleNameWeather = "Storm_Rain";
        UpdateWeather("Storm_Rain");
    }
    public void Storm()
    {
        particleNameWeather = "Storm";
        UpdateWeather("Storm");
    }
    public void Sun()
    {
        particleNameWeather = "Sun";
        UpdateWeather("Sun");
    }
    public void FallLeaves()
    {
        particleNameWeather = "FallLeaves";
        UpdateWeather("FallLeaves");
    }
    #endregion
}
