using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseTowerAI : Structure
{
    float towerCD = 0;
    public float maxTowerCD = 12.0f;
    public Collider towerRadius;
    List<BaseEnemyAI> enemyinRadius = new List<BaseEnemyAI>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetStructureType(StructureType.Tower);
    }
    // Update is called once per frame
    void Update()
    {
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
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Zombie"))
        {
            enemyinRadius.Add(other.gameObject.GetComponent<BaseEnemyAI>());
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.CompareTag("Zombie"))
        {
            enemyinRadius.Remove(other.gameObject.GetComponent<BaseEnemyAI>());
        }
    }
    bool DetectEnemy()
    {
        if (enemyinRadius.Count != 0)
        {
            return true;
        }
        else
            return false;
    }

    void CDTimer()
    {
        if (towerCD > 0)
        {
            towerCD -= Time.deltaTime;
        }
        else
        {
            return;
        }
    }
    void MaxTimerCD()
    {
        towerCD = maxTowerCD;
    }
}
