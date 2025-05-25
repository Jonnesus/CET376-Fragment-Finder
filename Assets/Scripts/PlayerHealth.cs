using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int health;
    public bool playerAlive = true;
    public bool iFrames = false;

    [SerializeField] private int maxHealth = 100;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private HUDController hudController;

    System.Random rnd = new System.Random();

    private void Start()
    {
        health = maxHealth;
        playerAlive = true;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        StartCoroutine(InvincibilityFrames());

        if (health <= 0)
        {
            Debug.Log("Player Dead");
            playerAlive = false;
            hudPanel.SetActive(false);
            deathPanel.SetActive(true);
            Cursor.visible = true;
            Time.timeScale = 0;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Death"))
        {
            TakeDamage(150);
        }
        if (collision.gameObject.CompareTag("Win"))
        {
            hudController.CalculateFragmentsCollected();
        }
    }

    private IEnumerator InvincibilityFrames()
    {
        yield return new WaitForSeconds(3);
        iFrames = false;
    }
}