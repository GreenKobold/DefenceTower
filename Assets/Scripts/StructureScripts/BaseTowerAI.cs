using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseTowerAI : Structure
{
    public float towerCD = 0;
    public float maxTowerCD = 12.0f;
    public float towerRange = 10f;                
    public LayerMask enemyMask;                 

    public List<ZombieAI> enemyinRadius = new List<ZombieAI>();

    protected float damage = 20;

    void Start()
    {
        SetStructureType(StructureType.Tower);
    }

    protected override void Update()
    {
        base.Update();
        CDTimer();

        RefreshEnemiesInRange();     

        if (DetectEnemy() && towerCD <= 0)
        {
            ZombieAI target = GetLowestHPEnemy();
            if (target != null)
            {
                AttackTarget(target);
                MaxTimerCD();
            }
        }
        else
            return;
    }

    void RefreshEnemiesInRange()
    {
        enemyinRadius.Clear();

        Collider[] hits = Physics.OverlapSphere(transform.position, towerRange, enemyMask);

        foreach (Collider c in hits)
        {
            ZombieAI z = c.GetComponent<ZombieAI>();
            if (z != null && z.isAlive)
            {
                enemyinRadius.Add(z);
            }
        }
    }

    protected ZombieAI GetLowestHPEnemy()
    {
        ZombieAI lowest = null;
        float lowestHP = Mathf.Infinity;

        foreach (ZombieAI z in enemyinRadius)
        {
            if (z == null) continue;

            Health h = z.GetComponent<Health>();
            if (h != null && h.currentHP < lowestHP)
            {
                lowestHP = h.currentHP;
                lowest = z;
            }
        }

        return lowest;
    }

    protected virtual void AttackTarget(ZombieAI target)
    {
        Attack(target);
    }

    public abstract void Attack(ZombieAI target);

    protected bool DetectEnemy()
    {
        return enemyinRadius.Count > 0;
    }

    protected void CDTimer()
    {
        if (towerCD > 0)
        {
            towerCD -= Time.deltaTime;
        }
    }
    protected void MaxTimerCD()
    {
        towerCD = maxTowerCD;
    }

    #region DamageGetters/Setters
    protected float GetDamage()
    {
        return damage;
    }
    protected void SetDamage(float newDamage)
    {
        damage = newDamage;
    }
    #endregion

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, towerRange);
    }
}
