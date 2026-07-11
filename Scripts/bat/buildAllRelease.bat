:: SPDX-FileCopyrightText: 2026 Sidzaru <110207359+Sidzaru@users.noreply.github.com>
:: SPDX-FileCopyrightText: 2026 OpenWendor
:: SPDX-License-Identifier: MIT

@echo off
cd ../../

call git submodule update --init --recursive
call dotnet build -c Release

pause
