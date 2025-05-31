using UnityEngine;

public static class UserData
{
    public static string UserName
    {
        get => PlayerPrefs.GetString("UserName", "User");
        set
        {
            PlayerPrefs.SetString("UserName", value);
            PlayerPrefs.Save();
        }
    }
    public static bool HasUser => PlayerPrefs.HasKey("UserName") && 
                                  !string.IsNullOrWhiteSpace(PlayerPrefs.GetString("UserName"));


    public static bool HasQuestItem = false;
}
