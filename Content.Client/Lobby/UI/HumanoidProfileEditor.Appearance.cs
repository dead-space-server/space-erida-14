using System.Linq;
using System.Numerics;
using Content.Client.UserInterface.Systems.Guidebook;
using Content.Shared.Guidebook;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Preferences;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Enums;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;

namespace Content.Client.Lobby.UI;

public sealed partial class HumanoidProfileEditor
{
    public event Action<List<ProtoId<GuideEntryPrototype>>>? OnOpenGuidebook;

    private ColorSelectorSliders _rgbSkinColorSelector;
    private List<SpeciesPrototype> _species = new();
    private static readonly ProtoId<GuideEntryPrototype> DefaultSpeciesGuidebook = "Species";

    public void UpdateSpeciesGuidebookIcon()
    {
        SpeciesInfoButton.StyleClasses.Clear();

        var species = Profile?.Species;
        if (species is null)
            return;

        if (!_prototypeManager.Resolve<SpeciesPrototype>(species, out var speciesProto))
            return;

        // Don't display the info button if no guide entry is found
        if (!_prototypeManager.HasIndex<GuideEntryPrototype>(species))
            return;

        const string style = "SpeciesInfoDefault";
        SpeciesInfoButton.StyleIdentifier = style;
    }

    private void UpdateGenderControls()
    {
        if (Profile == null)
        {
            return;
        }

        PronounsButton.SelectId((int)Profile.Gender);
    }

    private void UpdateAgeEdit()
    {
        AgeEdit.Text = Profile?.Age.ToString() ?? "";
    }

    private void UpdateSexControls()
    {
        if (Profile == null)
            return;

        SexButton.Clear();

        var sexes = new List<Sex>();

        // add species sex options, default to just none if we are in bizzaro world and have no species
        if (_prototypeManager.Resolve(Profile.Species, out var speciesProto))
        {
            foreach (var sex in speciesProto.Sexes)
            {
                sexes.Add(sex);
            }
        }
        else
        {
            sexes.Add(Sex.Unsexed);
        }

        // add button for each sex
        foreach (var sex in sexes)
        {
            SexButton.AddItem(Loc.GetString($"humanoid-profile-editor-sex-{sex.ToString().ToLower()}-text"), (int)sex);
        }

        if (sexes.Contains(Profile.Sex))
            SexButton.SelectId((int)Profile.Sex);
        else
            SexButton.SelectId((int)sexes[0]);
    }

    private void UpdateEyePickers()
    {
        if (Profile == null)
        {
            return;
        }

        _markingsModel.SetOrganEyeColor(Profile.Appearance.EyeColor);
        EyeColorPicker.SetData(Profile.Appearance.EyeColor);
    }

    private void UpdateSkinColor()
    {
        if (Profile == null)
            return;

        var skin = _prototypeManager.Index<SpeciesPrototype>(Profile.Species).SkinColoration;
        var strategy = _prototypeManager.Index(skin).Strategy;

        switch (strategy.InputType)
        {
            case SkinColorationStrategyInput.Unary:
                {
                    if (!Skin.Visible)
                    {
                        Skin.Visible = true;
                        RgbSkinColorContainer.Visible = false;
                    }

                    Skin.Value = strategy.ToUnary(Profile.Appearance.SkinColor);

                    break;
                }
            case SkinColorationStrategyInput.Color:
                {
                    if (!RgbSkinColorContainer.Visible)
                    {
                        Skin.Visible = false;
                        RgbSkinColorContainer.Visible = true;
                    }

                    _rgbSkinColorSelector.Color = strategy.ClosestSkinColor(Profile.Appearance.SkinColor);

                    break;
                }
        }
    }

    private void UpdateSpawnPriorityControls()
    {
        if (Profile == null)
        {
            return;
        }

        SpawnPriorityButton.SelectId((int)Profile.SpawnPriority);
    }

    /// <summary>
    /// Refreshes the species selector.
    /// </summary>
    public void RefreshSpecies()
    {
        SpeciesButton.Clear();
        _species.Clear();

        _species.AddRange(_prototypeManager.EnumeratePrototypes<SpeciesPrototype>().Where(o => o.RoundStart));
        _species.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase));
        var speciesIds = _species.Select(o => o.ID).ToList();

        for (var i = 0; i < _species.Count; i++)
        {
            var name = Loc.GetString(_species[i].Name);
            SpeciesButton.AddItem(name, i);

            if (Profile?.Species.Equals(_species[i].ID) == true)
            {
                SpeciesButton.SelectId(i);
            }
        }

        // If our species isn't available then reset it to default.
        if (Profile != null)
        {
            if (!speciesIds.Contains(Profile.Species))
            {
                SetSpecies(HumanoidCharacterProfile.DefaultSpecies);
            }
        }
    }

    private void SetSpecies(string newSpecies)
    {
        Profile = Profile?.WithSpecies(newSpecies);
        OnSkinColorOnValueChanged(); // Species may have special color prefs, make sure to update it.
        _markingsModel.OrganData = _markingManager.GetMarkingData(newSpecies);
        _markingsModel.ValidateMarkings();
        // In case there's job restrictions for the species
        RefreshJobs();
        // In case there's species restrictions for loadouts
        RefreshLoadouts();
        UpdateSexControls(); // update sex for new species
        UpdateSpeciesGuidebookIcon();
        ReloadPreview();
    }

    // Erida-start
    private void SetCustomSpecies(string newSpecies)
    {
        Profile = Profile?.WithCustomSpecies(newSpecies);
        IsDirty = true;
    }
    // Erida-end

    private void SetAge(int newAge)
    {
        Profile = Profile?.WithAge(newAge);
        ReloadPreview();
    }

    private void SetSex(Sex newSex)
    {
        Profile = Profile?.WithSex(newSex);
        // for convenience, default to most common gender when new sex is selected
        switch (newSex)
        {
            case Sex.Male:
                Profile = Profile?.WithGender(Gender.Male);
                break;
            case Sex.Female:
                Profile = Profile?.WithGender(Gender.Female);
                break;
            default:
                Profile = Profile?.WithGender(Gender.Epicene);
                break;
        }

        UpdateGenderControls();
        _markingsModel.SetOrganSexes(newSex);
        ReloadPreview();
    }

    private void SetGender(Gender newGender)
    {
        Profile = Profile?.WithGender(newGender);
        ReloadPreview();
    }

    private void SetSpawnPriority(SpawnPriorityPreference newSpawnPriority)
    {
        Profile = Profile?.WithSpawnPriorityPreference(newSpawnPriority);
        SetDirty();
    }

    private void OnSpeciesInfoButtonPressed(BaseButton.ButtonEventArgs args)
    {
        // TODO GUIDEBOOK
        // make the species guide book a field on the species prototype.
        // I.e., do what jobs/antags do.

        var guidebookController = UserInterfaceManager.GetUIController<GuidebookUIController>();
        var species = Profile?.Species ?? HumanoidCharacterProfile.DefaultSpecies;
        var page = DefaultSpeciesGuidebook;
        if (_prototypeManager.HasIndex<GuideEntryPrototype>(species))
            page = new ProtoId<GuideEntryPrototype>(species.Id); // Gross. See above todo comment.

        if (_prototypeManager.Resolve(DefaultSpeciesGuidebook, out var guideRoot))
        {
            var dict = new Dictionary<ProtoId<GuideEntryPrototype>, GuideEntry>();
            dict.Add(DefaultSpeciesGuidebook, guideRoot);
            //TODO: Don't close the guidebook if its already open, just go to the correct page
            guidebookController.OpenGuidebook(dict, includeChildren: true, selected: page);
        }
    }

    private void OnSkinColorOnValueChanged()
    {
        if (Profile is null) return;

        var skin = _prototypeManager.Index<SpeciesPrototype>(Profile.Species).SkinColoration;
        var strategy = _prototypeManager.Index(skin).Strategy;

        switch (strategy.InputType)
        {
            case SkinColorationStrategyInput.Unary:
                {
                    if (!Skin.Visible)
                    {
                        Skin.Visible = true;
                        RgbSkinColorContainer.Visible = false;
                    }

                    var color = strategy.FromUnary(Skin.Value);

                    _markingsModel.SetOrganSkinColor(color);
                    Profile = Profile.WithCharacterAppearance(Profile.Appearance.WithSkinColor(color));

                    break;
                }
            case SkinColorationStrategyInput.Color:
                {
                    if (!RgbSkinColorContainer.Visible)
                    {
                        Skin.Visible = false;
                        RgbSkinColorContainer.Visible = true;
                    }

                    var color = strategy.ClosestSkinColor(_rgbSkinColorSelector.Color);

                    _markingsModel.SetOrganSkinColor(color);
                    Profile = Profile.WithCharacterAppearance(Profile.Appearance.WithSkinColor(color));

                    break;
                }
        }

        ReloadProfilePreview();
    }

    // begin Goobstation: port EE height/width sliders
    private void UpdateHeightWidthSliders()
    {
        if (Profile is null)
            return;

        var species = _species.Find(x => x.ID == Profile?.Species) ?? _species.First();

        // we increase the min/max values of the sliders before we set their value, just so that we don't accidentally clamp down on a value loaded from a profile when we shouldn't
        HeightSlider.MinValue = 0;
        HeightSlider.MaxValue = 2;
        HeightSlider.SetValueWithoutEvent(Profile?.Height ?? species.DefaultHeight);
        HeightSlider.MinValue = species.MinHeight;
        HeightSlider.MaxValue = species.MaxHeight;

        WidthSlider.MinValue = 0;
        WidthSlider.MaxValue = 2;
        WidthSlider.SetValueWithoutEvent(Profile?.Width ?? species.DefaultWidth);
        WidthSlider.MinValue = species.MinWidth;
        WidthSlider.MaxValue = species.MaxWidth;

        var height = MathF.Round(species.AverageHeight * HeightSlider.Value);
        HeightLabel.Text = Loc.GetString("humanoid-profile-editor-height-label", ("height", (int)height));

        var width = MathF.Round(species.AverageWidth * WidthSlider.Value);
        WidthLabel.Text = Loc.GetString("humanoid-profile-editor-width-label", ("width", (int)width));

        UpdateDimensions(SliderUpdate.Both);
    }

    private enum SliderUpdate
    {
        Height,
        Width,
        Both
    }

    private void UpdateDimensions(SliderUpdate updateType)
    {
        if (Profile == null)
            return;

        var species = _species.Find(x => x.ID == Profile?.Species) ?? _species.First();

        var heightValue = Math.Clamp(HeightSlider.Value, species.MinHeight, species.MaxHeight);
        var widthValue = Math.Clamp(WidthSlider.Value, species.MinWidth, species.MaxWidth);
        var sizeRatio = species.SizeRatio;
        var ratio = heightValue / widthValue;

        if (updateType == SliderUpdate.Height || updateType == SliderUpdate.Both)
            if (ratio < 1 / sizeRatio || ratio > sizeRatio)
                widthValue = heightValue / (ratio < 1 / sizeRatio ? (1 / sizeRatio) : sizeRatio);

        if (updateType == SliderUpdate.Width || updateType == SliderUpdate.Both)
            if (ratio < 1 / sizeRatio || ratio > sizeRatio)
                heightValue = widthValue * (ratio < 1 / sizeRatio ? (1 / sizeRatio) : sizeRatio);

        heightValue = Math.Clamp(heightValue, species.MinHeight, species.MaxHeight);
        widthValue = Math.Clamp(widthValue, species.MinWidth, species.MaxWidth);

        HeightSlider.SetValueWithoutEvent(heightValue);
        WidthSlider.SetValueWithoutEvent(widthValue);

        SetProfileHeight(heightValue);
        SetProfileWidth(widthValue);

        var height = MathF.Round(species.AverageHeight * HeightSlider.Value);
        HeightLabel.Text = Loc.GetString("humanoid-profile-editor-height-label", ("height", (int)height));

        var width = MathF.Round(species.AverageWidth * WidthSlider.Value);
        WidthLabel.Text = Loc.GetString("humanoid-profile-editor-width-label", ("width", (int)width));

        UpdateWeight();
    }

    private void UpdateWeight()
    {
        if (Profile == null)
            return;

        var species = _species.Find(x => x.ID == Profile.Species) ?? _species.First();
        //  TODO: Remove obsolete method
        _prototypeManager.Index(species.Prototype).TryGetComponent<FixturesComponent>(out var fixture, _entManager.ComponentFactory);

        if (fixture != null)
        {
            var avg = (Profile.Width + Profile.Height) / 2;
            var weight = FixtureSystem.GetMassData(fixture.Fixtures["fix1"].Shape, fixture.Fixtures["fix1"].Density).Mass * avg;
            WeightLabel.Text = Loc.GetString("humanoid-profile-editor-weight-label", ("weight", (int)weight));
        }
        else // Whelp, the fixture doesn't exist, guesstimate it instead
            WeightLabel.Text = Loc.GetString("humanoid-profile-editor-weight-label", ("weight", (int)71));

        if (SpriteView.Sprite != null)
            _sharedScaleVisualsSystem.SetSpriteScale(SpriteView.PreviewDummy, new Vector2(Profile.Width, Profile.Height));
    }

    private void SetProfileHeight(float height)
    {
        Profile = Profile?.WithHeight(height);
        ReloadProfilePreview();
        IsDirty = true;
    }

    private void SetProfileWidth(float width)
    {
        Profile = Profile?.WithWidth(width);
        ReloadProfilePreview();
        IsDirty = true;
    }
    // end Goobstation: port EE height/width sliders

    // Corvax-TTS-Start
    private void SetVoice(string newVoice)
    {
        Profile = Profile?.WithVoice(newVoice);
        IsDirty = true;
    }
    // Corvax-TTS-End
}
