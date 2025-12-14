using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ScoreManager : MonoBehaviour
{
    private string databaseURL = "https://gc-td-34b1d-default-rtdb.firebaseio.com/leaderboard.json";

    public void SubmitScore()
    {
        
        string savedName = PlayerPrefs.GetString("PlayerName", "Adsiz");
        string cleanName = ConvertToEnglish(savedName);

        
        int currentScore = 0;

        
        if (GameManager.Instance != null)
        {
            currentScore = GameManager.Instance.Score;
        }
        else
        {
            Debug.LogError("HATA: Sahnede GameManager bulunamadi! Skor 0 olarak gidiyor.");
        }

        
        StartCoroutine(PostScore(cleanName, currentScore));
    }

    IEnumerator PostScore(string name, int score)
    {
        string json = "{\"name\":\"" + name + "\",\"score\":" + score + "}";

        var request = new UnityWebRequest(databaseURL, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Skor basariyla gönderildi: " + name + " - " + score);
        }
        else
        {
            Debug.LogError("Hata: " + request.error);
        }
    }

    
    string ConvertToEnglish(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        text = text.Replace("ý", "i").Replace("ð", "g").Replace("ü", "u")
                   .Replace("þ", "s").Replace("ö", "o").Replace("ç", "c")
                   .Replace("Ý", "I").Replace("Ð", "G").Replace("Ü", "U")
                   .Replace("Þ", "S").Replace("Ö", "O").Replace("Ç", "C");
        return text;
    }
}