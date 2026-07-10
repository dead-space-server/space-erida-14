// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.FixedPoint;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Speech;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ShadowCloakEntityComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? User;

    [DataField]
    public ProtoId<EmoteSoundsPrototype>? EmoteSounds = "ShadowCloak";

    [DataField]
    public ProtoId<SpeechSoundsPrototype>? SpeechSounds = "ShadowCloak";

    [DataField]
    public ProtoId<SpeechVerbPrototype> SpeechVerb = "Hiss";

    [DataField]
    public bool DebuffOnEarlyReveal;

    [DataField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(0.5f);

    [DataField]
    public TimeSpan SlowdownTime = TimeSpan.FromSeconds(10f);

    [DataField]
    public EntProtoId SlowdownEffect = "ShadowCloakRevealStatusEffect";

    [DataField]
    public float DoAfterSlowdown = 3f;

    [DataField]
    public FixedPoint2 DamageBeforeReveal = 25;

    [DataField]
    public float RevealDamageMultiplier = 1f;

    [DataField]
    public FixedPoint2 SustainedDamage = 0f;

    [DataField]
    public TimeSpan RevealCooldown = TimeSpan.FromMinutes(1f);

    [DataField]
    public TimeSpan ForceRevealCooldown = TimeSpan.FromMinutes(2f);

    [DataField]
    public FixedPoint2 SustainedDamageReductionRate = 1;
}
