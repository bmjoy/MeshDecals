# MeshDecals
A simple and un-optimized bare bones mesh decal system for Unity.

## Read
Initially, this project was going to be just a skinned decal system, but then, I decided to integrate Deni35 static decals system. The skinned mesh decal system was made by me. This asset is not optimized. The decal creation is expensive, could be threaded (could it ?). 

Ever wondered why modern games don't have skinned decals at all ? or end up using sliding deferred decals ? Mesh based decals can be expensive, as they depend on the amount of vertices your mesh has, so, super high poly meshes (like the ones in AAA games) are super bad to creates decals to, thats one of the reasons why they don't do it. Another reason is because they are lazy.

## Todo
* Add support for terrain decals (should be easy, as Deni35 already actually did it, just need to integrate that)
* Add clipping support for skinned decals (hard)

![](skinDecal1.gif)

## Credits
* Deni35 - His/hers free simple decal system at the asset store
