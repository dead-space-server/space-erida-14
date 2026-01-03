using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client._Erida.Overlays;

public sealed partial class DesaturationOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> Shader = "DesaturationOverlay";

    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;
    private readonly ShaderInstance _desaturationShader;

    public DesaturationOverlay()
    {
        IoCManager.InjectDependencies(this);
        _desaturationShader = _prototypeManager.Index(Shader).InstanceUnique();
        ZIndex = 1;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        var handle = args.WorldHandle;
        _desaturationShader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _desaturationShader.SetParameter("desaturation", 0.1f); // 10% desaturation
        handle.UseShader(_desaturationShader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }
}
