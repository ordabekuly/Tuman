using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyHealth : MonoBehaviour
{
    public TumanGame.EnemyStats stats;
    public bool CountsForProgress;
    [SerializeField, Min(1)] private int maxHealth;
    public int CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0;

    private void Awake() => CurrentHealth = stats ? stats.maxHealth : maxHealth;

    public void TakeDamage(int damage)
    {
        if (!IsAlive || damage <= 0) return;
        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        if (IsAlive) return;
        var ai = GetComponent<EnemyAI>();
        if (ai) ai.enabled = false;
        var body = GetComponent<Rigidbody2D>();
        if (body) body.linearVelocity = Vector2.zero;
        foreach (var collider in GetComponentsInChildren<Collider2D>()) collider.enabled = false;
        if(CountsForProgress && TumanGame.GameSession.Instance) TumanGame.GameSession.Instance.EnemyDefeated(this);
        var anim = GetComponent<TumanGame.EnemyMotion>();
        if(anim) { anim.Die(); return; }
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
