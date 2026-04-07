using Content.Shared._Goobstation.GPS;
using Content.Shared._Goobstation.GPS.Components;

namespace Content.Client._Goobstation.GPS;

public sealed class GpsSystem : SharedGpsSystem
{
    protected override void UpdateUi(Entity<GPSComponent> ent)
    {
        if (UiSystem.TryGetOpenUi<GpsBoundUserInterface>(ent.Owner,
                GpsUiKey.Key,
                out var bui))
            bui.UpdateWindow();
    }
}
