# Vibe-Code-Dungeon-Ninja
As we are required to learn the vibe coding to keep ourselves modern and efficient, I'm starting this project in learning purpose. The idea is to create a simple game, mostly using the AI generated code. I will actively use both the Vibe coding and the Agentic workflow.

# Ninja Runner - 2D Rogue-Lite Dungeon Crawler

A fast-paced 2D top-down roguelite where you're always running.

---

## Overview

You play as a ninja who never stops moving. Navigate through dungeon rooms, avoid enemies and projectiles, defeat bosses, and return to town for upgrades. Each run through the dungeon makes it harder but more rewarding.

**Key Features:**
- Always-running movement - you choose direction, not when to stop
- Fast-paced combat - run through enemies to damage them
- Skill-based gameplay - dodge projectiles and charged attacks
- Room-based progression - clear enemies to unlock doors
- Rogue-lite progression - earn money, buy upgrades between runs
- Town hub - spend money on upgrades and new abilities

---

## Core Gameplay

### Movement
- Character is ALWAYS moving in one direction
- Player chooses direction (up/down/left/right)
- Can't stop - only change direction at intersections or room edges
- Speed increases over time

### Combat
- **Contact Damage**: Run through enemies to hurt them
- **Skills**: Active abilities for extra damage or ranged attacks
- **Projectiles**: Dodge enemy attacks by changing direction
- **Charged Attacks**: Enemies charge up - time your movement to avoid

### Room Mechanics
- Doors are LOCKED while enemies remain alive
- Clear ALL enemies to unlock doors
- Treasure spawns in some rooms

### Death & Progression
- Hit by enemy/projectile = freeze + direction
- Die choose new = run ends, return to town
- Defeat boss = complete run, return to town
- Earn money from enemies and treasure
- Spend money on upgrades in town

---

## Game Loop

1. **Start Run** - Enter dungeon from town
2. **Navigate Rooms** - Clear enemies, unlock doors
3. **Defeat Boss** - Reach and defeat floor boss
4. **Return to Town** - Boss defeated, warp back
5. **Spend Money** - Buy upgrades, new skills
6. **Repeat** - New dungeon, harder but more rewarding

---

## Town (Hub)

Safe area between dungeon runs:

| Shop | Function |
|------|----------|
| Weapon Shop | Buy new skills/abilities |
| Armor Shop | Buy defensive upgrades |
| Trinket Shop | Buy passive bonuses |
| Heal | Restore health between runs |

---

## Dungeon Generation

- **Grid-based** - Built from pre-defined tiles
- **Procedural rooms** - Random enemy placement
- **Progressive difficulty** - Each run harder than last
- **Visual changes** - Dungeon appearance shifts each run

---

## Technical Stack

- Unity 6000+
- 2D Top-Down view
- Service-oriented player architecture
- R3 for events
- New Input System
- URP Rendering

---

## Development Phases

### Phase 1: Core Movement
- [x] Player always-running movement
- [x] Direction change input
- [x] Basic collision

### Phase 2: Combat System
- [x] Contact damage on enemies
- [ ] Enemy movement
- [ ] Enemy attacks
- [ ] Enemy hit feedback
- [ ] Freeze on player hit
- [ ] Direction selection after hit

### Phase 3: Room System
- [ ] Multiple room types
- [ ] Door locking mechanic
- [ ] Enemy spawn system
- [ ] Room clearing detection

### Phase 4: Dungeon Generation
- [ ] Grid-based tile system
- [ ] Procedural room placement
- [ ] Enemy waves
- [ ] Treasure spawning

### Phase 5: Boss Fights
- [ ] Boss room generation
- [ ] Boss enemy AI
- [ ] Boss rewards

### Phase 6: Town Hub
- [ ] Town scene
- [ ] Upgrade shops
- [ ] Money/economy system
- [ ] Run completion screen

### Phase 7: Polish
- [ ] Skills and abilities
- [ ] More enemy types
- [ ] Visual effects
- [ ] Sound

---

## Controls

- **Arrow Keys / WASD** - Change running direction

---

See `AGENTS.md` for detailed architecture guidelines.

