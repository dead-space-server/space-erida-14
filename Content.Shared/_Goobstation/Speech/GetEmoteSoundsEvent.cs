namespace Content.Shared._Goobstation.Speech;

[ByRefEvent]
public record struct GetEmoteSoundsEvent(string? EmoteSoundProtoId = null, bool Handled = false);
