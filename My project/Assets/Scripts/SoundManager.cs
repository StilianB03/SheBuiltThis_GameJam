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
		[Range(0f, 1f)] public float volume = 1.0f;
		[Range(0.1f, 8.0f)] public float pitch = 1.0f;
	}

	[Header("Boss Sounds")]
	public List<SoundEntry> bossEffects;

	[Header("Player Sounds")]
	public List<SoundEntry> playerEffects;

	[Header("Star Sounds")]
	public List<SoundEntry> starEffects;

	private AudioSource audioSource;

	void Awake()
	{
		if (instance == null) instance = this;
		else Destroy(gameObject);
		audioSource = GetComponent<AudioSource>();
	}

	private SoundEntry GetEntry(string name)
	{
		SoundEntry entry = bossEffects.Find(s => s.name == name) ??
						   playerEffects.Find(s => s.name == name) ??
						   starEffects.Find(s => s.name == name);

		if (entry == null)
			Debug.LogWarning("SoundManager: Clip not found - " + name);

		return entry;
	}

	public void PlayOnce(string name)
	{
		SoundEntry entry = GetEntry(name);
		if (entry != null && entry.clip != null)
		{
			audioSource.pitch = entry.pitch;
			audioSource.PlayOneShot(entry.clip, entry.volume);
		}
	}

	public void PlayLooping(string name)
	{
		SoundEntry entry = GetEntry(name);
		if (entry != null && entry.clip != null)
		{
			if (audioSource.clip != entry.clip || !audioSource.isPlaying)
			{
				audioSource.clip = entry.clip;
				audioSource.volume = entry.volume;
				audioSource.pitch = entry.pitch;
				audioSource.loop = true;
				audioSource.Play();
			}
		}
	}

	public void StopLooping()
	{
		audioSource.Stop();
		audioSource.loop = false;
		audioSource.clip = null;
	}
}