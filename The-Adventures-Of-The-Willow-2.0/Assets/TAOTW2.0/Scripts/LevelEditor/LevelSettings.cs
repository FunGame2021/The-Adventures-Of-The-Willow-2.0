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
            Image buttonImage = musicButton.GetComponent<Image>();
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
        // Se j� tiver um Bioma selecionado, desmarcar o bot�o
        if (selectedBiomeButton != null)
        {
            // Resetar a cor do bot�o selecionado anteriormente
            var colors = selectedBiomeButton.colors;
            colors.normalColor = Color.white; // Defina a cor normal do bot�o (sem sele��o)
            selectedBiomeButton.colors = colors;
        }

        // Marcar o bot�o do Bioma selecionado
        selectedBiomeButton = biomeButton;
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
        Image buttonImage = button.GetComponent<Image>();
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
}
