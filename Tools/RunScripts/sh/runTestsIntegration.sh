#!/usr/bin/env bash
set -e

cd ../../../

mkdir -p Scripts/logs
rm -f Scripts/logs/Content.IntegrationTests.log

for shard in core-systems content-construction maps-load maps-systems game-rules round-admin-db entity-core entity-damage entity-systems misc; do
    echo "Running Content.IntegrationTests shard $shard"
    echo "Running Content.IntegrationTests shard $shard" >> Scripts/logs/Content.IntegrationTests.log
    mkdir -p "test_results/integration-$shard"
    if ! DOTNET_gcServer=1 dotnet test Content.IntegrationTests/Content.IntegrationTests.csproj \
        --configuration DebugOpt \
        -p:IntegrationTestShard="$shard" \
        -m:2 /nr:false \
        -- \
        NUnit.ConsoleOut=0 \
        NUnit.MapWarningTo=Failed \
        NUnit.TestOutputXml=logs \
        NUnit.WorkDirectory="$(pwd)/test_results/integration-$shard" >> Scripts/logs/Content.IntegrationTests.log 2>&1; then
        echo "Content.IntegrationTests shard $shard failed. See Scripts/logs/Content.IntegrationTests.log"
        exit 1
    fi
done

echo "Tests complete. Press enter to continue."
read
