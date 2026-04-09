using Content.Server.DoAfter;
using Content.Shared._Erida.CRP;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._Erida.CPR;

public sealed class CardiopulmonaryResuscitationSystem : EntitySystem
{
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IngestionSystem _ingestionSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CardiopulmonaryResuscitationComponent, GetVerbsEvent<AlternativeVerb>>(AddCRPVerb);
        SubscribeLocalEvent<CardiopulmonaryResuscitationComponent, CRPDoAfterEvent>(CRPDoAfter);
    }

    private void AddCRPVerb(EntityUid uid, CardiopulmonaryResuscitationComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        if (!_mobStateSystem.IsCritical(args.Target))
            return;

        if (!HasComp<CardiopulmonaryResuscitationComponent>(args.Target))
        {
            return;
        }

        AlternativeVerb verb = new()
        {
            Act = () =>
            {
                StartCRP((args.Target, component), ref args);
            },
            Text = Loc.GetString("CRP-verb"),
            Priority = 3
        };

        args.Verbs.Add(verb);
    }

    private void StartCRP(Entity<CardiopulmonaryResuscitationComponent> entity, ref GetVerbsEvent<AlternativeVerb> args)
    {
        EntityUid сRPer = args.User;
        EntityUid сRPied = args.Target;

        if (!CheckMouth(сRPer, сRPied))
            return;

        SendPopupMessages(сRPer, сRPied);

        TimeSpan length = TimeSpan.FromSeconds(entity.Comp.CRPHowLong);

        var ev = new CRPDoAfterEvent();
        var argsDoAfter = new DoAfterArgs(_entityManager, сRPer, length, ev, сRPied, target: сRPied)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
        };

        _doAfterSystem.TryStartDoAfter(argsDoAfter);
    }
    private void CRPDoAfter(EntityUid uid, CardiopulmonaryResuscitationComponent component, CRPDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || !CheckMouth(args.Args.User, uid) || !_mobStateSystem.IsCritical(uid))
            return;

        CRP(component.CRPAsphyxiationAmount, ref args);
        args.Repeat = true;
        args.Args.Event.Repeat = args.Repeat;
        args.Handled = true;
    }

    private void CRP(float сRPAsphyxiationAmount, ref CRPDoAfterEvent args)
    {
        var сRPied = args.Target;

        if (!TryComp<DamageableComponent>(сRPied, out var dCcomp))
            return;

        var damageSpec = new DamageSpecifier();
        damageSpec.DamageDict = new Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2>()
        {
            { "Asphyxiation", FixedPoint2.New(сRPAsphyxiationAmount) }
        };

        _damageable.TryChangeDamage(
            (сRPied.Value, dCcomp),
            damageSpec,
            ignoreResistances: true,
            interruptsDoAfters: true
        );
    }

    private bool CheckMouth(EntityUid cRPer, EntityUid cRPied)
    {
        if (!_ingestionSystem.HasMouthAvailable(cRPer, cRPer))
        {
            _popup.PopupEntity(Loc.GetString("CRP-CRPer-has-blocked-mouth"), cRPer, cRPer);
            return false;
        }

        if (!_ingestionSystem.HasMouthAvailable(cRPied, cRPied))
        {
            _popup.PopupEntity(Loc.GetString("CRP-CRPied-has-blocked-mouth"), cRPer, cRPer);
            return false;
        }

        return true;
    }

    private void SendPopupMessages(EntityUid cRPer, EntityUid cRPied)
    {
        _popup.PopupEntity(Loc.GetString("CRP-start-CRP-CRPer",
            ("CRPied", Identity.Name(cRPied, EntityManager, cRPer))),
            cRPied, cRPer, Shared.Popups.PopupType.Medium);
        _popup.PopupEntity(Loc.GetString("CRP-start-CRP-CRpied",
            ("CRPer", Identity.Name(cRPer, EntityManager, cRPied))),
            cRPer, cRPied, Shared.Popups.PopupType.Medium);

        var filter = Filter.Pvs(cRPer, entityManager: EntityManager);
        if (filter != null)
        {
            _popup.PopupEntity(Loc.GetString("CRP-start-CRP-others",
                ("CRPer", Identity.Name(cRPer, EntityManager)),
                ("CRPied", Identity.Name(cRPied, EntityManager))),
                cRPer, filter.RemoveWhere(e => e.AttachedEntity == cRPer || e.AttachedEntity == cRPied),
                true, Shared.Popups.PopupType.Medium);
        }
    }
}
