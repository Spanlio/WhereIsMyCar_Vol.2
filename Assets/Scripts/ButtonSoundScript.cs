    using UnityEngine;

public class ButtonSoundScript : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonSound;

    public void PlayButtonSound()
    {
        if (audioSource == null || buttonSound == null)
            return;

        audioSource.PlayOneShot(buttonSound);
    }
}