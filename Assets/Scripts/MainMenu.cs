using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
public class MainMenu : MonoBehaviour
{
    public TextMeshProUGUI resultText;
    public TMP_InputField inputField;
    public void playgame()
    {
        SceneManager.LoadSceneAsync(1);
    }
    public void quitgame()
    {
        Application.Quit();
    }
    public void ValidateInput()
    {
        string name = inputField.text;
        if (name.Length < 3)
        {
            resultText.text = "invalid Input";
        }
        else if( name.Length >= 20)
        {
            resultText.text = "invalid Input";
        }
        else
        {
            PlayerPrefs.SetString("PlayerName", name);
            playgame();
        }
    }

    }
