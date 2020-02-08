﻿/* CharacterController class deals with general input processing for
 * movements, aiming, shooting.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class CharacterController : MonoBehaviour
{
    [Header("Movement")]
    public float acceleration = 35f;
    public float jumpStrength = 12f;
    public float doubleJumpStrength = 10f;
    public float bodyHeight = 2f;
    public float bodyHeightOffset = 0.15f; // offset used for the sides casts
    public float bodyWidth = 0.5f;
    public float bodyWidthOffset = 0.05f; // offset used for the feet/head casts
    public float rollDistance = 5f;
    public float rollSpeedMult = 1.5f;
    public LayerMask immoveables; //LayerMask for bound checks

    [Header("Physics")]
    public float gravity = 18f;
    public float terminalVel = 15;
    public float fallSpeedMult = 1.45f;
    public float airControlMult = 0.5f;

    [Header("Camera")]
    public Camera mainCam;

    [Header("Gun")]
    public GameObject gunObject;
    public GameObject crosshair;
    public float sensitivity;

    private PlayerInputActions inputActions;
    private Rigidbody rb;
    private Vector2 movementInput;
    private Vector2 aimInputMouse;
    private Vector2 aimInputController;
    private Vector3 moveVec = Vector3.zero;
    private Vector3 aimVec = Vector3.zero;
    private float maxSpeed;
    private float vertVel;
    private float currentSpeed = 0;
    private bool canDoubleJump = true;
    private float distanceRolled = 0;
    private int rollDirection;
    private Animator animator;
    private Gun gun;
    private bool isGrounded = false;
    private bool isRolling = false;
    private bool isLookingRight = true;
    private bool gunFired = false;
    private Vector3 lastRollingPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputActions = new PlayerInputActions();
        inputActions.PlayerControls.Move.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        inputActions.PlayerControls.AimMouse.performed += ctx => aimInputMouse = ctx.ReadValue<Vector2>();
        inputActions.PlayerControls.AimController.performed += ctx => aimInputController = ctx.ReadValue<Vector2>();
        animator = GetComponent<Animator>();
        gun = GetComponent<Gun>();
    }

    private void Start()
    {
        maxSpeed = GetComponent<Player>().GetSpeed().GetValue();
    }

    private void Update()
    {
        AnimStateUpdate();
        //LogDebug();
    }

    private void FixedUpdate()
    {
        ApplyGravity();
        Aim();
        UpdateDodgeRoll();
        Move();
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    /* This controls the character general movements
     * It updates the movement vector every frame then apply
     * it based on the input
     */
    private void Move()
    {
        float multiplier = IsGrounded() ? 1 : airControlMult;
        // this is to deal with left stick returning floats
        var input = movementInput.x < 0 ? Mathf.Floor(movementInput.x) : Mathf.Ceil(movementInput.x);
        // grabbing maxSpeed from player class
        maxSpeed = GetComponent<Player>().GetSpeed().GetValue();
        // if the state is not rolling
        if (!isRolling)
        {
            if (movementInput.x < 0)
            {
                if (!IsBlocked(Vector3.left))
                {
                    currentSpeed += input * multiplier * acceleration * Time.fixedDeltaTime;
                    currentSpeed = Mathf.Max(currentSpeed, -maxSpeed);
                }
                else
                {
                    currentSpeed = 0;
                }   
            }
            else if (movementInput.x > 0)
            {
                if (input > 0 && !IsBlocked(Vector3.right))
                {
                    currentSpeed += input * multiplier * acceleration * Time.fixedDeltaTime;
                    currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
                }
                else
                {
                    currentSpeed = 0;
                }
            }
            else
            {
                if (currentSpeed > 0)
                {
                    currentSpeed -= acceleration * Time.fixedDeltaTime;
                    currentSpeed = Mathf.Max(currentSpeed, 0);
                }
                else if (currentSpeed < 0)
                {
                    currentSpeed += acceleration * Time.fixedDeltaTime;
                    currentSpeed = Mathf.Min(currentSpeed, 0);
                }
            }
        }
        moveVec.x = currentSpeed;
        moveVec.y = vertVel;
        rb.velocity = transform.TransformDirection(moveVec);
    }

    /* This function is invoked when player press 
     * roll button for dodgerolling. 
     * It will put the player into rolling state
     */
    public void DodgeRoll()
    {
        isRolling = true;
        lastRollingPosition = transform.position;
        if (movementInput.x != 0)
            rollDirection = movementInput.x > 0 ? 1 : -1;
        else
            rollDirection =  isLookingRight ? 1 : -1;
        //shrink 
    }

    /* This function keeps track of rolling state
     * and make player exit rolling state if necessary
     */
    private void UpdateDodgeRoll()
    {
        if (isRolling)
        {
            if (distanceRolled < rollDistance)
            {
                currentSpeed = rollDirection * maxSpeed * rollSpeedMult;
                distanceRolled += Vector3.Distance(transform.position, lastRollingPosition);
            }
            else
            {
                isRolling = false;
                //resets speed only when it's on the ground
                // this is to stop it from jitter in mid air
                currentSpeed = IsGrounded() ? 0 : currentSpeed;
                distanceRolled = 0;
            }
            lastRollingPosition = transform.position;
        }

        if ((moveVec.x > 0 && IsBlocked(Vector3.right)) || (moveVec.x < 0 && IsBlocked(Vector3.left)))
        {
            Debug.Log("Rolling is Blocked");
            isRolling = false;
            currentSpeed = 0;
            distanceRolled = 0;
        }
    }

    /* This checks if the character is currently on the ground
     * LayerMask named ground controls what surfaces 
     * group the player can jump on
     */
    public bool IsGrounded()
    {
        isGrounded = IsBlocked(Vector3.down);
        if (isGrounded && canDoubleJump != true)
            canDoubleJump = true;
        return isGrounded;
    }

    /* This function is called with an event invoked
     * when player press jump button to make player jump
     */
    public void Jump()
    {
        if (IsGrounded())
        {
            vertVel = jumpStrength;
            canDoubleJump = true;
        }
        
        if (!IsGrounded() && canDoubleJump)
        {
            vertVel = doubleJumpStrength;
            canDoubleJump = false;
            if (moveVec.x > 0 && movementInput.x <= 0)
                moveVec.x = 0;
            if (moveVec.x < 0 && movementInput.x >= 0)
                moveVec.x = 0;
        }
    }

    /* This function control character aiming
     * Crosshair is moved using mouse/rightStick
     * Gun rotates to point at crosshair
     */
    private void Aim()
    {
        Vector3 pos = mainCam.WorldToScreenPoint(transform.position);
        aimVec.x = aimInputMouse.x - pos.x; 
        aimVec.y = aimInputMouse.y - pos.y;

        float tmpAngle = Mathf.Atan2(aimVec.y, aimVec.x) * Mathf.Rad2Deg;
        gunObject.transform.localRotation = Quaternion.Euler(0, 0, tmpAngle);
        if (tmpAngle < 89 && tmpAngle > -91)
            isLookingRight = true;
        else
            isLookingRight = false;
    }

    /* This function checks if the model is blocked
     * in a certain direction(argument == Vector3.up/Vector3.down.....)
     */
    private bool IsBlocked(Vector3 dir)
    {
        bool isBlock = false;
        RaycastHit hit;
        float radius;

        if (dir == Vector3.up)
        {
            radius = (bodyWidth / 2) + bodyWidthOffset;
            isBlock = Physics.SphereCast(transform.position, radius, Vector3.up, out hit, (bodyHeight / 2) - radius, immoveables, QueryTriggerInteraction.UseGlobal);
            if (isBlock && hit.collider.isTrigger)
                isBlock = false;
        }
        else if(dir == Vector3.down)
        {
            radius = (bodyWidth / 2) + bodyWidthOffset;
            isBlock = Physics.SphereCast(transform.position, radius, Vector3.down, out hit, (bodyHeight / 2) - radius, immoveables, QueryTriggerInteraction.UseGlobal);
            if (isBlock && hit.collider.isTrigger)
                isBlock = false;
        }
        else if(dir == Vector3.right)
        {
            isBlock = Physics.BoxCast(transform.position, new Vector3(0, (bodyHeight / 2) - bodyHeightOffset, 0), Vector3.right, out hit, Quaternion.identity, bodyWidth, immoveables, QueryTriggerInteraction.UseGlobal);
            if (isBlock && hit.collider.isTrigger || hit.normal != Vector3.left)
                isBlock = false;
        }
        else if(dir == Vector3.left)
        {
            isBlock = Physics.BoxCast(transform.position, new Vector3(0, (bodyHeight / 2) - bodyHeightOffset, 0), Vector3.left, out hit, Quaternion.identity, bodyWidth, immoveables, QueryTriggerInteraction.UseGlobal);
            if (isBlock && hit.collider.isTrigger || hit.normal != Vector3.right)
                isBlock = false;
        }
        else
        {
            Debug.LogError("IsBlocked() direction Vector is invalid, try Vector3.up, etc.");
        }
        return isBlock;
    }

    /* This fuction applies gravity to player
     * when the player's off the ground
     */
    private void ApplyGravity()
    {
        if (!IsGrounded())
        {
            //check if character is stuck to ceiling and zero the speed so it can start falling
            if (IsBlocked(Vector3.up))
                vertVel = -2;
            // multiplier to make character fall faster on the way down
            var fallMult = rb.velocity.y < 0 ? fallSpeedMult : 1;
            vertVel -= gravity * fallMult * Time.fixedDeltaTime;
            //lock falling speed at terminal velocity
            if (vertVel < 0)
                vertVel = Mathf.Max(vertVel, -terminalVel);
        }
        else
        {
            //resets vertVel when player's grounded
            if (vertVel < 0)
                vertVel = 0;
        }
    }

    /* This function updates information
     * about the player's state for animations
     */
    private void AnimStateUpdate()
    {
        var xSpeed = moveVec.x != 0 ? moveVec.x / maxSpeed : 0;
        var ySpeed = moveVec.y != 0 ? moveVec.y / Mathf.Abs(moveVec.y) : 0;
        gunFired = gun.getFired();
        animator.SetFloat("xSpeed", xSpeed);
        animator.SetFloat("ySpeed", ySpeed);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isRolling", isRolling);
        animator.SetBool("isLookingRight", isLookingRight);
        animator.SetBool("gunFired", gunFired);
    }


    private void LogDebug()
    {
        //Debug.Log(movementInput);
        //Debug.Log("blockedRight?:" + IsBlocked(Vector3.right));
        //Debug.Log("blockedLeft?:" + IsBlocked(Vector3.left));
        //Debug.Log(rb.velocity.y);
        //Debug.Log("lastRollingPosition: " + lastRollingPosition);
        //Debug.Log(IsGrounded());
        //Debug.Log(canDoubleJump);
        Debug.Log("distanceRolled: " + distanceRolled);
    }
}
