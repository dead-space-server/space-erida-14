// SPDX-FileCopyrightText: 2026 Lytheriia
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared._DV.Traits.Conditions;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Shared._Erida.Traits.Conditions;

/// <summary>
/// A condition that checks whether a player is of a specific gender.
/// </summary>
public sealed partial class IsGenderCondition : BaseTraitCondition
{
    [DataField(required: true)]
    public Gender[] Genders = [];

    protected override bool EvaluateImplementation(TraitConditionContext ctx)
    {
        if (ctx.Profile == null)
            return false;

        return Genders.Contains(ctx.Profile.Gender);
    }

    public override string GetTooltip(IPrototypeManager proto, ILocalizationManager loc)
    {
        var genderList = string.Join(", ", Genders.Select(g => loc.GetString($"gender-{g.ToString().ToLower()}")));

        return Invert
            ? loc.GetString("trait-condition-gender-not", ("gender", genderList))
            : loc.GetString("trait-condition-gender-is", ("gender", genderList));
    }
}
