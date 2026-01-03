using Content.Client.Stylesheets;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._Erida.Roadmap;

public sealed class RoadmapButton : Button
{

    public RoadmapButton()
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void EnteredTree()
    {
        base.EnteredTree();
        Text = Loc.GetString("roadmap-button");
        StyleClasses.Add(StyleNano.ButtonCaution);
    }

    protected override void ExitedTree()
    {
        base.ExitedTree();

    }
}
