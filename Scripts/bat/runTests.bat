:: SPDX-FileCopyrightText: 2026 Sidzaru <110207359+Sidzaru@users.noreply.github.com>
:: SPDX-FileCopyrightText: 2026 OpenWendor
:: SPDX-License-Identifier: MIT

cd ..\..\

mkdir Scripts\logs

del Scripts\logs\Content.Tests.log
dotnet test Content.Tests/Content.Tests.csproj -c DebugOpt -- NUnit.ConsoleOut=0 > Scripts\logs\Content.Tests.log

pause
