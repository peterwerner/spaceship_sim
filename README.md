###### THIS HAS BEEN MOVED TO A PRIVATE REPO ON GITLAB

# spaceship_sim
Simulation of atmosphere + forces within a spaceship

### Implemented / In-progress:

###### Atmosphere flow simulation
Rooms automatically construct fields of voxels representing atmosphere at discrete points.  Connectors automatically join voxels between rooms.  Connectors act as doorways / windows and can be either open or closed.  Atmosphere 'flows' through the discrete voxels, which each keep track of an atmosphere value and a flow vector (equivalent to the rate of change of atmosphere).  Forces are applied using the flow vector of the closest voxel.  Currently, the only transformation of the parent room that this approach supports is translation / movement - if the parent room rotates or is scaled, this model breaks.  This also only supports rooms and connectors which are axis aligned in terms of rotation.  These limitations could both be changed but it would add extra overhead and may not be worth it.

##### Particle visualization of atmosphere flow
Each collection of rooms should have an associated particle system.  The number of particles produced is proportional to the number of voxels (ie: the volume of the rooms) and the magnitude of flow throughout the rooms.  Each particle's velocity is affected by the flow vectors (simulation of forces).

##### Player controller
Acts like a standard FPS player controller when under the influence of a room's gravity. Move around using jets (like a space-walk), with full range of motion when in space / not under the influence of gravity.

#### Pathfinding / NPC character controller
A* pathfinding + character controller.  Plays nice with variable directions of gravity.
