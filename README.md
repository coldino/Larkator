# Larkator : ARK Dino Finder

> Reads your saved ARK. Finds your dinos.

### What is is?

Need help finding your next high-stat tame for better dino breeding?
Larkator uses your ARK save file to help you find both wild and tamed creatures.

![Larkator Screenshot](Assets/screenshot.png)

### Requirements

Larkator requires **ark-tools** to be installed (or at least, extracted).
On first run Larkator will ask you to locate it.
You can get it from the [Steam forum post](https://survivetheark.com/index.php?/forums/topic/80750-ark-tools-v064-tools-for-reading-and-manipulating-ark-savegame-files/),
or directly from [Qowyn/ark-tools](https://github.com/Qowyn/ark-tools/releases).

Before using Larkator, ark-tools must have downloaded its database (update-data). For anyone struggling with the command-prompt side of things open a Command Prompt and type the following, replacing `<path-to-ark-tools-directory>` with the correct path for your system:
```
cd <path-to-ark-tools-directory>
ark-tools.exe update-data
```

### Features

 - Find both wild and tamed creatures
 - Filter based on species, gender, min and max levels
 - Show the results on a map with full corrdinates
 - Helps you find your lost tames
 - Creature stats are shown to help you find that elusive next tame
 - Automatically re-reads your save file when it changes

Currently only has a map for the island, although it will load any ARK.

### Tips

 - Use your mouse's scroll wheel over a search filter's gender and level seletor to change them
 - While ingame, use the command `saveworld` to force the game to save - Larkator will update immediately

### Limitations

Larkator is already very useful, but it is very new and is limited in some ways.

 - Only has a map for the island, so far
 - If you have too many searches the list will not scroll
 - The results list is not scrollable either
 - No new Aberation stats are shown

### License

Larkator is released under the MIT license and contributions are encouraged.

### Thanks

Thanks to [Roland Firmont (Qowyn)](https://github.com/Qowyn) for ark-tools, without which this tool would not be possible.

The project includes [ListView Layout Manager](https://www.codeproject.com/Articles/25058/ListView-Layout-Manager) from Jani Giannoudis.
