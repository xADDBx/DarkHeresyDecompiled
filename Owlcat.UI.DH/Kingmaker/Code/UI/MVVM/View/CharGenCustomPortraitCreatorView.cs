using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenCustomPortraitCreatorView : View<CharGenCustomPortraitCreatorVM>
{
	[SerializeField]
	private FadeAnimator m_Animator;

	[Header("Labels")]
	[SerializeField]
	private TextMeshProUGUI m_DescriptionLabel;

	[SerializeField]
	private TextMeshProUGUI m_OpenFolderLabel;

	[SerializeField]
	private TextMeshProUGUI m_RefreshPortraitLabel;

	[SerializeField]
	private TextMeshProUGUI m_HeaderLabel;

	[Header("Views")]
	[SerializeField]
	private CharGenPortraitView m_PortraitHalf;

	[SerializeField]
	private CharGenPortraitView m_PortraitSmall;

	[SerializeField]
	private CharGenPortraitView m_PortraitView;

	[Header("Buttons")]
	[SerializeField]
	protected OwlcatMultiButton m_OpenFolderButton;

	[SerializeField]
	protected OwlcatMultiButton m_RefreshPortraitButton;

	private bool m_IsInit;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_Animator.Initialize();
			m_PortraitHalf.Initialize();
			m_PortraitSmall.Initialize();
			m_IsInit = true;
		}
	}

	protected override void OnBind()
	{
		Show();
		base.ViewModel.Portrait.Subscribe(delegate(PortraitVM vm)
		{
			m_PortraitHalf.Bind(vm);
			m_PortraitSmall.Bind(vm);
			m_PortraitView.Or(null)?.Bind(vm);
			m_DescriptionLabel.text = UIStrings.Instance.CharGen.UploadPortraitManual;
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_OpenFolderButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnOpenFolderClick();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_RefreshPortraitButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnRefreshPortraitClick();
		}).AddTo(this);
		SetupLabels();
	}

	protected override void OnUnbind()
	{
		Hide();
	}

	protected virtual void Show()
	{
		m_Animator.AppearAnimation();
	}

	protected virtual void Hide()
	{
		m_Animator.DisappearAnimation();
	}

	private void SetupLabels()
	{
		m_OpenFolderLabel.text = UIStrings.Instance.CharGen.OpenPortraitFolder;
		m_RefreshPortraitLabel.text = UIStrings.Instance.CharGen.RefreshPortrait;
		m_HeaderLabel.text = UIStrings.Instance.CharGen.CustomPortraitHeader;
	}
}
