# BigMod

Changes a bunch of stuff in RimWorld.

Mainly the GUI, with a Entity based system built ontop of Unity's IMGUI system, allowing easy reusing and moving of elements.


#### Previews
![General Overview](https://github.com/Onidotmoe/Big-Mod/assets/32226493/ea77db90-9c84-4f4c-bb14-86e9012bf940)
![Inspect Panel and Splitting Pawns Window](https://github.com/Onidotmoe/Big-Mod/assets/32226493/e3d96691-5d2d-429a-8bbe-2cd360fbae3e)
![Moveable Windows and Gizmos](https://github.com/Onidotmoe/Big-Mod/assets/32226493/c895f441-1873-4092-8948-0dbaa421b245)
![Overview window](https://github.com/Onidotmoe/Big-Mod/assets/32226493/45f652ae-fd22-4508-8298-695bd94a94d4)
![Trading Screen](https://github.com/Onidotmoe/Big-Mod/assets/32226493/57c781ef-1377-459a-902f-3170313e70a9)

#### Generic Pawn Element
You'll see this element all over the place, you can shift-click it to add the Pawn to your current selection, or click it to just select the pawn, or double click it to goto the pawn on the map, this will make the camera follow that pawn, the camera following code is a bit funky.

#### Windows
I tried to keep parity with the vanilla UI in terms of funtionality, basically if you could do something in the vanilla UI, you should be able to do it here too and more!

You can still access the old menus with the hotkeys for them.
All windows are resizable and movable.
Windows that aren't covered will just use the vanilla ones instead.

Generally windows should have searchbars, tho some might still not have.

    * Animals, Wildlife, Mechs
        Alot of the same information displayed clearer and cleaner.
        I tried to display as much information as possible.
        Notably you can remove a master from mechs which i don't know if you can do in vanilla.

    * Architect
        A much smaller footprint design that allows easier selection of the different materials and display how much you have and have not, including how much you need.
        The different categories have been manually made and can be changed in the xml file, tho that's inaccessible as it's compiled for now

    * Inspect
        New design allows multiple selection to be displayed with as much information cramed in as possible.
        Multiple identical objects will be combined into a single inspect entry.
        The old inspect functions are still available for pawns in the top of the entry.

    * Orders
        Can now be resized and moved, it should be able to take gizmos from mods but that isn't a guarantee.
        Tried to support the Simple sidearms mod! as best as i could, would have prefered to make my own version.
    
    * Overview
        This panel is largely unfinished and alot of stuff is sorta-working.
        It's a Pawn Overview panel that shows their inventory, your entire stockpile and allows dragging and shift right-clicking to move stuff from panel to panel.
        Note that the ListViews that are cut, can be scrolled to see the rest of the stats.

    * Pawns
        General overview of all pawns on the map, including neutral and hostiles.
        You can change the renderstyle for each type to a box instead of a list item.
        With Alt + Left-click dragging you can move pawns out into their own panels, dragging a pawn onto another will add them to that window and next to that pawn.
        Shift + Left-click dragging and you can select multiple pawns at once, or just click once to add to current selection.
        Right-clicking opens a context menu, with a bunch of options, including a "Drop All" items option.

    * Resource
        Much more userfriendly than the vanilla panel.
        I tried to make and add icons the best i could for everything that could use a icon.
        The different categories have been manually made and can be changed in the xml file, tho that's inaccessible as it's compiled for now.
        
    * Schedule
        Easier brushing, you can modify a whole row with shift-clicking on it, same goes for columns.

    * Trade
        Has been changed to be like the trading menu in Fallout New Vegas, currently is kinda broken.

    * Work
        For work priorities, you can use the same click modifiers as with Schedule.

    * Tray
        Located in the bottom-right, one of the bottom opens the vanilla menu panel, while the other expands the control and allows you to reopen/close the various windows.
        Also allows to toggle the different vanilla overlays.

#### Miscellaneous
I wanted to make eveything toggable, they're all on right now.

    * Right-Click drags the view, unfortunately affected by gamespeed for some reason and can have right-click complications.
    * Removes Flammability from Gold, Jade, Silver, Steel, Plasteel, Uranium
    * Pressing "." over a window will lock/unlock its position.
    * Double clicking on something will select everything that's identical to it on the screen.
    * All the windows with Datagrids can be sorted by clicking a column, you can change column values in some with holding shift and mouse scrollwheel or mouse left click or right click.
    * Alt-clicking on a Pawn component will open the vanilla info panel.
    * The 4th speed is on by default and part of the media controls panel in the bottom right.
    * Control + Alt + Left clicking on a window will reset it to its default position and size.
    * Control + Alt + Right clicking closes it.

Currently panel positions and opened panels are not saved between sessions.

This is a large project with alot of tiny extras that i can't fully remember, i might have missed things.

If i ever return to this project, whatever DLCs that would be out by then would probably be required, if i use them myself.
Dunno how this will work on different resolution monitors than mine.

I won't be working on this project again anytime soon.

License is GPLv3
[Steam Mod Page](https://steamcommunity.com/sharedfiles/filedetails/?id=2979582708)

