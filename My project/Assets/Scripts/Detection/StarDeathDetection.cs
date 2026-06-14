using UnityEngine;
using System;
using System.Collections.Generic;

public class StarDeathDetection : MonoBehaviour
{
	public List<StarCompanion> myWaveStars;

	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			FindObjectOfType<StarManager>().TriggerWaveAbsorption(myWaveStars);
			gameObject.SetActive(false); // Disable trigger so it only fires once
		}
	}
}