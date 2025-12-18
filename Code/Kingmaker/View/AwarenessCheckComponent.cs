using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Gameplay.Parts;
using Kingmaker.View.MapObjects;
using UnityEngine;

namespace Kingmaker.View;

[RequireComponent(typeof(EntityViewBase))]
[DisallowMultipleComponent]
[KnowledgeDatabaseID("df652852c690a714e9d6119f9873ed7e")]
public class AwarenessCheckComponent : EntityPartComponent<PartAwarenessCheck, AwarenessCheckSettings>, ISerializationCallbackReceiver
{
	[SerializeField]
	[HideInInspector]
	[Obsolete("VS")]
	private bool _converted;

	[HideInInspector]
	[Obsolete("VS")]
	public SkillCheckDifficulty Difficulty;

	[SerializeField]
	[HideInInspector]
	[Obsolete("VS")]
	private int DC;

	[HideInInspector]
	[Obsolete("VS")]
	public float Radius = 7f;

	[Obsolete("VS")]
	public int GetDC()
	{
		if (Difficulty != 0)
		{
			return Difficulty.GetDC();
		}
		return DC;
	}

	[Obsolete("VS")]
	public int GetCustomDC()
	{
		return DC;
	}

	[Obsolete("VS")]
	public void SetCustomDC(int value)
	{
		DC = value;
	}

	private void ConvertSettings()
	{
		if (!_converted)
		{
			Settings.Difficulty = Difficulty;
			Settings.CustomDifficulty = DC;
			Settings.Radius = Radius;
			_converted = true;
		}
	}

	private void Reset()
	{
		_converted = true;
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		ConvertSettings();
	}
}
