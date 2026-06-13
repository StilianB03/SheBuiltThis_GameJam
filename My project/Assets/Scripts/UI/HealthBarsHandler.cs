using UnityEngine;
using UnityEngine.UI; // Required for using Slider components
using System;

public class HealthBarsHandler : MonoBehaviour
{
	public enum HealthTarget { Player, Boss }

	[Header("Setup")]
	public HealthTarget target;
	public Slider healthSlider;

	private void OnEnable()
	{
		if (target == HealthTarget.Player)
		{
			PlayerController.OnHealthChanged += UpdateSlider;
		}
		else if (target == HealthTarget.Boss)
		{
			BossController.OnHealthChanged += UpdateSlider;
		}
	}

	private void OnDisable()
	{
		if (target == HealthTarget.Player)
		{
			PlayerController.OnHealthChanged -= UpdateSlider;
		}
		else if (target == HealthTarget.Boss)
		{
			BossController.OnHealthChanged -= UpdateSlider;
		}
	}

	private void UpdateSlider(float currentHealth, float maxHealth)
	{
		if (healthSlider != null)
		{
			healthSlider.value = (currentHealth / maxHealth) * healthSlider.maxValue;
		}
	}
}