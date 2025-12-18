using System;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.View.MapObjects;

[Serializable]
public class LocalMapMarkerSettings
{
	public LocalMapMarkType Type = LocalMapMarkType.VeryImportantPerson;

	[StringCreateWindow(StringCreateWindowAttribute.StringType.MapMarker)]
	public SharedStringAsset Description;

	[SerializeField]
	[FormerlySerializedAs("DescriptionUnit")]
	private BlueprintUnitReference m_DescriptionUnit;

	public bool StartHidden;

	public BlueprintUnit DescriptionUnit
	{
		get
		{
			return m_DescriptionUnit?.Get();
		}
		set
		{
			m_DescriptionUnit = value.ToReference<BlueprintUnitReference>();
		}
	}
}
