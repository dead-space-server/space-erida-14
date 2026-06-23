using Content.Shared.CrewManifest;
using Content.Client.Popups;
using Content.Shared.Popups;
using Content.Shared.StatusIcon;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;
using System.Numerics;
using Content.Shared.Roles;

namespace Content.Client.CrewManifest.UI;

public sealed class CrewManifestSection : BoxContainer
{
    private readonly IClipboardManager _clipboard; // Erida edit
    private readonly PopupSystem _popup; // Erida edit

    public CrewManifestSection(
        IPrototypeManager prototypeManager,
        SpriteSystem spriteSystem,
        DepartmentPrototype section,
        List<CrewManifestEntry> entries)
    {
        _clipboard = IoCManager.Resolve<IClipboardManager>(); // Erida edit
        _popup = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<PopupSystem>(); // Erida edit

        Orientation = LayoutOrientation.Vertical;
        HorizontalExpand = true;

        AddChild(new Label()
        {
            StyleClasses = { "LabelBig" },
            Text = Loc.GetString(section.Name)
        });

        var gridContainer = new GridContainer()
        {
            HorizontalExpand = true,
            Columns = 2
        };

        AddChild(gridContainer);

        foreach (var entry in entries)
        {
            var name = new RichTextLabel()
            {
                HorizontalExpand = true,
            };
            name.SetMessage(entry.Name);

            var titleContainer = new BoxContainer()
            {
                Orientation = LayoutOrientation.Horizontal,
                HorizontalExpand = true
            };

            var title = new RichTextLabel();
            title.SetMessage(entry.JobTitle);


            if (prototypeManager.TryIndex<JobIconPrototype>(entry.JobIcon, out var jobIcon))
            {
                var icon = new TextureRect()
                {
                    TextureScale = new Vector2(2, 2),
                    VerticalAlignment = VAlignment.Center,
                    Texture = spriteSystem.Frame0(jobIcon.Icon),
                    Margin = new Thickness(0, 0, 4, 0)
                };

                titleContainer.AddChild(icon);
                titleContainer.AddChild(title);
            }
            else
            {
                titleContainer.AddChild(title);
            }

            // Erida start
            gridContainer.AddChild(CreateClipboardButton(entry.Name, name));
            gridContainer.AddChild(CreateClipboardButton(entry.JobTitle, titleContainer));
            // Erida end
        }
    }

    // Erida start
    private ContainerButton CreateClipboardButton(string text, Control child)
    {
        var button = new ContainerButton
        {
            HorizontalExpand = true
        };

        button.OnPressed += _ =>
        {
            _clipboard.SetText(text);
            _popup.PopupCursor(Loc.GetString("crew-manifest-entry-copied"), PopupType.Small);
        };
        button.AddChild(child);

        return button;
    }
    // Erida end
}
