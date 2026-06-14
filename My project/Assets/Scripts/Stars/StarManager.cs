using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StarManager : MonoBehaviour
{
	public Transform player;
	public float absorptionSpeed = 25f;
	public int totalStarsToCollect = 10;
	public Rigidbody playerRb;

	//Boss spawn vars//
	public GameObject bossPrefab;
	public float bossSpawnY = 1.55f;

	//Emission//
	public float baseEmissionIntensity = 1.0f;
	private float currentIntensity = 1.0f;
	private float intensityIncrement = 16.0f;

	public Renderer playerRenderer;
	private Material playerMaterial;
	private Queue<StarCompanion> starQueue = new Queue<StarCompanion>();
	public List<StarCompanion> collectedStars = new List<StarCompanion>(); 
	public List<StarCompanion> allCollectedStars = new List<StarCompanion>();

	private bool sequenceStarted = false;
	private bool isOrbiting = false;
	private float orbitTimer = 0f;

	private SoundManager mySM;
	private EventManager myEM;

	//Orbit vars//
	public float radius = 1.0f;
	public float rotationSpeed = 1.0f;

	void Start()
	{
		if (playerRenderer != null)
		{
			playerMaterial = playerRenderer.material;
		}

		mySM = UnityEngine.Object.FindFirstObjectByType<SoundManager>();
		myEM = UnityEngine.Object.FindFirstObjectByType<EventManager>();
	}

	void Update()
	{
		if (isOrbiting)
		{
			orbitTimer += Time.deltaTime;
			UpdateStarOrbits();
		}
	}

	public void RegisterCollectedStar(StarCompanion star)
	{
		if (!starQueue.Contains(star))
		{
			starQueue.Enqueue(star);
		}
	}

	//private IEnumerator AbsorbStarsOneByOne()
	//{
	//	while (starQueue.Count > 0)
	//	{
	//		StarCompanion star = starQueue.Dequeue();
	//		if (star == null) continue;

	//		StartCoroutine(AbsorbSingleStar(star));
	//		yield return new WaitForSeconds(0.2f);
	//	}

	//	// Spawn Boss

	//	yield return new WaitUntil(() =>
	//	myEM != null && myEM.arenaEntered); 

	//	if (bossPrefab != null)
	//	{
	//		myEM.TriggeredArenaEntered();
	//		Vector3 spawnPos = new Vector3(0, bossSpawnY - 7.55f, 0);
	//		Instantiate(bossPrefab, spawnPos, Quaternion.identity);
	//	}
	//}

	private IEnumerator AbsorbSingleStar(StarCompanion star)
	{
		float orbitRadius = 4.5f;
		float orbitSpeed = 7.5f;
		float orbitDuration = 1.0f;

		//Orbit
		float elapsed = 0f;
		while (elapsed < orbitDuration)
		{
			float angle = elapsed * orbitSpeed;
			Vector3 offset = new Vector3(Mathf.Sin(angle), 0.5f, Mathf.Cos(angle)) * orbitRadius;
			Vector3 targetOrbitPos = player.position + offset;

			star.transform.position = Vector3.Lerp(star.transform.position, targetOrbitPos, 5f * Time.deltaTime);
			elapsed += Time.deltaTime;
			yield return null;
		}

		//Merge
		float currentAbsorptionSpeed = absorptionSpeed * 1.5f;
		while (Vector3.Distance(star.transform.position, player.position) > 0.1f)
		{
			star.transform.position = Vector3.MoveTowards(
				star.transform.position,
				player.position,
				currentAbsorptionSpeed * Time.deltaTime
			);
			yield return null;
		}
		mySM.PlayOnce("starEnter");

		//Emission
		currentIntensity += intensityIncrement;
		ApplyEmission(currentIntensity);
		star.gameObject.SetActive(false);
		collectedStars.Add(star);

		yield return new WaitForSeconds(0.3f);
		yield return StartCoroutine(FadeToBaseIntensity(1.0f));
	}

	private IEnumerator FadeToBaseIntensity(float targetIntensity)
	{
		float fadeDuration = 0.5f;
		float elapsed = 0f;
		float startIntensity = currentIntensity;

		while (elapsed < fadeDuration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / fadeDuration;

			// Lerp intensity
			currentIntensity = Mathf.Lerp(startIntensity, targetIntensity, t);
			ApplyEmission(currentIntensity);

			yield return null;
		}

		currentIntensity = targetIntensity;
		ApplyEmission(currentIntensity);
	}

	private void ApplyEmission(float intensity)
	{
		if (playerMaterial != null)
		{
			Color finalColor = Color.yellow * intensity;
			playerMaterial.SetColor("_EmissionColor", finalColor);

			DynamicGI.SetEmissive(playerRenderer, finalColor);
		}
	}

	private void UpdateStarOrbits()
	{
		int count = collectedStars.Count;

		for (int i = 0; i < count; i++)
		{
			StarCompanion star = collectedStars[i];
			if (star == null) continue;

			float angle = (i * Mathf.PI * 2f / count) + (orbitTimer * rotationSpeed);

			Vector3 offset = new Vector3(Mathf.Cos(angle), 1f, Mathf.Sin(angle)) * radius;
			star.transform.position = player.position + offset;
		}
	}

	public IEnumerator StartAscendSequence()
	{
		if (player == null) yield break;

		foreach (var star in allCollectedStars)
		{
			if (star != null)
			{
				star.gameObject.SetActive(true); 
				star.transform.position = player.position; 
				star.SetAppearance(Color.yellow, 5.0f, 0.2f);
			}
		}
		isOrbiting = true;

		yield return new WaitForSeconds(4.0f);

		BossController bossScript = FindObjectOfType<BossController>();
		if (bossScript != null)
		{
			bossScript.shouldAscend = true;
		}
	}

	public void TriggerWaveAbsorption(List<StarCompanion> waveStars)
	{
		StartCoroutine(AbsorbWave(waveStars));
	}

	private IEnumerator AbsorbWave(List<StarCompanion> stars)
	{
		// 1. Lock Player Movement
    if (playerRb != null) playerRb.isKinematic = true;

    // 2. Filter: Only process stars that haven't been absorbed yet
    List<StarCompanion> newStars = new List<StarCompanion>();
    foreach (var star in stars)
    {
        if (star != null && !star.isAbsorbed)
        {
            newStars.Add(star);
        }
    }

    // 3. Cinematic Rise & Dim for the NEW set only
    float duration = 1.5f;
    float elapsed = 0f;
    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        
        foreach (var star in newStars)
        {
            // Move up
            star.transform.position += Vector3.up * Time.deltaTime * 0.5f;
            // Dim emission (Assuming you added SetEmissionIntensity to StarCompanion)
            star.SetEmissionIntensity(Mathf.Lerp(1.0f, 0.1f, t));
        }
        yield return null;
    }

    // 4. Merge NEW stars
    foreach (var star in newStars)
    {
        if (!allCollectedStars.Contains(star))
            allCollectedStars.Add(star);

        yield return StartCoroutine(AbsorbSingleStar(star));
        star.isAbsorbed = true; // Mark as done
    }

    // 5. Unlock Player Movement
    if (playerRb != null) playerRb.isKinematic = false;

    // Final check for boss spawn
    if (allCollectedStars.Count >= totalStarsToCollect)
    {
        StartCoroutine(SpawnBossSequence());
    }
	}

	private IEnumerator SpawnBossSequence()
	{
		yield return new WaitUntil(() => myEM != null && myEM.arenaEntered);

		if (bossPrefab != null)
		{
			if (FindObjectOfType<BossController>() == null)
			{
				myEM.TriggeredArenaEntered();
				Vector3 spawnPos = new Vector3(0, bossSpawnY - 7.55f, 0);
				Instantiate(bossPrefab, spawnPos, Quaternion.identity);
			}
		}
	}
}