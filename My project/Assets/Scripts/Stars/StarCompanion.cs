using UnityEngine;
using System.Collections;
using System;

public class StarCompanion : MonoBehaviour
{
	[Header("Follow Settings")]
	public Transform player; 
	public float activationRange = 5f;
	public float followSpeed = 5f;
	public Vector3 offset = new Vector3(0, 1.5f, 0);

	[Header("Hover Settings")]
	public float hoverSpeed = 5f;   
	public float hoverHeight = 0.5f; 
	public bool isAbsorbed = false;

	private bool isCollected = false; 
	private bool isRegistered = false;
	private bool partnerEffectTriggered = false;
	
	public enum StarType { Regular, Partner }
	public StarType type;
	private Vector3 startPosition;
	private SoundManager mySM;

	void Start()
	{
		startPosition = transform.position;
		mySM = UnityEngine.Object.FindFirstObjectByType<SoundManager>();
	}

	void FixedUpdate()
	{
		if (player == null) return;

		float distance = Vector3.Distance(transform.position, player.position);
		if (distance < activationRange)
		{
			if (type == StarType.Partner && !partnerEffectTriggered)
			{
				StartCoroutine(FoundPartner());
				partnerEffectTriggered = true;
			}

			isCollected = true;
		}

		if (isCollected)
		{
			FollowPlayer();

			// Register only once
			if (!isRegistered)
			{
				RegisterWithManager();
			}
		}
		else
		{
			float hover = Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
			transform.position = startPosition + new Vector3(0, hover, 0);
		}
	}

	private void RegisterWithManager()
	{
		PlayerController pc = player.GetComponent<PlayerController>();
		if (pc != null && pc.isFinalBoss)
		{
			StarManager manager = FindAnyObjectByType<StarManager>();
			if (manager != null)
			{
				isRegistered = true; // Prevents calling this every frame
			}
		}
	}

	private void FollowPlayer()
	{
		Vector3 targetPos = player.position + offset;
		transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
		transform.Rotate(Vector3.up * 100 * Time.deltaTime);
	}

	private IEnumerator FoundPartner()
	{
		mySM.PlayOnce("starMeeting"); 
		yield break;
	}

	public void SetEmissionIntensity(float intensity)
	{
		Renderer rend = GetComponent<Renderer>();
		if (rend != null)
		{
			rend.material.SetColor("_emissive_color", Color.yellow * intensity);
		}
	}

	public void SetAppearance(Color color, float emissionIntensity, float alpha)
	{
		Renderer rend = GetComponent<Renderer>();
		if (rend != null)
		{
			rend.material.SetColor("_emissive_color", color * emissionIntensity);

			Color baseColor = rend.material.GetColor("_Base_Color_1");
			baseColor.a = alpha;
			rend.material.SetColor("_Base_Color_1", baseColor);
		}
	}
}