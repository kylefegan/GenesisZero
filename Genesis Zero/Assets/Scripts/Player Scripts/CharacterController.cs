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
    public float acceleration = 20f;
    public float jumpStrength = 13f;
    public float doubleJumpStrength = 10f;
    public float distToGround = 0.5f; //dist from body origin to ground
    public float bodyRadius = 0.5f; //radius of the spherecast for IsGrounded
    
    public LayerMask ground;

    [Header("Physics")]
    public float gravity = 14f;
    public float terminalVel = 16;
    public float fallSpeedMult = 1.6f;
    public float airControlMult = 0.5f;

    [Header("Camera")]
    public Camera mainCam;

    [Header("Gun")]
    public GameObject gun;
    public GameObject crosshair;
    public float sensitivity;

    private PlayerInputActions inputActions;
    private Rigidbody rb;
    private Vector2 movementInput;
    private Vector2 aimInputMouse;
    private Vector2 aimInputController;
    private Vector3 moveVec = Vector3.zero;
    private Vector3 aimVec = Vector3.zero;

    private float vertVel;
    private float currentSpeed = 0;
    private bool canDoubleJump = true;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputActions = new PlayerInputActions();
        inputActions.PlayerControls.Move.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        inputActions.PlayerControls.AimMouse.performed += ctx => aimInputMouse = ctx.ReadValue<Vector2>();
        inputActions.PlayerControls.AimController.performed += ctx => aimInputController = ctx.ReadValue<Vector2>();
    }

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Aim();
        Move();
        ApplyGravity();
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
        if (!IsGrounded())
        {
            //check if character is stuck to ceiling and zero the speed so it can start falling
            if (rb.velocity.y == 0)
                vertVel = 0;
            // multiplier to make character fall faster on the way down
            var fallMult = rb.velocity.y < 0 ? fallSpeedMult : 1;
            vertVel -= gravity * fallMult * Time.fixedDeltaTime;
            Debug.Log(rb.velocity.y);
            //lock falling speed at terminal velocity
            if (vertVel < 0)
                vertVel = Mathf.Max(vertVel, -terminalVel);
        }
        else
        {
            if (vertVel < 0)
                vertVel = 0;
        }
        //Debug.Log(movementInput);
        float multiplier = IsGrounded() ? 1 : airControlMult;
        //Debug.Log(multiplier);
        if (movementInput.x != 0)
        {
            // this is to deal with left stick returning floats
            var input = movementInput.x < 0 ? Mathf.Floor(movementInput.x) : Mathf.Ceil(movementInput.x);
            currentSpeed += input * multiplier * acceleration * Time.fixedDeltaTime;
            currentSpeed = Mathf.Min(Mathf.Abs(currentSpeed), Mathf.Abs(GetComponent<Player>().GetSpeed().GetValue())) * input;
        }

        if (movementInput.x == 0)
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
        moveVec.x = currentSpeed;
        moveVec.y = vertVel;
        rb.velocity = transform.TransformDirection(moveVec);
    }

    /* This checks if the character is currently on the ground
     * LayerMask named ground controls what surfaces 
     * group the player can jump on
     */
    public bool IsGrounded()
    {
        RaycastHit hit;
        //bool isGrounded = Physics.BoxCast(transform.position, new Vector3(bodyRadius, 0, 0), Vector3.down, out hit, Quaternion.identity, distToGround, ground, QueryTriggerInteraction.UseGlobal);
        bool isGrounded = Physics.SphereCast(transform.position, bodyRadius, Vector3.down, out hit, distToGround, ground, QueryTriggerInteraction.UseGlobal);
        if (isGrounded != false && hit.collider.isTrigger)
            isGrounded = false;
        if (isGrounded && canDoubleJump != true)
            canDoubleJump = true;
        return isGrounded;
    }

    /* This function is called with an event
     * invoked when player press jump button
     */
    public void Jump()
    {
        //Debug.Log(IsGrounded());
        if (IsGrounded())
        {
            vertVel = jumpStrength;
        }
        //Debug.Log(canDoubleJump);
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
        if (tmpAngle != 0)
            gun.transform.localRotation = Quaternion.Euler(0, 0, tmpAngle);
    }

    /* This function checks if the model is blocked
     * in a certain direction
     */
    private bool IsBlocked(Vector3 dir)
    {
        bool isBlock = false;

        return isBlock;
    }

    private void ApplyGravity()
    {
        
    }
}
