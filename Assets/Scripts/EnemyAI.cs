using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class EnemyAI : MonoBehaviour
{
    public TumanGame.EnemyStats stats;
    public void Configure(Transform target, Transform[] points, TumanGame.EnemyStats values)
    {
        player=target;patrolPoints=points;stats=values;
    }
    private void ApplyStats()
    {
        if(!stats)return;
        attackRange=stats.attackRange;windupTime=stats.attackWindup;attackCooldown=stats.attackCooldown;damage=stats.damage;
        detectionDistance=stats.visionRange;chaseSpeed=stats.chaseSpeed;stopDistance=stats.stopDistance;
        lostSightTime=stats.lostSightTime;patrolSpeed=stats.patrolSpeed;arrivalDistance=stats.arrivalDistance;waitTime=stats.patrolWait;
    }
    public enum State { Patrol, Chase, Attack }
    [Header("Attack")]
    [SerializeField, Min(0.1f)] private float attackRange;
    [SerializeField, Min(0.1f)] private float windupTime;
    [SerializeField, Min(0.1f)] private float attackCooldown;
    [SerializeField, Min(1)] private int damage;
    [Header("Vision and chase")]
    [SerializeField] private Transform player;
    [SerializeField, Min(0.1f)] private float detectionDistance;
    [SerializeField, Min(0.1f)] private float chaseSpeed;
    [SerializeField, Min(0.1f)] private float stopDistance;
    [SerializeField, Min(0f)] private float lostSightTime;
    [SerializeField] private LayerMask sightMask = Physics2D.DefaultRaycastLayers;
    [SerializeField] private State currentState;
    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField, Min(0.1f)] private float patrolSpeed;
    [SerializeField, Min(0.01f)] private float arrivalDistance;
    [SerializeField, Min(0f)] private float waitTime;

    private Rigidbody2D body;
    private int pointIndex;
    private float waitRemaining;
    private Collider2D ownCollider;
    private Collider2D playerCollider;
    private Vector2 lastSeenPosition;
    private float timeWithoutSight;
    private PlayerHealth playerHealth;
    private SpriteRenderer sprite;
    private Color originalColor;
    private bool windingUp;
    private float strikeAt;
    private float nextAttackAt;
    private readonly RaycastHit2D[] sightHits = new RaycastHit2D[32];
    public State CurrentState => currentState;

    private void Awake()
    {
        ApplyStats();
        body = GetComponent<Rigidbody2D>();
        ownCollider = GetComponent<Collider2D>();
        if (player != null) playerCollider = player.GetComponent<Collider2D>();
        if (player != null) playerHealth = player.GetComponent<PlayerHealth>();
        sprite = GetComponent<SpriteRenderer>();
        if (sprite) originalColor = sprite.color;
        currentState = State.Patrol;
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogWarning("EnemyAI needs patrol points in the Inspector.", this);
            enabled = false;
        }
    }

    private void FixedUpdate()
    {
        if(TumanGame.GameSession.Instance && !TumanGame.GameSession.Instance.IsPlaying) { body.linearVelocity=Vector2.zero;return; }
        ApplyStats();
        bool visible = CanSeePlayer();
        if (visible)
        {
            if (currentState == State.Patrol) currentState = State.Chase;
            timeWithoutSight = 0f;
            lastSeenPosition = player.position;
        }
        else if (currentState != State.Patrol)
        {
            timeWithoutSight += Time.fixedDeltaTime;
            if (timeWithoutSight >= lostSightTime) ReturnToPatrol();
        }

        if (currentState != State.Patrol && visible && playerHealth &&
            Vector2.Distance(body.position, player.position) <= attackRange)
        {
            currentState = State.Attack;
            bool ranged=stats&&stats.rangedAttack;
            body.linearVelocity = ranged&&Vector2.Distance(body.position,player.position)<stats.retreatDistance
                ? (body.position-(Vector2)player.position).normalized*chaseSpeed : Vector2.zero;
            if (!windingUp && Time.time >= nextAttackAt)
            {
                windingUp = true;
                strikeAt = Time.time + windupTime;
                if (sprite) sprite.color = Color.yellow;
            }
            if (windingUp && Time.time >= strikeAt)
            {
                // Recheck sight and range at the hit, so the player can dodge.
                if (CanSeePlayer() && Vector2.Distance(body.position, player.position) <= attackRange)
                {
                    if(ranged)TumanGame.EnemyBolt.Create(ownCollider.bounds.center,playerCollider.bounds.center,gameObject,stats);
                    else playerHealth.TakeDamage(damage);
                }
                CancelWindup();
                nextAttackAt = Time.time + attackCooldown;
            }
            return;
        }
        if (currentState == State.Attack)
        {
            CancelWindup();
            nextAttackAt = Mathf.Max(nextAttackAt, Time.time + (stats ? stats.recoveryTime : .2f));
            currentState = State.Chase;
        }
        if (currentState == State.Chase)
        {
            Vector2 difference = lastSeenPosition - body.position;
            body.linearVelocity = difference.sqrMagnitude <= stopDistance * stopDistance
                ? Vector2.zero : difference.normalized * chaseSpeed;
            return;
        }
        Patrol();
    }

    public bool CanSeePlayer()
    {
        if (!player || !playerCollider || !playerCollider.enabled || !player.gameObject.activeInHierarchy ||
            (playerHealth != null && !playerHealth.IsAlive))
            return false;
        Vector2 origin = ownCollider ? ownCollider.bounds.center : transform.position;
        Vector2 target = playerCollider.bounds.center;
        Vector2 difference = target - origin;
        float distance = difference.magnitude;
        if (distance > detectionDistance) return false;
        if (distance < .001f) return true;

        var filter = new ContactFilter2D();
        filter.SetLayerMask(sightMask);
        filter.useTriggers = false;
        int count = Physics2D.Raycast(origin, difference / distance, filter, sightHits, distance + .05f);
        if (count == sightHits.Length) return false;
        Collider2D nearest = null;
        float nearestDistance = float.PositiveInfinity;
        for (int i = 0; i < count; i++)
        {
            var hit = sightHits[i];
            if (hit.collider.attachedRigidbody == body || hit.collider.transform.IsChildOf(transform)) continue;
            if (hit.distance < nearestDistance)
            {
                nearestDistance = hit.distance;
                nearest = hit.collider;
            }
        }
        Debug.DrawLine(origin, target, nearest == playerCollider ? Color.red : Color.gray);
        return nearest != null && (nearest == playerCollider || nearest.transform.IsChildOf(player));
    }

    private void ReturnToPatrol()
    {
        CancelWindup();
        currentState = State.Patrol;
        timeWithoutSight = 0f;
        waitRemaining = 0f;
        if (patrolPoints == null) return;
        float nearest = float.PositiveInfinity;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (!patrolPoints[i]) continue;
            float distance = ((Vector2)patrolPoints[i].position - body.position).sqrMagnitude;
            if (distance < nearest) { nearest = distance; pointIndex = i; }
        }
    }

    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        pointIndex %= patrolPoints.Length;
        Transform target = patrolPoints[pointIndex];
        if (target == null)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        if (waitRemaining > 0f)
        {
            waitRemaining -= Time.fixedDeltaTime;
            body.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 difference = (Vector2)target.position - body.position;
        if (difference.sqrMagnitude <= arrivalDistance * arrivalDistance)
        {
            body.linearVelocity = Vector2.zero;
            pointIndex = (pointIndex + 1) % patrolPoints.Length;
            waitRemaining = waitTime;
            return;
        }

        float speed = Mathf.Min(patrolSpeed, difference.magnitude / Time.fixedDeltaTime);
        body.linearVelocity = difference.normalized * speed;
    }

    private void OnDisable()
    {
        CancelWindup();
        if (body != null) body.linearVelocity = Vector2.zero;
    }

    private void CancelWindup()
    {
        windingUp = false;
        if (sprite) sprite.color = originalColor;
    }

    private void OnDrawGizmosSelected()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            Transform a = patrolPoints[i];
            Transform b = patrolPoints[(i + 1) % patrolPoints.Length];
            if (a == null) continue;
            Gizmos.DrawWireSphere(a.position, arrivalDistance);
            if (b != null) Gizmos.DrawLine(a.position, b.position);
        }
    }
}
