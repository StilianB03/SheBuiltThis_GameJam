using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
	[Header("References")]
	public Transform centerPoint;
	public Transform cameraTransform;
	public PlayerInput playerInput;

	[Header("Movement Settings")]
	public float moveSpeed = 7f;
	public float rotationSpeed = 15f;

	[Header("Camera Settings")]
	public float cameraDistance = 8f;
	public float cameraHeight = 3.5f;

	private InputAction moveAction;
	private Vector2 moveInput;
	private bool isFinalBoss = false;

	void Start()
	{
		if (playerInput == null)
			playerInput = GetComponent<PlayerInput>();

		if (cameraTransform == null && Camera.main != null)
			cameraTransform = Camera.main.transform;

		if (playerInput != null && playerInput.actions != null)
		{
			moveAction = playerInput.actions.FindAction("Move");
		}
	}

	void Update()
	{
		if (moveAction == null || cameraTransform == null || centerPoint == null) return;

		// 1. Get Input
		moveInput = moveAction.ReadValue<Vector2>();

		// 2. Get camera directions (flattened)
		Vector3 camForward = cameraTransform.forward;
		Vector3 camRight = cameraTransform.right;

		camForward.y = 0f;
		camRight.y = 0f;
		camForward.Normalize();
		camRight.Normalize();

		Vector3 movementDirection = (camForward * moveInput.y) + (camRight * moveInput.x);

		//Move player
		transform.position += movementDirection * moveSpeed * Time.deltaTime;

		//Rotate player toward movement dir
		if (movementDirection.sqrMagnitude > 0.001f)
		{
			Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
			transform.rotation = Quaternion.Slerp(
				transform.rotation,
				targetRotation,
				rotationSpeed * Time.deltaTime
			);
		}

		//Placeholderfor final boss cam behaviour
		isFinalBoss = true;
		if (isFinalBoss)
		{
			UpdateCameraPosition();
		}
	}

	void UpdateCameraPosition()
	{
		// Get line from center through player
		Vector3 centerToPlayer = transform.position - centerPoint.position;
		centerToPlayer.y = 0f; 
		centerToPlayer.Normalize();

		Vector3 targetCamPos = transform.position + (centerToPlayer * cameraDistance);
		targetCamPos.y = transform.position.y + cameraHeight;
		cameraTransform.position = targetCamPos;

		Vector3 lookTarget = new Vector3(centerPoint.position.x, transform.position.y, centerPoint.position.z);
		cameraTransform.LookAt(lookTarget);
	}
}