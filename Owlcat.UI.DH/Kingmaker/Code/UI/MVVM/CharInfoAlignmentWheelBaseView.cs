using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoAlignmentWheelBaseView : CharInfoComponentView<CharInfoAlignmentVM>
{
	[Header("Elements")]
	[SerializeField]
	private OwlcatMultiSelectable m_StateSelectable;

	[Header("Alignment Groups")]
	[SerializeField]
	protected CharInfoAlignmentGroupView TorianGroupView;

	[SerializeField]
	protected CharInfoAlignmentGroupView MonodominanceGroupView;

	[SerializeField]
	protected CharInfoAlignmentGroupView XanthiteGroupView;

	[SerializeField]
	protected CharInfoAlignmentGroupView XenophiliaGroupView;

	[Header("MixAbilities")]
	[SerializeField]
	protected CharInfoAlignmentMixView m_XantMonoMixView;

	[SerializeField]
	protected CharInfoAlignmentMixView m_XantTorMixView;

	[SerializeField]
	protected CharInfoAlignmentMixView m_XenoMonoMixView;

	[SerializeField]
	protected CharInfoAlignmentMixView m_XenoTorMixView;

	protected override void OnBind()
	{
		base.ViewModel.UnitIsXenos.Subscribe(delegate(bool s)
		{
			m_StateSelectable.SetActiveLayer(s ? "Xenos" : "Default");
		}).AddTo(this);
		base.ViewModel.TorianSector.Subscribe(delegate(CharInfoAlignmentGroupVM s)
		{
			TorianGroupView.BindSection(s);
		}).AddTo(this);
		base.ViewModel.MonodominanceSector.Subscribe(delegate(CharInfoAlignmentGroupVM s)
		{
			MonodominanceGroupView.BindSection(s);
		}).AddTo(this);
		base.ViewModel.XanthiteSector.Subscribe(delegate(CharInfoAlignmentGroupVM s)
		{
			XanthiteGroupView.BindSection(s);
		}).AddTo(this);
		base.ViewModel.XenophiliaSector.Subscribe(delegate(CharInfoAlignmentGroupVM s)
		{
			XenophiliaGroupView.BindSection(s);
		}).AddTo(this);
		base.ViewModel.XantMonoMix.Subscribe(delegate(CharInfoAlignmentMixVM s)
		{
			m_XantMonoMixView.BindSection(s);
		}).AddTo(this);
		base.ViewModel.XantTorMix.Subscribe(delegate(CharInfoAlignmentMixVM s)
		{
			m_XantTorMixView.BindSection(s);
		}).AddTo(this);
		base.ViewModel.XenoMonoMix.Subscribe(delegate(CharInfoAlignmentMixVM s)
		{
			m_XenoMonoMixView.BindSection(s);
		}).AddTo(this);
		base.ViewModel.XenoTorMix.Subscribe(delegate(CharInfoAlignmentMixVM s)
		{
			m_XenoTorMixView.BindSection(s);
		}).AddTo(this);
		base.OnBind();
	}
}
