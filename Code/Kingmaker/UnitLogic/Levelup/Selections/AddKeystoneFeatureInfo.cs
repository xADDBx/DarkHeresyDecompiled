using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.Selections;

[Serializable]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintPath))]
[ComponentName("Facts And Buffs/AddKeystoneFeatureInfo")]
[TypeId("314c8c682eeb8f9499785f0e590fecfd")]
public class AddKeystoneFeatureInfo : BlueprintComponent
{
	[SerializeField]
	private BpRef<BlueprintFeature> m_Feature = new BpRef<BlueprintFeature>();

	[SerializeField]
	private Sprite m_AlternativeIcon;

	[SerializeField]
	private LocalizedString m_Title;

	[SerializeField]
	private LocalizedString m_Description;

	public BlueprintFeature Feature => m_Feature;

	public Sprite Icon
	{
		get
		{
			if (!(m_AlternativeIcon == null))
			{
				return m_AlternativeIcon;
			}
			return Feature?.Icon;
		}
	}

	public string Title
	{
		get
		{
			if (!m_Title.IsEmpty())
			{
				return m_Title;
			}
			return Feature?.Name;
		}
	}

	public string Description
	{
		get
		{
			if (!m_Description.IsEmpty())
			{
				return m_Description;
			}
			return Feature?.Description;
		}
	}
}
