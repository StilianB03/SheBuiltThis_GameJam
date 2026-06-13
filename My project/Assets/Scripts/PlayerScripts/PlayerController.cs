using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerController : MonoBehaviour
{
	[Header("References")]
	public Transform centerPoint;
	public Transform cameraTransform;
	public PlayerInput playerInput;
	private Rigidbody rb;

	[Header("Movement Settings")]
	public float moveSpeed = 7f;
	public float rotationSpeed = 15f;
	public float airControlFactor = 0.4f;

	[Header("Jump Settings")]
	public float jumpForce = 6f;
	public float groundCheckDistance = 0.2f;
	public float fallMultiplier = 2.5f;
	public float JumpCutoff = 2f;
	private bool isGrounded;
	public LayerMask groundLayer;

	[Header("Health Settings")]
	public float maxHealth = 100f;
	private float currentHealth = 100f;

	[Header("Camera Settings")]
	public float cameraDistance = 8f;
	public float cameraHeight = 3.5f;

	[Header("Shooting Settings")]
	public ProjectileData projectileData;
	public Transform shootPoint;
	private InputAction shootAction;

	private InputAction moveAction;
	private InputAction jumpAction;
	private Vector2 moveInput;
	private bool isFinalBoss = false;

	public static event Action<float, float> OnHealthChanged;

	private void Awake()
	{
		if (playerInput == null)
			playerInput = GetComponent<PlayerInput>();

		moveAction = playerInput.actions.FindAction("Move");
		jumpAction = playerInput.actions.FindAction("Jump");
		shootAction = playerInput.actions.FindAction("Shoot");
	}

	void Start()
	{
		//Rb setup
		rb = GetComponent<Rigidbody>();
		rb.freezeRotation = true;
		rb.useGravity = true;
		rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

		if (playerInput == null)
			playerInput = GetComponent<PlayerInput>();

		if (cameraTransform == null && Camera.main != null)
			cameraTransform = Camera.main.transform;

		currentHealth = maxHealth;
		OnHealthChanged?.Invoke(currentHealth, maxHealth);

		//Placeholder
		isFinalBoss = true;
	}

	void Update()
	{
		if (moveAction == null || cameraTransform == null || centerPoint == null) return;

		//Get Input
		moveInput = moveAction.ReadValue<Vector2>();

		HandleJump();
		HandleCameraAndRotation();
	}

	void FixedUpdate()
	{
		GroundCheck();
		HandleLocomotion();
		ApplyJumpGravity();
	}

	void GroundCheck()
	{
		//Raycast for ground check
		isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
	}

	void HandleCameraAndRotation()
	{
		Vector3 camForward = cameraTransform.forward;
		Vector3 camRight = cameraTransform.right;
		camForward.y = 0f;
		camRight.y = 0f;
		camForward.Normalize();
		camRight.Normalize();
		Vector3 movementDirection = (camForward * moveInput.y) + (camRight * moveInput.x);

		if (movementDirection.sqrMagnitude > 0.001f)
		{
			Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
		}

		if (isFinalBoss)
		{
			UpdateCameraPositionBoss();
		}
	}

	void UpdateCameraPositionBoss()
	{
		Vector3 centerToPlayer = transform.position - centerPoint.position;
		centerToPlayer.y = 0f;
		centerToPlayer.Normalize();

		// Position camera behind player
		Vector3 targetCamPos = transform.position + (centerToPlayer * cameraDistance);

		targetCamPos.y = transform.position.y + cameraHeight;
		cameraTransform.position = targetCamPos;

		Vector3 lookTarget = new Vector3(centerPoint.position.x, transform.position.y, centerPoint.position.z);
		cameraTransform.LookAt(lookTarget);
	}

	void HandleLocomotion()
	{
		Vector3 camForwardHorizontal = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
		Vector3 camRightHorizontal = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;

		Vector3 movementDirection = (camForwardHorizontal * moveInput.y) + (camRightHorizontal * moveInput.x);
		if (movementDirection.magnitude > 1f) movementDirection.Normalize();

		float currentSpeed = isGrounded ? moveSpeed : (moveSpeed * airControlFactor);
		Vector3 targetVelocity = movementDirection * currentSpeed;

		if (!isGrounded && movementDirection.sqrMagnitude < 0.001f)
		{
			targetVelocity.x = rb.linearVelocity.x;
			targetVelocity.z = rb.linearVelocity.z;
		}

		targetVelocity.y = rb.linearVelocity.y;
		rb.linearVelocity = targetVelocity;
	}

	void ApplyJumpGravity()
	{
		if (isGrounded) return;

		//Standard fall
		if (rb.linearVelocity.y < 0)
		{
			rb.AddForce(Vector3.down * (fallMultiplier - 1) * Physics.gravity.magnitude, ForceMode.Acceleration);
		}
		//Cutoff fall on jump release
		else if (rb.linearVelocity.y > 0 && jumpAction != null && !jumpAction.IsPressed())
		{
			rb.AddForce(Vector3.down * (JumpCutoff - 1) * Physics.gravity.magnitude, ForceMode.Acceleration);
		}
	}

	void HandleJump()
	{
		if (isGrounded && jumpAction != null && jumpAction.WasPressedThisFrame())
		{
			rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
		}
	}

	// HEALTH HANDLING//
	public void TakeDamage(float damageAmount)
	{
		currentHealth = Mathf.Clamp(currentHealth - damageAmount, 0f, maxHealth);
		OnHealthChanged?.Invoke(currentHealth, maxHealth);

		if (currentHealth <= 0f)
		{
			Die();
		}
	}

	private void Die()
	{
		Debug.Log("Its okay to die");
	}

	// SHOOTING //
	void Shoot()
	{
		if (projectileData?.projectilePrefab != null && shootPoint != null)
		{
			GameObject bulletObj = Instantiate(
				projectileData.projectilePrefab,
				shootPoint.position,
				shootPoint.rotation);

			Projectile projectileScript = bulletObj.GetComponent<Projectile>();

			if (projectileScript != null)
			{
				projectileScript.Initialize(projectileData.speed);
			}
		}
	}

	private void OnEnable()
	{
		if (shootAction != null)
		{
			shootAction.performed += OnShootPerformed;
			shootAction.Enable();
		}
	}

	private void OnDisable()
	{
		if (shootAction != null)
		{
			shootAction.performed -= OnShootPerformed;
			shootAction.Disable();
		}
	}

	private void OnShootPerformed(InputAction.CallbackContext ctx)
	{
		Shoot();
	}
}