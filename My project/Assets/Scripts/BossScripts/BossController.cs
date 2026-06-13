using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 20f; 
    public float followPlayerOffsetAngle = 30f;

	public Transform playerTransform;
	private bool isTurning = false;

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
    }

    void Update()
    {
	}

	void FixedUpdate()
	{
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
}
