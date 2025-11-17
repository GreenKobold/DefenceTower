using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    [Header("Settings")]
    public float visionRange = 10f;
    public float communicationRange = 15f;
    public float attackRange = 2f;
    public float attackDamage = 10f;
    public float attackCooldown = 2f;
    public float forwardConeAngle = 120f;
    public float stepAfterDestruction = 1.2f;
    public float attackOffset = 1.0f;
    public bool debugMode = true;

    [Header("References")]
    public NavMeshAgent agent;
    public Animator animator;
    public LayerMask wallMask;
    public LayerMask losBlockMask;
    public Transform baseCore;

    [HideInInspector] public bool alertReceived = false;
    [HideInInspector] public GameObject alertedBy = null;
    [HideInInspector] public GameObject targetWall = null;
    [HideInInspector] public GameObject targetPlayer = null;
    [HideInInspector] public bool isAlive = true;
    [HideInInspector] public bool isCommanded = false;
    [HideInInspector] public Vector3 commandedDestination;
    [HideInInspector] public GameObject commandedTarget;

    bool hasFoundBase = false;
    float attackTimer = 0f;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }
    }

    void Update()
    {
        if (!isAlive) return;
        attackTimer -= Time.deltaTime;

        if (isCommanded)
        {
            if (commandedTarget != null && commandedTarget.activeInHierarchy)
            {
                Vector3 pos = GetAttackPosition(commandedTarget.transform.position);
                ForceMove(pos);
                if (InAttackRange(commandedTarget)) StopAndAttack(commandedTarget);
            }
            else
            {
                ForceMove(commandedDestination);
            }
            return;
        }

        GameObject player = CheckForPlayerInRange();
        if (player != null)
        {
            hasFoundBase = true;
            targetPlayer = player;
            AlertNearbyZombies(player);
            Vector3 pos = GetAttackPosition(player.transform.position);
            if (!InAttackRange(player)) ForceMove(pos);
            else StopAndAttack(player);
            return;
        }

        bool coreAlerted = alertReceived && targetWall == (baseCore != null ? baseCore.gameObject : null);
        if (CanSeeCore() || coreAlerted)
        {
            hasFoundBase = true;
            if (CanSeeCore() && alertedBy == null && baseCore != null)
                AlertNearbyZombies(baseCore.gameObject);
            if (baseCore != null)
            {
                Vector3 pos = GetAttackPosition(baseCore.position);
                if (!InAttackRange(baseCore.gameObject)) ForceMove(pos);
                else StopAndAttack(baseCore.gameObject);
            }
            return;
        }

        if (alertReceived && targetWall != null && targetWall.activeInHierarchy && targetWall != (baseCore != null ? baseCore.gameObject : null))
        {
            hasFoundBase = true;
            Vector3 pos = GetAttackPosition(targetWall.transform.position);
            if (!InAttackRange(targetWall)) ForceMove(pos);
            else StopAndAttack(targetWall);
            return;
        }

        if (targetWall != null && !targetWall.activeInHierarchy)
        {
            Vector3 forwardStep = transform.position + transform.forward * stepAfterDestruction;
            targetWall = null;
            alertReceived = false;
            ForceMove(forwardStep);
            return;
        }

        GameObject structureTarget = FindVisibleStructure();
        if (structureTarget != null)
        {
            hasFoundBase = true;
            targetWall = structureTarget;
            float dist = Vector3.Distance(transform.position, structureTarget.transform.position);
            Log("sees structure " + structureTarget.name + " at " + dist);
            Vector3 pos = GetAttackPosition(structureTarget.transform.position);
            if (!InAttackRange(structureTarget)) ForceMove(pos);
            else StopAndAttack(structureTarget);
            return;
        }

        if (!hasFoundBase)
        {
            if (agent.remainingDistance < 1f || !agent.hasPath)
                ForceMove(Vector3.zero);
        }
        else
        {
            if (agent.remainingDistance < 0.5f && !agent.pathPending)
            {
                GameObject newStruct = FindVisibleStructure();
                if (newStruct != null)
                {
                    targetWall = newStruct;
                    Vector3 pos = GetAttackPosition(newStruct.transform.position);
                    ForceMove(pos);
                }
            }
        }
    }

    Vector3 GetAttackPosition(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        Vector3 adjusted = targetPos - dir * attackOffset;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(adjusted, out hit, 1.5f, NavMesh.AllAreas))
            return hit.position;
        for (float i = 0.2f; i <= 1.0f; i += 0.2f)
        {
            Vector3 closer = targetPos - dir * (attackOffset - i);
            if (NavMesh.SamplePosition(closer, out hit, 1.5f, NavMesh.AllAreas))
                return hit.position;
        }
        return transform.position;
    }

    bool InAttackRange(GameObject target)
    {
        if (target == null) return false;
        float dist = Vector3.Distance(transform.position, target.transform.position);
        return dist <= attackRange * 1.25f;
    }

    void StopAndAttack(GameObject target)
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        transform.LookAt(target.transform.position);
        Attack(target);
    }

    GameObject FindVisibleStructure()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, visionRange, wallMask);
        GameObject bestBuilding = null;
        float bestBuildingHP = Mathf.Infinity;
        GameObject bestWall = null;
        float bestWallHP = Mathf.Infinity;

        foreach (Collider c in hits)
        {
            if (c == null) continue;
            Vector3 dir = (c.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dir);
            if (angle > forwardConeAngle * 0.5f) continue;

            Health h = c.GetComponent<Health>();
            if (h == null) continue;

            if (c.CompareTag("Building"))
            {
                if (h.currentHP < bestBuildingHP)
                {
                    bestBuildingHP = h.currentHP;
                    bestBuilding = c.gameObject;
                }
            }
            else if (c.CompareTag("Wall"))
            {
                if (h.currentHP < bestWallHP)
                {
                    bestWallHP = h.currentHP;
                    bestWall = c.gameObject;
                }
            }
        }

        if (bestBuilding != null)
        {
            Log("prioritizing building " + bestBuilding.name + " over walls");
            return bestBuilding;
        }

        return bestWall;
    }

    bool CanSeeCore()
    {
        if (baseCore == null) return false;
        float dist = Vector3.Distance(transform.position, baseCore.position);
        if (dist > visionRange) return false;
        Vector3 eye = transform.position + Vector3.up;
        if (Physics.Linecast(eye, baseCore.position, losBlockMask)) return false;
        return true;
    }

    void ForceMove(Vector3 pos)
    {
        if (agent == null || !agent.isOnNavMesh) return;
        agent.isStopped = false;
        agent.ResetPath();
        if (agent.SetDestination(pos))
            Log("destination set: " + pos);
        else
            Log("failed to set destination");
    }

    void Attack(GameObject target)
    {
        if (attackTimer > 0f) return;
        attackTimer = attackCooldown;
        Health h = target.GetComponent<Health>();
        if (h != null) h.TakeDamage(attackDamage);
        if (animator != null) animator.SetTrigger("Attack");
        Log("dealt " + attackDamage + " damage to " + target.name);
    }

    void AlertNearbyZombies(GameObject target)
    {
        if (alertedBy != null) return;
        Collider[] hits = Physics.OverlapSphere(transform.position, communicationRange);
        foreach (Collider c in hits)
        {
            if (c.gameObject == gameObject) continue;
            ZombieAI z = c.GetComponent<ZombieAI>();
            if (z != null && !z.alertReceived)
            {
                z.alertReceived = true;
                z.targetWall = target;
                z.alertedBy = this.gameObject;
                Log("alerted " + z.name + " about " + target.name);
            }
        }
    }

    GameObject CheckForPlayerInRange()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, visionRange);
        foreach (Collider c in hits)
        {
            if (c.CompareTag("Player")) return c.gameObject;
        }
        return null;
    }

    public void Die()
    {
        isAlive = false;
        if (animator != null) animator.SetTrigger("Die");
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
        Log("died");
        Destroy(gameObject, 3f);
    }

    void Log(string msg)
    {
        if (debugMode) Debug.Log(name + " - " + msg);
    }

    public float GetCurrentHP()
    {
        Health h = GetComponent<Health>();
        return h != null ? h.currentHP : 99999f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, communicationRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
