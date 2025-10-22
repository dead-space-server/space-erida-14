namespace Content.Goobstation.Server.Blob.Components;

[RegisterComponent]
public sealed partial class StationBlobConfigComponent : Component
{
    public const int DefaultStageBegin = 50;
    public const int DefaultStageCritical = 300;
    public const int DefaultStageEnd = 500;

    [DataField]
    public int StageBegin { get; set; } = DefaultStageBegin;

    [DataField]
    public int StageCritical { get; set; } = DefaultStageCritical;

    [DataField]
    public int StageTheEnd { get; set; } = DefaultStageEnd;
}
