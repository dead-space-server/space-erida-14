// SPDX-FileCopyrightText: 2026 vanomorodellefake <vanomorodellefake29@gmail.com>
// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using Robust.Shared.Animations;

namespace Content.Server._Erida.LightBehaviourServer;

[RegisterComponent, Access(typeof(LightBehaviourServerSystem))]
public sealed partial class LightBehaviourServerComponent : Component
{
    [DataField]
    public float? TimeOfStart = null;

    [DataField]
    public List<LightBehaviourAnimationTrackServer> Behaviours = new();
}

[Serializable]
[ImplicitDataDefinitionForInheritors]
public abstract partial class LightBehaviourAnimationTrackServer
{
    [DataField("id")] public string ID { get; set; } = string.Empty;

    [DataField("startValue")] public float StartValue { get; set; } = 0f;

    [DataField("endValue")] public float EndValue { get; set; } = 2f;

    [DataField("maxDuration")] public float MaxDuration { get; set; } = 2f;

    [DataField("interpolate")] public AnimationInterpolationMode InterpolateMode { get; set; } = AnimationInterpolationMode.Linear;

    public abstract float CalculateCurrentRadius(
        float startValue,
        float endValue,
        float startTime,
        float curTime,
        bool reverseWhenFinished = false);
}

[UsedImplicitly]
public sealed partial class FadeBehaviourServer : LightBehaviourAnimationTrackServer
{
    [DataField("reverseWhenFinished")]
    public bool ReverseWhenFinished { get; set; }

    public override float CalculateCurrentRadius(
        float startValue,
        float endValue,
        float startTime,
        float curTime,
        bool reverseWhenFinished = false)
    {
        var playingTime = curTime - startTime;
        var interpolateValue = playingTime / MaxDuration;

        if (reverseWhenFinished)
        {
            if (interpolateValue < 0.5f)
            {
                interpolateValue = interpolateValue * 2;
            }
            else
            {
                interpolateValue = (interpolateValue - 0.5f) * 2;
                (startValue, endValue) = (endValue, startValue);
            }
        }

        return ApplyInterpolation(startValue, endValue, interpolateValue);
    }

    private float ApplyInterpolation(
        float start,
        float end,
        float interpolateValue)
    {
        switch (InterpolateMode)
        {
            case AnimationInterpolationMode.Linear:
                return MathHelper.Lerp(start, end, interpolateValue);
            case AnimationInterpolationMode.Cubic:
                return MathHelper.InterpolateCubic(end, start, end, start, interpolateValue);
            default:
            case AnimationInterpolationMode.Nearest:
                return interpolateValue < 0.5f ? start : end;
        }
    }
}
