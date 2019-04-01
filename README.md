# SkinnedDecals
A simple un-optimized bare bones skinned decal system for Unity.

## How it works
The system tests each vertice of a triangle against the 6 planes of a view frustum (ortho). You can get view frustum planes by using GeometryUtility.CalculateFrustumPlanes(Maxtrix4x4 matrix). The view is not taken from the camera, but from a custom Matrix4x4 I took a day to get it working, in other words, you dont need a camera to project decals.

## Note
The system is not optimized, therefore not actually usable in a real project. The decal creation is expensive (around 30ms here), could be threaded, and the decals skinned meshes are not combined (lots of drawcalls).

![](skinDecal1.gif)
