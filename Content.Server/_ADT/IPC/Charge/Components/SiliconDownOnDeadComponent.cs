using System.Threading;

namespace Content.Server._ADT.Silicon.Death;

[RegisterComponent]
public sealed partial class SiliconDownOnDeadComponent : Component
{
    public CancellationTokenSource? WakeToken { get; set; }

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("deadBuffer")]
    public float DeadBuffer { get; set; } = 2.5f;

    public TimeSpan Time = TimeSpan.FromSeconds(60);

    public bool Dead { get; set; } = false;
}
