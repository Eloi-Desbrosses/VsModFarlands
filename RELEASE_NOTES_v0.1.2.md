# v0.1.2 — Per-world coverage slider

The Far Lands footprint is now controlled by a single slider in the world
creation UI, expressed as a percentage of the map surface (0 to 100, default
30, step 5). Solo players no longer need to touch environment variables.

## What changed

- **New world-config attribute** `farLandsCoverage` in the **Customize World**
  screen under the **Worldgen** category. Slider 0–100%, default 30%, step 5%.
- **Coverage scales with world size**: 30% on a 1M-block world and 30% on a
  65M-block world both produce a border crust that occupies the same fraction
  of playable surface. Math: ring depth per border = `worldSize × (1 − √(1 − C/100)) / 2`.
- **`onlyDuringWorldCreate: true`**: the value is locked once the world is
  created, so chunks don't disagree about ring depth between regions
  generated at different times.
- **Env vars still work** and override the slider when set. Useful for Docker
  servers or admins who want a single coverage value across multiple worlds.

## What the percentages mean

| Coverage | Ring depth on 1024k world | Vanilla area | Far Lands area |
|----------|--------------------------|--------------|----------------|
| 0%       | 0                        | 100%         | 0% (mod off)   |
| 10%      | 26 000                   | 90%          | 10%            |
| 30% (default) | 84 000              | 70%          | 30%            |
| 50%      | 150 000                  | 50%          | 50%            |
| 100%     | 512 000                  | 0%           | 100%           |

## Upgrade notes

- **Existing worlds**: keep their previously generated chunks. The slider does
  not retroactively apply. New chunks in those worlds will use the default
  (30%) unless an env var or a fresh worldconfig entry is set.
- **From v0.1.1**: previous v0.1.1 worlds were generated against a fixed
  `world/12` auto-scale (also 30%-equivalent on the default map size), so the
  visible outcome on standard worlds is identical. Custom worlds with unusual
  sizes may shift slightly because the new formula is exact rather than approximate.

## Asset

`vsmodfarlands_0.1.2.zip` — 19 364 bytes
SHA-256: `485de3fe64f26e6f14760be8705dec65836e309d80c761a42e676f79224265c5`

## Compatibility

- Vintage Story 1.22.2.
- Server-side only, no client install required.
- Safe to add or remove on an existing world.

## License

MIT. See [LICENSE](https://github.com/Eloi-Desbrosses/VsModFarlands/blob/main/LICENSE).
