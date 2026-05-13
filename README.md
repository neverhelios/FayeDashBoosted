# FayeDashBoosted
My small first mod on Deathbulge, for me it's a playfield to know what I can do but it's working so enjoy ?

Grants invulerability to the Faye's dash and changes its sound effect

You even have some "achivements" telling you when you did a lot of dashes (or not a lot at first lol)

## Install the mod

First you need [BepInEx V 5.4 64 bit](https://docs.bepinex.dev/articles/user_guide/installation/index.html#tabpanel_bHGHmlrG6S_tabid-win) on your game

1. https://github.com/BepInEx/BepInEx/releases/
2. Download BepInEx 5.4.x win_x64
3. Open Deathbulge's Gamefiles
4. Extract `BepInEx` in the base Folder (so that `doorstop_config.ini` and `winhttp.dll` are next to the `deathbulge.exe`)
5. Start the Game once
6. Open the `BepInEx` Folder 
7. Open the `plugins` Folder
8. (Optional) Create a `FayeDashBoosted` folder
9. Paste the `FayeDashBoosted.ddl` to the current folder

And that's it !

You can now start the game once, and then in the `BepInEx` Folder there is now a folder `config` that contains `FayeDashBoosted.cfg`

This file contains a lot of cool options to tweak your dash, don't hesitate to look ! 

## Building at home

Well I didn't test in another PC so I can't really be sure that it will work on your machine , I'll update it when I have the motivation (or if someone wants it, neverhelios on discord)


Things to add:
- Making Briff do the ability as an option (easy but I'm lasy LEARNING :d)
- The Ian's shockwave, This is managed VERY differently than the two others abilities (at least the sound part) so it requires a lot more work
- Fix the fact that we can move during popups (this is due to me not desactivating the player ability to move during dash, oops)
- Add more sound and custom sounds (far future lol)
- Change the icon of the achievements