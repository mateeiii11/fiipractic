using UnityEngine;
public class SoundManager : MonoBehaviour
{
    public AudioSource footstepsSound;

    void Update()
    {
        bool pressingKeys = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;

        footstepsSound.enabled = pressingKeys;
    }
}
