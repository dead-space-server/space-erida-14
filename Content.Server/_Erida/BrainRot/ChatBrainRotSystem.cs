

using System.Linq;
using Content.Server.Chat.Managers;
using Content.Shared._Orion.ServerProtection.Chat;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using SQLitePCL;

public sealed partial class ChatBrainRotSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    private DamageableSystem _damageableSystem => EntityManager.System<DamageableSystem>();


    private readonly HashSet<string> _brainrotWords = new();
    private readonly string _brainrotWordsCollection = "BrainRotWords";
    private bool _cacheDone;
    private DamageSpecifier _brainRotDamage = new()
    {
        DamageDict = new()
        {
            { "Cellular", 10 }
        }
    };

    public override void Initialize()
    {
        base.Initialize();

        _prototypeManager.PrototypesReloaded += OnProtoReloaded;
    }

    public void CheckBrainRot(EntityUid entity, string message)
    {
        if (!_cacheDone)
            UpdateBrainrotWords();

        foreach (var word in _brainrotWords.Where(word => message.Contains(word, StringComparison.OrdinalIgnoreCase)))
        {
            HandleViolation(entity, word);
            return;
        }
    }

    private void OnProtoReloaded(PrototypesReloadedEventArgs args)
    {
        UpdateBrainrotWords();
    }

    private void UpdateBrainrotWords()
    {
        _brainrotWords.Clear();

        foreach (var proto in _prototypeManager.EnumeratePrototypes<ChatProtectionListPrototype>())
        {
            if (proto.ID != _brainrotWordsCollection) continue;

            foreach (var word in proto.Words)
            {
                _brainrotWords.Add(word);
            }
        }

        _cacheDone = true;
    }

    private void HandleViolation(EntityUid entity, string word)
    {
        _damageableSystem.TryChangeDamage(entity, _brainRotDamage, true);
        if (TryComp<ActorComponent>(entity, out var sourceActor))
        {
            var caution = Loc.GetString("brainrot-warning-message", ("word", word));
            _chatManager.ChatMessageToOne(ChatChannel.Server, caution, caution, default, false, sourceActor.PlayerSession.Channel);
        }
    }
}
