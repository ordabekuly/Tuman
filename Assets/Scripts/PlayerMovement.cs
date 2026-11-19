using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public TumanGame.PlayerStats stats;
    [SerializeField] private float speed;
    [SerializeField] private MobileJoystick joystick;

    private Rigidbody2D body;
    private Vector2 movement;
    public void SetJoystick(MobileJoystick target)=>joystick=target;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (TumanGame.GameSession.Instance && !TumanGame.GameSession.Instance.IsPlaying) { movement = Vector2.zero; return; }
        movement = Vector2.zero;
        Keyboard keyboard = Keyboard.current;

        if (keyboard != null)
        {
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            movement.x -= 1f;

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            movement.x += 1f;

        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            movement.y -= 1f;

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            movement.y += 1f;
        }
        movement = movement.sqrMagnitude > 0 ? movement.normalized : joystick ? joystick.Value : Vector2.zero;
    }

    private void FixedUpdate()
    {
        body.linearVelocity = movement * (stats ? stats.moveSpeed : speed);
    }

    private void OnDisable()
    {
        movement = Vector2.zero;

        if (body != null)
            body.linearVelocity = Vector2.zero;
    }
}
