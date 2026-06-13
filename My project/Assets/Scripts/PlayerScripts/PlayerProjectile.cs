using UnityEngine;
using UnityEngine.VFX;

public class Projectile : MonoBehaviour
{
	public float speed = 20f;
	public float lifetime = 3f;
	public VisualEffect vfx;

	void Start()
	{
		Vector3 currentRotation = transform.rotation.eulerAngles;
		transform.rotation = Quaternion.Euler(90f, currentRotation.y, currentRotation.z);

		Destroy(gameObject, lifetime);
		if (vfx != null) vfx.Play();
	}

	void Update()
	{
		transform.position += transform.up * speed * Time.deltaTime;
	}

	private void OnTriggerEnter(Collider other)
	{
		Debug.Log("Hit: " + other.name);

		Destroy(gameObject);
	}
}