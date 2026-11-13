# REDLINER Source Register and Evidence Notes

The sources below were used to assemble the movement reference. Community guides are not official source code or developer documentation. Treat advanced techniques and numerical values as community-reported and version-sensitive.

## S1. Official Roblox experience listing

**URL:** https://www.roblox.com/games/94987506187454/REDLINER  
**Type:** Official Roblox game page  
**Can establish:** Exact experience identity, public description, advertised no-speed-limit statement, default listed controls, keybinds are changeable, credits.  
**Cannot establish:** Acceleration, friction, jump height, dash equations, grapple force, actual cap behavior, or hidden implementation.  
**Evidence status:** [D] for public listing facts.

## S2. REDLINER Wave 1 community mechanics guide

**URL:** https://redliner.likegravity.dev/wiki  
**Type:** Player/community-maintained mechanics and advanced-tech guide (“Wave 1 Liners”).  
**Topics:** Dash, slide, momentum, grapple Ground Slam, Chase, Slam Slide/Sage Slide, slingshot, BDG, Willow Walling/Hyper Blitz, Super Jump, Slam Storage, JarBoost, Slide Redirect, Wallrun Redirect, Zipline Redirect, Wall Sling, Zip Boost, bounce pads, historical changelog.  
**Can establish:** What the contributing community documented/reported, technique names and historical input sequences, reported speeds and resources.  
**Cannot establish:** Current November 2026 behavior, developer-confirmed internals, independently calibrated units, or repeatable exact physics.  
**Evidence status:** [C]. The guide itself warns that techniques can change in future updates; its changelog includes December 2025 entries.

## S3. REDLINER movement guide — redliner.net

**URL:** https://www.redliner.net/movement/  
**Type:** Fan-made movement guide; listed as updated October 25, 2026.  
**Topics:** Core controls, slide, dash, wallrun, grapple, Ground Slam, Slam Cancel, bhop, bhop strafe, slide hop, grapple redirects, BDG, gun recoil movement.  
**Can establish:** A recent community description of how players are taught to use the movement system.  
**Cannot establish:** Exact internal physics or experimentally measured parameter values.  
**Evidence status:** [C].

## S4. REDLINER controls — redliner.net

**URL:** https://www.redliner.net/controls/  
**Type:** Fan-made controls guide; listed as updated October 25, 2026.  
**Topics:** Default keybinds, rebinding, reachable movement/aim inputs.  
**Can establish:** Contemporary fan-guide description of default controls and control ergonomics.  
**Cannot establish:** Hidden input priority or engine implementation.  
**Evidence status:** [C], with official controls still sourced from S1.

## S5. REDLINER movement guide — redliner.org

**URL:** https://redliner.org/movement/  
**Type:** Fan-made movement route guide; updated June 14, 2026.  
**Topics:** Dash, slide, grapple, wallrun, routes, camera-first movement, visible-keystroke video companion.  
**Can establish:** Practical community route recommendations and UX principles.  
**Cannot establish:** Exact physics parameters.  
**Evidence status:** [C].

## S6. REDLINER movement basics — redliner.org

**URL:** https://redliner.org/how-to-play/movement-basics/  
**Type:** Fan-made tutorial; updated June 14, 2026.  
**Topics:** Dash, slide, grapple, wallrun basics, camera-first route planning, movement practice.  
**Can establish:** Player-facing technique recommendations.  
**Cannot establish:** Source-code implementation or numerical physics.  
**Evidence status:** [C].

## S7. REDLINER slide guide — redliner.org

**URL:** https://redliner.org/movement/slide/  
**Type:** Fan-made slide guide; June 2026.  
**Topics:** Slide exits, momentum/route flow, dash-slide combinations, camera positioning.  
**Can establish:** Community usage advice.  
**Cannot establish:** Exact friction curve or state duration.  
**Evidence status:** [C].

## S8. REDLINER resources index

**URL:** https://www.redliner.net/resources/  
**Type:** Fan-made resources index.  
**Can establish:** Links and the publisher's guide taxonomy.  
**Cannot establish:** Physics.  
**Evidence status:** [C].

## S9. REDLINER mechanics wiki

**URL:** https://redliners.wiki/wiki/mechanics/  
**Type:** Fan wiki.  
**Can establish:** Community descriptions of grappling, dash, slide, wallrun, jump, and combat mechanics.  
**Cannot establish:** Whether precise numeric claims or implementation claims were tested or current.  
**Evidence status:** [C]; use cautiously, especially for exact claims.

## S10. REDLINER official Roblox metrics mirror (secondary)

**URL:** https://www.rolimons.com/game/94987506187454  
**Type:** Third-party Roblox game data mirror.  
**Can establish:** Additional listing metadata and update recency snapshot.  
**Cannot establish:** Movement patch details.  
**Evidence status:** Secondary reference only.

## S11. Qitadel movement demonstration

**URL:** https://www.youtube.com/watch?v=VaHC5DVGzdc  
**Type:** Gameplay/movement demonstration identified by community reference material.  
**Can establish:** Potential visual reference when inspected directly.  
**Cannot establish from the metadata alone:** Precise input sequences, frame timings, or velocity values.  
**Evidence status:** Visual lead; requires direct playback/frame analysis.

## S12. MineBlox movement guide

**Reference:** YouTube video titled “REDLINER Movement Guide” by MineBlox; June 18, 2026.  
**Type:** Gameplay tutorial.  
**Can establish:** Demonstrated basic movement techniques if playback is inspected.  
**Cannot establish from description alone:** Exact physics or hidden parameters.  
**Evidence status:** Community demonstration lead.

## S13. GaIaxy momentum tutorial

**Reference:** YouTube video titled “How to NEVER lose your MOMENTUM AGAIN in REDLINER” by GaIaxy; June 19, 2026.  
**Type:** Gameplay tutorial.  
**Can establish:** Player-recommended bunny-hop and slide-redirect technique if playback is inspected.  
**Cannot establish from description alone:** Exact engine physics or calibrated speed.  
**Evidence status:** Community demonstration lead.

## S14. Cato advanced-tech video

**URL:** https://www.youtube.com/watch?v=zh7pocajrTw  
**Title:** “USE these TECHS after BGD NERF” (July 18, 2026).  
**Type:** Advanced movement tutorial.  
**Can establish:** Historical community response to reported BDG balance change if directly inspected.  
**Cannot establish:** Current November behavior.  
**Evidence status:** Community demonstration lead.

## S15. August 2026 player discussion

**Reference URL:** https://www.reddit.com/ (discussion surfaced in research results; exact thread link should be retrieved before relying on it in a formal citation).  
**Type:** Player anecdote.  
**Topic:** Reported difficulty retaining BDG speed in combat after an update.  
**Can establish:** At least one player perception of a version change.  
**Cannot establish:** Actual patch contents or measured speed reduction.  
**Evidence status:** Anecdotal [C], low confidence. Do not cite as patch fact.

## Source use rules for the coding agent

1. The official game page is the only source in this register for officially listed REDLINER controls and identity.
2. The Wave 1 guide is the primary source for named advanced techniques and historical reported values, but those are community evidence and version-sensitive.
3. The 2026 movement guides are useful for current player-facing vocabulary, camera-first route habits, and learning sequence.
4. No source here proves REDLINER's original code architecture or exact physics equations.
5. Do not copy their numerical `u/s` values into REBOUND.
6. Do not import gun, sword, parry, instability, or combat designs into REBOUND.
7. If the agent chooses to research more, it should prefer recent developer/official material and direct footage with visible input overlays, and record exact timestamps only after inspecting the video.
