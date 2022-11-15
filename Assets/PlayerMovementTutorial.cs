using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System;

public class PlayerMovementTutorial : MonoBehaviourPunCallbacks
{
    [Header("Movement")]
    public float        moveSpeed;
    public float        groundDrag;
    public Transform    orientation;

    [HideInInspector] public float walkSpeed;
    [HideInInspector] public float sprintSpeed;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode attackKey = KeyCode.Mouse0;
    public KeyCode DashKey = KeyCode.LeftShift;

    [Header("Jump")]
    public float    jumpForce;
    public float    jumpCooldown;
    public float    airMultiplier;
    bool            readyToJump;
    bool            jDown;

    [Header("Dash")]
    public float    dashSpeed;
    public float    dashForce;
    public float    dashUpwardForce;
    public float    maxDashYSpeed;
    public float    dashDuration;
    public float    dashFov;
    public float    dashCd;
    private float   dashCdTimer;
    public bool     useCameraForward = true;
    public bool     allowAllDirections = true;
    public bool     disableGravity = false;
    public bool     resetVel = true;
    bool            dDown;
    bool            Dashing = false;

    [Header("Ground Check")]
    public float        playerHeight;
    public LayerMask    whatIsGround;
    bool                grounded;

    /*[Header("Stair Check")]
    [SerializeField] GameObject stepRayUpper;
    [SerializeField] GameObject stepRayLower;
    [SerializeField] float      stepHeight = 0.3f;
    [SerializeField] float      stepSmooth = 2f;*/

    [Header("Attack")]
    public GameObject   Attack_Script;
    bool                fDown;
    bool                isFireReady=true;
    float               fireDelay;

    [Header("Health")]
    public int          currentHelth;
    public int          maxHp = 100;
    public GameObject   HpCanvas;
    bool                onDamaged = false;

    private Animator    anima;
    float               horizontalInput;
    float               verticalInput;
    Vector3             moveDirection;
    Rigidbody           rb;
    Material            mat;
    Color               matColor;
    public PhotonView   PV;

    // IPunObservable
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(currentHelth);
            stream.SendNext(Game_Manager._IsDead);
        }
        else
        {
            // Network player, receive data
            this.currentHelth = (int)stream.ReceiveNext();
            Game_Manager._IsDead = (bool)stream.ReceiveNext();
        }
    }

    private void Start()
    {
        anima = GetComponentInChildren<Animator>();
        PV = GetComponentInParent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
        currentHelth = maxHp;
        mat = GetComponentInChildren<MeshRenderer>().material;
        matColor = mat.color;

        if (!this.PV.IsMine)
        {
            Destroy(HpCanvas);
        }

    }

    private void Update()
    {
        if (PV.IsMine)
        {
            Dead();
            // ground check
            grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);
            MyInput();
            SpeedControl();
            if (dDown && grounded)
            {
                Dash();
            }
        
            doAttack();
            Jump();

            // handle drag
            if (grounded || !Dashing)
            {
                rb.drag = groundDrag;
                moveSpeed = 5;
            }
            else
            {
                if (Dashing)
                    moveSpeed = dashSpeed;    
                rb.drag = 0;
            }
            //dashCoolDown
            if (dashCdTimer > 0)
                dashCdTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        if (!isFireReady || onDamaged)
        {
            verticalInput = 0;
            horizontalInput = 0;
        }
        else
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            verticalInput = Input.GetAxisRaw("Vertical");
        }
        jDown = Input.GetKey(jumpKey);
        dDown = Input.GetKey(DashKey);
        fDown = Input.GetKey(attackKey);
        PlayerAnima();
        
    }

    private void MovePlayer()
    {

        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        // on ground
        if (grounded )
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity if needed
        if(flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        if (jDown && readyToJump && grounded)
        {
            Debug.Log("jump");
            readyToJump = false;

            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

            Invoke(nameof(ResetJump), jumpCooldown);
        }

    }
    private void ResetJump()
    {
        readyToJump = true;
    }

    //제대로 작동 안됨
    private void Dash()
    {
            if (dashCdTimer > 0) return;
            else dashCdTimer = dashCd;

            Dashing = true;
            Transform forwardT;
            forwardT = orientation;             /// where you're facing (no up or down)
            Vector3 direction = GetDirection(forwardT);
            Vector3 forceToApply = direction * dashForce + orientation.up * dashUpwardForce;

            if (disableGravity)
                rb.useGravity = false;

            delayedForceToApply = forceToApply;
            Invoke(nameof(DelayedDashForce), 0.025f);

            Invoke(nameof(ResetDash), dashDuration);
    }

    private Vector3 delayedForceToApply;
    private void DelayedDashForce()
    {
        if (resetVel)
            rb.velocity = Vector3.zero;

        rb.AddForce(delayedForceToApply, ForceMode.Impulse);
    }

    private void ResetDash()
    {
        Dashing = false;

        if (disableGravity)
            rb.useGravity = true;
    }

    private Vector3 GetDirection(Transform forwardT)
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3();

        if (allowAllDirections)
            direction = forwardT.forward * verticalInput + forwardT.right * horizontalInput;
        else
            direction = forwardT.forward;
        if (verticalInput == 0 && horizontalInput == 0)
            direction = forwardT.forward;
        return direction.normalized;
    }

    //attack
    void doAttack()
    {
        //check Attack Cooldown
        fireDelay += Time.deltaTime;
        isFireReady = fireDelay > Attack_Script.GetComponent<Player_Attack>().rate;
        //do Attack
        if(fDown && isFireReady)
        {
            Attack_Script.GetComponent<Player_Attack>().Attack();
            anima.SetBool("isAttack",true);
            fireDelay = 0;
        }
    }


    //Collider check
    void OnTriggerEnter(Collider coll)
    {
        if (coll.tag == "melee")
        {
            if (!PV.IsMine)
            {
                Debug.Log($"OnTriggerEnter: {coll.gameObject.name}");
                int _damage = Attack_Script.GetComponent<Player_Attack>().Damage;
                photonView.RPC("TakeHitRPC", RpcTarget.All, _damage);
            }
        }
    }

    [PunRPC]
    public void TakeHitRPC(int _damage)
    {
        currentHelth -= _damage;
        StartCoroutine(DamageAnim());
       
    }

    public IEnumerator DamageAnim()
    {
        Debug.Log("OnDamaged");
        onDamaged = true;
        anima.SetBool("isDamaged", true);
        yield return new WaitForSeconds(0.3f);
        onDamaged = false;
        anima.SetBool("isDamaged", false);
        yield break;
    }


    public void Dead()
    {
        if (currentHelth <= 0)
            Game_Manager._IsDead = true;
    }

    /**player Animation**/
    private void PlayerAnima()
    {
        if (verticalInput >= 1)
        {
            anima.SetBool("isWalking_F", true);
            anima.SetBool("isWalking_B", false);
            anima.SetBool("isWalking_R", false);
            anima.SetBool("isWalking_L", false);
        }
        else if (verticalInput <= -1)
        {
            anima.SetBool("isWalking_F", false);
            anima.SetBool("isWalking_B", true);
            anima.SetBool("isWalking_R", false);
            anima.SetBool("isWalking_L", false);
        }
        else if (horizontalInput >= 1)
        {
            anima.SetBool("isWalking_R", true);
            anima.SetBool("isWalking_L", false);
            anima.SetBool("isWalking_F", false);
            anima.SetBool("isWalking_B", false);
        }
        else if (horizontalInput <= -1)
        {
            anima.SetBool("isWalking_R", false);
            anima.SetBool("isWalking_L", true);
            anima.SetBool("isWalking_F", false);
            anima.SetBool("isWalking_B", false);
        }
        else
        {
            anima.SetBool("isWalking_R", false);
            anima.SetBool("isWalking_L", false);
            anima.SetBool("isWalking_F", false);
            anima.SetBool("isWalking_B", false);
        }
    }


    //실험중인 코드
/*    private void stepClimb()
    {
        RaycastHit hitLower;
        if (Physics.Raycast(stepRayLower.transform.position, transform.TransformDirection(Vector3.forward), out hitLower, 0.1f))
        {
            RaycastHit hitUpper;
            if (!Physics.Raycast(stepRayUpper.transform.position, transform.TransformDirection(Vector3.forward), out hitUpper, 0.2f))
            {
                rb.position -= new Vector3(0f, -stepSmooth * Time.deltaTime, 0f);
            }
        }

        RaycastHit hitLower45;
        if (Physics.Raycast(stepRayLower.transform.position, transform.TransformDirection(1.5f, 0, 1), out hitLower45, 0.1f))
        {

            RaycastHit hitUpper45;
            if (!Physics.Raycast(stepRayUpper.transform.position, transform.TransformDirection(1.5f, 0, 1), out hitUpper45, 0.2f))
            {
                rb.position -= new Vector3(0f, -stepSmooth * Time.deltaTime, 0f);
            }
        }

        RaycastHit hitLowerMinus45;
        if (Physics.Raycast(stepRayLower.transform.position, transform.TransformDirection(-1.5f, 0, 1), out hitLowerMinus45, 0.1f))
        {

            RaycastHit hitUpperMinus45;
            if (!Physics.Raycast(stepRayUpper.transform.position, transform.TransformDirection(-1.5f, 0, 1), out hitUpperMinus45, 0.2f))
            {
                rb.position -= new Vector3(0f, -stepSmooth * Time.deltaTime, 0f);
            }
        }
    }*/
}