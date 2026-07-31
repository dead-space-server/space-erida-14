// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Heretic.Systems;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared._Goobstation.Eye.Blinding.Systems;
using Content.Shared._Goobstation.Heretic;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.NPC.Systems;
using Content.Shared.Popups;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.StationAi;
using Content.Shared.StatusEffect;
using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Heretic.Systems;

public abstract partial class SharedMansusGraspSystem : EntitySystem
{
    private static readonly ProtoId<TagPrototype> HereticBladeBladeTag = "HereticBladeBlade";
    private static readonly ProtoId<TagPrototype> WallTag = "Wall";
    private static readonly ProtoId<TagPrototype> CatwalkTag = "Catwalk";
    private static readonly ProtoId<TagPrototype> MeatTag = "Meat";
    private static readonly ProtoId<TagPrototype> BotTag = "Bot";
    private static readonly ProtoId<DamageGroupPrototype> BruteDamageGroup = "Brute";

    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private IComponentFactory _compFactory = default!;
    [Dependency] private INetManager _net = default!;

    [Dependency] private SharedDoorSystem _door = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private StatusEffectsSystem _statusEffect = default!;
    [Dependency] private TagSystem _tag = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedVoidCurseSystem _voidCurse = default!;
    [Dependency] private SharedMindSystem _mind = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedStarMarkSystem _starMark = default!;

    public bool TryApplyGraspEffectAndMark(EntityUid user,
        HereticComponent hereticComp,
        EntityUid target,
        EntityUid? grasp,
        out bool triggerGrasp)
    {
        triggerGrasp = true;

        if (hereticComp.CurrentPath == null)
            return true;

        if (hereticComp.PathStage >= 2)
        {
            if (!ApplyGraspEffect(user, hereticComp, target, grasp, out var applyMark, out triggerGrasp))
                return false;

            if (!applyMark)
                return true;
        }

        if (hereticComp.PathStage >= 4 && HasComp<StatusEffectsComponent>(target))
        {
            var markComp = EnsureComp<HereticCombatMarkComponent>(target);
            markComp.DisappearTime = markComp.MaxDisappearTime;
            markComp.Path = hereticComp.CurrentPath;
            markComp.Repetitions = hereticComp.CurrentPath == "Ash" ? 5 : 1;
            Dirty(target, markComp);

            if (hereticComp.CurrentPath == "Cosmos")
            {
                var cosmosMark = EnsureComp<HereticCosmicMarkComponent>(target);
                cosmosMark.CosmicDiamondUid = Spawn(cosmosMark.CosmicDiamond, Transform(target).Coordinates);
                _transform.AttachToGridOrMap(cosmosMark.CosmicDiamondUid.Value);
            }
        }

        return true;
    }

    public bool ApplyGraspEffect(EntityUid performer,
        HereticComponent heretic,
        EntityUid target,
        EntityUid? grasp,
        out bool applyMark,
        out bool triggerGrasp)
    {
        applyMark = true;
        triggerGrasp = true;

        switch (heretic.CurrentPath)
        {
            case "Ash":
            {
                var timeSpan = TimeSpan.FromSeconds(5f);
                _statusEffect.TryAddStatusEffect(target,
                    TemporaryBlindnessSystem.BlindingStatusEffect,
                    timeSpan,
                    false,
                    TemporaryBlindnessSystem.BlindingStatusEffect);
                break;
            }

            case "Blade":
            {
                if (grasp != null && heretic.PathStage >= 7 && _tag.HasTag(target, HereticBladeBladeTag))
                {
                    // empowering blades and shit
                    var infusion = EnsureComp<MansusInfusedComponent>(target);
                    infusion.AvailableCharges = infusion.MaxCharges;
                    break;
                }

                break;
            }

            case "Lock":
            {
                if (!TryComp<DoorComponent>(target, out var door))
                    break;

                if (TryComp<DoorBoltComponent>(target, out var doorBolt))
                    _door.SetBoltsDown((target, doorBolt), false);

                _door.StartOpening(target, door);
                _audio.PlayPredicted(new SoundPathSpecifier("/Audio/_GoobStation/Heretic/hereticknock.ogg"),
                    target,
                    performer);
                break;
            }

            case "Flesh":
            {
                if (TryComp<MobStateComponent>(target, out var mobState) && mobState.CurrentState != MobState.Alive &&
                    !HasComp<BorgChassisComponent>(target))
                {
                    if (HasComp<GhoulComponent>(target))
                    {
                        if (_net.IsServer)
                            _popup.PopupEntity(Loc.GetString("heretic-ability-fail-target-ghoul"), performer, performer);
                        break;
                    }

                    if (!_mind.TryGetMind(target, out _, out _))
                    {
                        if (_net.IsServer)
                            _popup.PopupEntity(Loc.GetString("heretic-ability-fail-target-no-mind"), performer, performer);
                        break;
                    }

                    EnsureComp<HereticMinionComponent>(target).BoundHeretic = performer;

                    var ghoul = _compFactory.GetComponent<GhoulComponent>();
                    ghoul.GiveBlade = true;

                    AddComp(target, ghoul);
                    applyMark = false;
                    triggerGrasp = false;
                }

                break;
            }

            case "Void":
            {
                _voidCurse.DoCurse(target, 2);
                break;
            }

            case "Rust":
            {
                if (TryComp(target, out StationAiHolderComponent? aiHolder))
                    QueueDel(aiHolder.Slot.ContainerSlot?.ContainedEntity);
                else if (HasComp<RustGraspComponent>(grasp) && _tag.HasAnyTag(target, WallTag, CatwalkTag) ||
                         HasComp<HereticRitualRuneComponent>(
                             target))
                    return false;
                else if (TryComp(target, out DamageableComponent? damageable) &&
                         !_tag.HasTag(target, MeatTag) &&
                         !HasComp<ShadowCloakEntityComponent>(target) &&
                         (!HasComp<MobStateComponent>(target) ||
                          HasComp<BorgChassisComponent>(target) ||
                          _tag.HasTag(target, BotTag)))
                {
                    _damage.TryChangeDamage(target,
                        new DamageSpecifier(_proto.Index<DamageGroupPrototype>(BruteDamageGroup), 500),
                        ignoreResistances: true,
                        origin: performer);
                }

                break;
            }

            case "Cosmos":
            {
                if (_starMark.TryApplyStarMark(target))
                    _starMark.SpawnCosmicField(Transform(performer).Coordinates, heretic.PathStage);
                break;
            }
            default:
                return true;
        }

        return true;
    }
}
