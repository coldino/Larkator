# Larkator : ARK Dino Finder
> Reads your ARK. Finds your dinos.

### What is is?
Need help locating your next high-stat tame for better dino breeding?
Larkator uses your ARK save file to help you locate both wild and tamed creatures.

![Larkator Screenshot](Assets/screenshot.png)

### Features
 - Find both wild and tamed creatures
 - Filter based on species, gender, min and max levels
 - Show the results on a map with full coordinates
 - Helps you find your lost tames
 - Creature stats are shown to help you find that elusive next tame
 - Automatically re-reads your save file when it changes
 - Supports all five of the standard ARK maps

### Requirements
Larkator requires **ark-tools** to be installed (or at least, extracted).
On first run Larkator will ask you to locate it.
You can get it from the [forum post](https://survivetheark.com/index.php?/forums/topic/80750-ark-tools-v064-tools-for-reading-and-manipulating-ark-savegame-files/),
or directly from [Qowyn/ark-tools](https://github.com/Qowyn/ark-tools/releases).

(note: Larkator now updates the ark-tools database (*update-data*) each time it is launched)

### Limitations
Larkator is already very useful, but it is very new and is limited in some ways.

 - The search and results lists do not scroll if there are too many entries
 - None of the new Aberation stats are shown

### Installation
Windows makes installing a simple app into a series of 'yes I'm sure' steps. Here's each little detail for anyone who's unsure:

Note: If running Windows 10 you might need to enable installing apps from outside the app store...
Open **Settings** and go to **Apps**. Under **App & Features** the first setting is called **Installing apps**. If this is set to "Allow apps from the Store only" then set it either "Warn me..." or "Allow apps from anywhere", depending on your level of trust. Warn is the safest.

 1. Find the latest release of Larkator and download the ...setup.exe. Click Run/Open once the download is completed.
 1. Windows and/or your browser might chime in and tell you that you are opening an untrusted app. If you want to continue you might have to click "More info" to reveal the button to allow the installer to run.
 1. The installer's "Application Install - Security Warning" window pops up, telling you once again that the app is unsigned. Choose Install if you wish to continue.
 1. At this point Windows 10 chimes in once again and requires you to click "More info" then "Run anyway" if you wish to actually install.
 1. Larkator is now installed and pops up its Welcome window.
 1. If you do not have ark-tools installed:
    1. Click the given link to take you to the ark-tools forum page. Follow the link to ark-tools on GitHub, then to releases and download the latest released ark-tools.zip.
    1. Ark tools has no installer, so make a new folder somewhere to keep it. If unsure, make a folder called ark-tools in Downloads.
    1. Open the downloaded zip file and extract all of its contents to the folder you created for it.
 1. In the Larkator Welcome window, follow step 1) and locate ark-tools.exe.
 1. Follow step 2) and locate your saved ARK e.g. C:\SteamLibrary\steamapps\common\ARK\ShooterGame\Saved\SavedArksLocal\TheIsland.ark
 1. Click "Let's Go" and you're in.
 1. Larkator will auto-update, but will only update after it is restarted (and with an option to skip).

### Use and Tips
 - Larkator will use ark-tools to translate your saved ARK on first load and each time it changes. A large spinning cog appears over the map while this is happening. No changes are made to your saved ARK as the translated output lives in Temp.
 - Select a filter on the left side of the window to show a map of the results. Details of the results are show on the right.
 - Use your mouse's scroll wheel over a search filter's gender and level selector to change them.
 - To add a new species click the big add button. Set a category name, the species and any other filters you would like then click add.
 - Filters can be dragged around in the list to re-order or change categories.
 - While ingame, use the command `saveworld` to force the game to save - Larkator will update immediately
 - Locate that max-level Quetzal and go get it!

### Known Issues
 1) Larger saved arks cause ark-tools to generate an OutOfMemory error. To avoid this the memory allocated to ark-tools.exe must be upped.
    To achieve this, create simple text file beside ark-tools.exe called **ark-tools.l4j.ini**. It should contain only the text **-Xmx4G**.

### License
Larkator is Open Source and released under the MIT license. Contributions are encouraged.

### Thanks

Thanks to [Roland Firmont (Qowyn)](https://github.com/Qowyn) for ark-tools, without which this tool would not be possible.

The project includes [ListView Layout Manager](https://www.codeproject.com/Articles/25058/ListView-Layout-Manager) from Jani Giannoudis.
