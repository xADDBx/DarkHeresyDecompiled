using JetBrains.Annotations;
using Kingmaker.DialogSystem;

namespace Kingmaker.Controllers.Dialog;

internal interface IDialogControllerStartScheduledDialogImmediately
{
	void StartScheduledDialogImmediately([NotNull] DialogData dialog);
}
