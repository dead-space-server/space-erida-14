using Content.Server.Administration.Managers;
using Content.Shared.Administration;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Robust.Shared.Console;
using Robust.Shared.Player;

namespace Content.Server.Administration.Commands
{
    [AdminCommand(AdminFlags.ForceDeadmin | AdminFlags.Host | AdminFlags.Stealth)]
    public sealed class ForceDeadminCommand : LocalizedEntityCommands
    {
        [Dependency] private readonly IAdminManager _adminManager = default!;
        [Dependency] private readonly ISharedPlayerManager _players = default!;
        [Dependency] private readonly ISharedAdminLogManager _sharedAdminLogManager = default!;

        public override string Command => "forceDeadmin";

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var player = shell.Player;
            if (player == null)
            {
                shell.WriteLine(Loc.GetString($"shell-cannot-run-command-from-server"));
                return;
            }

            // На всякий
            if (_adminManager.GetAdminData(player, includeDeAdmin: true) == null)
            {
                shell.WriteLine(Loc.GetString($"cmd-readmin-not-an-admin"));
                return;
            }

            var target = args[0];
            if (_players.TryGetSessionByUsername(target, out var session)
                && session != null
                && _adminManager.IsAdmin(session))
            {
                _adminManager.DeAdmin(session);
                _sharedAdminLogManager.Add(LogType.AdminCommands, LogImpact.Medium, $"{shell.Player} forced deadmin for {target}");
            }
            else
            {
                shell.WriteError(Loc.GetString("cmd-force-deadmin-error", ("str", target)));
            }
        }

        public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            if (args.Length == 0)
                return CompletionResult.Empty;

            var last = args[^1];

            var admins = new List<string>();

            foreach (var user in _players.Sessions)
            {
                if (user.Name != string.Empty
                && user.Name.StartsWith(last, StringComparison.CurrentCultureIgnoreCase)
                && user.AttachedEntity is EntityUid uid
                && _adminManager.IsAdmin(uid) == true)
                {
                    admins.Add(user.Name);
                }
            }

            var hint = Loc.GetString("cmd-force-deadmin-hint");
            return CompletionResult.FromHintOptions(admins, hint);
        }
    }
}
