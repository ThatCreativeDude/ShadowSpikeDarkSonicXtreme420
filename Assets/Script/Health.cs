using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 1;
    public int currentHealth;

    public float invincibleTime = 1f;
    private bool isInvincible = false;


    void Awake()
    {
        currentHealth = maxHealth;
        isInvincible = false;
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible)
            return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(IFrames());
    }

    System.Collections.IEnumerator IFrames()
    {
        isInvincible = true;

        yield return new WaitForSeconds(invincibleTime);

        isInvincible = false;
    }

    void Die()
    {
        Debug.Log(gameObject.name + " died!");
        Destroy(gameObject);
    }
}