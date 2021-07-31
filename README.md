# ConfigurableStart

This is a simple KSP mod that builds upon stock's career difficulty options, offering many new ways to customize your starting point.

It should work with both stock and custom planet packs / tech trees, but has only been tested with stock and RSS/RP-1 (for which the mod was designed).

## What does it do?
It's designed to give both modders and regular players a way to start a career with as much freedom as possible. It allows setting a starting date, unlocking nodes,
upgrading facilities, adding KCT launchpads, and setting an arbitrary Funds, Science and Reputation amount. It all happens when the game is created, so you won't have to
do anything after you see the KSC for the first time; the mod will stop too.

It offers an in-game GUI that allows you to choose an available preset and edit any of the custom fields, but also allows modders to include their own config file which will
be automatically detected by the mod and added to the avaialbe presets. For more information on what each field does, and how to format your config, refer to the scenarios.cfg
file in GameData/KSPScenarioManager or to the [wiki](https://github.com/Standecco/ConfigurableStart/wiki).

## Ideas and options
If you're a planet maker and want to create a career experience that suits your system more, or you want to create an elaborate contract progression, or perhaps you want
to do all these things and more, then this mod allows you to basically start the game at any arbitrary tech level without needing numerous ModuleManager patches.

ConfigurableStart will probably be more useful to modders, but you're just a player and aren't interested in diving deeply into config files for tens of other mods, there might be various
ways that it might help you. Here are a few:
- If you want to recreate a particular mission in RO, or have a particular planet alignment that you want to try and experiment with, then you can simply set the date and the game
will automatically start there.
- If you want to play RP-1, but skip the sounding rocket phase, or jump to any particular year and tech level (to roleplay a more recent space agency, for example), then you can set
the starting date, technology level, starting facilities and initial funding amount 

### Currently supported mods:
- KCT (create KCT pads)
- TestFlight (set DU for engines)
- RealFuels (unlock engine configs)
- RP-1
- Kopernicus/custom planet packs
- Principia (should be fine, not tested yet)

### Planned support (open to suggestions):
- automatic contract completion through Contract configurator
