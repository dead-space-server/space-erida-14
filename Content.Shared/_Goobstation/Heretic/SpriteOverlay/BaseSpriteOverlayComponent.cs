// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.Heretic.SpriteOverlay;

public abstract partial class BaseSpriteOverlayComponent : Component
{
    public abstract Enum Key { get; set; }

    public abstract SpriteSpecifier? Sprite { get; set; }

    public virtual bool Unshaded { get; set; } = true;

    public virtual Vector2 Offset { get; set; } = Vector2.Zero;
}
