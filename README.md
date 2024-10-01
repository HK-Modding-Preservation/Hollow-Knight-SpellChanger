# SpellChanger

A library mod for Hollow Knight that aims to provide an easy way for many mods to edit spells and nail arts without compatibility issues.
To cycle through added spells, select the spell slot within the inventory. It will automatically cycle. There is no mod menu.


I created a mod called ExtraSpells with examples showing how to use this library: https://github.com/Samihamer1/Hollow-Knight-ExtraSpells

The main rules to remember are:
-Do not mess with the vanilla states of the Knight at all. To maximise compatibility, clone the states if you must edit the original spell. You can see an example of this in the ExplosionDive spell in ExtraSpells.
-All states made using this library have a unique id appended to the end of their name that you do not have access to. To get around this, create all states for custom spells using the library functions instead of directly editing FSMs. Also, if you need to use a state name made within a CustomSpell object, remember to account for the id appended to the end.


