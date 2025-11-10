using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Player : ControllableCharacter
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float pullSpeed = 2f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Push / Pull")]
    [SerializeField] private float pushForce = 5f;
    [SerializeField] private float pullMinDistance = 1.5f;

    [Header("Pickup / Throw")]
    [SerializeField] private Transform holdPoint;
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float throwUpwardForce = 2f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Companion")]
    [SerializeField] private Follower companion;

    private CharacterController controller;
    private PlayerControls controls;

    private Vector3 velocity;
    private bool isGrounded;
    private bool isMoving;
    private bool isSprinting;
    private bool isPulling;
    private bool isPushing;
    private bool isHoldingObject;

    private Rigidbody pulledCube;
    private GameObject heldItem;

    private float minXBound = float.NegativeInfinity;
    private float maxXBound = float.PositiveInfinity;

    #region Update and Awake

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        controls = new PlayerControls();
        SetupControls();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void SetupControls()
    {
        controls.Player.Jump.performed += _ => Jump();
        controls.Player.Pull.performed += _ => PullStart();
        controls.Player.Pull.canceled += _ => PullStop();
        controls.Player.Interact.performed += _ => Interact();
        controls.Player.Throw.performed += _ => Throw();
        controls.Player.Whistle.performed += _ => Whistle();
    }

    private void Update()
    {
        if (!isActiveCharacter) return;

        HandleGravity();

        Vector2 moveInput = controls.Player.Move.ReadValue<Vector2>();
        isSprinting = controls.Player.Sprint.ReadValue<float>() > 0;

        Vector3 moveVector = new Vector3(moveInput.x, 0f, moveInput.y);

        Move(moveInput);
        HandleRotation(moveInput);

        HandlePushing();
        if (isPulling)
            HandlePulling(ref moveVector, ref pullSpeed);

        UpdateAnimations();
    }
    #endregion

    #region Movement / Gravity

    public override void Move(Vector2 input)
    {
        Vector3 move = new Vector3(input.x, 0, input.y);
        isMoving = move.sqrMagnitude > 0.01f;

        float speed = isSprinting ? sprintSpeed : walkSpeed;

        if (isPulling && pulledCube != null)
            HandlePulling(ref move, ref speed);

        Vector3 finalMove = move * speed * Time.deltaTime + velocity * Time.deltaTime;
        controller.Move(finalMove);

        Vector3 clampedPos = transform.position;
        clampedPos.x = Mathf.Clamp(clampedPos.x, minXBound, maxXBound);
        transform.position = clampedPos;
    }

    public override void Jump()
    {
        if (!isActiveCharacter || !isGrounded) return;
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        animator.SetTrigger("Jump");
    }

    private void HandleGravity()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
    }

    private void HandleRotation(Vector2 moveInput)
    {
        Vector3 dir = new Vector3(moveInput.x, 0, moveInput.y);
        if (isPulling && pulledCube != null)
        {
            Vector3 lookDir = pulledCube.position - transform.position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), 10f * Time.deltaTime);
        }
        else if (dir.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);
        }
    }

    #endregion

    #region Pull / Push

    public override void PullStart()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 2f))
        {
            if (hit.collider.CompareTag("Interactable") && hit.rigidbody != null && !hit.rigidbody.isKinematic)
            {
                pulledCube = hit.rigidbody;
                isPulling = true;
            }
        }
    }

    public override void PullStop()
    {
        isPulling = false;
        pulledCube = null;
    }

    private void HandlePulling(ref Vector3 move, ref float speed)
    {
        if (pulledCube == null)
        {
            isPulling = false;
            return;
        }

        isPulling = true;
        move.y = 0;
        if (move.sqrMagnitude > 1f) move.Normalize();
        speed = pullSpeed;

        // Calculate a smooth target position in front of the player
        Vector3 targetPos = transform.position - transform.forward * 1.5f;
        targetPos.y = pulledCube.position.y; // keep original height

        // Move the cube toward targetPos using MoveTowards for consistent speed
        pulledCube.position = Vector3.MoveTowards(
            pulledCube.position,
            targetPos,
            pullSpeed * Time.deltaTime
        );

        // Rotate cube to match player rotation
        pulledCube.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);

        // Stop pulling if cube is too far
        if (Vector3.Distance(transform.position, pulledCube.position) > pullMinDistance + 2f)
            PullStop();
    }


    private void HandlePushing()
    {
        if (!isActiveCharacter) return;

        Vector3 moveDir = new Vector3(controls.Player.Move.ReadValue<Vector2>().x, 0, controls.Player.Move.ReadValue<Vector2>().y);
        if (moveDir.sqrMagnitude < 0.01f) { isPushing = false; return; }

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, moveDir.normalized, out RaycastHit hit, 1f))
        {
            Rigidbody rb = hit.collider.attachedRigidbody;
            if (rb != null && hit.collider.CompareTag("Interactable"))
            {
                isPushing = true;
                // Manual movement for kinematic objects
                if (rb.isKinematic)
                    rb.transform.position += moveDir.normalized * pullSpeed * Time.deltaTime;
                else
                    rb.AddForce(moveDir.normalized * pushForce, ForceMode.Force);

                return;
            }
        }

        isPushing = false;
    }


    #endregion

    #region Pickup / Throw

    public override void Interact()
    {
        if (isPulling || isPushing) return;

        Vector3 origin = holdPoint.position;
        if (heldItem == null)
        {
            Collider[] hits = Physics.OverlapSphere(origin + transform.forward * pickupRange / 2f, pickupRange);
            GameObject closestObj = null;
            float closestDist = Mathf.Infinity;

            foreach (Collider col in hits)
            {
                if (col.CompareTag("Pickupable"))
                {
                    float dist = Vector3.Distance(origin, col.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestObj = col.gameObject;
                    }
                }
            }

            if (closestObj != null)
            {
                heldItem = closestObj;
                heldItem.transform.SetParent(holdPoint);
                heldItem.transform.localPosition = Vector3.zero;
                heldItem.transform.localRotation = Quaternion.identity;

                Rigidbody rb = heldItem.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;

                isHoldingObject = true;
            }
        }
        else
        {
            Rigidbody rb = heldItem.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false;

            heldItem.transform.SetParent(null);
            heldItem = null;
            isHoldingObject = false;
        }
    }

    public override void Throw()
    {
        if (!isHoldingObject || heldItem == null) return;

        heldItem.transform.SetParent(null);
        Rigidbody rb = heldItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(transform.forward * throwForce + Vector3.up * throwUpwardForce, ForceMode.Impulse);
        }

        heldItem = null;
        isHoldingObject = false;
    }

    #endregion

    #region Whistle / Companion

    public override void Whistle()
    {
        animator.SetTrigger("Whistle");
        companion?.Whistle();
    }

    #endregion

    #region Animations

    private void UpdateAnimations()
    {
        float moveSpeed = isSprinting ? sprintSpeed : walkSpeed;
        float normalizedSpeed = isMoving ? moveSpeed / sprintSpeed : 0f;

        animator.SetFloat("Speed", normalizedSpeed);
        animator.SetBool("isPushing", isPushing);
        animator.SetBool("isPulling", isPulling);
    }

    #endregion

    #region Room Bounds

    public void SetRoomBounds(float minX, float maxX)
    {
        minXBound = minX;
        maxXBound = maxX;
    }

    #endregion
}















