using UnityEngine;
using UnityEngine.VFX;

public class Projectile : MonoBehaviour
{
	public float lifetime = 3f;
	public float bulletDmg = 7.5f;
	public VisualEffect vfx;
	private float speed;

	public void Initialize(float speedFromData)
	{
		speed = speedFromData;
	}

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
		BossController boss = other.GetComponentInParent<BossController>();

		if (boss != null)
		{
			if (other == boss.hitbox1 || other == boss.hitbox2)
			{
				boss.TakeDamage(bulletDmg);
				Debug.Log("Boss HIT!");
				Destroy(gameObject);
			}
		}
	}
}