using UnityEngine;

public class EnemyAI : MonoBehaviour
{
// ranges stats
    public float chaseRange = 3.5f;
    public float chargeRange = 9f;
    public float loseRange = 14f;

    // movement stats
    public float moveSpeed = 3f;
    public float rotationSpeed = 500f;

    // charge attack stats
    public float windupTime = 1f;     
    public float chargeSpeed = 14f;   
    public float chargeTime = 4f;      
    public float recoveryTime = 0.8f;  
    // combat settings
    public float attackCooldown = 1.5f;
    public int contactDamage = 1;

    public bool IsCharging => state == State.Charge;

 //references 
    private Animator anim;
    private Rigidbody2D rb;
    private Transform player;

    private float lastAttackTime;
    private Vector2 chargeDir;
    private bool chargeInterrupted = false; 
    private bool playerEnteredChaseRange = false; 

    private enum State
    {
        Idle,
        Chase,
        Windup,
        Charge,
        Recovery
    }

    private State state;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (!player) return;

        if (state == State.Windup || state == State.Charge || state == State.Recovery)
        {
            HandleAnimation();
            return;
        }

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer > loseRange)
        {
            playerEnteredChaseRange = false;
            state = State.Idle;
            HandleAnimation();
            return;
        }

        bool chargeReady = Time.time >= lastAttackTime + attackCooldown;

        if (distToPlayer <= chaseRange)
        {
            playerEnteredChaseRange = true;
            state = State.Chase;
        }
        else if (distToPlayer <= chargeRange)
        {
            if (playerEnteredChaseRange && chargeReady)
            {
                playerEnteredChaseRange = false;
                StartCoroutine(ChargeRoutine());
            }
            else
            {
                state = State.Idle;
            }
        }
        else
        {
            playerEnteredChaseRange = false;
            state = State.Idle;
        }

        HandleAnimation();
    }

    void FixedUpdate()
    {
        if (!player) return;

        Vector2 toPlayer = (player.position - transform.position).normalized;

        switch (state)
        {
            case State.Chase:
                rb.linearVelocity = toPlayer * moveSpeed;
                Rotate(toPlayer);
                break;

            case State.Windup:
                rb.linearVelocity = Vector2.zero;
                Rotate(chargeDir);
                break;

            case State.Charge:
                // straight line, like a pool ball or should I say pole ball.. haha.. get it? cuz its a polar bear and pool and pole and.. i should stfu
                rb.linearVelocity = chargeDir * chargeSpeed;
                break;

            case State.Idle:
            case State.Recovery:
                rb.linearVelocity = Vector2.zero;
                break;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (state != State.Charge) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Health playerHealth = collision.gameObject.GetComponent<Health>();

            if (playerHealth != null)
                playerHealth.TakeDamage(contactDamage);

            CameraShake.Instance?.ShakeCamera(2f);
        }

        chargeInterrupted = true;
        rb.linearVelocity = Vector2.zero;
    }

    void HandleAnimation()
    {
        if (!anim) return;

        anim.SetBool("IsIdle", state == State.Idle);
        anim.SetBool("IsRunning", state == State.Chase);
        anim.SetBool("IsAttacking", state == State.Charge || state == State.Windup);
    }

    System.Collections.IEnumerator ChargeRoutine()
    {
        if (state == State.Windup || state == State.Charge)
            yield break;

        chargeDir = (player.position - transform.position).normalized;
        chargeInterrupted = false;

        state = State.Windup;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(windupTime);

        state = State.Charge;
        lastAttackTime = Time.time;

        float t = 0f;
        while (t < chargeTime && !chargeInterrupted)
        {
            t += Time.deltaTime;
            yield return null;
        }

        state = State.Recovery;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(recoveryTime);

        state = State.Idle;
    }

    void Rotate(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.01f) return;

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        float newAngle = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, rotationSpeed * Time.fixedDeltaTime);

        rb.MoveRotation(newAngle);
    }
}