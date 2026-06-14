	using UnityEngine;

public class StarCompanion : MonoBehaviour
{
	[Header("Follow Settings")]
	public Transform player; 
	public float activationRange = 5f;
	public float followSpeed = 5f;
	public Vector3 offset = new Vector3(0, 1.5f, 0);

	[Header("Hover Settings")]
	public float hoverSpeed = 5f;   
	public float hoverHeight = 0.5f;

	private bool isCollected = false; 
	private bool isRegistered = false;
	private Vector3 startPosition;

	void Start()
	{
		startPosition = transform.position;
	}

	void FixedUpdate()
	{
		if (player == null) return;

		float distance = Vector3.Distance(transform.position, player.position);
		if (distance < activationRange)
		{
			isCollected = true;
		}

		if (isCollected)
		{
			FollowPlayer();

			// Register only once
			if (!isRegistered)
			{
				RegisterWithManager();
			}
		}
		else
		{
			float hover = Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
			transform.position = startPosition + new Vector3(0, hover, 0);
		}
	}

	private void RegisterWithManager()
	{
		PlayerController pc = player.GetComponent<PlayerController>();
		if (pc != null && pc.isFinalBoss)
		{
			StarManager manager = FindAnyObjectByType<StarManager>();
			if (manager != null)
			{
				manager.RegisterCollectedStar(this);
				isRegistered = true; // Prevents calling this every frame
			}
		}
	}

	private void FollowPlayer()
	{
		Vector3 targetPos = player.position + offset;
		transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
		transform.Rotate(Vector3.up * 100 * Time.deltaTime);
	}
}