using UnityEngine;

namespace Managers
{
    public class PlayerPrefManager : MonoBehaviour
    {
        private const string rememberCredentials = "rememberAccountCredentials";
        private const string rememberedEmail = "rememberedEmail";
        private const string rememberedPass = "rememberedPassword";
        private const string hasPlayedTutorial = "hasPlayedTutorial";

        public static bool RememberCredentials()
        {
            if (PlayerPrefs.HasKey(rememberCredentials))
            {
                return PlayerPrefs.GetInt(rememberCredentials) != 0;   
            }
            return false;
        }

        public static string Email()
        {
            return PlayerPrefs.GetString(rememberedEmail);
        }
        public static string Pass()
        {
            return PlayerPrefs.GetString(rememberedPass);
        }

        public static bool Tutorial()
        {
            return PlayerPrefs.GetInt(hasPlayedTutorial) != 0;
        }

        public static void Email(string mail)
        {
            PlayerPrefs.SetString(rememberedEmail,mail);
        }
        public static void Pass(string pass)
        {
            PlayerPrefs.SetString(rememberedPass,pass);
        }
    
        public static void Remember(int set)
        {
            PlayerPrefs.SetInt(rememberCredentials,set);
        }
    
        public static void Tutorial(int set)
        {
            PlayerPrefs.SetInt(hasPlayedTutorial,set);
        }
    }
}

