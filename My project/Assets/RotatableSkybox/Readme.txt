--- CREDITS ---

The cubemap in the demo scene was created by Road Turtle Games, many thanks to them for allowing me to include it here. Their website is https://roadturtlegames.com with a tutorial for how they created the skybox at https://roadturtlegames.com/planetguard/blogs/blog-4.html
This and a few other beautiful nebula skybox cubemaps can be found on the Unity Asset Store here: https://assetstore.unity.com/packages/2d/textures-materials/sky/nebula-skyboxes-219924

--- HOW TO ---

1) SkyboxRotationHelper.cs works only in editor mode and uses a the "Look Direction" and transform.up of the Skybox Rotation Helper GameObject (I have added transform.forward and transform.up graphics for this object to help visualize what rotation changes are occuring) to set the initial rotation of the Cubemap Skybox Material. Its set to update each editor frame, but you can disable that by making the updateEachFrame bool to false. The "Update Rotation" button in the Inspector will update the rotation when pressed.

2) Assign the skybox directional light to the skyboxLight field of the Rotatable Skybox Controller Object and rotate the light to match where the main light would come from your cubemap.

3) Assign the cubemap skybox material to the skyboxMaterial field of the Rotatable Skybox Controller Object. This material should use the shader Skybox/Rotatable, so that it has the "_QuaternionRotation" property.

4) At runtime, any rotations applied to the skyboxLight field GameObject will be applied to your skyboxMaterial material if it uses the Skybox/Rotatable shader :)

5) RotatableSkyboxController.cs can update the Global Illumination each frame (if the updateGiEachFrame bool is true) or when called via the public function RotatableSkyboxController.UpdateGI() - Updating the GI can be expensive, so I wanted to allow both options.
