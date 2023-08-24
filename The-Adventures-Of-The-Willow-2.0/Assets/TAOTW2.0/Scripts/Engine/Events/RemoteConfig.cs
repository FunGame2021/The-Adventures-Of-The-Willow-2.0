using System.Threading.Tasks;
using Unity.Services.RemoteConfig;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using TMPro;

public class RemoteConfig : MonoBehaviour
{
	public static RemoteConfig Instance;
    public struct userAttributes { }
    public struct appAttributes { }

    //Season count Down
    public float msToWait;
	//Event Name
	public string EventName;

    //BOOLS if is season
    public bool IsHalloween;
	public bool IsChristmas;
	public bool IsEaster;
	public bool IsWinter;
	public bool IsAutumm;
	public bool IsSummer;
	public bool IsSpring;
	public bool IsRainny;
		
	//GameObject Scene Objects to activate on season
    public GameObject HalloweenObject;
    public GameObject[] ChristmasObject;
    public GameObject EasterObject;
	public GameObject AutummObject;
	public GameObject SummerObject;
	public GameObject SpringObject;
	public GameObject WinterObject;
	public GameObject RainnyObject;
	
	//TXT BOOLS
	public bool ShowTXT;
	
	//TXT GameObject with timers
	public GameObject EventTXT;

    async Task InitializeRemoteConfigAsync()
    {
        // initialize handlers for unity game services
        await UnityServices.InitializeAsync();

        // remote config requires authentication for managing environment information
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    async Task Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        // initialize Unity's authentication and core services, however check for internet connection
        // in order to fail gracefully without throwing exception if connection does not exist
        if (Utilities.CheckForInternetConnection())
        {
            await InitializeRemoteConfigAsync();
        }

        RemoteConfigService.Instance.FetchCompleted += ApplyRemoteSettings;
        RemoteConfigService.Instance.FetchConfigs(new userAttributes(), new appAttributes());
    }

    void ApplyRemoteSettings(ConfigResponse configResponse)
    {
        switch (configResponse.requestOrigin)
        {
            case ConfigOrigin.Default:
                Debug.Log("No settings loaded this session; using default values.");
                break;
            case ConfigOrigin.Cached:
                Debug.Log("No settings loaded this session; using cached values from a previous session.");
                break;
            case ConfigOrigin.Remote:
                Debug.Log("New settings loaded this session; update values accordingly.");
				//Season CountDown
				msToWait = RemoteConfigService.Instance.appConfig.GetFloat("msToWait");
				//EventName
				EventName = RemoteConfigService.Instance.appConfig.GetString("EventName");
                //Get bool scene objectts
                IsHalloween = RemoteConfigService.Instance.appConfig.GetBool("IsHalloween");
                IsChristmas = RemoteConfigService.Instance.appConfig.GetBool("IsChristmas");
                IsEaster = RemoteConfigService.Instance.appConfig.GetBool("IsEaster");
				IsAutumm = RemoteConfigService.Instance.appConfig.GetBool("IsAutumm");
				IsSummer = RemoteConfigService.Instance.appConfig.GetBool("IsSummer");
				IsWinter = RemoteConfigService.Instance.appConfig.GetBool("IsWinter");
				IsSpring = RemoteConfigService.Instance.appConfig.GetBool("IsSpring");
				IsRainny = RemoteConfigService.Instance.appConfig.GetBool("IsRainny");
				
				//Get Bool TXT
				ShowTXT = RemoteConfigService.Instance.appConfig.GetBool("ShowTXT");
				break;
        }
    }

    private void Update()
    {
		//Activate Normal seasons
		if(IsAutumm)
		{
			AutummObject.SetActive(true);
		}
		else
		{
			AutummObject.SetActive(false);
		}
		
		if(IsWinter)
		{
			WinterObject.SetActive(true);
		}
		else
		{
			WinterObject.SetActive(false);
		}
		
		if(IsSummer)
		{
			SummerObject.SetActive(true);
		}
		else
		{
			SummerObject.SetActive(false);
		}
		
		if(IsSpring)
		{
			SpringObject.SetActive(true);
		}
		else
		{
			SpringObject.SetActive(false);
		}
		
		if(IsRainny)
		{
			RainnyObject.SetActive(true);
		}
		else
		{
			RainnyObject.SetActive(false);
		}
		
		//Activate Objects In Scene /Like Season is there
        if (IsHalloween) 
        {
            HalloweenObject.SetActive(true);
        }
        else
        {
            HalloweenObject.SetActive(false);
        }
		
		if (IsChristmas) 
        {
            foreach (GameObject tagged in ChristmasObject)
            {
                tagged.SetActive(true);
            }
        }
        else
        {
            foreach (GameObject tagged in ChristmasObject)
            {
                tagged.SetActive(false);
            }
        }
		
		if (IsEaster) 
        {
            EasterObject.SetActive(true);
        }
        else
        {
            EasterObject.SetActive(false);
        }
		
		
		//Active TXT Name CountDown To Events
		if(ShowTXT)
		{
			EventTXT.SetActive(true);
		}
		else
		{
			EventTXT.SetActive(false);
		}
    }
}