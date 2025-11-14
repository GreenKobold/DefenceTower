using UnityEngine;

public class CrossBowTower : BaseTowerAI
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    override public void Attack()
    {
        
        enemyinRadius[0].TakeDamage(damage);
    }
}
