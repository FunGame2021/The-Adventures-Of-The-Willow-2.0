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
    public MusicData musicData; // Referência ao MusicData ScriptableObject
    public Transform biomeButtonContainer; // Container para os botões de Biomas
    public Transform musicButtonContainer; // Container para os botões de músicas

    // Referência ao botão de Bioma selecionado
    private Button selectedBiomeButton;

    // Referência ao botão de música selecionado
    private Button selectedMusicButton;

    [SerializeField] private Button buttonPrefab;

    private int selectedMusicID;
    public int MusicIDToSave;

    // Variável para armazenar o sprite do bioma selecionado
    private Sprite selectedBiomeSprite;

    // Variável para armazenar o ID da música atualmente em reprodução
    private int currentPlayingMusicID = -1;

    // Variáveis para armazenar os eventos FMOD
    private FMOD.Studio.EventInstance musicEventInstance; // Evento da música selecionada

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
    // Função para instanciar os botões de Biomas
    public void CreateBiomeButtons()
    {
        // Limpar os botões de Biomas existentes
        foreach (Transform child in biomeButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // Instanciar os botões para cada Bioma
        foreach (MusicData.BiomeCategory biome in musicData.biomeCategories)
        {
            Button biomeButton = InstantiateButton(biome.biomeSprite, biomeButtonContainer);
            biomeButton.onClick.AddListener(() => OnBiomeButtonClicked(biomeButton, biome));
        }

    }

    // Função para instanciar os botões de Músicas para um determinado Bioma
    public void CreateMusicButtons(MusicData.BiomeCategory biome)
    {
        // Limpar os botões de Músicas existentes
        foreach (Transform child in musicButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // Instanciar os botões para cada Música do Bioma
        foreach (MusicData.Music music in biome.musicList)
        {
            Button musicButton = InstantiateButton(music.musicSprite, musicButtonContainer);
            musicButton.onClick.AddListener(() => OnMusicButtonClicked(musicButton, music));

            // Verificar se o botão atual corresponde ao botão da música selecionada
            if (music.musicID == selectedMusicID)
            {
                // Marcar o botão como selecionado
                selectedMusicButton = musicButton;
                // (se necessário, adicione aqui a lógica para marcar o botão, por exemplo, mudando sua cor ou adicionando um indicador visual)
            }

            // Configurar a sprite da música no botão (se você deseja adicionar sprites ou ícones específicos às músicas)
            Image buttonImage = musicButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = music.musicSprite;
            }
        }

        // Selecionar automaticamente o botão do Bioma da música atual
        if (selectedMusicButton != null)
        {
            // Encontrar o Bioma da música atual
            MusicData.BiomeCategory biomeOfSelectedMusic = GetBiomeOfSelectedMusic();

            // Se encontrarmos o Bioma, selecionar o botão correspondente
            if (biomeOfSelectedMusic != null)
            {
                selectedBiomeButton = biomeButtonContainer.Find(biomeOfSelectedMusic.biomeName).GetComponent<Button>();
                selectedBiomeButton.interactable = false;
            }
        }
    }

    // Função chamada quando um botão de Bioma é clicado
    public void OnBiomeButtonClicked(Button biomeButton, MusicData.BiomeCategory biome)
    {
        // Se já tiver um Bioma selecionado, desmarcar o botão
        if (selectedBiomeButton != null)
        {
            // Resetar a cor do botão selecionado anteriormente
            var colors = selectedBiomeButton.colors;
            colors.normalColor = Color.white; // Defina a cor normal do botão (sem seleção)
            selectedBiomeButton.colors = colors;
        }

        // Marcar o botão do Bioma selecionado
        selectedBiomeButton = biomeButton;
        selectedBiomeButton.interactable = false;

        // Armazenar o sprite do bioma selecionado
        selectedBiomeSprite = biome.biomeSprite;

        // Alterar a cor do botão selecionado
        var selectedColors = selectedBiomeButton.colors;
        selectedColors.normalColor = Color.green; // Defina a cor do botão selecionado
        selectedBiomeButton.colors = selectedColors;

        // Criar e exibir o subpainel de músicas para o Bioma selecionado
        CreateMusicButtons(biome);
        // (se necessário, adicione aqui a lógica para exibir o subpainel)
    }

    // Função chamada quando um botão de Música é clicado
    public void OnMusicButtonClicked(Button musicButton, MusicData.Music music)
    {
        // Se a música atual for a mesma que a música selecionada, não é necessário fazer nada
        if (music.musicID == currentPlayingMusicID)
        {
            return;
        }

        // Parar a música atualmente em reprodução, se houver
        StopMusic();

        // Marcar o botão da música selecionada
        selectedMusicButton = musicButton;

        // Armazenar o ID da música selecionada
        selectedMusicID = music.musicID;

        MusicIDToSave = selectedMusicID;
        // Reproduzir a música atual
        PlayMusic();
    }

    // Função para instanciar um botão com um sprite específico e adicioná-lo a um container
    private Button InstantiateButton(Sprite buttonSprite, Transform container)
    {
        Button button = Instantiate<Button>(buttonPrefab, container);

        // Configurar a imagem do botão com o sprite fornecido
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.sprite = buttonSprite;
        }

        return button;
    }

    // Função para encontrar o Bioma da música selecionada
    private MusicData.BiomeCategory GetBiomeOfSelectedMusic()
    {
        // Verificar se há um botão de música selecionado
        if (selectedMusicButton != null)
        {
            // Obter a música selecionada usando o ID
            MusicData.Music selectedMusic = musicData.biomeCategories
                .SelectMany(biome => biome.musicList)
                .FirstOrDefault(music => music.musicID == selectedMusicID);

            if (selectedMusic != null)
            {
                // Obter o nome da música
                string selectedMusicName = selectedMusic.musicName;

                // Procurar o bioma que contém a música com o nome selecionado
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
        // Aqui você pode implementar qualquer lógica adicional para lidar com a mudança de MusicID, se necessário.
    }

    // Função para reproduzir a música selecionada
    private void PlayMusic()
    {
        // Carregar o evento FMOD associado à música selecionada
        if (FMODEvents.instance != null && FMODEvents.instance.musicList.ContainsKey((FMODEvents.MusicID)selectedMusicID))
        {
            EventReference musicEvent = FMODEvents.instance.musicList[(FMODEvents.MusicID)selectedMusicID];

            musicEventInstance = RuntimeManager.CreateInstance(musicEvent);

            // Tocar a música a partir do início
            if (musicEventInstance.isValid())
            {
                musicEventInstance.start();
                // Armazenar o ID da música atualmente em reprodução
                currentPlayingMusicID = selectedMusicID;
            }
        }
    }

    // Função para parar a música em reprodução
    public void StopMusic()
    {
        // Verificar se há uma música em reprodução e se é válida antes de parar
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
            // O valor é um número inteiro válido
            levelTime = timeValue;
        }
        else
        {
            // O valor não é um número inteiro válido, faça algo para lidar com isso
            Debug.LogWarning("Invalid integer input!");
        }
    }
    #endregion
}
