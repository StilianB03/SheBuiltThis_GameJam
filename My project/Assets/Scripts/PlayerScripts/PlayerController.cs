using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
	[Header("References")]
	public Transform centerPoint;
	public Transform cameraTransform; // Drag your Main Camera here!

	[Header("Ring Settings")]
	public float radius = 5f;
	public float moveSpeed = 180f; // Degrees per second
	public float verticalSpeed = 5f;

	[Header("Limits")]
	public float minHeight = 0f;
	public float maxHeight = 10f;

	private float angle;
	private float height;
	private Vector2 moveInput;

	void Start()
	{
		if (centerPoint == null)
		{
			Debug.LogError("Please assign a Center Point object!");
			return;
		}

		// Initialize the camera reference automatically if forgotten
		if (cameraTransform == null && Camera.main != null)
		{
			cameraTransform = Camera.main.transform;
		}

		// Calculate starting position based on where you placed the capsule in the editor
		Vector3 offset = transform.position - centerPoint.position;
		angle = Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;
		height = transform.position.y - centerPoint.position.y;
	}

	public void OnMove(InputAction.CallbackContext context)
	{
		moveInput = context.ReadValue<Vector2>();
	}

	void Update()
	{
		if (centerPoint == null || cameraTransform == null) return;

		// 1. CALCULATE CAMERA RELATIVE DIRECTION
		// Get the direction from the camera to the center point on a flat horizontal plane
		Vector3 camToCenter = centerPoint.position - cameraTransform.position;
		camToCenter.y = 0;
		camToCenter.Normalize();

		// Calculate the camera's screen-right vector matching the ring's curve
		Vector3 camRight = Vector3.Cross(Vector3.up, camToCenter).normalized;

		// 2. DETERMINE MOVEMENT SIGN
		// Figure out if moving along the screen-right vector increases or decreases the circle angle
		Vector3 playerPosOnPlane = new Vector3(transform.position.x, centerPoint.position.y, transform.position.z);
		Vector3 tangent = Vector3.Cross(Vector3.up, playerPosOnPlane - centerPoint.position).normalized;

		// This factor accurately adjusts the movement direction regardless of screen inversion
		float directionFactor = Vector3.Dot(camRight, tangent) > 0 ? 1f : -1f;

		// 3. APPLY INPUT
		float inputX = moveInput.x * directionFactor;
		float inputY = moveInput.y;

		// Orbit around center
		angle += inputX * moveSpeed * Time.deltaTime;

		// Handle vertical movement
		height += inputY * verticalSpeed * Time.deltaTime;
		height = Mathf.Clamp(height, minHeight, maxHeight);

		// Convert polar -> cartesian coordinates
		float rad = angle * Mathf.Deg2Rad;
		Vector3 newOffset = new Vector3(
			Mathf.Cos(rad) * radius,
			height,
			Mathf.Sin(rad) * radius
		);

		// Snap position and look at the center structure
		transform.position = centerPoint.position + newOffset;
		transform.LookAt(new Vector3(centerPoint.position.x, transform.position.y, centerPoint.position.z));
	}
}