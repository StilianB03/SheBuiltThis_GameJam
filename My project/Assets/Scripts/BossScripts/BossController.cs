using UnityEngine;

public class BossController : MonoBehaviour
{
	public enum BossState { Normal, Hidden, Up }
	public BossState currentState = BossState.Normal;

	[Header("Rotation Settings")]
    public float rotationSpeed = 20f; 
    public float followPlayerOffsetAngle = 30f;

	[Header("State Heights Fixed")]
	public float normalHeight = 1.55f;
	public float hiddenHeight = -2.2f;
	public float upHeight = 4.8f;

	[Header("State Durations")]
	public float minNormalTime = 6f;
	public float maxNormalTime = 12f;
	public float minHiddenTime = 2f;
	public float maxHiddenTime = 7f;
	public float minUpTime = 2f;
	public float maxUpTime = 7f;

	[Header("Vertical Movement")]
	public float moveTime = 0.15f;
	public float maxMoveSpeed = 50f;

	[Header("Transition Chances ")]
	[Range(0, 100)] public float normalToHidden = 50f;
	[Range(0, 100)] public float normalToUp = 50f;

	[Header("References")]
	public Transform playerTransform;

	private bool isTurning = false;
	private bool isHiding = false; 
	private float targetHeight;
	private float yVelocity; 
	private float stateTimer;

	void Start()
    {
        //Ensure we find player
        if (playerTransform == null) {
			PlayerController playerScript = Object.FindFirstObjectByType<PlayerController>();

			if (playerScript != null)
			{
				playerTransform = playerScript.transform;
			}
		}

		EnterState(currentState);
		targetHeight = transform.position.y;
	}

    void Update()
    {
	}

	void FixedUpdate()
	{
		HandleStateTimer();
		HandleMovement();
		if (playerTransform == null) return;

		HandleRotation();
	}

	void HandleRotation() { 
        Vector3 targetDir = playerTransform.position - transform.position;
        targetDir.y = 0;

		if (targetDir.sqrMagnitude > 0.001f)
		{ 
			float angleToPlayer = Vector3.Angle(transform.forward, targetDir);

			if (!isTurning && angleToPlayer > followPlayerOffsetAngle)
			{
				isTurning = true;
			}
			else if (isTurning && angleToPlayer < 1f)
			{
				isTurning = false;
			}

			if (angleToPlayer > followPlayerOffsetAngle)
			{
				Quaternion targetRot = Quaternion.LookRotation(targetDir);
				transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
			}
		}
	}

	void EnterState(BossState newState)
	{
		currentState = newState;

		switch (currentState)
		{
			case BossState.Normal:
				targetHeight = normalHeight;
				stateTimer = Random.Range(minNormalTime, maxNormalTime);
				break;

			case BossState.Hidden:
				targetHeight = hiddenHeight;
				stateTimer = Random.Range(minHiddenTime, maxHiddenTime);
				break;

			case BossState.Up:
				targetHeight = upHeight;
				stateTimer = Random.Range(minUpTime, maxUpTime);
				break;
		}
	}

	void DecideOnState()
	{
		if (currentState == BossState.Normal)
		{
			float totalWeight = normalToHidden + normalToUp;
			float roll = Random.Range(0f, totalWeight);

			if (roll <= normalToHidden)
			{
				EnterState(BossState.Hidden);
			}
			else
			{
				EnterState(BossState.Up);
			}
		}
		else
		{
			EnterState(BossState.Normal);
		}
	}

	void HandleStateTimer()
	{
		stateTimer -= Time.fixedDeltaTime;

		if (stateTimer <= 0f)
		{
			DecideOnState();
		}
	}

	void HandleMovement()
	{
		Vector3 currentPos = transform.position;
		currentPos.y = Mathf.SmoothDamp(
			currentPos.y,
			targetHeight,
			ref yVelocity,
			moveTime,
			maxMoveSpeed,
			Time.fixedDeltaTime
		);
		transform.position = currentPos;
	}
}
