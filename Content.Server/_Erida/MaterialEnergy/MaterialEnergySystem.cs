using Content.Server.Power.EntitySystems;
using Content.Server.Stack;
using Content.Shared.Erida.MaterialEnergy;
using Content.Shared.Interaction;
using Content.Shared.Materials;
using Content.Shared.Stacks;
using Content.Shared.Power.Components;

namespace Content.Erida.Server.MaterialEnergy
{
    public sealed class MaterialEnergySystem : EntitySystem
    {
        [Dependency] private readonly BatterySystem _batterySystem = default!;
        [Dependency] private readonly StackSystem _stack = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<MaterialEnergyComponent, InteractUsingEvent>(OnInteract);
        }

        private void OnInteract(EntityUid uid, MaterialEnergyComponent component, InteractUsingEvent args)
        {
            if (component.MaterialWhiteList == null
            || !TryComp<PhysicalCompositionComponent>(args.Used, out var composition)
            || !TryComp<StackComponent>(args.Used, out var materialStack))
                return;

            foreach (var fueltype in component.MaterialWhiteList)
            {
                if (composition.MaterialComposition.ContainsKey(fueltype))
                {
                    AddBatteryCharge(
                    uid,
                    args.Used,
                    composition.MaterialComposition[fueltype],
                    materialStack.Count);

                    args.Handled = true;
                    break;
                }
            }
        }

        private void AddBatteryCharge(
            EntityUid cutter,
            EntityUid material,
            int materialPerSheet,
            int sheetsInStack)
        {

            if (!TryComp<BatteryComponent>(cutter, out var battery))
                return;

            var currentCharge = _batterySystem.GetCharge(cutter);

            var maxCharge = battery.MaxCharge;

            var remainingSpace = maxCharge - currentCharge;
            var totalMaterialValue = materialPerSheet * sheetsInStack;
            float chargeToAdd = Math.Min(totalMaterialValue, remainingSpace);

            _batterySystem.ChangeCharge(cutter, chargeToAdd);

            var sheetsToSpend = (int)Math.Ceiling(chargeToAdd / materialPerSheet);
            var toDel = _stack.Split(
                material,
                sheetsToSpend,
                Transform(material).Coordinates);
            if (toDel != null && !Deleted(toDel))
                QueueDel(toDel);
        }
    }
}
