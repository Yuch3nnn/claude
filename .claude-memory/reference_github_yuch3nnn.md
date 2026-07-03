---
name: reference-github-yuch3nnn
description: "User's GitHub account (Yuch3nnn) and local dev setup for querying it directly instead of via browser"
metadata: 
  node_type: memory
  type: reference
  originSessionId: 5a87d23f-6521-4138-8159-eb7dadb1d1bf
---

User's GitHub username is **Yuch3nnn**. Public repo `Yuch3nnn/claude` is a grab-bag repo containing, among other things, the [[project-magic-stone-calculator]] tool. There's also a private repo `Yuch3nnn/linebot_self_discipline`.

On this machine (home PC), as of 2026-07-03:
- Git was already installed.
- GitHub CLI (`gh`) was installed via `winget install --id GitHub.cli` and authenticated via `gh auth login --web` (logged in as Yuch3nnn).
- `Yuch3nnn/claude` was cloned to `C:\Users\north\projects\claude`.

**Why:** User wants to give git/GitHub commands directly (like "check today's commits") the same way they can at their office machine, rather than going through browser automation. On this machine, the claude-in-chrome browser-automation tab group does NOT share a login session with the user's regular visible Chrome window — attempting to check GitHub via the automated browser hits a logged-out session and cannot see private repos or the user's authenticated view. Driving the browser to find/verify the right window wasted significant time.

**How to apply:** For any request about this user's git history, commits, or repo state, prefer `git`/`gh` shell commands against the local clone (or `gh repo clone`/`gh api` for other repos) over browser automation. Only fall back to the browser if a repo isn't cloned locally and gh CLI can't reach it (e.g., needs manual OAuth in a UI gh doesn't handle).
