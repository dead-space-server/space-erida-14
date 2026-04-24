using Robust.Shared.Serialization;

// ReSharper disable once CheckNamespace
namespace Content.Shared._Erida.Botany.SeedDna;

/// <summary>
/// РљРѕРЅС‚РµР№РЅРµСЂ РґР»СЏ РїРµСЂРµРґР°С‡Рё СЃРѕСЃС‚РѕСЏРЅРёСЏ UI РјРµР¶РґСѓ РєР»РёРµРЅС‚РѕРј Рё СЃРµСЂРІРµСЂРѕРј
/// </summary>
[Serializable, NetSerializable]
public sealed class SeedDnaConsoleBoundUserInterfaceState(
    bool isSeedsPresent,
    string seedsName,
    SeedDataDto? seedData,
    bool isDnaDiskPresent,
    string dnaDiskName,
    SeedDataDto? dnaDiskData
) : BoundUserInterfaceState
{
    public readonly bool IsSeedsPresent = isSeedsPresent;
    public readonly string SeedsName = seedsName;
    public readonly SeedDataDto? SeedData = seedData;

    public readonly bool IsDnaDiskPresent = isDnaDiskPresent;
    public readonly string DnaDiskName = dnaDiskName;
    public readonly SeedDataDto? DnaDiskData = dnaDiskData;
}
