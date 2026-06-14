using UnityEngine;

public class EnteredArenaDetection : MonoBehaviour
{
	private EventManager myEM;
	public Collider trapCollider;
	public Collider realTrigg;

	private void Awake()
	{
		myEM = FindFirstObjectByType<EventManager>();
	}

	private void OnTriggerEnter(Collider other) 
	{
		if (other.CompareTag("Player"))
		{
			if (trapCollider != null)
			{
				trapCollider.enabled = true;
			}

			if (realTrigg != null)
			{
				Destroy(realTrigg);
			}

			if (myEM != null)
			{
				Debug.Log("Player entered arena, setting flag in EventManager.");
				myEM.arenaEntered = true;
			}
		}
	}
}