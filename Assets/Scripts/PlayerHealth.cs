using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class PlayerHealth : MonoBehaviour
{
    public TumanGame.PlayerStats stats;
    [SerializeField, Min(1)] private int maxHealth;
    [SerializeField, Min(0f)] private float invulnerabilityTime;
    [SerializeField] private bool showPrototypeHUD = true;
    private int health;
    private float invulnerableUntil;
    private SpriteRenderer sprite;
    private Color originalColor;
    public int CurrentHealth => health;
    public int MaxHealth => stats ? stats.maxHealth : maxHealth;
    public bool IsAlive => health > 0;

    private void Awake()
    {
        health = MaxHealth;
        sprite = GetComponent<SpriteRenderer>();
        if (sprite) originalColor = sprite.color;
    }

    public bool TakeDamage(int amount)
    {
        if (!IsAlive || amount <= 0 || Time.time < invulnerableUntil) return false;
        health = Mathf.Max(0, health - amount);
        TumanGame.GameAudio.Instance?.Play(IsAlive?TumanGame.SoundEvent.Hit:TumanGame.SoundEvent.Death);
        invulnerableUntil = Time.time + (stats ? stats.invulnerabilityTime : invulnerabilityTime);
        if (!IsAlive)
        {
            var movement = GetComponent<PlayerMovement>();
            if (movement) movement.enabled = false;
            var body = GetComponent<Rigidbody2D>();
            if (body) body.linearVelocity = Vector2.zero;
        }
        return true;
    }

    private void Update()
    {
        if (sprite) sprite.color = !IsAlive ? Color.gray :
            Time.time < invulnerableUntil ? Color.red : originalColor;
    }

    private void OnDisable() { if (sprite) sprite.color = originalColor; }

    private void OnGUI()
    {
        if(!showPrototypeHUD)return;
        // Temporary prototype HUD; gameplay health is independent of presentation.
        Matrix4x4 previous = GUI.matrix;
        float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
        GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
        var style = new GUIStyle(GUI.skin.box) { fontSize = 28, alignment = TextAnchor.MiddleCenter };
        GUI.Box(new Rect(28, 28, 230, 70), $"HP: {health} / {maxHealth}", style);
        if (!IsAlive)
        {
            float x = Screen.width / scale / 2 - 170;
            float y = Screen.height / scale / 2 - 80;
            GUI.Box(new Rect(x, y, 340, 80), "YOU DIED", style);
            if (GUI.Button(new Rect(x, y + 90, 340, 90), "RESTART", style))
                RestartLevel();
        }
        GUI.matrix = previous;
    }

    public void Restore()
    {
        health = MaxHealth; invulnerableUntil = 0;
        var movement = GetComponent<PlayerMovement>(); if(movement) movement.enabled = true;
        transform.rotation = Quaternion.identity;
        if(sprite) sprite.color = originalColor;
    }
    public void Heal(int amount) { if(IsAlive && amount > 0) health = Mathf.Min(MaxHealth, health + amount); }
    public void RestartLevel()
    {
        if(TumanGame.GameSession.Instance) TumanGame.GameSession.Instance.Retry();
        else SceneManager.LoadScene(SceneManager.GetActiveScene().path);
    }
}
