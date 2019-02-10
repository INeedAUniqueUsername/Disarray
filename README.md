# ` [ D I S A R R A Y ] `
A minimal CLI-based 1v1 strategy game where the best defense makes a good offense.
# Rules
- Each player starts with 16 Core HP and 8 Defenses, each starting at level 4.
- Players take turns spending Action points on Building and Attacking
- Each player gets 3 Action points per turn.
- Build: Upgrade a Defense by 1 HP. If the Defense has 0 HP, then it gains an additional 1 HP.
- Attack: Choose a Defense with at least 3 HP to launch, and then a target player and a target Defense.
  - The target player's targeted Defense is dealt damage equal to the HP of the launched Defense.
  - If the targeted Defense reaches 0 HP, any remaining damage is dealt to the target player's Core HP.
    - Idea: Remaining damage must be at least 2 HP
  - The launched Defense is reset to 0 HP.
- Players with 0 HP are destroyed
- Last player alive wins
