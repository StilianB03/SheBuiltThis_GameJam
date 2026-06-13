using UnityEngine;

public class LaserHitDetection : MonoBehaviour
{
	public float laserDmg = 10f;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			PlayerController player = other.GetComponent<PlayerController>();

			if (player != null)
			{
				player.TakeDamage(laserDmg);
			}
		}
	}
}
