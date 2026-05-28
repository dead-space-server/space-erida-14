using Content.Server.Chat.Managers;
using Content.Shared.Chat;
using Content.Shared.Weather;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._DV.Weather;

public sealed partial class WeatherSchedulerSystem : EntitySystem
{
    [Dependency] private IChatManager _chat = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedWeatherSystem _weather = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<WeatherSchedulerComponent>();
        while (query.MoveNext(out var map, out var comp))
        {
            if (now < comp.NextUpdate)
                continue;

            if (comp.Stage >= comp.Stages.Count)
                comp.Stage = 0;

            var stage = comp.Stages[comp.Stage++];
            var duration = stage.Duration.Next(_random);
            comp.NextUpdate = now + TimeSpan.FromSeconds(duration);

            var mapId = Comp<MapComponent>(map).MapId;
            if (stage.Weather is { } weather)
            {
                _weather.TrySetWeather(mapId, _proto.Index(weather), out var weatherEnt, TimeSpan.FromSeconds(duration));
            }

            if (stage.Message is { } message)
            {
                var msg = Loc.GetString(message);
                _chat.ChatMessageToManyFiltered(
                    Filter.BroadcastMap(mapId),
                    ChatChannel.Radio,
                    msg,
                    msg,
                    map,
                    false,
                    true,
                    null);
            }
        }
    }
}
