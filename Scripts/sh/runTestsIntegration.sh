# SPDX-FileCopyrightText: 2026 Sidzaru <110207359+Sidzaru@users.noreply.github.com>
# SPDX-FileCopyrightText: 2026 OpenWendor
# SPDX-License-Identifier: MIT

cd ../../

mkdir Scripts/logs

rm Scripts/logs/Content.IntegrationTests.log
dotnet test Content.IntegrationTests/Content.IntegrationTests.csproj -c DebugOpt -- NUnit.ConsoleOut=0 NUnit.MapWarningTo=Failed > Scripts/logs/Content.IntegrationTests.log

echo "Tests complete. Press enter to continue."
read
