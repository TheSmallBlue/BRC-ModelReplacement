# BRC-ModelReplacement
 A mod to make player-model replacement in Bomb Rush Cyberfunk easier

 # This mod will not get any new updates. For more features, please use [SGiygas' mod](https://github.com/SGiygas/BrcCustomCharacters) instead
 
Here's a video showing how you'd use this:

[![Very rough video](https://img.youtube.com/vi/BoFOzLmoFgA/0.jpg)](https://www.youtube.com/watch?v=BoFOzLmoFgA)

# FAQ

* ### Are multiple custom characters a thing yet?
* As of the latest version, they are! You can follow the same steps shown in the video above, then follow the steps in [This guide](https://github.com/TheSmallBlue/BRC-ModelReplacement/wiki/How-to-convert-an-old-version-of-the-mod-to-the-new-one-to-have-multiple-character-support)

* ### "In-game, the model's outline is huge"
* See this:

![image](https://github.com/TheSmallBlue/BRC-ModelReplacement/assets/24967815/ff537217-5f0e-425b-9bcc-abfa7bf25fe6)

* ### "My spraycan/phone are way too big for the model!"
* Go into your FBX in your assets folder, and go into the model tab, on the top you'll see a "Scale factor" option. Change it to a larger number.
After doing this, delete the old character from the Chara prefab, and re-add the fbx (unpacking and organizing just as before). When adding the spraycan or phone again, they should look better!
If this doesn't work, you'll need to scale your models in your 3d modelling program of choice. If you're doing this in blender, make sure to apply your transforms before exporting (by doing ctrl + a)

* ### "My attacks dont work on police officers / enemies!!"
* My bad, I assumed this wasnt used for anything so I ommited it in the tutorial: __Make sure to rename your lower leg bones to "leg2r" and "leg2l" respectively!__





