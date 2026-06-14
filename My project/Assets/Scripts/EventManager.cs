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
		yield return new WaitForSeconds(0.5f);
	}

	private IEnumerator ShowHealthBars()
	{
		//shit falls
		yield return new WaitForSeconds(0.5f);
	}
}
