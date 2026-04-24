using Content.Shared.Lock;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Utility;

namespace Content.Client.Silicons.StationAi;

public sealed partial class StationAiSystem
{
    private void InitializeBorgControl()
    {
        SubscribeLocalEvent<BorgChassisComponent, GetStationAiRadialEvent>(OnBorgGetRadial);
    }

    private void OnBorgGetRadial(Entity<BorgChassisComponent> ent, ref GetStationAiRadialEvent args)
    {
        if (HasComp<BorgControlComponent>(ent.Owner))
        {
            args.Actions.Add(new StationAiRadial
            {
                Sprite = new SpriteSpecifier.Rsi(_aiActionsRsi, "ai_core"),
                Tooltip = Loc.GetString("station-ai-borg-control"),
                Event = new StationAiControlBorgEvent
                {
                    TakeControl = true
                }
            });
        }

        if (!TryComp<LockComponent>(ent.Owner, out var lockComp))
            return;

        args.Actions.Add(new StationAiRadial
        {
            Sprite = lockComp.Locked
                ? new SpriteSpecifier.Rsi(_aiActionsRsi, "unbolt_door")
                : new SpriteSpecifier.Rsi(_aiActionsRsi, "bolt_door"),
            Tooltip = lockComp.Locked
                ? Loc.GetString("station-ai-borg-unlock")
                : Loc.GetString("station-ai-borg-lock"),
            Event = new StationAiSetBorgLockEvent
            {
                Locked = !lockComp.Locked
            }
        });
    }
}
