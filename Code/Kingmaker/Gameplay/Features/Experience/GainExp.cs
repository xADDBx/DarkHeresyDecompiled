using System;
using Kingmaker.Code.Gameplay.Enums.Stats;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Experience;

[Obsolete]
[PlayerUpgraderAllowed(false)]
[TypeId("db6095c1906d85748b0a8506de7c9dd7")]
public sealed class GainExp : GameAction, IExperienceSettings
{
	[SerializeField]
	private ExperienceType _type;

	[SerializeField]
	private bool _override;

	[SerializeField]
	[ShowIf("_override")]
	private int _overrideValue;

	[SerializeField]
	[HideIf("_override")]
	private bool _overrideCR;

	[SerializeField]
	[ShowIf("ShowOverrideCRValue")]
	private int _overrideCRValue;

	[SerializeField]
	private string _comment;

	[InfoBox("Этот блок настроек - meta информация для статистики")]
	public Chapter Chapter;

	public Cluster Cluster;

	public ExperienceType Type => _type;

	public int? OverrideValue
	{
		get
		{
			if (!_override)
			{
				return null;
			}
			return _overrideValue;
		}
	}

	public int? OverrideCR
	{
		get
		{
			if (!_overrideCR)
			{
				return null;
			}
			return _overrideCRValue;
		}
	}

	private bool ShowOverrideCRValue
	{
		get
		{
			if (!_override)
			{
				return _overrideCR;
			}
			return false;
		}
	}

	public string Comment => _comment;

	public override string GetDescription()
	{
		return "Выдает игроку опыт в соответствии с указанным типом";
	}

	public override string GetCaption()
	{
		if (!_override)
		{
			return string.Format("Gain XP (CR={0}, {1})", OverrideCR?.ToString() ?? "Area", Type);
		}
		return $"Gain XP (Value={_overrideValue}, {Type})";
	}

	protected override void RunAction()
	{
		Experience.Gain(this);
	}
}
