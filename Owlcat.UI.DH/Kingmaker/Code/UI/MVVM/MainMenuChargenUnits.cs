using System;
using System.Collections;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.DLC;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.Core.Utility;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class MainMenuChargenUnits : IDisposable
{
	public static MainMenuChargenUnits Instance;

	private readonly ReactiveProperty<ChargenUnit> m_CurrentPregenUnit = new ReactiveProperty<ChargenUnit>();

	private List<ChargenUnit> m_PregensForChargen;

	private List<ChargenUnit> m_AllUnits;

	public BlueprintDlcRewardCampaign DlcReward;

	public ReadOnlyReactiveProperty<ChargenUnit> CurrentPregenUnit => m_CurrentPregenUnit;

	public BlueprintPortrait CustomCharacterPortrait => ConfigRoot.Instance.CharGenRoot.CustomPortrait;

	public MainMenuChargenUnits()
	{
		Instance = this;
		EnsureNewGamePregens(null);
	}

	public void Dispose()
	{
		Instance = null;
		CurrentPregenUnit?.Dispose();
		DisposeUnitsForChargen();
	}

	public void EnsureNewGamePregens(Action<List<ChargenUnit>> readyCallback)
	{
		if (m_PregensForChargen == null)
		{
			MonoSingleton<CoroutineRunner>.Instance.StartCoroutine(PrepareNewGamePregensCoroutine(readyCallback));
		}
		else
		{
			readyCallback?.Invoke(m_PregensForChargen);
		}
	}

	private IEnumerator PrepareNewGamePregensCoroutine(Action<List<ChargenUnit>> readyCallback = null)
	{
		m_PregensForChargen = new List<ChargenUnit>();
		ReferenceArrayProxy<BlueprintUnit> pregens = BlueprintCharGenRoot.Instance.Pregens;
		yield return PrepareChargenUnitsCoroutine(pregens, m_PregensForChargen, readyCallback);
	}

	private IEnumerator PrepareChargenUnitsCoroutine(IEnumerable<BlueprintUnit> units, List<ChargenUnit> resultList, Action<List<ChargenUnit>> readyCallback = null)
	{
		if (m_AllUnits == null)
		{
			m_AllUnits = new List<ChargenUnit>();
		}
		foreach (BlueprintUnit unit in units)
		{
			ChargenUnit item = new ChargenUnit(unit);
			resultList.Add(item);
			m_AllUnits.Add(item);
			yield return null;
		}
		readyCallback?.Invoke(resultList);
	}

	private void DisposeUnitsForChargen()
	{
		foreach (ChargenUnit item in m_AllUnits.EmptyIfNull())
		{
			if (ShouldDisposeChargenUnit(item.Unit))
			{
				item.Unit.Dispose();
			}
		}
		m_AllUnits?.Clear();
		m_AllUnits = null;
		m_PregensForChargen?.Clear();
		m_PregensForChargen = null;
	}

	private static bool ShouldDisposeChargenUnit(BaseUnitEntity unit)
	{
		if (unit.IsDisposed || unit.IsMainCharacter)
		{
			return false;
		}
		UnitPartCompanion companionOptional = unit.GetCompanionOptional();
		bool flag;
		if (companionOptional != null)
		{
			CompanionState state = companionOptional.State;
			if ((uint)(state - 1) <= 1u || state == CompanionState.InPartyDetached)
			{
				flag = true;
				goto IL_0033;
			}
		}
		flag = false;
		goto IL_0033;
		IL_0033:
		if (flag)
		{
			return false;
		}
		return true;
	}
}
