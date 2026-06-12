using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerControls : MonoBehaviour
{
    // haha pengi go brrr
    public float acceleration = 25f;
    public float maxSpeed = 8f;
    public float deceleration = 18f;

// when I was a young kid I always wanted to slide but I could never figure out how to do it in a way that felt good, so now I'm doing it in my game and it's really fun (Vs code comments autoc complete said that and its funny so I am leaving it also yes you have to read the whole line to get the joke, also I like latinas call me if you found one my phone number is 202-456-1111  just dont rickroll me please thank you whoever noone is reading this xoxo)
    public float slideAcceleration = 40f;
    public float slideMaxSpeed = 14f;
    public float slideCooldown = 0.5f;
 // my gym teacher said the most important part of the training for building muscles is recovery so I thought it was gonna be a nice addition to my games because I am very obviously muscular and strong and I am sharing this because I am an inspiring figure in every community and I want to share my wisdom with everyone, also recovery is just a good way to balance the slide so you cant just spam it and feel like a god, you have to actually time it and use it strategically which is fun for the player and makes the game more engaging, also I am very humble and not at all narcissistic (vs code completed this one too and its really funny and creepy how its copying me)
    public float recoveryDuration = 1f;
    public float recoverySpeedMultiplier = 0.6f;

    // I love rotating like helicopter maybe when I grow up I will be a helicopter
    public float normalRotationSpeed = 700f;
    public float slideRotationSpeed = 180f;

 // This is supposed to be the combat stats I guess
    public float knockbackForce = 8f;
    public int slideDamage = 1;
    public float hitStunTime = 0.2f;

 // Comonents references are so epic fr 
    private Rigidbody2D rb;
    private Animator anim;
  // actions in inputs are inputs in action
    private Vector2 input;

    private InputAction moveAction;
    private InputAction slideAction;
 // Haha player states go brr
    private enum MoveState
    {
        Normal,
        Slide,
        SlideRecovery
    }
// default settings
    private MoveState state = MoveState.Normal;
    private bool canSlide = true;
    private bool isKnockedBack;
    private bool isRecovering = false; //added so SlideRecovery doesn't run twice at the same time also because I am smart

    void Awake()
    {
        //reference all the components and input actions
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();

        var playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Move"];
        slideAction = playerInput.actions["Jump"];
    }

    void OnEnable()
    { 
        moveAction.Enable();
        slideAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        slideAction.Disable();
    }

    void Update()
    {
        input = moveAction.ReadValue<Vector2>();

        HandleSlide();
        HandleAnimation();
    }
// physics in fixed update because physics makes dani go yes
    void FixedUpdate()
    {
        Move();
        RotatePlayer();
    }

    void Move()
    {
        if (isKnockedBack) return;

        Vector2 velocity = rb.linearVelocity;

        float accel = acceleration;
        float max = maxSpeed;

        if (state == MoveState.Slide)
        {
            accel = slideAcceleration;
            max = slideMaxSpeed;
        }
        else if (state == MoveState.SlideRecovery)
        {
            // slow the player down during recovery so they cant just spam slide like a maniac and feel like a god, they have to actually time it and use it strategically which is fun for the player and makes the game more engaging, also I am very humble and not at all narcissistic (vs code completed this one too and its really funny and creepy how it's copying me (EVEN THIS PART OMG WTF IS GOING ON))
            accel *= recoverySpeedMultiplier;
            max *= recoverySpeedMultiplier;
        }

        Vector2 direction;

        // during a slide keep moving in the direction the player is facing 
        if (state == MoveState.Slide || state == MoveState.SlideRecovery)
            direction = GetFacingDirection();
        else
            direction = input.normalized;

        if (input.sqrMagnitude > 0.01f)
        {
            velocity += direction * accel * Time.fixedDeltaTime;

            if (velocity.magnitude > max)
                velocity = velocity.normalized * max;
        }
        else if (state == MoveState.Normal)
        {
            // decelerate when no input is held
            velocity = Vector2.Lerp(velocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
        }

        rb.linearVelocity = velocity;
    }

    void HandleSlide()
    {
        if (!canSlide) return;

        bool held = slideAction.IsPressed();

        if (held)
        {
            // only start the slide if we are already moving
            if (state == MoveState.Normal && rb.linearVelocity.magnitude > 0.5f)
            {
                state = MoveState.Slide;
            }
        }
        else
        {
            //on this part wakatime told me I am on fire, I know I am hot thank you wakatime but please stop telling me that every time I stop sliding it is very distracting and I am trying to be humble here
            if (state == MoveState.Slide)
            {
                StartCoroutine(SlideRecovery());
            }
        }
    }

    IEnumerator SlideRecovery()
    {
        if (isRecovering) yield break;

        isRecovering = true;
        state = MoveState.SlideRecovery;

        yield return new WaitForSeconds(recoveryDuration);

        state = MoveState.Normal;
        isRecovering = false;

        StartCoroutine(SlideCooldown());
    }

    IEnumerator SlideCooldown()
    {
        canSlide = false;
        yield return new WaitForSeconds(slideCooldown);
        canSlide = true;
    }

    // not currently used but keeping it just in case 
    public void StopSlide()
    {
        state = MoveState.Normal;
        rb.linearVelocity *= 0.2f;
    }

    void RotatePlayer()
    {
        if (input.sqrMagnitude < 0.01f) return;

        float targetAngle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg - 90f;

        float speed = (state == MoveState.Slide) ? slideRotationSpeed : normalRotationSpeed;

        if (state == MoveState.SlideRecovery)
            speed *= 0.4f;

        float angle = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, speed * Time.fixedDeltaTime);

        rb.MoveRotation(angle);
    }

    IEnumerator Knockback(Vector2 force)
    {
        isKnockedBack = true;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(hitStunTime);

        isKnockedBack = false;
    }

    Vector2 GetFacingDirection()
    {
        float angle = (rb.rotation + 90f) * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (state != MoveState.Slide)
            return;

        // check if this is a charging enemy if so then the slide can't deflect a charge
        EnemyAI enemyAI = collision.gameObject.GetComponent<EnemyAI>();
        if (enemyAI != null && enemyAI.IsCharging)
        {
            // the charge hits the player no matter what(even mid-slide)
            Vector2 bounceDir = (transform.position - collision.transform.position).normalized;
            StartCoroutine(Knockback(bounceDir * knockbackForce * 1.5f));
            StartCoroutine(SlideRecovery());
            CameraShake.Instance?.ShakeCamera(2.5f);
            return;
        }

        Health enemy = collision.gameObject.GetComponent<Health>();

        Vector2 hitDir = (collision.transform.position - transform.position).normalized;
        Vector2 knockDir = (hitDir + Vector2.up * 0.2f).normalized;

        if (enemy != null)
        {
            enemy.TakeDamage(slideDamage);

            Rigidbody2D enemyRb = collision.rigidbody;

            if (enemyRb != null)
            {
                enemyRb.linearVelocity = Vector2.zero;
                enemyRb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
            }

            // bounce the player back from the hit 
            StartCoroutine(Knockback(-knockDir * knockbackForce));
        }

        // trigger recovery whether we hit an enemy or just a wall
        StartCoroutine(SlideRecovery());

        CameraShake.Instance?.ShakeCamera(2f);
    }

    void HandleAnimation()
    {
        float speed = rb.linearVelocity.magnitude / maxSpeed;

        anim.SetFloat("Speed", speed);
        anim.SetBool("IsSliding", state == MoveState.Slide);

        anim.speed = Mathf.Clamp(speed, 0.5f, 2f);
    }
}