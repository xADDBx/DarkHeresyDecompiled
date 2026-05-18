using System.Collections.Generic;
using System.Linq;
using System.Text;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.Code.UI.MVVM;

public class AlignmentShiftListener : NotificationListenerBase, IAlignmentRankShiftHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IAlignmentReachMarkHandler
{
	private readonly Dictionary<AlignmentAxis, int> m_AlignmentShifts = new Dictionary<AlignmentAxis, int>();

	private readonly List<(AlignmentAxis Axis, int PreviousMark, int ReachedMark)> m_ReachedMarks = new List<(AlignmentAxis, int, int)>();

	public override bool HasData => m_AlignmentShifts.Count > 0;

	public override int Order => 9;

	public override NotificationCategory Category => NotificationCategory.Common;

	public override DialogNotificationSoundType SoundType => DialogNotificationSoundType.ConvictionShift;

	public AlignmentShiftListener(DialogUIType dialogUIType)
		: base(dialogUIType)
	{
	}

	public void HandleAlignmentRankShift(AlignmentShift shift)
	{
		m_AlignmentShifts.TryGetValue(shift.Axis, out var value);
		m_AlignmentShifts[shift.Axis] = value + shift.Value;
	}

	public void HandleAlignmentMarkShift(AlignmentAxis axis, int previousMark, int reachedMark)
	{
		m_ReachedMarks.Add((axis, previousMark, reachedMark));
	}

	public override List<DialogNotificationVM> CreateNotifications()
	{
		if (m_AlignmentShifts.Count == 0)
		{
			return new List<DialogNotificationVM>();
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < m_AlignmentShifts.Count; i++)
		{
			var (alignmentAxis2, number) = (KeyValuePair<AlignmentAxis, int>)(ref m_AlignmentShifts.ElementAt(i));
			if (i > 0)
			{
				stringBuilder.Append(", ");
			}
			string text = NotificationFormatter.GenerateLink(UIUtilityAlignment.GetAlignmentDirectionText(alignmentAxis2).Text, $"{EntityLink.Type.Alignment}:{alignmentAxis2}");
			stringBuilder.Append(text + " " + number.ToStringWithSign());
		}
		StringBuilder stringBuilder2 = new StringBuilder(string.Format(UINotificationTexts.Instance.AlignmentShiftFormat, stringBuilder));
		BlueprintAlignmentMarksRoot alignmentMarksRoot = ConfigRoot.Instance.AlignmentMarksRoot;
		foreach (var reachedMark in m_ReachedMarks)
		{
			AlignmentAxis item = reachedMark.Axis;
			int item2 = reachedMark.PreviousMark;
			int item3 = reachedMark.ReachedMark;
			for (int j = item2; j < item3; j++)
			{
				foreach (BlueprintMechanicEntityFact item4 in from f in alignmentMarksRoot.GetFactsForMark(item, j)
					where !(f is BlueprintFeatureBase blueprintFeatureBase) || !blueprintFeatureBase.HideInUI
					select f)
				{
					stringBuilder2.Append("<br>");
					stringBuilder2.Append(NotificationFormatter.GenerateLink(item4.Name, "f:" + item4.AssetGuid));
				}
			}
		}
		return new List<DialogNotificationVM>
		{
			new DialogNotificationVM(NotificationFormatter.FormatText(stringBuilder2.ToString()))
		};
	}

	public override void Clear()
	{
		m_AlignmentShifts.Clear();
		m_ReachedMarks.Clear();
	}
}
