using Content.Shared._Erida.Botany.SeedDna;
using Content.Shared.FixedPoint;
using Robust.Client.UserInterface.Controls;
using System.Globalization;

namespace Content.Client._Erida.Botany.UI;

public sealed class SeedDnaConsoleWindowRow
{
    private const float MarginValue = 15f / 2f;
    private static readonly Thickness LeftMargin = new(0, 0, MarginValue, 0);
    private static readonly Thickness LeftRightMargin = new(MarginValue, 0);
    private static readonly Thickness RightMargin = new(MarginValue, 0, 0, 0);

    private Label? _titleLabel;
    private Label? _seedValueLabel;
    private Label? _dnaDiskValueLabel;
    private Button? _extractButton;
    private Button? _replaceButton;

    private Action? _actionExtract;
    private Action? _actionReplace;

    private readonly Func<object?> _getterSeedValue;
    private readonly Func<object?> _getterDnaDiskValue;
    private readonly Func<float?>? _getSeedPotency;
    private readonly Func<float?>? _getDiskPotency;

    private SeedDnaConsoleWindowRow(
        string title,
        bool seedPresent,
        bool dnaDiskPresent,
        Func<object?> getterSeedValue,
        Func<object?> getterDnaDiskValue,
        Action<object?> setterSeedValue,
        Action<object?> setterDnaDiskValue,
        Func<bool> flagUpdateImmediately,
        Action<TargetSeedData> submit,
        Action refreshRows,
        Func<float?>? getSeedPotency = null,
        Func<float?>? getDiskPotency = null)
    {
        _getterSeedValue = getterSeedValue;
        _getterDnaDiskValue = getterDnaDiskValue;
        _getSeedPotency = getSeedPotency;
        _getDiskPotency = getDiskPotency;

        _titleLabel = CreateTitleLabel(title);

        var seedValue = getterSeedValue();
        var diskValue = getterDnaDiskValue();

        var seedPotencyValue = _getSeedPotency?.Invoke();
        var diskPotencyValue = _getDiskPotency?.Invoke();

        _seedValueLabel = CreateValueLabel();
        _dnaDiskValueLabel = CreateValueLabel();
        SetLabelValue(_seedValueLabel, seedValue, seedPotencyValue);
        SetLabelValue(_dnaDiskValueLabel, diskValue, diskPotencyValue);

        _extractButton = CreateActionButton(Loc.GetString("seed-dna-extract-btn"));
        _replaceButton = CreateActionButton(Loc.GetString("seed-dna-replace-btn"));

        _actionExtract = SetupActionButton(
            _extractButton,
            dnaDiskPresent,
            getterSeedValue,
            setterDnaDiskValue,
            _seedValueLabel,
            _dnaDiskValueLabel,
            flagUpdateImmediately,
            submit,
            refreshRows,
            TargetSeedData.DnaDisk);

        _actionReplace = SetupActionButton(
            _replaceButton,
            seedPresent,
            getterDnaDiskValue,
            setterSeedValue,
            _dnaDiskValueLabel,
            _seedValueLabel,
            flagUpdateImmediately,
            submit,
            refreshRows,
            TargetSeedData.Seed);
    }

    public SeedDnaConsoleWindowRow IncludeToContainer(Container container)
    {
        container.AddChild(_titleLabel!);
        container.AddChild(_seedValueLabel!);
        container.AddChild(_dnaDiskValueLabel!);
        container.AddChild(new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 5,
            Margin = RightMargin,
            Children =
            {
                _extractButton!,
                _replaceButton!,
            },
        });

        return this;
    }

    public void DoExtract()
    {
        _actionExtract?.Invoke();
    }

    public void DoReplace()
    {
        _actionReplace?.Invoke();
    }

    public void RefreshValues()
    {
        SetLabelValue(_seedValueLabel!, _getterSeedValue(), _getSeedPotency?.Invoke());
        SetLabelValue(_dnaDiskValueLabel!, _getterDnaDiskValue(), _getDiskPotency?.Invoke());
    }

    public static SeedDnaConsoleWindowRow? Create(
        string title,
        bool seedPresent,
        bool dnaDiskPresent,
        Func<object?> getterSeedValue,
        Func<object?> getterDnaDiskValue,
        Action<object?> setterSeedValue,
        Action<object?> setterDnaDiskValue,
        Func<bool> flagUpdateImmediately,
        Action<TargetSeedData> submit,
        Action refreshRows,
        Func<float?>? getSeedPotency = null,
        Func<float?>? getDiskPotency = null)
    {
        if (getterSeedValue() == null && getterDnaDiskValue() == null)
            return null;

        return new SeedDnaConsoleWindowRow(
            title,
            seedPresent,
            dnaDiskPresent,
            getterSeedValue,
            getterDnaDiskValue,
            setterSeedValue,
            setterDnaDiskValue,
            flagUpdateImmediately,
            submit,
            refreshRows,
            getSeedPotency,
            getDiskPotency);
    }

    private Action SetupActionButton(
        Button actionBtn,
        bool secondDataPresent,
        Func<object?> getter,
        Action<object?> setter,
        Label sourceLabel,
        Label targetLabel,
        Func<bool> flagUpdateImmediately,
        Action<TargetSeedData> submit,
        Action refreshRows,
        TargetSeedData target)
    {
        actionBtn.Disabled = getter() == null || !secondDataPresent || sourceLabel.Text == targetLabel.Text;

        var targetPotencyFunc = target == TargetSeedData.Seed ? _getSeedPotency : _getDiskPotency;

        var action = () =>
        {
            var value = getter();
            if (value == null)
                return;

            setter(value);
            SetLabelValue(targetLabel, value, targetPotencyFunc?.Invoke());
            refreshRows();
            _extractButton!.Disabled = true;
            _replaceButton!.Disabled = true;

            if (flagUpdateImmediately())
                submit(target);
        };

        actionBtn.OnPressed += _ => action();
        return action;
    }

    private static Label CreateTitleLabel(string title)
    {
        return new Label
        {
            Text = title,
            Margin = LeftMargin,
        };
    }

    private static Label CreateValueLabel()
    {
        return new Label
        {
            StyleClasses = { "monospace" },
            Margin = LeftRightMargin,
        };
    }

    private static Button CreateActionButton(string title)
    {
        return new Button
        {
            Text = title,
        };
    }

    private Label SetLabelValue(Label valueLabel, object? value, float? potency = null)
    {
        if (value == null)
        {
            valueLabel.Text = "-";
            valueLabel.Align = Label.AlignMode.Center;
            return valueLabel;
        }

        if (value is SeedChemQuantityDto chem)
        {
            if (potency == null)
            {
                valueLabel.Text = "-";
                valueLabel.Align = Label.AlignMode.Center;
                return valueLabel;
            }

            var amount = chem.PotencyDivisor <= 0
                ? chem.Max
                : FixedPoint2.Min(chem.Min + FixedPoint2.New(potency.Value / chem.PotencyDivisor), chem.Max);

            valueLabel.Text = $"{amount.ToString(null, CultureInfo.InvariantCulture)}u";
            valueLabel.Align = Label.AlignMode.Right;
            return valueLabel;
        }

        valueLabel.Text = FormatValue(value);
        valueLabel.Align = Label.AlignMode.Right;
        return valueLabel;
    }

    private static string FormatValue(object value)
    {
        return value switch
        {
            float floatValue => floatValue.ToString("0.##", CultureInfo.InvariantCulture),
            double doubleValue => doubleValue.ToString("0.##", CultureInfo.InvariantCulture),
            FixedPoint2 fixedPoint => fixedPoint.ToString(null, CultureInfo.InvariantCulture),
            bool boolValue => boolValue ? Loc.GetString("seed-dna-value-yes") : Loc.GetString("seed-dna-value-no"),
            SharedHarvestTypeDto harvestType => Loc.GetString($"seed-dna-harvest-{harvestType.ToString().ToLowerInvariant()}"),
            _ => value.ToString() ?? string.Empty,
        };
    }
}
