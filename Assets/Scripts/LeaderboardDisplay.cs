using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Linq;

public class LeaderboardDisplay : MonoBehaviour
{
    
    [System.Serializable]
    public class PlayerData
    {
        public string name;
        public int score;
    }

    public string databaseURL = "https://gc-td-34b1d-default-rtdb.firebaseio.com/leaderboard.json";

    [Header("UI Baðlantýlarý")]
    public Transform contentArea; 
    public GameObject rowPrefab;  
    public GameObject leaderboardPanel; 

    
    public void OpenLeaderboard()
    {
        leaderboardPanel.SetActive(true);
        StartCoroutine(GetScores());
    }

    public void CloseLeaderboard()
    {
        leaderboardPanel.SetActive(false);
    }

    IEnumerator GetScores()
    {
        
        foreach (Transform child in contentArea)
        {
            Destroy(child.gameObject);
        }

        
        string url = databaseURL + "?orderBy=\"score\"&limitToLast=10";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                
                string json = request.downloadHandler.text;

                
                List<PlayerData> playerList = ParseJsonManually(json);

                
                playerList.Reverse();

                
                int rank = 1;
                foreach (PlayerData player in playerList)
                {
                    GameObject newRow = Instantiate(rowPrefab, contentArea);
                    TextMeshProUGUI textComp = newRow.GetComponent<TextMeshProUGUI>();

                    
                    textComp.text = $"{rank}. {player.name} - {player.score}";
                    rank++;
                }
            }
            else
            {
                Debug.LogError("Hata: " + request.error);
            }
        }
    }

    
    List<PlayerData> ParseJsonManually(string json)
    {
        List<PlayerData> list = new List<PlayerData>();

       

        var entries = json.Split(new string[] { "}," }, System.StringSplitOptions.RemoveEmptyEntries);

        foreach (var entry in entries)
        {
            try
            {
                
                string nameKey = "\"name\":\"";
                int nameStart = entry.IndexOf(nameKey) + nameKey.Length;
                int nameEnd = entry.IndexOf("\"", nameStart);
                string name = entry.Substring(nameStart, nameEnd - nameStart);

                
                string scoreKey = "\"score\":";
                int scoreStart = entry.IndexOf(scoreKey) + scoreKey.Length;
                
                string scoreStr = entry.Substring(scoreStart).Split('}')[0].Trim();
                int score = int.Parse(scoreStr);

                PlayerData newPlayer = new PlayerData();
                newPlayer.name = name;
                newPlayer.score = score;
                list.Add(newPlayer);
            }
            catch
            {
                
            }
        }

        
        return list.OrderBy(x => x.score).ToList();
    }
}