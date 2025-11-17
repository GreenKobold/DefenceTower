using UnityEngine;

public class CrossBowTower : BaseTowerAI
{
    void Start()
    {

    }

    public override void Attack(ZombieAI target)
    {
        if (target == null) return;

        Health h = target.GetComponent<Health>();
        if (h != null)
            h.TakeDamage(damage);
    }
}
