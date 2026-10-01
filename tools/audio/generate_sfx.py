#!/usr/bin/env python3
"""Generate short placeholder WAV clips for KUT (royalty-free procedural)."""
from __future__ import annotations

import math
import struct
import wave
from pathlib import Path

OUT = Path(__file__).resolve().parents[2] / "unity/Kut/Assets/_Project/Resources/Audio/SFX"
RATE = 44100


def write_wav(path: Path, freq: float, duration: float, volume: float = 0.25) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    n = int(RATE * duration)
    with wave.open(str(path), "w") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(RATE)
        for i in range(n):
            t = i / RATE
            env = min(1.0, t * 20) * max(0.0, 1.0 - (t / duration))
            sample = math.sin(2 * math.pi * freq * t) * volume * env
            w.writeframes(struct.pack("<h", int(max(-1, min(1, sample)) * 32767)))


def main() -> None:
    clips = {
        "ui_click.wav": (880, 0.05),
        "swap.wav": (520, 0.06),
        "match_earth.wav": (440, 0.08),
        "match_water.wav": (660, 0.08),
        "match_fire.wav": (330, 0.09),
        "match_air.wav": (790, 0.08),
        "special_chime.wav": (988, 0.15),
        "special_drum_earth.wav": (220, 0.2),
        "special_drum_water.wav": (280, 0.2),
        "special_bomb.wav": (180, 0.25),
        "victory_stinger.wav": (523, 0.35),
    }
    for name, (f, d) in clips.items():
        write_wav(OUT / name, f, d)
        print("wrote", name)
    music = OUT.parent / "Music" / "mus_ch1_ambient.wav"
    write_wav(music, 110, 2.0, 0.08)
    write_wav(OUT.parent / "Music" / "mus_ch2_ambient.wav", 98, 2.0, 0.08)
    print("wrote music loops (short)")


if __name__ == "__main__":
    main()
