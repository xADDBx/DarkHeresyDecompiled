using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoAlignmentWheelPCView : CharInfoComponentView<CharInfoAlignmentVM>, ICharInfoComponentConsoleView, ICharInfoComponentView
{
	[Header("SoulMarks Groups")]
	[SerializeField]
	private CharInfoSoulMarkSectorView m_TorianSectorView;

	[SerializeField]
	private CharInfoSoulMarkSectorView m_MonodominanceSectorView;

	[SerializeField]
	private CharInfoSoulMarkSectorView m_XanthiteSectorView;

	[SerializeField]
	private CharInfoSoulMarkSectorView m_XenophiliaSectorView;

	[Header("Background Groups")]
	[SerializeField]
	private GameObject m_MainCharGroup;

	[SerializeField]
	private GameObject m_CompanionGroup;

	private FloatConsoleNavigationBehaviour m_NavigationBehaviour;

	[SerializeField]
	private FloatConsoleNavigationBehaviour.NavigationParameters m_NavigationParameters;

	protected override void OnBind()
	{
		base.ViewModel.TorianSector.Subscribe(delegate(CharInfoAlignmentMarksSectorVM s)
		{
			m_TorianSectorView.BindSection(s);
		});
		base.ViewModel.MonodominanceSector.Subscribe(delegate(CharInfoAlignmentMarksSectorVM s)
		{
			m_MonodominanceSectorView.BindSection(s);
		});
		base.ViewModel.XanthiteSector.Subscribe(delegate(CharInfoAlignmentMarksSectorVM s)
		{
			m_XanthiteSectorView.BindSection(s);
		});
		base.ViewModel.XenophiliaSector.Subscribe(delegate(CharInfoAlignmentMarksSectorVM s)
		{
			m_XenophiliaSectorView.BindSection(s);
		});
		base.OnBind();
	}

	protected override void RefreshView()
	{
		base.RefreshView();
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour = new FloatConsoleNavigationBehaviour(m_NavigationParameters).AddTo(this);
		RefreshNavigation();
	}

	private void RefreshNavigation()
	{
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour.AddEntities(m_TorianSectorView.GetEntities());
		m_NavigationBehaviour.AddEntities(m_MonodominanceSectorView.GetEntities());
		m_NavigationBehaviour.AddEntities(m_XanthiteSectorView.GetEntities());
		m_NavigationBehaviour.AddEntities(m_XenophiliaSectorView.GetEntities());
	}

	public void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget)
	{
		navigationBehaviour.AddColumn<FloatConsoleNavigationBehaviour>(m_NavigationBehaviour);
	}
}
