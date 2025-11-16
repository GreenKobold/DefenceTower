using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHP = 100f;
    public float currentHP;

    void Start() => currentHP = maxHP;

    public void TakeDamage(float dmg)
    {
        currentHP -= dmg;
        if (currentHP <= 0) Destroy(gameObject);
    }
}
