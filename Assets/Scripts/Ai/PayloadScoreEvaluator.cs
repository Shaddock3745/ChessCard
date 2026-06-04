using UnityEngine;

public interface PayloadScoreEvaluator
{
    float evaluatePayloadScore(bool isLocal, EffectTargetPayload targets, int currentTurnCount);
}
