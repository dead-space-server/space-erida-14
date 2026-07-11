:: SPDX-FileCopyrightText: 2026 Sidzaru <110207359+Sidzaru@users.noreply.github.com>
:: SPDX-FileCopyrightText: 2026 OpenWendor
:: SPDX-License-Identifier: MIT

cd ..\..\

mkdir Scripts\logs

del Scripts\logs\Content.YAMLLinter.log
dotnet run --project Content.YAMLLinter/Content.YAMLLinter.csproj -c DebugOpt -- NUnit.ConsoleOut=0 > Scripts\logs\Content.YAMLLinter.log

pause
