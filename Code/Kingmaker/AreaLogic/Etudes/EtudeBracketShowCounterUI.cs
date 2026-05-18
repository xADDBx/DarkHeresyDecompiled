using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Framework;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AreaLogic.Etudes;

[Serializable]
[TypeId("15e731eb83be4db9a1840339fff7b8ea")]
public class EtudeBracketShowCounterUI : EtudeBracketTrigger
{
	public LocalizedString Label;

	public PropertyCalculator Value;

	public PropertyCalculator TargetValue;

	protected override void OnEnter()
	{
		Show();
	}

	protected override void OnExit()
	{
		Hide();
	}

	protected override void OnResume()
	{
		Show();
	}

	private string GetCounterId()
	{
		return base.Fact.Blueprint.AssetGuid + name;
	}

	private void Show()
	{
		BaseUnitEntity mainCharacter = Game.Instance.Player.MainCharacterEntity;
		IEvalContext evalContext = base.Context;
		EventBus.RaiseEvent(delegate(IEtudeCounterHandler h)
		{
			h.ShowEtudeCounter(GetCounterId(), Label, ValueGetter, TargetValueGetter);
		});
		int TargetValueGetter()
		{
			return TargetValue.GetValue(mainCharacter, evalContext);
		}
		int ValueGetter()
		{
			return Value.GetValue(mainCharacter, evalContext);
		}
	}

	private void Hide()
	{
		EventBus.RaiseEvent(delegate(IEtudeCounterHandler h)
		{
			h.HideEtudeCounter(GetCounterId());
		});
	}
}
