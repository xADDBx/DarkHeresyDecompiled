using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Localization;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoPartPreciseAttack : UnitInfoPart
{
	[Serializable]
	private struct InfoElement
	{
		public UnitInfoPartElementView View;

		public AppliedMarker AppliedMarker;
	}

	[Serializable]
	private struct CompareInfoElement
	{
		public UnitInfoPartCompareElementView View;

		public AppliedMarker AppliedMarker;
	}

	[Serializable]
	private struct AppliedMarker
	{
		public GameObject AppliedObject;

		public GameObject NotAppliedObject;

		public void SetApplied(bool isApplied)
		{
			AppliedObject.SetActive(isApplied);
			NotAppliedObject.SetActive(!isApplied);
		}
	}

	[SerializeField]
	private InfoElement[] m_CriticalEffects;

	[SerializeField]
	private CompareInfoElement[] m_CompareCriticalEffects;

	[SerializeField]
	private UnitInfoPartElementView m_AdditionalEffectPrefab;

	[SerializeField]
	private Transform m_AdditionalEffectRoot;

	[SerializeField]
	private UnitInfoPartCompareElementView m_CompareAdditionalEffectPrefab;

	[SerializeField]
	private Transform m_CompareAdditionalEffectRoot;

	[SerializeField]
	private UnitInfoPartElementView m_CritsThroughArmorElement;

	[SerializeField]
	private UnitInfoPartHeader m_ProtectedByArmorElement;

	[SerializeField]
	private MonoBehaviour m_CritsThroughArmorHintSource;

	[SerializeField]
	private MonoBehaviour m_ProtectedByArmorHintSource;

	private InfoElement[] m_CompareElements;

	private IDisposable m_ArmorHintDisposable;

	private IDisposable m_CritsThroughArmorHintDisposable;

	private readonly List<UnitInfoPartElementView> m_AdditionalEffectPool = new List<UnitInfoPartElementView>();

	private readonly List<UnitInfoPartCompareElementView> m_CompareAdditionalEffectPool = new List<UnitInfoPartCompareElementView>();

	private readonly List<string> m_AdditionalEffectBuffer = new List<string>();

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.Data.BodyPart.Subscribe(SetupMainBlock).AddTo(this);
		base.ViewModel.CompareData.BodyPart.Subscribe(SetupCompareBlock).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_ArmorHintDisposable?.Dispose();
		m_ArmorHintDisposable = null;
		m_CritsThroughArmorHintDisposable?.Dispose();
		m_CritsThroughArmorHintDisposable = null;
	}

	protected override void ShowImpl(UnitInfoPartState state)
	{
		base.gameObject.SetActive(state.HasHit && state.IsPreciseAttack && !state.PreciseAttackHasNoTarget);
	}

	private void SetupMainBlock(BlueprintBodyPart bodyPart)
	{
		int effectsCount = bodyPart?.CriticalEffectStagesCount ?? 0;
		CreateCriticalEffects(bodyPart, m_CriticalEffects, effectsCount);
		RebuildAdditionalEffects(bodyPart);
		SetupCriticalEffectsHintElements();
		base.ViewModel.SetDirtyContent(isDirty: true);
	}

	private void SetupCompareBlock(BlueprintBodyPart bodyPart)
	{
		int num = 0;
		BlueprintBodyPart currentValue = base.ViewModel.Data.BodyPart.CurrentValue;
		if (bodyPart != null && currentValue != null)
		{
			num = Mathf.Min(bodyPart.CriticalEffectStagesCount, currentValue.CriticalEffectStagesCount);
		}
		if (m_CompareElements == null)
		{
			m_CompareElements = m_CompareCriticalEffects.Select(delegate(CompareInfoElement e)
			{
				InfoElement result = default(InfoElement);
				result.View = e.View.ElementView;
				result.AppliedMarker = e.AppliedMarker;
				return result;
			}).ToArray();
		}
		CreateCriticalEffects(bodyPart, m_CompareElements, num);
		RebuildCompareAdditionalEffects(bodyPart);
		for (int i = 0; i < num; i++)
		{
			bool alreadyApplied;
			int criticalEffectChance = base.ViewModel.GetCriticalEffectChance(currentValue, i, out alreadyApplied);
			bool alreadyApplied2;
			int criticalEffectChance2 = base.ViewModel.GetCriticalEffectChance(bodyPart, i, out alreadyApplied2);
			bool valueChanged = criticalEffectChance != criticalEffectChance2 && !alreadyApplied && !alreadyApplied2;
			bool valueIncreased = (alreadyApplied && !alreadyApplied2) || criticalEffectChance < criticalEffectChance2;
			m_CompareCriticalEffects[i].View.SetValueChangeMarker(valueChanged, valueIncreased);
		}
	}

	private void CreateCriticalEffects(BlueprintBodyPart bodyPart, InfoElement[] criticalEffectsList, int effectsCount)
	{
		InfoElement[] array;
		if (bodyPart == null)
		{
			array = criticalEffectsList;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].View.SetActive(active: false);
			}
			base.ViewModel.SetDirtyContent(isDirty: true);
			return;
		}
		array = criticalEffectsList;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].View.SetActive(active: false);
		}
		if (effectsCount != 0)
		{
			for (int j = 0; j < effectsCount; j++)
			{
				BlueprintBuff criticalEffectStageBuff = bodyPart.GetCriticalEffectStageBuff(j + 1);
				bool alreadyApplied;
				int criticalEffectChance = base.ViewModel.GetCriticalEffectChance(bodyPart, j, out alreadyApplied);
				AddCriticalEffect(j, criticalEffectStageBuff, criticalEffectChance, alreadyApplied, criticalEffectsList);
			}
		}
	}

	private void AddCriticalEffect(int index, BlueprintBuff criticalEffect, int effectChance, bool alreadyApplied, InfoElement[] criticalEffectsList)
	{
		int num = Mathf.Clamp(index, 0, criticalEffectsList.Length - 1);
		InfoElement infoElement = criticalEffectsList[num];
		infoElement.View.SetIcon(criticalEffect.Icon);
		string text = ((effectChance > 0 || alreadyApplied) ? criticalEffect.Name : ("<s>" + criticalEffect.Name + "</s>"));
		infoElement.View.SetName(text);
		infoElement.View.SetTooltip(new TooltipTemplateSimple(criticalEffect.Name, criticalEffect.Description)).AddTo(this);
		infoElement.View.SetValue($"{effectChance}%");
		infoElement.AppliedMarker.SetApplied(alreadyApplied);
		infoElement.View.SetActive(active: true);
	}

	private void RebuildAdditionalEffects(BlueprintBodyPart bodyPart)
	{
		ClearPool(m_AdditionalEffectPool);
		base.ViewModel.CollectAdditionalEffects(bodyPart, m_AdditionalEffectBuffer);
		foreach (string item in m_AdditionalEffectBuffer)
		{
			UnitInfoPartElementView widget = WidgetFactory.GetWidget(m_AdditionalEffectPrefab, activate: false, strictMatching: true);
			widget.transform.SetParent(m_AdditionalEffectRoot, worldPositionStays: false);
			widget.SetName(item);
			widget.gameObject.SetActive(value: true);
			m_AdditionalEffectPool.Add(widget);
		}
	}

	private void RebuildCompareAdditionalEffects(BlueprintBodyPart bodyPart)
	{
		ClearPool(m_CompareAdditionalEffectPool);
		base.ViewModel.CollectAdditionalEffects(bodyPart, m_AdditionalEffectBuffer);
		foreach (string item in m_AdditionalEffectBuffer)
		{
			UnitInfoPartCompareElementView widget = WidgetFactory.GetWidget(m_CompareAdditionalEffectPrefab, activate: false, strictMatching: true);
			widget.transform.SetParent(m_CompareAdditionalEffectRoot, worldPositionStays: false);
			widget.SetName(item);
			widget.gameObject.SetActive(value: true);
			m_CompareAdditionalEffectPool.Add(widget);
		}
	}

	private static void ClearPool<T>(List<T> pool) where T : MonoBehaviour
	{
		foreach (T item in pool)
		{
			WidgetFactory.DisposeWidget(item);
		}
		pool.Clear();
	}

	private void SetupCriticalEffectsHintElements()
	{
		int critsThroughArmor = base.ViewModel.GetCritsThroughArmor();
		int num = base.ViewModel.Data.BodyPart.CurrentValue?.CriticalEffectStagesCount ?? 0;
		bool num2 = base.ViewModel.Data.ArmorLeft.CurrentValue > 0;
		bool flag = base.ViewModel.HealthDamage > 0;
		bool flag2 = num2 && !flag;
		bool flag3 = critsThroughArmor > 0 && flag2 && num > 0;
		m_CritsThroughArmorElement.SetActive(flag3);
		m_ProtectedByArmorElement.SetActive(flag2);
		m_ArmorHintDisposable?.Dispose();
		m_CritsThroughArmorHintDisposable?.Dispose();
		if (flag2)
		{
			LocalizedString criticalEffectsLockedByArmorHint = UIStrings.Instance.UnitInfo.CriticalEffectsLockedByArmorHint;
			m_ArmorHintDisposable = m_ProtectedByArmorHintSource.SetHint(criticalEffectsLockedByArmorHint);
		}
		if (flag3)
		{
			GameLogContext.Count = critsThroughArmor;
			LocalizedString criticalEffectsThroughArmorHint = UIStrings.Instance.UnitInfo.CriticalEffectsThroughArmorHint;
			m_CritsThroughArmorHintDisposable = m_CritsThroughArmorHintSource.SetHint(criticalEffectsThroughArmorHint);
			LocalizedString criticalEffectsThroughArmor = UIStrings.Instance.UnitInfo.CriticalEffectsThroughArmor;
			m_CritsThroughArmorElement.SetName(criticalEffectsThroughArmor);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ClearPool(m_AdditionalEffectPool);
		ClearPool(m_CompareAdditionalEffectPool);
	}

	private void Awake()
	{
		LocalizedString criticalEffectsLockedByArmor = UIStrings.Instance.UnitInfo.CriticalEffectsLockedByArmor;
		m_ProtectedByArmorElement.SetTitle(criticalEffectsLockedByArmor);
	}
}
