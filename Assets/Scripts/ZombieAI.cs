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
    public LayerMask zombieMask;
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
            if (commandedTarget != null)
            {
                if (!commandedTarget.activeInHierarchy)
                {
                    isCommanded = false;
                    commandedTarget = null;
                    Log("lost commanded target, reverting.");
                }
                else
                {
                    Vector3 dest = GetAttackPosition(commandedTarget.transform.position);
                    ForceMove(dest);
                    Log("following commanded target " + commandedTarget.name);
                    if (InAttackRange(commandedTarget))
                        StopAndAttack(commandedTarget);
                    return;
                }
            }
            else
            {
                Log("moving to commanded destination");
                ForceMove(commandedDestination);
                return;
            }
        }

        if (targetWall != null && !targetWall.activeInHierarchy)
        {
            Log("target wall destroyed, moving slightly forward to reacquire.");
            Vector3 forwardStep = transform.position + transform.forward * stepAfterDestruction;
            targetWall = null;
            ForceMove(forwardStep);
            return;
        }

        GameObject wallTarget = FindVisibleWall();
        if (wallTarget != null)
        {
            hasFoundBase = true;
            targetWall = wallTarget;
            float dist = Vector3.Distance(transform.position, wallTarget.transform.position);
            Log("sees wall " + wallTarget.name + " at " + dist);
            if (dist > attackRange)
            {
                Vector3 attackPos = GetAttackPosition(wallTarget.transform.position);
                ForceMove(attackPos);
                Log("pathing to wall " + wallTarget.name + " via attack position " + attackPos);
            }
            else
            {
                StopAndAttack(wallTarget);
                Log("attacking wall " + wallTarget.name);
            }
            return;
        }

        if (CanSeeCore())
        {
            hasFoundBase = true;
            Vector3 attackPos = GetAttackPosition(baseCore.position);
            Log("sees core");
            ForceMove(attackPos);
            if (InAttackRange(baseCore.gameObject))
                StopAndAttack(baseCore.gameObject);
            return;
        }

        if (alertReceived && targetWall != null)
        {
            hasFoundBase = true;
            float dist = Vector3.Distance(transform.position, targetWall.transform.position);
            Log("alert received, target wall " + targetWall.name);
            if (dist > attackRange)
            {
                Vector3 attackPos = GetAttackPosition(targetWall.transform.position);
                ForceMove(attackPos);
                Log("pathing (alert) to " + targetWall.name);
            }
            else
            {
                StopAndAttack(targetWall);
                Log("attacking (alert) wall " + targetWall.name);
            }
            return;
        }

        GameObject player = CheckForPlayerInRange();
        if (player != null)
        {
            hasFoundBase = true;
            targetPlayer = player;
            AlertNearbyZombies(player);
            float dist = Vector3.Distance(transform.position, player.transform.position);
            Log("sees player");
            if (dist > attackRange)
            {
                Vector3 attackPos = GetAttackPosition(player.transform.position);
                ForceMove(attackPos);
                Log("pathing to player");
            }
            else
            {
                StopAndAttack(player);
                Log("attacking player");
            }
            return;
        }

        if (!hasFoundBase)
        {
            if (agent.remainingDistance < 1f || !agent.hasPath)
            {
                Log("wandering toward center");
                ForceMove(Vector3.zero);
            }
        }
        else
        {
            if (agent.remainingDistance < 0.5f && !agent.pathPending)
            {
                Log("idle but has found base, scanning for next wall");
                GameObject newWall = FindVisibleWall();
                if (newWall != null)
                {
                    targetWall = newWall;
                    Vector3 attackPos = GetAttackPosition(newWall.transform.position);
                    ForceMove(attackPos);
                    Log("reacquired wall " + newWall.name);
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

    GameObject FindVisibleWall()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, visionRange, wallMask);
        GameObject best = null;
        float lowestHP = Mathf.Infinity;
        foreach (Collider c in hits)
        {
            if (c == null) continue;
            Vector3 dir = (c.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dir);
            if (angle > forwardConeAngle * 0.5f) continue;
            Health h = c.GetComponent<Health>();
            if (h != null && h.currentHP < lowestHP)
            {
                lowestHP = h.currentHP;
                best = c.gameObject;
            }
        }
        return best;
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
        Collider[] hits = Physics.OverlapSphere(transform.position, communicationRange, zombieMask);
        foreach (Collider c in hits)
        {
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
