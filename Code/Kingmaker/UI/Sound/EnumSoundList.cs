using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using UnityEngine;

namespace Kingmaker.UI.Sound;

[Serializable]
public class EnumSoundList<TEnum> where TEnum : struct, Enum
{
	[SerializeField]
	private List<EnumSound<TEnum>> m_Sounds = new List<EnumSound<TEnum>>();

	public UISound GetSound(TEnum type)
	{
		return m_Sounds.FirstOrDefault((EnumSound<TEnum> x) => x.Enum.Equals(type))?.Sound ?? UISounds.Instance.Sounds.DoNothingEvent;
	}

	public void AddMissingValues()
	{
		if (m_Sounds == null)
		{
			m_Sounds = new List<EnumSound<TEnum>>();
		}
		foreach (object value in Enum.GetValues(typeof(TEnum)))
		{
			if (m_Sounds.Find((EnumSound<TEnum> x) => x.Enum.Equals((TEnum)value)) == null)
			{
				m_Sounds.Add(new EnumSound<TEnum>
				{
					Enum = (TEnum)value,
					Sound = UIConfig.Instance.BlueprintUISound?.NotificationsSounds.Notifications.NewInformation
				});
			}
		}
		m_Sounds.Sort((EnumSound<TEnum> x, EnumSound<TEnum> y) => x.Enum.CompareTo(y.Enum));
	}
}
