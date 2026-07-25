// SPDX-FileCopyrightText: 2026 vanomorodellefake <vanomorodellefake29@gmail.com>
// SPDX-License-Identifier: MIT

using Content.Server.Light.Components;
using Content.Shared.IgnitionSource;
using Content.Shared.Light.Components;
using Robust.Shared.Timing;
using DependencyAttribute = Robust.Shared.IoC.DependencyAttribute;

namespace Content.Server._Erida.LightBehaviourServer;

public sealed partial class LightBehaviourServerSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LightBehaviourServerComponent, IgnitionEvent>(OnIgnition);
    }

    private void OnIgnition(Entity<LightBehaviourServerComponent> entity, ref IgnitionEvent args)
    {
        entity.Comp.TimeOfStart = _timing.CurTime;

        if (!TryComp<ExpendableLightComponent>(entity, out var elComp))
            return;

        entity.Comp.TimeToStartFadeOut = entity.Comp.TimeOfStart + elComp.GlowDuration;
    }

    public bool TryGetRadius(EntityUid entity, out float? radius, LightBehaviourServerComponent? lbsComp = null)
    {
        radius = null;

        if (!Resolve(entity, ref lbsComp, false))
            return false;

        if (lbsComp.TimeOfStart == null)
            return false;

        if (!TryComp<ExpendableLightComponent>(entity, out var elComp)
            || elComp.Activated == false)
            return false;

        var currentState = elComp.CurrentState;
        if (!TryGetCurrentId(currentState, elComp, out var id)
            || id == null)
            return false;

        if (!GetCurrentBehaviours(lbsComp, id, currentState, out var currentBehaviour)
            || currentBehaviour == null)
            return false;

        var curTime = _timing.CurTime;
        var startOfAnimationTrack = lbsComp.TimeToStartFadeOut != null && curTime >= lbsComp.TimeToStartFadeOut ? lbsComp.TimeToStartFadeOut.Value : lbsComp.TimeOfStart.Value;

        radius = currentBehaviour.CalculateCurrentRadius(currentBehaviour.StartValue, currentBehaviour.EndValue, startOfAnimationTrack, curTime);

        if (radius != null)
            return true;

        return false;
    }

    private bool GetCurrentBehaviours(LightBehaviourServerComponent comp, string id, ExpendableLightState currentState, out LightBehaviourAnimationTrackServer? resultBehaviour, float? curTime = null)
    {
        resultBehaviour = null;

        foreach (var behaviour in comp.Behaviours)
            if (behaviour.ID == id)
                resultBehaviour = behaviour;

        if (resultBehaviour != null)
            return true;

        return false;
    }

    private bool TryGetCurrentId(ExpendableLightState currentState, ExpendableLightComponent elComp, out string? id)
    {
        id = null;

        switch (currentState)
        {
            case ExpendableLightState.Lit:
                id = elComp.TurnOnBehaviourID;
                return true;
            case ExpendableLightState.Fading:
                id = elComp.FadeOutBehaviourID;
                return true;
            default:
                return false;
        }
    }
}
