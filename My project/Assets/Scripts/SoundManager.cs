using UnityEngine;
using System;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
	public static SoundManager instance;

	[Serializable]
	public class SoundEntry
	{
		public string name;
		public AudioClip clip;
	}

	[Header("Boss Sounds")]
	public List<SoundEntry> effects;

	[Header("Player Sounds")]
	public List<SoundEntry> environment;

	private AudioSource audioSource;

	void Awake()
	{
		if (instance == null) instance = this;
		else Destroy(gameObject);
		audioSource = GetComponent<AudioSource>();
	}

	private AudioClip GetClip(string name)
	{
		SoundEntry entry = effects.Find(s => s.name == name) ??
						   environment.Find(s => s.name == name);

		if (entry != null)
		{
			return entry?.clip;
		}
	}

	public void PlayOnce(string name)
	{
		AudioClip clip = GetClip(name);
		if (clip) audioSource.PlayOneShot(clip);
	}

	public void PlayLooping(string name)
	{
		AudioClip clip = GetClip(name);
		if (clip && (audioSource.clip != clip || !audioSource.isPlaying))
		{
			audioSource.clip = clip;
			audioSource.loop = true;
			audioSource.Play();
		}
	}

	public void StopLooping()
	{
		audioSource.Stop();
		audioSource.loop = false;
		audioSource.clip = null;
	}
}