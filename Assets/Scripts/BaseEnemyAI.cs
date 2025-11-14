using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public abstract class BaseEnemyAI : MonoBehaviour
{
    //Enemy Stats
    public float HP = 200f;
    public float speed = 5f;

    protected float damage = 5f;

    //variables for pathfinding and decision making
    bool alertReceived = false;
    //targetPlayer = null
    Collider communicationRange;
    float visionRange;
    public float attackRange = 3f;
    protected WallScript weakestWall = null;
    protected WallScript targetWall = null;
    //alertedBy = null

    // Lists for variables 
    List<WallScript> wallsInRadius = new List<WallScript>();
    List<WallScript> visibleWalls = new List<WallScript>();
    List<BaseEnemyAI> zombiesInRange = new List<BaseEnemyAI>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        SenseEnvironment();
        if(HP <= 0)
        {
            Destroy(this.gameObject);
        }
        if (alertReceived == true)
        { 
            MoveTo(targetWall);
            if (InAttackRange())
            {
                Attack();
            }
        }
        weakestWall = FindWeakestWallInRange();
        if (weakestWall != null)
        {
            //    targetWall = ResolveTies(weakestWall)

            //    if (IsUniqueWeakness(targetWall))
            //    AlertNearbyZombies(targetWall)

            MoveTo(targetWall);
            if (InAttackRange())
                Attack();
            //    continue
        }
        //playerVisible = CheckForPlayerInRange()
        //if (playerVisible)
        //    targetPlayer = playerVisible
        //    AlertNearbyZombies(targetPlayer)
        //    MoveTo(targetPlayer)
        //    if (InAttackRange(targetPlayer))
        //    Attack(targetPlayer);
        //else
        //    WanderOrPathToBase();
    }
    public abstract void Attack();
    public void TakeDamage(float damageTaken)
    {
        HP -= damageTaken;
    }
    //Triggers used to add walls/zombies into list 
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Zombie"))
        {
            zombiesInRange.Add(other.gameObject.GetComponent<BaseEnemyAI>());
        }
        if (other.gameObject.CompareTag("Wall"))
        {
            wallsInRadius.Add(other.gameObject.GetComponent<WallScript>());
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Zombie"))
        {
            zombiesInRange.Remove(other.gameObject.GetComponent<BaseEnemyAI>());
        }
        if (other.gameObject.CompareTag("Wall"))
        {
            wallsInRadius.Remove(other.gameObject.GetComponent<WallScript>());
        }
    }
    void SenseEnvironment()
    {

    }
    #region Pathfinding
    WallScript FindWeakestWallInRange()
    {
        WallScript testWall = null;
        visibleWalls = GetWallsWithinRange(visionRange);
        if (visibleWalls.Count == 0)
            return null;
        else
        {
            foreach (WallScript wall in visibleWalls)
            {
                if (testWall == null)
                {
                    testWall = wall;
                }
                else
                {
                    if (wall.GetHP() < testWall.GetHP())
                    {
                        testWall = wall;
                    }
                }
            }
            return weakestWall;
        }
        
    }
    /*
     * Method uses the total walls in radius to calculate walls that are actually visible by the zombie
     */
    List<WallScript> GetWallsWithinRange(float range)
    {
        List<WallScript> tempList = new List<WallScript>();

        return tempList;
    }
    bool InAttackRange()
    {
        if (targetWall != null)
        {
            if (Vector3.Distance(targetWall.transform.position, this.gameObject.transform.position) < attackRange)
            {
                return true;
            }
            else return false;
        }
        else return false;
    }
    //void ResolveTies(weakestWalls)
    //{
    //    if (multiple walls share same lowest HP)
    //    return closest wall by distance
    //    else
    //        return weakest wall
    //}
    bool IsUniqueWeakness(WallScript targetWall)
    {
        //if (multiple walls have same HP)
        return true;
        //return false;

    }
    void AlertNearbyZombies()
    {
        foreach (BaseEnemyAI zombie in zombiesInRange)
        {
            if (zombie.alertReceived == false)
            {   zombie.alertReceived = true;
                zombie.targetWall = targetWall;
                //zombie.alertedBy = this.BaseEnemyAI;
            }
        }
    }
    void MoveTo(WallScript wall)
    {
    //    Pathfind toward target using A* search
    //    Follow path nodes until in attack range
    //Attack(target):
    //Deal damage per attack cycle to target
    //Play attack animation/ sound
    }
    #endregion
    #region Getter/Setters for the enemies
    float GetHP()
    {
        return HP;
    }
    void SetHP(float health)
    {
        HP = health;
    }
    float GetDamage()
    {
        return damage;
    }
    void SetDamage(float newDamage)
    {
        damage = newDamage;
    }
    #endregion
}
