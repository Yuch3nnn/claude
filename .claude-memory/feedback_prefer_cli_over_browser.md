---
name: feedback-prefer-cli-over-browser
description: User prefers being able to issue direct git/gh CLI-style commands rather than relying on browser automation
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 5a87d23f-6521-4138-8159-eb7dadb1d1bf
---

Set up local tooling (git clone, `gh` CLI auth) so the user can direct work the same way they do on their office machine, instead of relying on browser automation to check things like GitHub commits.

**Why:** On 2026-07-03, checking today's commits via the claude-in-chrome browser automation turned into a long back-and-forth — the automated browser tab group didn't share a login session with the user's visible/real Chrome window, so it kept showing "Sign in" and missed private repos. The user explicitly said they want to "command Claude the same way as at the office," implying office setup already has direct CLI/git access rather than needing browser navigation. See [[reference-github-yuch3nnn]] for what's now configured on this machine.

**How to apply:** When a task can be done via an installed CLI (git, gh, npm, etc.) against local or cloned state, prefer that over spinning up browser automation to click through a web UI — it's faster and avoids session/auth mismatches. Only reach for the browser when there's no CLI/API equivalent.
