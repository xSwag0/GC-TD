using UnityEngine;

public class PlaySoundOnSpawn : MonoBehaviour
{
    [SerializeField] private AudioClip clip; 
    [Range(0f, 1f)][SerializeField] private float volume = 1f;

    private void OnEnable()
    {
        if (clip != null)
        {
           
            AudioSource.PlayClipAtPoint(clip, transform.position, volume);
        }
    }
}