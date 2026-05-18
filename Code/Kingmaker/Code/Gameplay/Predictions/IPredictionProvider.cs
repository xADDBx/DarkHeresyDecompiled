namespace Kingmaker.Code.Gameplay.Predictions;

public interface IPredictionProvider<out TPrediction, in TContext>
{
	TPrediction Get(TContext ctx);
}
