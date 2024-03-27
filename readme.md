# REviewer (RE viewer)

REviewer is a SRT (Speedrun Tool) designed specifically for the OG (Original) versions of Resident Evil games, with a focus on compatibility with Biorand.

**Note: REviewer is currently in alpha stage and UNSTABLE. Use with caution.**


## Overview

### Resident Evil 1
![Alt Text](img/reviewer-re1.png)

### Resident Evil 2
![Alt Text](img/reviewer-re2.png)

### Resident Evil 3
![Alt Text](img/reviewer-re3.png)

REviewer provides a comprehensive set of features to enhance the speedrunning experience for Resident Evil games. It allows users to monitor various aspects of the game, including:

- Health status
- Character information
- Key items collected
- Inventory management
- Segment timers
- Selected item
- Last item seen

## Work In Progress

- [x] RE1 (Mediakit version) - 95% / Testing phase
- [X] RE2 (SourceNext) - 95% / Testing phase
- [X] RE3 (Rebirth) - 80% / Testing phase - NO IGT YET
- [ ] CVX (PS2)

Possible other port

- RE2 (Platinium -> China) 
- RE3 (CHN/TWN) 

## Known Bugs

- Enemy Tracker is not super stable in some cases.
- Save/Load State are still not totally working
- If you are confronted to a game that is not recognized but everything is green, close and re-run it.

## Features

### Health Monitoring

REviewer provides real-time monitoring of the player's health status. It displays the current health level and any changes that occur during gameplay.

#### What is the health status color system?

- Green: Fine
- Yellow: Light Caution
- Orange: Caution
- Red: Danger

### Character Information

The tool allows users to track important information about the character they are controlling. This includes attributes such as health, stamina, and other relevant stats.

### Key Item Tracking

REviewer keeps track of the key items collected by the player. It provides a visual representation of the items obtained and their current status.

### Inventory Management

The tool offers an inventory management system that helps players keep track of their items and optimize their inventory space.

### Segment Timers

REviewer includes segment timers to help speedrunners track their progress and optimize their route.

### Selected Item

The tool displays the currently selected item, making it easier for players to keep track of the item they are currently using.

### Last Item Seen

REviewer remembers the last item seen by the player, allowing them to quickly reference it when needed.

## Usage

To start using REviewer, follow these steps:

1. Download and install the tool. 
2. Open REviewer
3. Edit -> Settings -> Save the save repository path of your game (usually /savedata)
4. Select the appropriate game version.
5. Launch your game
6. Enjoy your speedrunning experience!

## FAQ

### My game is not recognized, what i'm doing wrong

SRT is able to recognize some process name:

- Bio.exe
- bio.exe
- Biohazard.exe
- biohazard.exe
- Bio2 1.10.exe
- bio2 1.10.exe
- bio2 v1.1.exe
- bio2 1.1.exe

### How works Timers?

they are 5 timers in this tool, the first 4 are the segment timers, and the last one is the total time. The segment timers are used to measure the time of each segment of the game, and the total time is used to measure the time of the entire game. The segment timers are reset when the game is reset, and the total time is reset when the game is closed.

It's possible that you have only 1 segment timer, in this case, the segment timer will be the total time.

### What means the color background behind the key items?

- Green: The item is in the inventory/Item Box, or it has been used
- Orange: The item has been seen but is not in the inventory

## Contributing

I welcome contributions from the community to improve REviewer. If you have any suggestions, bug reports, or feature requests, please submit them through the official GitHub repository or Discord.

## License

REviewer is released under the [MIT License](https://opensource.org/licenses/MIT). Please refer to the LICENSE file for more details.

## Acknowledgements

I would like to thank the following individuals and projects for their contributions to REviewer:

- Biorand - for their support and collaboration.
- Resident Evil speedrunning community - for their valuable feedback and testing.
