#!/usr/bin/env sh

# SPDX-FileCopyrightText: 2026 Sidzaru <110207359+Sidzaru@users.noreply.github.com>
# SPDX-FileCopyrightText: 2026 OpenWendor
# SPDX-License-Identifier: MIT

# make sure to start from script dir
if [ "$(dirname $0)" != "." ]; then
    cd "$(dirname $0)"
fi

cd ../../
dotnet run --project Content.Client --no-build
