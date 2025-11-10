using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterController), typeof(Animator))]
public class Follower : ControllableCharacter
{
    [Header("Follow Settings")]
    [SerializeField] private Transform player;
    [SerializeField] private float stopDistance = 1.5f;
    [SerializeField] private float walkDistance = 3f;
    [SerializeField] private float runDistance = 6f;
    [SerializeField] private float walkSpeed = 1.5f;
    [SerializeField] private float runSpeed = 3f;

    [Header("Manual Control")]
    [SerializeField] private float manualMoveSpeed = 2f;
    [SerializeField] private float sprintSpeed = 4f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Push / Pull Settings")]
    [SerializeField] private float pushForce = 5f;
    [SerializeField] private float pullSpeed = 2f;
    [SerializeField] private float pullDistance = 1.5f;

    [Header("Pickup / Throw")]
    [SerializeField] private Transform holdPoint;
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float throwUpwardForce = 2f;

    private CharacterController controller;
    private Animator animator;
    private PlayerControls controls;
    private NavMeshAgent agent;

    private Vector3 velocity;
    private Rigidbody pulledCube;
    private GameObject heldItem;

    private bool isFollowing = false;
    private bool isMoving = false;
    private bool isSprinting = false;
    private bool isPulling = false;
    private bool isPushing = false;
    private bool isHoldingObject = false;
    private bool isGrounded = false;
    private bool goingToLever = false;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        controls = new PlayerControls();
        agent = GetComponent<NavMeshAgent>();

        agent.updatePosition = true;
        agent.updateRotation = false;

        SetupControls();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void SetupControls()
    {
        controls.Player.Move.performed += _ => { };
        controls.Player.Sprint.performed += _ => { };
        controls.Player.Jump.performed += _ => Jump();
        controls.Player.Pull.performed += _ => PullStart();
        controls.Player.Pull.canceled += _ => PullStop();
        controls.Player.Interact.performed += _ => Interact();
        controls.Player.Throw.performed += _ => Throw();
        controls.Player.Whistle.performed += _ => Whistle();
    }

    private void Update()
    {
        if (isActiveCharacter)
        {
            HandleManualControl();
        }
        else
        {
            if (goingToLever) HandleGoToLever();
            else HandleFollowAI();
        }
    }

    #region Manual Control

    private void HandleManualControl()
    {
        Vector2 moveInput = controls.Player.Move.ReadValue<Vector2>();
        isSprinting = controls.Player.Sprint.ReadValue<float>() > 0;

        HandleGravity();
        HandlePushing();
        Move(moveInput);
        HandleRotation(moveInput);
        UpdateAnimations();
    }

    public override void Move(Vector2 input)
    {
        Vector3 moveDir = new Vector3(input.x, 0, input.y);
        isMoving = moveDir.sqrMagnitude > 0.01f;

        float speed = isSprinting ? sprintSpeed : manualMoveSpeed;

        if (isPulling && pulledCube != null)
            HandlePulling(ref moveDir, ref speed);

        Vector3 finalMove = moveDir * speed * Time.deltaTime + velocity * Time.deltaTime;
        controller.Move(finalMove);
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
        if (isPulling && pulledCube != null)
        {
            Vector3 dir = pulledCube.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);
        }
        else if (moveInput.sqrMagnitude > 0.01f)
        {
            Vector3 dir = new Vector3(moveInput.x, 0, moveInput.y);
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
        if (pulledCube == null) { isPulling = false; return; }
        move.y = 0;
        if (move.sqrMagnitude > 1f) move.Normalize();
        speed = pullSpeed;

        Vector3 targetPos = transform.position - transform.forward * 1.5f;
        targetPos.y = pulledCube.position.y;
        pulledCube.position = Vector3.Lerp(pulledCube.position, targetPos, pullSpeed * Time.deltaTime);
        pulledCube.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);

        if (Vector3.Distance(transform.position, pulledCube.position) > pullDistance + 2f)
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
            if (rb != null && !rb.isKinematic && hit.collider.CompareTag("Interactable"))
            {
                isPushing = true;
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
            heldItem.transform.position += Vector3.up * 0.1f;
            Vector3 throwDir = transform.forward * throwForce + Vector3.up * throwUpwardForce;
            rb.AddForce(throwDir, ForceMode.Impulse);
        }

        heldItem = null;
        isHoldingObject = false;
    }

    #endregion

    #region Whistle / AI Follow

    public override void Whistle()
    {
        isFollowing = true;
    }

    private void HandleFollowAI()
    {
        if (!isFollowing || player == null) { animator.SetFloat("Speed", 0f); return; }

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        float distance = dir.magnitude;

        if (distance > stopDistance)
        {
            dir.Normalize();
            float t = Mathf.InverseLerp(stopDistance, runDistance, distance);
            float speed = Mathf.Lerp(walkSpeed, runSpeed, t);

            if (agent != null)
            {
                if (!agent.enabled) agent.enabled = true;
                agent.SetDestination(player.position);
                animator.SetFloat("Speed", agent.velocity.magnitude / agent.speed * runSpeed);
            }
            else
            {
                controller.Move(dir * speed * 0.7f * Time.deltaTime);
                animator.SetFloat("Speed", speed);
            }

            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
            }
        }
        else
        {
            isFollowing = false;
            animator.SetFloat("Speed", 0f);
        }
    }

    #endregion

    #region Active Character Switch

    public override void SetActive(bool active)
{
    base.SetActive(active);

    if (active)
    {
        // Player controls companion
        isFollowing = false;

        if (agent != null) agent.enabled = false; // disable NavMeshAgent
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            {
                rb.isKinematic = true;
                rb.constraints = RigidbodyConstraints.FreezeRotation;
            }
    }
    else
    {
        // AI controls companion
        if (agent != null) agent.enabled = true;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;
    }
}


    #endregion

    #region Lever Interaction

    public void GoToLever(Vector3 targetPosition)
    {
        if (agent == null) return;

        isFollowing = false;
        goingToLever = true;
        agent.isStopped = false;
        agent.SetDestination(targetPosition);
    }

    private void HandleGoToLever()
    {
        if (!goingToLever || agent == null) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            goingToLever = false;
            agent.isStopped = true;
            animator.SetTrigger("Idle");
        }

        float speedPercent = agent.velocity.magnitude / agent.speed;
        animator.SetFloat("Speed", speedPercent);
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
}












