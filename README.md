# MeshDecals
A simple and un-optimized bare bones mesh decal system for Unity.

## Note
The system is not optimized. The decal creation is expensive, could be threaded (could it ?). Ever wondered why modern games don't have skinned decals at all ? or end up using sliding deferred decals ? Mesh based decals can be expensive, as they depend on the amount of vertices your mesh has, so, super high poly meshes (like the ones in AAA games) are super bad to creates decals to, thats one of the reasons of why they don't do it. The other reason is because they are lazy.

![](skinDecal1.gif)
