# spaceship_sim
Simulation of atmosphere + forces within a spaceship

## Implemented / In-progress:

##### Atmosphere flow simulation
Rooms automatically construct fields of voxels representing atmosphere at discrete points.  Connectors automatically join voxels between rooms.  Connectors act as doorways / windows and can be either open or closed.  Atmosphere 'flows' through the discrete voxels, which each keep track of an atmosphere value and a flow vector (equivalent to the rate of change of atmosphere).  Forces are applied using the flow vector of the closest voxel.  Currently, this only works for static rooms - making it respond to transformations (movement and rotation and scaling) of the room object will add extra overhead - maybe there should be a toggleable option to switch between static / dynamic.

## To-do:

#### Particle visualization of atmosphere flow
Each collection of rooms should have an associated particle system.  The number of particles produced should be proportional to the number of voxels (ie: the volume of the rooms) and the magnitude of flow throughout the rooms.  Each particle's velocity should be affected by the flow vectors (simulation of forces).  Two main problems to solve:
- Efficient lookup of the appropriate flow vector.  If there are N rooms and M particles, this can be done in O(N*M) time.  The inneficiency lies in finding the room the particle is in (O(N)).  Should try to figure out a way to speed up this operation closer to constant time.
- Intelligently spawning the particles.  Simplest solution:  In each update, pick a random room (possibly weighted to favor rooms with higher average flow magnitude) and move the particle system there.  Could also randomize position in the room.

#### Player controller
Needs to do the following:
- Act like a standard FPS player controller when under the influence of a room's gravity.
- Move around using jets (like a space-walk), including vertical control when in space (ie: not under the influence of gravity).
Main problem to solve:
- Different rooms may have different gravity orientations.  In space, should the player be able to rotate freely (there is no 'down' in space)?  How do we resolve the player's rotation when they enter a field of gravity?  Will Unity's physics resolve issues where we try to rotate the player and they end up stuck in some object?
