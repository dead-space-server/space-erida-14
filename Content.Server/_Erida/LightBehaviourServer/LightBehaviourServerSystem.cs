using System.Linq;
using System.Runtime.CompilerServices;
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
        entity.Comp.TimeOfStart = (float)_timing.CurTime.TotalSeconds;
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
        string id = default!;

        switch (currentState)
        {
            case ExpendableLightState.Lit:
                id = elComp.TurnOnBehaviourID;
                break;
            case ExpendableLightState.Fading:
                id = elComp.FadeOutBehaviourID;
                break;
            default:
                return false;
        }

        if (!GetCurrentBehaviours(lbsComp, id, currentState, out var currentBehaviour)
            || currentBehaviour == null)
            return false;

        var curTime = (float)_timing.CurTime.TotalSeconds;

        radius = currentBehaviour.CalculateCurrentRadius(currentBehaviour.StartValue, currentBehaviour.EndValue, lbsComp.TimeOfStart.Value, curTime);

        if (radius != null)
            return true;

        return false;
    }

    private bool GetCurrentBehaviours(LightBehaviourServerComponent comp, string id, ExpendableLightState state, out LightBehaviourAnimationTrackServer? resultBehaviour)
    {
        resultBehaviour = null;

        foreach (var behaviour in comp.Behaviours)
        {
            if (behaviour.ID == id)
            {
                resultBehaviour = behaviour;
            }
        }

        if (resultBehaviour != null)
            return true;

        return false;
    }
}
