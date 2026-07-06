---
name: project-magic-stone-set-bonuses
description: "套裝加成(set bonus) shape combos for 勇者來開團 magic stone calculator — 4 special 5-shape combinations, effects not yet defined"
metadata:
  type: project
---

Game screenshots (talent/skill enhancement panels: 背後伏擊·強化, 暗影增幅·強化, 暗影爆發·強化, and possibly one tied to "獲得罌粟慾望") showed rows of 5 shape icons that look like 套裝加成 (set bonus) recipes. User confirmed the shape composition of 4 such combos, labeled A–D:

- **A**: 圓形, 三角形, 菱形, 星星型, 月亮型 (5 distinct shapes)
- **B**: 圓形, 三角形, 六邊形, 星星型, 月亮型 (5 distinct shapes)
- **C**: 三角形, 三角形, 六邊形, 星星型, 月亮型 (two 三角形)
- **D**: 圓形, 圓形, 菱形, 星星型, 月亮型 (two 圓形)

Which letter maps to which named skill enhancement, and the actual numeric effect of each, is **not yet confirmed** — only the shape composition is recorded so far.

**Why:** [[project-magic-stone-calculator]] deliberately skipped 套裝加成 in the calc engine per the user's earlier instruction ("套裝加成先不管"). This is the first concrete data point toward eventually implementing it.

**How to apply:** Don't implement combo-matching/set-bonus logic yet — only shapes are confirmed, not which effect goes with which combo or its magnitude. When the user provides that mapping, wire it into the calculator (`magic-stone-calculator/index.html`) as an additional bonus layer, consistent with the existing self-mod/global-mechanic pattern already in `computeCombo`.
