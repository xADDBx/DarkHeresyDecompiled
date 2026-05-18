using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.CombatText;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public sealed class ActionBarUnitView : CombatUnitView<CombatMechanicEntityVM>
{
	[SerializeField]
	private CombatCurrentUnitActionPointsSlider m_MPPrediction;

	[SerializeField]
	private CombatCurrentUnitActionPointsSlider m_APPrediction;

	[SerializeField]
	private CanvasGroup[] m_PredictionVisuals;

	[Header("Name (needed for enemy turn)")]
	[SerializeField]
	private TextMeshProUGUI m_UnitNameLabel;

	[SerializeField]
	private CombatTextBlockView m_CombatTextBlockView;

	[SerializeField]
	private CanvasGroup m_SquadGroup;

	[SerializeField]
	private SurfaceCombatUnitPortraitZone[] m_SquadPortraits;

	[SerializeField]
	private TextMeshProUGUI m_SquadRoleLabel;

	[SerializeField]
	private Image m_Frame;

	[SerializeField]
	private Sprite m_FriendFrame;

	[SerializeField]
	private Sprite m_EnemyFrame;

	[SerializeField]
	private GameObject[] m_HideOnPortraitHiddenObjects;

	private readonly CompositeDisposable m_Disposable = new CompositeDisposable();

	protected override void OnBind()
	{
		if (!base.ViewModel.HasUnit)
		{
			return;
		}
		base.OnBind();
		base.ViewModel.ForceHidePortrait.Subscribe(delegate(bool forceHide)
		{
			GameObject[] hideOnPortraitHiddenObjects = m_HideOnPortraitHiddenObjects;
			for (int i = 0; i < hideOnPortraitHiddenObjects.Length; i++)
			{
				hideOnPortraitHiddenObjects[i].SetActive(!forceHide);
			}
		}).AddTo(this);
		base.ViewModel.MechanicEntityUIState.IsInCombat.And(base.ViewModel.IsCurrent.Or(base.ViewModel.MechanicEntityUIState.IsPreparationTurn)).Subscribe(SetPredictionVisuals).AddTo(this);
		base.ViewModel.ActionPointVM.Subscribe(delegate(ActionPointsVM points)
		{
			m_Disposable.Clear();
			if (points != null && base.ViewModel.IsPlayer.CurrentValue)
			{
				m_MPPrediction.SetVisible(visible: true);
				m_APPrediction.SetVisible(visible: true);
				m_Disposable.Add(points.CurrentMP.Subscribe(m_MPPrediction.SetCurrentValue));
				m_Disposable.Add(points.CurrentAP.Subscribe(m_APPrediction.SetCurrentValue));
				m_Disposable.Add(m_MPPrediction.Bind(points.MaxMP, points.CurrentMP, points.PredictedMP));
				m_Disposable.Add(m_APPrediction.Bind(points.MaxAP, points.CurrentAP, points.PredictedAP));
			}
			else
			{
				m_MPPrediction.SetVisible(visible: false);
				m_MPPrediction.Dispose();
				m_APPrediction.SetVisible(visible: false);
				m_APPrediction.Dispose();
			}
		}).AddTo(this);
		m_CombatTextBlockView.Bind(base.ViewModel.CombatTextBlockVM);
		base.ViewModel.IsInSquad.Subscribe(SetupSquad).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(Button.OnRightClickAsObservable(), delegate
		{
			InvokeUnitInspect();
		}).AddTo(this);
		SetupFrame(base.ViewModel.FactionInfo.CurrentValue.isEnemy);
		SetupName();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		SetPredictionVisuals(visible: false);
		m_Disposable.Clear();
	}

	private void SetupFrame(bool isEnemy)
	{
		m_Frame.sprite = (isEnemy ? m_EnemyFrame : m_FriendFrame);
	}

	private void SetupName()
	{
		m_UnitNameLabel.text = base.ViewModel.DisplayName;
	}

	private void SetPredictionVisuals(bool visible)
	{
		CanvasGroup[] predictionVisuals = m_PredictionVisuals;
		for (int i = 0; i < predictionVisuals.Length; i++)
		{
			predictionVisuals[i].alpha = (visible ? 1f : 0f);
		}
	}

	private void SetupSquad(bool isInSquad)
	{
		SurfaceCombatUnitPortraitZone[] squadPortraits = m_SquadPortraits;
		for (int i = 0; i < squadPortraits.Length; i++)
		{
			squadPortraits[i].Hide();
		}
		if (!isInSquad)
		{
			m_SquadGroup.alpha = 0f;
			return;
		}
		int num = 0;
		foreach (UnitReference unit in base.ViewModel.Squad.Units)
		{
			if (num >= m_SquadPortraits.Length)
			{
				break;
			}
			m_SquadPortraits[num].SetUnit(unit.ToBaseUnitEntity());
		}
		m_SquadRoleLabel.text = (base.ViewModel.IsSquadLeader.CurrentValue ? UIStrings.Instance.HUDTexts.SquadLeaderHint : UIStrings.Instance.HUDTexts.SquadMemberHint);
		m_SquadGroup.alpha = 1f;
	}

	private void InvokeUnitInspect()
	{
		EventBus.RaiseEvent(delegate(IUnitClickUIHandler h)
		{
			h.HandleUnitRightClick(base.ViewModel.UnitAsBaseUnitEntity);
		});
	}
}
