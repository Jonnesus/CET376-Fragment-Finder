using UnityEngine;

public class FragmentLogic : MonoBehaviour
{
    [SerializeField] private AudioSource playerAudioSource;
    [SerializeField] private HUDController hudController;

    private void Update()
    {
        transform.Rotate(0, 0, 0.5f);
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerAudioSource.Play();
            hudController.fragmentsCollected++;
            GameObject.Destroy(gameObject);
        }
    }
}