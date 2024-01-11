# EvilCompany
Do you ever feel that spectating is a little too boring and passive? Do you want to take revenge over that one guy who blocked you while you were jumping causing you to fall to your death? Now you can!

## Features
You earn evil points at the end of every round. Spend evil points while spectating a player to affect them by pressing any of the following keys:
- 0 Key (Kill Player)
- 9 Key (Damage Player for a default of 10 hp)
- 8 Key (Force them to crouch or uncrouch)
- 7 Key (Delete held item)
- 6 Key (Prevent jumping)
- 5 Key (View evil points through console)

## Requirements
* Lethal Company
* LC_API
* InputUtils

### To Do
- Display how many points the player has 

## FAQ
### How do I install this?
Either use the Thunderstore mod manager or place the dll in the BepInEx/plugins folder.

### Does everyone need this mod?
Yes... I'm currently working on making a host only version of this mod but for now all players need this mod for it to work.

### But I want to mess with my friends without them knowning...
Either:
- If you have coding and modding knowledge you can take the code here and make it yourself
- Wait until I make a host only version of this mod

### How do I use this?
You must be dead and spectating a player before you press any key from 6-0. You must also have enough points to affect them; if you are spectating player A and you press 0 while having enough points, then player A drops dead. You can press 5 at any time before looking at the console (the one that opens up when you launch the game modded) to know how many points you have and the costs for each evil action.

### You earn too few points per round
You can change how many points you earn each round, the cost of performing any of the evil actions, etc. through the thunderstore manager config like any other mod. Note that you will instead use the host's config values if you are not the host.

### I keep crashing
This is my first ever BepInEx mod so that's to be expected. All I can say is to not spam any of the keys.

### Current Issues
Evil points don't save after you leave the server, it gets reset to the host's starting evil points value.

### Other notes
Clients sync with the host's config values when someone lands the ship.
If someone is already affected with "No Jump" and another player tries to affect them with another "No Jump" effect, their points still get deducted.