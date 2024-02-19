using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour
{
    public enum MusicID
    {
        Forest01 = 1,
        Forest02 = 2,
        // Adicione mais IDs de música conforme necessário
    }

    [field: SerializeField]
    public Dictionary<MusicID, EventReference> musicList { get; private set; } // Dicionário de eventos de música associados com ID personalizado


    [field: Header("Ambience")]
    [field: SerializeField] public EventReference ambience { get; private set; }
    [field: SerializeField] public EventReference Thunder { get; private set; }
    [field: SerializeField] public EventReference Birds { get; private set; }
    [field: SerializeField] public EventReference BirdsAndWind { get; private set; }
    [field: SerializeField] public EventReference WaterDrops { get; private set; }
    [field: SerializeField] public EventReference WaterFall { get; private set; }
    [field: SerializeField] public EventReference Acid { get; private set; }
    [field: SerializeField] public EventReference Machine { get; private set; }
    [field: SerializeField] public EventReference Campfire { get; private set; }

    [field: Header("Editor Music")]
    [field: SerializeField] public EventReference EditorRandom { get; private set; }

    [field: Header("Menu Music")]
    [field: SerializeField] public EventReference MenuMusicA { get; private set; }

    [field: Header("Game Music")]
    [field: SerializeField] public EventReference FinishMusic { get; private set; }
    [field: SerializeField] public EventReference music { get; private set; }
    //Forest
    [field: SerializeField] public EventReference Forest01 { get; private set; }
    [field: SerializeField] public EventReference Forest02 { get; private set; }
    [field: SerializeField] public EventReference FightMusic { get; private set; }
    [field: SerializeField] public EventReference Invinciblemusic { get; private set; }


    [field: Header("Player SFX")]
    [field: SerializeField] public EventReference playerFootsteps { get; private set; }
    [field: SerializeField] public EventReference playerJump { get; private set; }
    [field: SerializeField] public EventReference Dead { get; private set; }
    [field: SerializeField] public EventReference Hurt { get; private set; }
    [field: SerializeField] public EventReference PropellerHatUp { get; private set; }
    [field: SerializeField] public EventReference PropellerHatDown { get; private set; }
    [field: SerializeField] public EventReference PropellerHatDownLoop { get; private set; }
    [field: SerializeField] public EventReference SkillLevelUpTalk { get; private set; }
    [field: SerializeField] public EventReference SkillLevelUpMusic { get; private set; }


    [field: Header("Ambient SFX")]
    [field: SerializeField] public EventReference BirdsFlutter { get; private set; }


    [field: Header("Coin SFX, and PowerUps")]
    [field: SerializeField] public EventReference coinCollected { get; private set; }
    [field: SerializeField] public EventReference emptyBlock { get; private set; }
    [field: SerializeField] public EventReference HealthCollect { get; private set; }
    [field: SerializeField] public EventReference PowerUpCollect { get; private set; }
    [field: SerializeField] public EventReference StarCollect { get; private set; }
    [field: SerializeField] public EventReference TickTimer { get; private set; }
    [field: SerializeField] public EventReference Countdown { get; private set; }


    [field: Header("Objects Triggers")]
    [field: SerializeField] public EventReference CheckPoint { get; private set; }
    [field: SerializeField] public EventReference Teleport01{ get; private set; }


    [field: Header("Objects")]
    [field: SerializeField] public EventReference FallingObject { get; private set; }
    [field: SerializeField] public EventReference PoofObject { get; private set; }
    [field: SerializeField] public EventReference OpenDoor { get; private set; }
    [field: SerializeField] public EventReference Locked { get; private set; }


    [field: Header("Enemies")]
    [field: SerializeField] public EventReference Canon { get; private set; }
    [field: SerializeField] public EventReference MushroomKilled { get; private set; }
    [field: SerializeField] public EventReference Stomp { get; private set; }
    [field: SerializeField] public EventReference FrogJump { get; private set; }
    [field: SerializeField] public EventReference PlantShoot { get; private set; }
    [field: SerializeField] public EventReference SnailSlime { get; private set; }
    [field: SerializeField] public EventReference TwompFall { get; private set; }
    [field: SerializeField] public EventReference Ghost01 { get; private set; }


    [field: Header("Bosses")]
    [field: Header("Spider")]
    [field: SerializeField] public EventReference SFX1 { get; private set; }
    [field: SerializeField] public EventReference SFX2 { get; private set; }
    [field: SerializeField] public EventReference SFX3 { get; private set; }
    [field: SerializeField] public EventReference SFX4 { get; private set; }
    [field: SerializeField] public EventReference SFX5 { get; private set; }
    [field: SerializeField] public EventReference SFX6 { get; private set; }
    [field: SerializeField] public EventReference SFX7 { get; private set; }


    [field: Header("Big Fish")]
    [field: Header("Water Guardian")]
    [field: Header("Token")]
    [field: Header("Wolf")]

    public static FMODEvents instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one FMOD Events instance in the scene.");
        }
        instance = this;

        // Inicialize o dicionário de eventos de música
        musicList = new Dictionary<MusicID, EventReference>
        {
            { MusicID.Forest01, Forest01 },
            { MusicID.Forest02, Forest02 },
            // Adicione mais eventos de música aqui conforme necessário
        };
    }
}
