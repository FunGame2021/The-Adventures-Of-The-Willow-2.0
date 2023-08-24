using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class EditorMusicBackground : MonoBehaviour
{
    private EventInstance currentMusicInstance;

    private void Start()
    {
        PlayEditorMusic();
    }

    private void PlayEditorMusic()
    {
        currentMusicInstance = FMODUnity.RuntimeManager.CreateInstance(FMODEvents.instance.EditorRandom);
        currentMusicInstance.start();
    }

    private void OnDestroy()
    {
        // Verifica se a inst�ncia � v�lida antes de par�-la
        if (currentMusicInstance.isValid())
        {
            currentMusicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            currentMusicInstance.release(); // Libera a inst�ncia
        }
    }
    public void PauseEditorMusic()
    {
        currentMusicInstance.setPaused(true); // Pausa a inst�ncia    
    }

    public void ContinueEditorMusic()
    {
        currentMusicInstance.setPaused(false); // Continua a inst�ncia
    }
}
