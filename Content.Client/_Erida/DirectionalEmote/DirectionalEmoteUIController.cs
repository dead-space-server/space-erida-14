using Content.Client.Chat.TypingIndicator;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;

namespace Content.Client._Erida.DirectionalEmote;

[UsedImplicitly]
public sealed class DirectionalEmoteUIController : UIController
{
    [UISystemDependency] private readonly DirectionalEmoteSystem _directionalEmoteSystem = default!;
    [UISystemDependency] private readonly TypingIndicatorSystem _typingIndicator = default!;

    private DirectionalEmoteWindow _emoteWindow = default!;

    public void OpenWindow()
    {
        EnsureWindow();

        _emoteWindow.OpenCentered();
        _emoteWindow.MoveToFront();

        _typingIndicator.ClientChangedChatFocus(true);

        _emoteWindow.MessageChanged += () =>
        {
            _typingIndicator.ClientChangedChatText();
            _typingIndicator.ClientChangedChatFocus(true);
        };

        _emoteWindow.AcceptPressed += () =>
        {
            _typingIndicator.ClientChangedChatFocus(false);
            _typingIndicator.ClientSubmittedChatText();
            _directionalEmoteSystem.ShowMessage(_emoteWindow.Source, _emoteWindow.Target, _emoteWindow.Text);
            _emoteWindow.Close();
        };
    }

    private void EnsureWindow()
    {
        if (_emoteWindow is { Disposed: false })
            return;

        _emoteWindow = UIManager.CreateWindow<DirectionalEmoteWindow>();
    }

    public void ToggleWindow(NetEntity source, NetEntity emoteTarget)
    {
        EnsureWindow();
        _emoteWindow.Source = source;
        _emoteWindow.Target = emoteTarget;

        if (_emoteWindow.IsOpen)
        {
            _typingIndicator.ClientChangedChatFocus(false);
            _emoteWindow.Close();
        }
        else
        {
            OpenWindow();
        }
    }
}
