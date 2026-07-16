using System.Runtime.CompilerServices;
using Content.Shared._DV.Traits.Assorted;
using Content.Shared.Mind;
using Content.Shared.Roles;
using Content.Shared.Roles.Components;
using DependencyAttribute = Robust.Shared.IoC.DependencyAttribute;

namespace Content.Server.GameTicking.Rules;

/// <summary>
/// Handles paradox anomaly related things when spawning paradox clones.
/// </summary>
public sealed partial class ParadoxCloneRuleSystem
{
    [Dependency] private SharedRoleSystem _role = default!;

    private void FilterTargets(HashSet<Entity<MindComponent>> minds)
    {
        // TODO: use generic IMindFilter
        // no picking other antags or non-crew and entities with no paradox clone trait
        minds.RemoveWhere(mind => //_role.MindIsAntagonist(mind) || // Erida-edit hehehehe
            !_role.MindHasRole<JobRoleComponent>((mind, mind), out var role) ||
            role?.Comp1.JobPrototype == null ||
            (mind.Comp.OwnedEntity is { } entity && HasComp<NoParadoxCloneComponent>(entity))
        );
    }
}
