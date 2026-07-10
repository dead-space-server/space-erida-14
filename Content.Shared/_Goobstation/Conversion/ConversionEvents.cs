// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Goobstation.Conversion;

/// <summary>
/// Used to see if the entity can be converted.
/// </summary>
/// <param name="Uid">The entity being converted.</param>
/// <param name="Blocked">Can the entity be converted?.</param>
[ByRefEvent]
public record struct BeforeConversionEvent(EntityUid Uid, bool Blocked = false);
