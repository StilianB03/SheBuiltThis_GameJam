using UnityEngine;

public class LaserHitDetection : MonoBehaviour
{
	public float laserDmg = 10f;
	private SoundManager mySM;

	private void Start()
	{
		mySM = UnityEngine.Object.FindFirstObjectByType<SoundManager>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			PlayerController player = other.GetComponent<PlayerController>();

			if (player != null)
			{
				mySM.PlayOnce("playerHit");
				player.TakeDamage(laserDmg);
			}
		}
	}
}
