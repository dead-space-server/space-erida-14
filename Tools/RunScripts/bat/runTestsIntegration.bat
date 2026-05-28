@echo off
cd ../../../

if not exist Scripts\logs mkdir Scripts\logs
if exist Scripts\logs\Content.IntegrationTests.log del Scripts\logs\Content.IntegrationTests.log

set DOTNET_gcServer=1

for %%s in (core-systems content-construction maps-load maps-systems game-rules round-admin-db entity-core entity-damage entity-systems misc) do (
    echo Running Content.IntegrationTests shard %%s
    echo Running Content.IntegrationTests shard %%s >> Scripts\logs\Content.IntegrationTests.log
    if not exist test_results\integration-%%s mkdir test_results\integration-%%s
    dotnet test Content.IntegrationTests\Content.IntegrationTests.csproj --configuration DebugOpt -p:IntegrationTestShard=%%s -m:2 /nr:false -- NUnit.ConsoleOut=0 NUnit.MapWarningTo=Failed NUnit.TestOutputXml=logs NUnit.WorkDirectory=%CD%\test_results\integration-%%s >> Scripts\logs\Content.IntegrationTests.log 2>&1
    if errorlevel 1 (
        echo Content.IntegrationTests shard %%s failed. See Scripts\logs\Content.IntegrationTests.log
        pause
        exit /b 1
    )
)

pause
