using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class EventManager : MonoBehaviour
{
	public bool arenaEntered = false;

	public void TriggeredArenaEntered()
	{
		StartCoroutine(ArenaSequence());
		StartCoroutine(ShowHealthBars());
	}

	private IEnumerator ArenaSequence()
	{
		GameObject[] fallingObjects = GameObject.FindGameObjectsWithTag("Fall");

		List<GameObject> objectsToDestroy = new List<GameObject>();

		foreach (GameObject obj in fallingObjects)
		{
			Rigidbody rb = obj.GetComponent<Rigidbody>();

			if (rb != null)
			{
				rb.useGravity = true;
			}

			objectsToDestroy.Add(obj);
		}

		yield return new WaitForSeconds(3f);

		foreach (GameObject obj in objectsToDestroy)
		{
			if (obj != null)
			{
				Destroy(obj);
			}
		}
	}

	private IEnumerator ShowHealthBars()
	{
		//shit UI
		yield return new WaitForSeconds(0.5f);
	}
}
