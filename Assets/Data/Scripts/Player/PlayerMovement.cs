using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEngine.GraphicsBuffer;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement"), SerializeField]
                     internal float walkSpeed;
    [SerializeField] internal float dashSpeed, dashSpeedChangeFactor, groundDrag, moveSpeed, maxSpeed;

    [Header("Jumping"), SerializeField]
                     private float jumpForce; 
    [SerializeField] private float jumpCooldown, airMultiplier;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;

    [Header("Crouching")]
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
                     private float crouchModifier = 4;
                     private bool crouching = false;

    [Header("Ground Check"), SerializeField]
                     private float playerHeight;
    [SerializeField] private LayerMask whatIsGround;
    
    [SerializeField] private Transform orientation;
                     private MovementState state;


    [Header("Slope Handling")]
                     private float maxSlopeAngle;
                     private RaycastHit slopeHit;
                     private bool exitingSlope;

                     private bool readyToJump, grounded;
                     private float horizontalInput, verticalInput;
                     Vector3 moveDirection;
                     Rigidbody rb;
                     public bool dashing;

    [HideInInspector]public bool CanMove; 

                     private bool PlayerOnMove;
                     public bool _PlayerOnMove => PlayerOnMove;
                     private CompanionBehaviour _CompanionMovement;
                     private EnemyBehaviour enemyHealth;
                     
                     private RestartMenu restartMenu;
                     
                     private float speed;
                     
                     // player health
                     private const int _MaxHealth = 100;
                     
                     internal int _currentHealth;
                     public int CurretHealth => _currentHealth;

    [Header("Health bar")]
                     public HealthBar _healthBar;
    [HideInInspector]public bool HealthSetAtMax;
    [SerializeField] private int regenAmount;
    [SerializeField] private float regenTimer;
    [SerializeField] private bool regenOn;
    [SerializeField] private LayerMask aiLayer;

    [Header("Dash Explosion"), SerializeField]
                     private float explosionForce = 10f;
    [SerializeField] private float explosionDamage = 20f;  
                     public int shield = 0;




    private enum MovementState
    {
        walking,
        dashing,
        //for possible future implementation, but needs bugfixing
        //crouching,
        air
    }


    private void Start()
    {
        this.enabled = true;

        CanMove = true;

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;

        _CompanionMovement = FindObjectOfType<CompanionBehaviour>();
        restartMenu = FindObjectOfType<RestartMenu>();
        HealthSetAtMax = true;
        _currentHealth = 100;
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();
        CheatCheck();
        EnemiesAround();

        if (state == MovementState.walking /*|| state == MovementState.crouching*/)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }


    private void FixedUpdate()
    {
        if (regenOn == true)
        {
            Invoke("Regen", regenTimer);
        }

        MovePlayer();
        PlayerSpeed();     
    }

    private float desiredMoveSpeed, lastDesiredMoveSpeed;
    private MovementState lastState;
    private bool keepMomentum;
    private void StateHandler()
    {
        if (dashing)
        {
            state = MovementState.dashing;
            desiredMoveSpeed = dashSpeed;
            speedChangeFactor = dashSpeedChangeFactor;
        }

        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        /*else if (crouching)
        {
            state = MovementState.crouching;
            desiredMoveSpeed = walkSpeed - crouchModifier;
        }*/

        else
        {
            state = MovementState.air;

            if (desiredMoveSpeed < maxSpeed)
                desiredMoveSpeed = walkSpeed;
            else
                desiredMoveSpeed = maxSpeed;
        }

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;
        if(lastState == MovementState.dashing)   keepMomentum = true;

        if (desiredMoveSpeedHasChanged)
        {
            if(keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                StopAllCoroutines();
                moveSpeed = desiredMoveSpeed;
            }
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;

        

    }

    private float speedChangeFactor;
    private Vector3 lastPosition;

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        float boostFactor = speedChangeFactor;

        while(time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            time += Time.deltaTime * boostFactor;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
        speedChangeFactor = 1f;
        keepMomentum = false;
    }


    private void MyInput()
    {
        if(CanMove == true)
        {
          
            horizontalInput = Input.GetAxisRaw("Horizontal");
            verticalInput = Input.GetAxisRaw("Vertical");


            if (Input.GetKey(jumpKey) && readyToJump && grounded && !crouching)
            {
                readyToJump = false;

                Jump();

                Invoke(nameof(ResetJump), jumpCooldown);
            }

            if (Input.GetKeyDown(crouchKey) && grounded)
            {
                crouching = true;
            }
            else if (Input.GetKeyUp(crouchKey))
            {
                crouching = false;
            }
        }
        else if(CanMove == false)
        {
          
        }
        
    }

    private void MovePlayer()
    {
        if (state == MovementState.dashing) return;

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }


        if(state == MovementState.walking)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        /*
        if(state == MovementState.crouching)
            rb.AddForce(moveDirection.normalized * (moveSpeed - crouchModifier) * 10f, ForceMode.Force);
        else
            rb.AddForce(moveDirection.normalized * (moveSpeed - crouchModifier) * 10f * airMultiplier, ForceMode.Force);
        */
        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        if(OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }

        else
        { 
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if(flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);   
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }



    private void PlayerSpeed()
    {
        speed = (transform.position - lastPosition).magnitude / Time.deltaTime;
        lastPosition = transform.position;

        if(speed >= 5f)
        {
            _CompanionMovement._playerIsMoving = true;

        }
        else if (speed >= 12)
        {

            _CompanionMovement._playerIsMoving = true;
        }
        else
        {
            _CompanionMovement._playerIsMoving = false;
        }
    }

    void OnCollisionEnter(Collision other) 
    {
        if (other.gameObject.tag == "Enemy" && state == MovementState.dashing)
        {
            Rigidbody enemyRB = other.gameObject.GetComponent<Rigidbody>();
            Debug.Log("Dash damage");

            enemyHealth = other.gameObject.GetComponent<EnemyBehaviour>();
            
            //direction and distance of the push
            Vector3 pushDirection = other.transform.position - transform.position;
            float pushDistance = pushDirection.magnitude;
            pushDirection.y = 0;
            pushDirection.Normalize();

            Debug.Log("Push Direction: " + pushDirection);
            rb.AddForce(-pushDirection * explosionForce, ForceMode.Impulse);
            enemyRB.AddForce(pushDirection * explosionForce * 0.05f, ForceMode.Impulse);

            //explosion damage
            enemyHealth.TakeDamage((int)explosionDamage, WeaponType.Dash);
        }
    }

    public void Regen()
    {
        if ((_currentHealth < _MaxHealth))
        {
            GiveHealth(1);
            if (_currentHealth > _MaxHealth)
            {
                _currentHealth = _MaxHealth;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        CancelInvoke("Regen");
        regenOn = false;
        HealthSetAtMax = false;
        _currentHealth -= (damage - shield);

        _healthBar.SetHealth(_currentHealth);
        Debug.Log("Player Health: " + _currentHealth);

        if (_currentHealth <= 0)
        {
            Debug.Log("DEAD");
            //restartMenu.LoadRestart();
            SceneManager.LoadScene("RestartScene");
            //RestarMenu.SetActive(true);

            //Time.timeScale = 0f;
        }
    }

    private void EnemiesAround()
    {
        Collider[] aiHits = Physics.OverlapSphere(transform.position, 30f, aiLayer);
        // Collider[] hits = Physics.OverlapSphere(transform.position, AIradius);
        if (aiHits.Length == 0)
        {
            Debug.Log("Invoked Regen");
            Invoke("Regen", regenTimer);
        }
    }

    #region Cheats
    public void GiveHealth(int _health)
    {
        // Debug.Log("+ 15 health");
        _currentHealth += _health;

        Debug.Log("Player health is: " + _currentHealth);

        _healthBar.SetHealth(_currentHealth);

        if (_currentHealth >= _MaxHealth)
        {
            HealthSetAtMax = true;
            // how to variables equal?
            _currentHealth = 100;
            _healthBar.SetHealth(_currentHealth);
            Debug.Log("Player health: " + _currentHealth);

        }
    }

    private void CheatCheck()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            TakeDamage(20);
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            GiveHealth(20);
        }
    }
    #endregion

}