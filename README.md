# Gacha System Unity Package

A drop-in gacha (loot box) system for Unity. No coding required — configure everything from the Inspector.

---

## Features

- **Pull however many you want** — single pulls, 10-pulls, anything.
- **Custom rarities** — create your own rarity tiers (R, SR, SSR, whatever) as Scriptable Objects, no code changes needed.
- **Pity system** — guarantee a rarity after N pulls, configurable per rarity.
- **Dynamic rates** — optionally increase a rarity's chance the longer someone goes without it, using a curve.
- **Live rate preview** — the GachaManager Inspector shows a table of every rarity's real drop chance, so you always know your actual odds.
- **Tier helper** — see every tier number already in use across your project, so you don't accidentally reuse one by mistake.

---

## How Drop Rates Work

Drop rates are checked **one rarity at a time, rarest first** — think of it like a chain of coin flips, not slices of a pie. Your rates do **not** need to add up to 100%.

Example: R = 90%, SR = 9%, SSR = 1%

1. Check SSR first — 1% chance to land here.
2. Didn't land SSR? Check SR — 9% chance.
3. Didn't land either? You get R.

Your **most common rarity** (lowest Tier number) doesn't get checked at all — it's just whatever's left over. You can leave its Drop Rate at anything; it won't matter.

Don't want to do the math? Open your GachaManager — the Inspector shows each rarity's *real* chance (in the example above: SSR 1%, SR 8.91%, R 90.09%).

---

## Rarity Tiers

`Tier` decides the order rarities are checked in — **lower Tier = more common**. Only the order matters, not the exact numbers (1/2/3 works exactly like 10/20/30).

One thing to watch out for: if two rarities share the same Tier (including if you just leave everything at the default, `0`), they'll reset each other's pity every pull, and pity may never trigger. To help avoid this, each Rarity asset shows you every Tier already used in your project, so you can pick a free one instead of guessing.

Prefer not to type numbers at all? Drag your rarities into order (most common on top) in the GachaManager's list, then hit **Renumber Tiers from List Order**.

---

## Getting Started

1. **Import the package** — grab the latest `.unitypackage` from [Releases](https://github.com/finn7199/Gacha-System-Package/releases) and import it into your project.
2. **Add the GachaManager** — create an empty GameObject, name it `GachaManager`, and attach the `GachaSystem` script.
3. **Create your rarities** — right-click in the Project window → `Create > Gacha System > Gacha Rarity`. Set a name, drop rate, and tier for each.
4. **Create your items** — right-click → `Create > Gacha System > Gacha Item`. Give each one a name and assign it a rarity.
5. **Wire it all up** — on the `GachaManager`, add your rarities and items to their lists in the Inspector.

That's it — no code required to configure your gacha pool.

---

## Performing Pulls (Code)

```csharp
List<GachaItem> results = GachaSystem.Instance.PerformPull(n);
```

Replace `n` with how many pulls to do (`1` for a single pull, `10` for a multi-pull). You get back the list of items pulled.

### Hooking it up to a button

1. Add a UI Button to your scene.
2. Write a small script with a method that calls `GachaSystem.Instance.PerformPull(n)`.
3. Link that method to the button's `OnClick()` event.

---

## License

MIT — see [LICENSE](LICENSE.txt).

## Found a Bug? Have an Idea?

Open an [Issue](https://github.com/finn7199/Gacha-System-Package/issues) or send a [Pull Request](https://github.com/finn7199/Gacha-System-Package/pulls).
