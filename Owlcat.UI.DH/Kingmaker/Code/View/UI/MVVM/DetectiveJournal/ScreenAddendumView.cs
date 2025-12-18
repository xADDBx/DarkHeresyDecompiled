using DG.Tweening;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ScreenAddendumView : View<AddendumInfoVM>
{
	[Header("Elements")]
	[SerializeField]
	private RectTransform m_ParentTransform;

	[SerializeField]
	private TMP_Text m_AddendumId;

	[SerializeField]
	private TMP_Text m_Description;

	[SerializeField]
	private OwlcatMultiButton m_Button;

	[SerializeField]
	private OwlcatMultiSelectable m_AddendumState;

	[SerializeField]
	private OwlcatMultiButton m_GoToLinkedClue;

	[Header("Views")]
	[SerializeField]
	private CaseEntitySourceView m_CaseEntitySourceView;

	[SerializeField]
	private TextStyle m_AddendumTextStyle;

	[SerializeField]
	private TextStyle m_ClueTextStyle;

	[Header("Values")]
	[SerializeField]
	private float m_DefaultHeight = 150f;

	[SerializeField]
	private float m_Spacing = 10f;

	[SerializeField]
	private int m_DefaultHeightSymbolsCount = 120;

	[SerializeField]
	private float m_StudyDelay = 1f;

	private CanvasGroup m_CanvasGroup;

	private Sequence m_BlinkSequence;

	public int BlocksCount { get; private set; }

	protected override void OnBind()
	{
		if ((object)m_CanvasGroup == null)
		{
			m_CanvasGroup = base.gameObject.EnsureComponent<CanvasGroup>();
		}
		m_CanvasGroup.alpha = 0f;
		base.ViewModel.Description.Subscribe(delegate
		{
			using (GameLogContext.Scope)
			{
				GameLogContext.CaseItem = base.ViewModel.LinkedClue.CurrentValue;
				GameLogContext.TextStyle = m_ClueTextStyle;
				string text = base.ViewModel.Description.CurrentValue.Text;
				GameLogContext.TextStyle = UIConfig.Instance.DefaultTextStyle;
				text = text.TrimEnd(' ', '.');
				BlocksCount = Mathf.FloorToInt(1f * (float)text.Length / (float)m_DefaultHeightSymbolsCount) + 1;
				float y = (float)BlocksCount * m_DefaultHeight + (float)(BlocksCount - 1) * m_Spacing;
				m_ParentTransform.sizeDelta = new Vector2(m_ParentTransform.sizeDelta.x, y);
				m_Description.text = "<style=" + m_AddendumTextStyle.Style.name + ">" + text + "</style>";
			}
		}).AddTo(this);
		base.ViewModel.AddendumState.Subscribe(delegate(AddendumState value)
		{
			m_AddendumState.SetActiveLayer(value.ToString());
		}).AddTo(this);
		m_Button.OnLeftClickAsObservable().Subscribe(base.ViewModel.MarkAsViewed).AddTo(this);
		m_CaseEntitySourceView.Bind(base.ViewModel.SourceVM);
		m_GoToLinkedClue.OnLeftClickAsObservable().Subscribe(base.ViewModel.GoToLinkedClue).AddTo(this);
		base.ViewModel.LinkedClue.Subscribe(delegate(BlueprintClue value)
		{
			m_GoToLinkedClue.gameObject.SetActive(value != null);
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_BlinkSequence?.Kill();
		m_CaseEntitySourceView.Unbind();
		base.OnUnbind();
	}

	public void SetAddendumId(int id)
	{
		m_AddendumId.text = id.ToString();
	}

	public void Show(bool isFromStudy)
	{
		m_BlinkSequence?.Kill();
		if (!isFromStudy)
		{
			m_CanvasGroup.alpha = 1f;
			return;
		}
		m_CanvasGroup.alpha = 0f;
		(float, float)[] obj = new(float, float)[3]
		{
			(0.75f, 0.1f),
			(0.5f, 0.05f),
			(1f, 0.1f)
		};
		m_BlinkSequence = DOTween.Sequence().SetUpdate(isIndependentUpdate: true);
		m_BlinkSequence.Append(m_CanvasGroup.DOFade(0f, m_StudyDelay).SetUpdate(isIndependentUpdate: true));
		(float, float)[] array = obj;
		for (int i = 0; i < array.Length; i++)
		{
			(float, float) tuple = array[i];
			m_BlinkSequence.Append(m_CanvasGroup.DOFade(tuple.Item1, tuple.Item2).SetUpdate(isIndependentUpdate: true));
		}
		m_BlinkSequence.Play().SetUpdate(isIndependentUpdate: true);
	}
}
