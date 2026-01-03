using Content.Server.Chat;
using Content.Server.Chat.Systems;
using Content.Shared._Erida.Arousal.Components;
using Content.Shared.Alert;
using Robust.Shared.Timing;

namespace Content.Server._Erida.Arousal;

//
// License-Identifier: AGPL-3.0-or-later
//

public sealed class ArousalSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;
    [Dependency] private readonly AutoEmoteSystem _autoEmote = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    private readonly float _arousalUpdateInterval = 1.0f;
    private float _lastUpdate;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ArousalComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, ArousalComponent component, ComponentStartup args)
    {
        component.LastUpdateTime = (float)_gameTiming.CurTime.TotalSeconds;
        SetArousalAlert(uid);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = (float)_gameTiming.CurTime.TotalSeconds;
        if (curTime - _lastUpdate < _arousalUpdateInterval)
            return;

        var query = _entityManager.EntityQueryEnumerator<ArousalComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (component.ArousalCurrent <= component.MinArousal)
                continue;

            var elapsed = curTime - component.LastUpdateTime;
            if (elapsed <= 0)
                continue;

            // The higher the ArousalModifier, the slower the arousal decreases.
            var decayRate = component.ArousalDecayRate * (1.0f - component.ArousalModifier);
            component.ArousalCurrent -= decayRate * elapsed;

            component.LastUpdateTime = curTime;
            component.ArousalCurrent = Math.Clamp(component.ArousalCurrent,
                component.MinArousal, component.MaxArousal);

            HandleArousalTransitions(uid);
            SetArousalAlert(uid);
        }
    }

    public void IncreaseArousal(EntityUid uid, float amount)
    {
        if (!_entityManager.TryGetComponent(uid, out ArousalComponent? component))
            return;

        var actualAmount = amount * component.ArousalModifier;
        component.ArousalCurrent = Math.Clamp(component.ArousalCurrent + actualAmount,
            component.MinArousal, component.MaxArousal);

        component.LastUpdateTime = (float)_gameTiming.CurTime.TotalSeconds;

        HandleArousalTransitions(uid);
    }

    public void DecreaseArousal(EntityUid uid, float amount)
    {
        if (!_entityManager.TryGetComponent(uid, out ArousalComponent? component))
            return;

        var actualAmount = amount * component.ArousalModifier;
        component.ArousalCurrent = Math.Clamp(component.ArousalCurrent - actualAmount,
            component.MinArousal, component.MaxArousal);

        component.LastUpdateTime = (float)_gameTiming.CurTime.TotalSeconds;

        HandleArousalTransitions(uid);
    }

    public void HandleArousalTransitions(EntityUid uid)
    {
        if (!_entityManager.TryGetComponent(uid, out ArousalComponent? component))
            return;

        if (component.ArousalCurrent >= component.HighArousalThreshold &&
            !component.IsHighArousal)
        {
            HandleHighArousal(uid);
            component.IsHighArousal = true;
        }
        else if (component.ArousalCurrent < component.HighArousalThreshold &&
                 component.IsHighArousal)
        {
            HandleLowArousal(uid);
            component.IsHighArousal = false;
        }
    }

    private void HandleHighArousal(EntityUid uid)
    {
        EnsureComp<AutoEmoteComponent>(uid);
        _autoEmote.AddEmote(uid, "Moan");
    }

    private void HandleLowArousal(EntityUid uid)
    {
        _autoEmote.RemoveEmote(uid, "Moan");
    }

    private void SetArousalAlert(EntityUid uid, ArousalComponent? component = null)
    {
        if (!Resolve(uid, ref component, false) || component.Deleted)
            return;

        var severity = (int)Math.Floor(component.ArousalCurrent / 10f);
        severity = Math.Clamp(severity, 0, 10);
        if (severity == 0)
        {
            _alertsSystem.ClearAlert(uid, component.ArousalAlert);
            return;
        }

        _alertsSystem.ShowAlert(uid, component.ArousalAlert, (short) severity);
    }
}
