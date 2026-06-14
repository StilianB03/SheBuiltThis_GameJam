using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class EventManager : MonoBehaviour
{
	public bool arenaEntered = false; 
	public GameObject bossHealthBar; 
	public GameObject playerHealthBar;

	public void TriggeredArenaEntered()
	{
		StartCoroutine(ArenaSequence());
		StartCoroutine(ShowHealthBars());
	}

	private IEnumerator ArenaSequence()
	{
		GameObject[] fallingObjects = GameObject.FindGameObjectsWithTag("Fall");
		List<GameObject> objectsToDestroy = new List<GameObject>();

		// Store starting positions
		Dictionary<GameObject, Vector3> startPositions = new Dictionary<GameObject, Vector3>();

		foreach (GameObject obj in fallingObjects)
		{
			startPositions[obj] = obj.transform.position;
			objectsToDestroy.Add(obj);
		}

		float duration = 4.0f; 
		float elapsed = 0f;
		float fallDistance = 25f; 

		while (elapsed < duration)
		{
			foreach (GameObject obj in objectsToDestroy)
			{
				if (obj != null)
				{
					Vector3 targetPos = startPositions[obj] - new Vector3(0, fallDistance, 0);
					obj.transform.position = Vector3.Lerp(startPositions[obj], targetPos, elapsed / duration);
				}
			}
			elapsed += Time.deltaTime;
			yield return null;
		}

		yield return new WaitForSeconds(1f);
		foreach (GameObject obj in objectsToDestroy)
		{
			if (obj != null) Destroy(obj);
		}
	}

	private IEnumerator ShowHealthBars()
	{
		//shit UI
		yield return new WaitForSeconds(0.5f);

		bossHealthBar.SetActive(true);
		playerHealthBar.SetActive(true);
	}
}
