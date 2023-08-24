using UnityEngine;

public class ProfileInteraction : MonoBehaviour
{
    public void CreateProfileButton(int profileNumber)
    {
        ProfileManager.instance.CreateProfile(profileNumber);
    }

    public void SelectProfileButton(int profileNumber)
    {
        ProfileManager.instance.SelectProfile(profileNumber);
    }

    public void ResetProfileButton(int profileNumber)
    {
        ProfileManager.instance.ResetProfile(profileNumber);
    }
}

