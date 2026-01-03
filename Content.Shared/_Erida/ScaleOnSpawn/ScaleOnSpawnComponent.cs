
using System.Numerics;

namespace Content.Shared._Erida.ScaleOnSpawn;

[RegisterComponent]
public sealed partial class ScaleOnSpawnComponent : Component
{
    [DataField]
    public Vector2 Scale = new Vector2(1, 1);
}
