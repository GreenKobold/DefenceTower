using UnityEngine;

public class BasicZombie : BaseEnemyAI
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public override void Attack()
    {
        targetWall.TakeDamage(damage);
    }
    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }
}
