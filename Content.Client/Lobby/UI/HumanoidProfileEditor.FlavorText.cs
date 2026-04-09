using Content.Client.Stylesheets;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;

namespace Content.Client.Lobby.UI;

public sealed partial class HumanoidProfileEditor
{
    private bool _allowFlavorText;

    private FlavorText.FlavorText? _flavorText;
    private TextEdit? _flavorTextEdit;

    // Erida-Start
    private TextEdit? _flavorTextOOCEdit;
    private TextEdit? _characterTextEdit;
    private TextEdit? _greenTextEdit;
    private TextEdit? _yellowTextEdit;
    private TextEdit? _redTextEdit;
    private TextEdit? _tagsTextEdit;
    private TextEdit? _linksTextEdit;
    private TextEdit? _nsfwTextEdit;

    private TextEdit? _nsfwLinksTextEdit;
    private TextEdit? _flavorTextNSFWOOCEdit;
    private TextEdit? _nsfwTagsTextEdit;
    // Erida end

    /// <summary>
    /// Refreshes the flavor text editor status.
    /// </summary>
    public void RefreshFlavorText()
    {
        if (_allowFlavorText)
        {
            if (_flavorText != null)
                return;

            _flavorText = new FlavorText.FlavorText();
            TabContainer.AddChild(_flavorText);
            TabContainer.SetTabTitle(TabContainer.ChildCount - 1, Loc.GetString("humanoid-profile-editor-flavortext-tab"));
            _flavorTextEdit = _flavorText.CFlavorTextInput;

            // Erida-Start
            _flavorTextOOCEdit = _flavorText.CFlavorOOCTextInput;
            _characterTextEdit = _flavorText.CCharacterTextInput;
            _greenTextEdit = _flavorText.CGreenTextInput;
            _yellowTextEdit = _flavorText.CYellowTextInput;
            _redTextEdit = _flavorText.CRedTextInput;
            _tagsTextEdit = _flavorText.CTagsTextInput;
            _linksTextEdit = _flavorText.CLinksTextInput;

            _nsfwTextEdit = _flavorText.CNSFWTextInput;
            _nsfwLinksTextEdit = _flavorText.CNSFWLinksTextInput;
            _flavorTextNSFWOOCEdit = _flavorText.CFlavorNSFWOOCTextInput;
            _nsfwTagsTextEdit = _flavorText.CNSFWTagsTextInput;

            _flavorText.OnFlavorTextChanged += OnFlavorTextChange;
            _flavorText.OnFlavorOOCTextChanged += OnFlavorOOCTextChange;
            _flavorText.OnCharacterTextChanged += OnCharacterFlavorTextChange;
            _flavorText.OnGreenTextChanged += OnGreenFlavorTextChange;
            _flavorText.OnYellowTextChanged += OnYellowFlavorTextChange;
            _flavorText.OnRedTextChanged += OnRedFlavorTextChange;
            _flavorText.OnTagsTextChanged += OnTagsFlavorTextChange;
            _flavorText.OnLinksTextChanged += OnLinksFlavorTextChange;
            _flavorText.OnNSFWTextChanged += OnNSFWFlavorTextChange;

            _flavorText.OnNSFWLinksTextChanged += OnNSFWLinksFlavorTextChange;
            _flavorText.OnNSFWFlavorOOCTextChanged += OnNSFWFlavorTextChange;
            _flavorText.OnNSFWTagsTextChanged += OnNSFWTagsFlavorTextChange;

            _flavorText.OnFlavorTabChanged += OnTabChanged;
            // Erida end
        }
        else
        {
            if (_flavorText == null)
                return;

            TabContainer.RemoveChild(_flavorText);
            _flavorText.OnFlavorTextChanged -= OnFlavorTextChange;
            // Erida start
            _flavorText.OnFlavorOOCTextChanged -= OnFlavorOOCTextChange;
            _flavorText.OnCharacterTextChanged -= OnCharacterFlavorTextChange;
            _flavorText.OnGreenTextChanged -= OnGreenFlavorTextChange;
            _flavorText.OnYellowTextChanged -= OnYellowFlavorTextChange;
            _flavorText.OnRedTextChanged -= OnRedFlavorTextChange;
            _flavorText.OnTagsTextChanged -= OnTagsFlavorTextChange;
            _flavorText.OnLinksTextChanged -= OnLinksFlavorTextChange;
            _flavorText.OnNSFWTextChanged -= OnNSFWFlavorTextChange;

            _flavorText.OnNSFWLinksTextChanged -= OnNSFWLinksFlavorTextChange;
            _flavorText.OnNSFWFlavorOOCTextChanged -= OnFlavorNSFWOOCTextChange;
            _flavorText.OnNSFWTagsTextChanged -= OnNSFWTagsFlavorTextChange;
            _flavorText.OnFlavorTabChanged -= OnTabChanged;
            // Erida end
            _flavorText.Dispose();
            _flavorTextEdit?.Dispose();
            _flavorTextEdit = null;
            _flavorText = null;
            // Erida start
            _flavorTextOOCEdit = null;
            _characterTextEdit = null;
            _greenTextEdit = null;
            _yellowTextEdit = null;
            _redTextEdit = null;
            _tagsTextEdit = null;
            _linksTextEdit = null;
            _nsfwTextEdit = null;

            _nsfwLinksTextEdit = null;
            _flavorTextNSFWOOCEdit = null;
            _nsfwTagsTextEdit = null;
            // Erida end
        }
    }

    private void OnFlavorTextChange(string content)
    {
        if (Profile is null)
            return;

        Profile = Profile.WithFlavorText(content);
        SetDirty();
    }

    // Erida start
    private void UpdateFlavorTextEdit()
    {
        if (_flavorTextEdit != null)
        {
            _flavorTextEdit.TextRope = new Rope.Leaf(Profile?.FlavorText ?? "");
        }

        if (_flavorTextOOCEdit != null)
        {
            _flavorTextOOCEdit.TextRope = new Rope.Leaf(Profile?.OOCFlavorText ?? "");
        }

        if (_characterTextEdit != null)
        {
            _characterTextEdit.TextRope = new Rope.Leaf(Profile?.CharacterFlavorText ?? "");
        }

        if (_greenTextEdit != null)
        {
            _greenTextEdit.TextRope = new Rope.Leaf(Profile?.GreenFlavorText ?? "");
        }

        if (_yellowTextEdit != null)
        {
            _yellowTextEdit.TextRope = new Rope.Leaf(Profile?.YellowFlavorText ?? "");
        }

        if (_redTextEdit != null)
        {
            _redTextEdit.TextRope = new Rope.Leaf(Profile?.RedFlavorText ?? "");
        }

        if (_tagsTextEdit != null)
        {
            _tagsTextEdit.TextRope = new Rope.Leaf(Profile?.TagsFlavorText ?? "");
        }

        if (_linksTextEdit != null)
        {
            _linksTextEdit.TextRope = new Rope.Leaf(Profile?.LinksFlavorText ?? "");
        }

        if (_nsfwTextEdit != null)
        {
            _nsfwTextEdit.TextRope = new Rope.Leaf(Profile?.NSFWFlavorText ?? "");
        }

        if (_flavorTextNSFWOOCEdit != null)
        {
            _flavorTextNSFWOOCEdit.TextRope = new Rope.Leaf(Profile?.NSFWOOCFlavorText ?? "");
        }
        if (_nsfwLinksTextEdit != null)
        {
            _nsfwLinksTextEdit.TextRope = new Rope.Leaf(Profile?.NSFWLinksFlavorText ?? "");
        }
        if (_nsfwTagsTextEdit != null)
        {
            _nsfwTagsTextEdit.TextRope = new Rope.Leaf(Profile?.NSFWTagsFlavorText ?? "");
        }
    }

    private void UpdateCustomSpeciesEdit()
    {
        IsCustomSpecies.Pressed = false;
        CustomSpeciesContainer.Visible = false;

        CustomSpeciesEdit.Text = Profile?.CustomSpecies ?? string.Empty;
        if (!string.IsNullOrEmpty(Profile?.CustomSpecies))
        {
            IsCustomSpecies.Pressed = true;
            CustomSpeciesContainer.Visible = true;
        }
    }

    private void OnFlavorOOCTextChange(string content)
    {
        if (Profile is null)
            return;

        Profile = Profile.WithOOCFlavorText(content);
        SetDirty();


    }

    private void OnCharacterFlavorTextChange(string content)
    {
        if (Profile is null)
            return;

        Profile = Profile.WithCharacterText(content);
        SetDirty();


    }

    private void OnGreenFlavorTextChange(string content)
    {
        if (Profile is null)
            return;

        Profile = Profile.WithGreenPreferencesText(content);
        SetDirty();


    }

    private void OnYellowFlavorTextChange(string content)
    {
        if (Profile is null)
            return;

        Profile = Profile.WithYellowPreferencesText(content);
        SetDirty();


    }

    private void OnRedFlavorTextChange(string content)
    {
        if (Profile is null)
            return;

        Profile = Profile.WithRedPreferencesText(content);
        SetDirty();


    }

    private void OnTagsFlavorTextChange(string content)
    {
        if (Profile is null)
            return;

        Profile = Profile.WithTagsText(content);
        SetDirty();


    }

    private void OnLinksFlavorTextChange(string content)
    {
        if (Profile is null)
            return;

        Profile = Profile.WithLinksText(content);
        SetDirty();


    }

    private void OnNSFWFlavorTextChange(string content)
    {
        if (Profile is null)
            return;

        Profile = Profile.WithNSFWPreferencesText(content);
        SetDirty();


    }

    private void OnFlavorNSFWOOCTextChange(string content)
    {
        if (Profile is null)
            return;

        Profile = Profile.WithNSFWOOCFlavorText(content);
        SetDirty();


    }

    private void OnNSFWLinksFlavorTextChange(string content)
    {
        if (Profile is null)
            return;

        Profile = Profile.WithNSFWLinksText(content);
        SetDirty();


    }

    private void OnNSFWTagsFlavorTextChange(string content)
    {
        if (Profile is null)
            return;

        Profile = Profile.WithNSFWTagsText(content);
        SetDirty();


    }

    private void UpdateNSFWPreviewVisibility(bool showNsfw)
    {
        if (_flavorText == null)
            return;
    }
    private void OnTabChanged(int tab)
    {
        switch (tab)
        {
            case 3:
                UpdateNSFWPreviewVisibility(true);
                break;
            default:
                UpdateNSFWPreviewVisibility(false);
                break;
        }
    }

    private void ProcessLinks(string linksText, BoxContainer linksContainer)
    {
        if (linksContainer == null)
            return;

        linksContainer.RemoveAllChildren();
        if (string.IsNullOrEmpty(linksText))
            return;

        var links = linksText.Split(new[] { ',', ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var link in links)
        {
            if (IsValidUrl(link))
            {
                CreateLinkButton(link, linksContainer); // Erida edit
            }
            else
            {
                CreateLinkTextLabel(link, linksContainer); // Erida edit
            }
        }
    }

    private bool IsValidUrl(string url)
    {
        return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("www.", StringComparison.OrdinalIgnoreCase);
    }

    private void CreateLinkButton(string url, BoxContainer linksContainer) // Erida edit
    {
        var button = new Button
        {
            Text = GetLinkDisplayText(url),
            ToolTip = Loc.GetString("humanoid-profile-editor-link-tooltip", ("url", url)),
            HorizontalExpand = true,
            HorizontalAlignment = HAlignment.Center,
            StyleClasses = { StyleNano.ButtonOpenBoth }
        };

        button.OnPressed += _ => OpenLink(url);

        linksContainer.AddChild(button); // Erida edit
    }

    private void CreateLinkTextLabel(string text, BoxContainer linksContainer) // Erida edit
    {
        var label = new Label
        {
            Text = text,
            HorizontalExpand = true,
            HorizontalAlignment = HAlignment.Center,
            FontColorOverride = Color.Gray
        };

        linksContainer.AddChild(label); // Erida edit
    }

    private string GetLinkDisplayText(string url)
    {
        if (url.Length > 40)
        {
            return url[..37] + "...";
        }
        return url;
    }

    private void OpenLink(string url)
    {
        if (url.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
            url = "https://" + url;

        var uriOpener = IoCManager.Resolve<IUriOpener>();
        uriOpener.OpenUri(url);
    }
    // Erida end
}
