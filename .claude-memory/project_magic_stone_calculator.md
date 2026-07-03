---
name: project-magic-stone-calculator
description: "Magic stone (魔石) loadout calculator for the game 勇者來開團, being developed in Yuch3nnn/claude repo"
metadata: 
  node_type: memory
  type: project
  originSessionId: 5a87d23f-6521-4138-8159-eb7dadb1d1bf
---

User is building a magic stone (魔石) loadout calculator for the mobile/browser game "勇者來開團" (a game they play, shortcut also found on their desktop). It lives in the `magic-stone-calculator/` folder of the [[reference-github-yuch3nnn]] `claude` repo (public, branch `master`, not `main`).

Started and iterated entirely on 2026-07-03: initial brute-force 5-stone combo optimizer, then presets/auto-fill, async progress bar, assumptions panel, an XSS fix (stone name / imported JSON rendering), a fix for 潛能共鳴/同源潛能 stacking (additive, not max-of-highest), version tracking/stale-cache detection, mobile layout fix, and doc clarification for 角色基本素質.

**Why:** Personal tool to verify/optimize in-game stone combos against real game numbers (there's a manual 5-stone combo picker specifically for cross-checking against real game data).

**How to apply:** When user mentions 魔石, magic stone, or 勇者來開團, this is the project. Local clone is at `C:\Users\north\projects\claude` — see [[reference-github-yuch3nnn]] for how to query it.
