using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseTowerAI : Structure
{
    public float towerCD = 0;
    public float maxTowerCD = 12.0f;
    public Collider towerRadius;
    public List<BaseEnemyAI> enemyinRadius = new List<BaseEnemyAI>();

    protected float damage = 20;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetStructureType(StructureType.Tower);
    }
    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        CDTimer();
        if (DetectEnemy() && towerCD <= 0)
        {
            Attack();
            MaxTimerCD();
        }
        else
            return;
    }

    public abstract void Attack();
    //Towers require a rigid body for OnTriggerToWork
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Zombie"))
        {
            enemyinRadius.Add(other.gameObject.GetComponent<BaseEnemyAI>());
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Zombie"))
        {
            enemyinRadius.Remove(other.gameObject.GetComponent<BaseEnemyAI>());
        }
    }
    protected bool DetectEnemy()
    {
        if (enemyinRadius.Count > 0)
        {
            return true;
        }
        else
            return false;
    }

    protected void CDTimer()
    {
        if (towerCD > 0)
        {
            towerCD -= Time.deltaTime;
        }
        else
        {
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
}
