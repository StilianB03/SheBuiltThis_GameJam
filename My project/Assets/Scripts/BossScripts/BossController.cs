using UnityEngine;
using UnityEngine.VFX;
using System;
using System.Collections;
using System.Collections.Generic;

public class BossController : MonoBehaviour
{
	public enum BossState { Normal, Hidden, Up }
	public enum BossAttack { None, LaserSpin, NormalAttack2, UpAttack1, UpAttack2 }
	public BossState currentState = BossState.Normal;
	private BossAttack currentAttack = BossAttack.None;

	[Header("Hitboxes")]
	public Collider hitbox1;
	public Collider hitbox2;

	[Header("Health")]
	public float maxHealth = 30f;
	public float currentHealth = 1000f;

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

	[Header("Death Settings")]
	public Material deathMaterial;
	public float maxEmissionIntensity = 100f;
	public float baseEmissionIntensity = 1f; 
	public float chargeDuration = 3.0f;
	public float fadeDuration = 5.0f;
	public float ascendDuration = 7.0f;

	[Header("State Transition Chances ")]
	[Range(0, 100)] public float normalToHidden = 50f;
	[Range(0, 100)] public float normalToUp = 50f;

	[Header("Up State Attacks")]
	[Range(0, 100)] public float upAttack1Chance = 50f;
	[Range(0, 100)] public float upAttack2Chance = 50f;

	[Header("Normal State Attacks")]
	[Range(0, 100)] public float laserAttackChance = 50f;
	[Range(0, 100)] public float normalAttack2Chance = 50f;

	[Header("Laser Attack")]
	public float laserRotationSpeed = 120f;
	public int minRotationCount = 1;
	public int maxRotationCount = 3;
	private float spinDirection = 1f;
	public GameObject laserTriggerCollider; 
	public VisualEffect laserVfx;

	[Header("References")]
	public Transform playerTransform;

	private bool isTurning = false;
	[SerializeField] private bool isAttacking = false;
	[SerializeField] private bool stateTimeExpired = false;

	private float yVelocity; 
	private float stateTimer;
	private float targetHeight;

	private bool isSpawning = true;
	private bool shouldAscend = false;

	private float laserSpinLeft = 0f;
	public static event Action<float, float> OnHealthChanged;

	// START //
	void Start()
    {
        //Ensure we find player
        if (playerTransform == null) {
			PlayerController playerScript = UnityEngine.Object.FindFirstObjectByType<PlayerController>();

			if (playerScript != null)
			{
				playerTransform = playerScript.transform;
			}
		}

		if (laserTriggerCollider != null)
			laserTriggerCollider.SetActive(false);

		if (laserVfx != null)
			laserVfx.SendEvent("StopLaser"); 
			laserVfx.Reinit();

		currentHealth = maxHealth;
		OnHealthChanged?.Invoke(currentHealth, maxHealth);

		EnterState(currentState);
		targetHeight = transform.position.y;
		StartCoroutine(EntranceAnimation(targetHeight));
	}

	// FIXED UPDATE //
	void FixedUpdate()
	{
		if (isSpawning) return;

		HandleStateTimer();
		HandleMovement();
		if (playerTransform == null) return;

		if (isAttacking)
		{
			ExecuteAttack();
		}
		else
		{
			//If not attacking - Follow player / change state / chose attack
			HandleRotation();

			if (!stateTimeExpired)
			{
				PickAttack();
			}
			else
			{
				TransitionToState();
			}
		}
	}

	// ROTATION - PLAYER FOLLOW //
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

	// STATE MANAGEMENT //
	void DecideOnState()
	{
		if (currentState == BossState.Normal)
		{
			float totalWeight = normalToHidden + normalToUp;
			float roll = UnityEngine.Random.Range(0f, totalWeight);

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

	void EnterState(BossState newState)
	{
		currentState = newState;

		switch (currentState)
		{
			case BossState.Normal:
				targetHeight = normalHeight;
				stateTimer = UnityEngine.Random.Range(minNormalTime, maxNormalTime);
				break;

			case BossState.Hidden:
				targetHeight = hiddenHeight;
				stateTimer = UnityEngine.Random.Range(minHiddenTime, maxHiddenTime);
				break;

			case BossState.Up:
				targetHeight = upHeight;
				stateTimer = UnityEngine.Random.Range(minUpTime, maxUpTime);
				break;
		}
	}

	void TransitionToState()
	{
		stateTimeExpired = false;
		DecideOnState();
	}

	void HandleStateTimer()
	{
		if (stateTimer > 0f)
		{
			stateTimer -= Time.fixedDeltaTime;

			if (stateTimer <= 0f)
			{
				stateTimeExpired = true;
			}
		}
	}


	// ATTACK MANAGEMENT //
	void PickAttack() {

		//Hidden state has no attacks
		if (currentState == BossState.Hidden) return;

		isAttacking = true;
		isTurning = false;
		float totalWeight = 0f;
		float roll = 0f;

		// Roll for attack depending on state
		switch (currentState)
		{
			case BossState.Normal:
				totalWeight = laserAttackChance + normalAttack2Chance; 
				roll = UnityEngine.Random.Range(0f, totalWeight);
				if (roll <= laserAttackChance)
				{
					TriggerLaserAttack();
				}
				else
				{
					TriggerNormalAttack2();
				}
				break;

			case BossState.Up:
				totalWeight = upAttack1Chance + upAttack2Chance;
				roll = UnityEngine.Random.Range(0f, totalWeight);
				if (roll <= upAttack1Chance)
				{
					TriggerUpAttack1();
				}
				else
				{
					TriggerUpAttack2();
				}
				break;
		}
	}

	public void OnAttackComplete()
	{
		currentAttack = BossAttack.None; 
		isAttacking = false;
	}

	// ATTACKS //
	void TriggerLaserAttack()
	{
		currentAttack = BossAttack.LaserSpin; 

		int rotations = UnityEngine.Random.Range(minRotationCount, maxRotationCount + 1);
		laserSpinLeft = rotations * 360f;
		spinDirection = (UnityEngine.Random.value > 0.5f) ? 1f : -1f;

		if (laserVfx != null)
			laserVfx.SendEvent("StartLaser");

		if (laserTriggerCollider != null)
			laserTriggerCollider.SetActive(true);
	}

	void TriggerNormalAttack2()
	{
		currentAttack = BossAttack.NormalAttack2;
		OnAttackComplete();
	}

	void TriggerUpAttack1()
	{
		currentAttack = BossAttack.UpAttack1;
		OnAttackComplete();
	}

	void TriggerUpAttack2()
	{
		currentAttack = BossAttack.UpAttack2;
		OnAttackComplete();
	}

	// ATTACKS EXECUTION //
	void ExecuteAttack() 
	{
		switch (currentAttack)
		{
			case BossAttack.LaserSpin:
				if (laserSpinLeft > 0f)
				{
					float step = laserRotationSpeed * Time.fixedDeltaTime;
					transform.Rotate(0f, step * spinDirection, 0f);
					laserSpinLeft -= step;

					if (laserSpinLeft <= 0f)
					{
						if (laserTriggerCollider != null)
							laserTriggerCollider.SetActive(false);

						if (laserVfx != null)
							laserVfx.SendEvent("StopLaser");
							laserVfx.Reinit();
						OnAttackComplete();
					}
				}
				break;

			case BossAttack.NormalAttack2:
				// Frame calcs for NormalAttack2 here
				break;

			case BossAttack.UpAttack1:
				// Frame calcs for UpAttack1 here
				break;

			case BossAttack.UpAttack2:
				// Frame calcs for UpAttack2 here
				break;

			case BossAttack.None:
			default:
				break;
		}
	}

	// DAMAGE HANDLING //
	public void TakeDamage(float damageAmount)
	{
		currentHealth = Mathf.Clamp(currentHealth - damageAmount, 0f, maxHealth);
		OnHealthChanged?.Invoke(currentHealth, maxHealth);

		if (currentHealth <= 0f)
		{
			Die();
		}
	}


	// DEATH SEQUENCE //
	private void Die()
	{
		StopAllCoroutines();
		isAttacking = false;
		isSpawning = true;

		if (hitbox1) hitbox1.enabled = false;
		if (hitbox2) hitbox2.enabled = false;

		StartCoroutine(DeathSequence());
	}

	private IEnumerator DeathSequence()
	{
		List<Renderer> validRenderers = new();

		foreach (Renderer r in GetComponentsInChildren<Renderer>())
		{
			if (r.GetComponent<VisualEffect>() == null)
				validRenderers.Add(r);
		}

		if (laserVfx != null) 
			laserVfx.SendEvent("StopLaser"); 
			laserVfx.Reinit();
		if (laserTriggerCollider != null) laserTriggerCollider.SetActive(false);

		float elapsed = 0f;
		//Increase
		while (elapsed < chargeDuration)
		{
			HandleRotation();
			elapsed += Time.deltaTime;
			float t = elapsed / chargeDuration;
			float currentIntensity = Mathf.Lerp(baseEmissionIntensity, maxEmissionIntensity, t);

			foreach (Renderer r in validRenderers)
			{
				r.material.SetFloat("_emissive", currentIntensity);
			}
			yield return null;
		}

		//Material Swap
		if (deathMaterial != null)
		{
			foreach (Renderer r in validRenderers)
			{
				r.material = deathMaterial;
				r.material.SetFloat("_emissive", maxEmissionIntensity);
			}
		}

		//Fade down and rise
		elapsed = 0f;

		Vector3 startPos = transform.position;
		Vector3 risePos = startPos + new Vector3(0, 15f, 0);

		while (elapsed < fadeDuration)
		{
			HandleRotation();
			elapsed += Time.deltaTime;
			float t = elapsed / fadeDuration;

			float currentIntensity =
				Mathf.Lerp(maxEmissionIntensity, baseEmissionIntensity, t);

			foreach (Renderer r in validRenderers)
			{
				r.material.SetFloat("_emissive", currentIntensity);
			}

			yield return null;
		}

		elapsed = 0f;
		if (shouldAscend) {
			while (elapsed < ascendDuration)
			{
				HandleRotation();
				elapsed += Time.deltaTime;
				float t = elapsed / 2f;

				transform.position =
					Vector3.Lerp(startPos, risePos, t);

				yield return null;
			}
			Destroy(gameObject);
		}
	}

	//ENTRANCE ANIM//
	private IEnumerator EntranceAnimation(float finalTargetHeight)
	{
		isSpawning = true;

		float duration = 3.0f;
		float elapsed = 0f;

		Vector3 startPos = new Vector3(transform.position.x, finalTargetHeight + 10f, transform.position.z);
		Vector3 endPos = new Vector3(transform.position.x, finalTargetHeight, transform.position.z);

		transform.position = startPos;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / duration;

			transform.position = Vector3.Lerp(startPos, endPos, t);
			yield return null;
		}

		transform.position = endPos;
		isSpawning = false;
	}
}
