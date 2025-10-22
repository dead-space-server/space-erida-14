using Content.Shared.Backmen.Language;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared._Erida.Backmen.Language;

[RegisterComponent]
public sealed partial class LanguageAddComponent : Component
{
    [DataField("speaks", customTypeSerializer: typeof(PrototypeIdListSerializer<LanguagePrototype>), required: true)]
    public List<string> SpokenLanguages = new();

    [DataField("understands", customTypeSerializer: typeof(PrototypeIdListSerializer<LanguagePrototype>), required: true)]
    public List<string> UnderstoodLanguages = new();
}
