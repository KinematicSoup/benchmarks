# Unity Network Transform Benchmark Project

This repo contains a Unity project with a benchmark for the comparing bandwidth usage for syncing transform data of our
Reactor Multiplayer Engine against some alternative networking solutions for Unity.

## Purpose

This Benchmark is intended to compare network transform compression bandwidth between a selected number of frameworks. It implements
a simulation of orbiting, interacting asteroids to as a means generate structured motion with a significant random element. The simulation
updates each object's transform differently at different times, but allows for some commonality where groups of objects move similarly relative
to each other, similar to how objects in a game would move. Interactions between the objects generate additional random motion.

This benchmark includes copies of frameworks where the framework licensing allows it. PurrNet and Fishnet required naming modification to coexist
with other frameworks. Those modifications are detailed below where the settings for each framework is detailed. None of the modifications should 
affect performance.

### Frameworks

- Reactor
- PurrNet
- FishNet
- Photon Fusion 2
- Mirror
- SFS2X (SmartFoxServer 2X)
- NGO (Netcode for GameObjects)

### Running a Benchmark

Each framework has a folder under the Benchmarks folder, and each of those folders has a Scenes folder containing the benchmark scene for that
framework. Run that scene using multiplayer playmode or do a build with that scene and start a server and a client to run the benchmark. The
frames per second is displayed in the bottom left corner. Be sure to monitor this value to make sure it does not drop below the target sync rate
(defaults to 30 syncs per second). If the frame rate is too low, for the best performance, do a build and keep the server window focused.

Use Window's Resource Monitor or a packet capture tool such a [Wireshark](https://www.wireshark.org/) to view the bandwidth usage. All Frameworks
are configured to run on port 7777 when running locally.

#### Reactor

- Open the Reactor benchmark scene from 'Assets/Benchmarks/Reactor/Scenes/ReactorBenchmark'.
- Build configs **CTRL+F2**.
- Select the 'SphereRingBenchmarkRoom' game object and find the *ksRoomType* script. Click **Start Local Server**.
- Find the *ksConnect* script. Make sure 'Connect Mode' is set to 'Local'.
- Enter playmode. You should connect to the server. Some stats will be displayed in the top left, including the goodput (the amount of data received
excluding protocol overhead) and a bandwidth estimate. The sync rate should stay near 30 syncs/s.

#### Photon Fusion

You have to import the Photon Fusion 2 package on your own as their terms of service prevent us from distributing it, and you have to set up an
account and get a Fusion App id.

Sometimes the Photon Fusion client fails to connect silently. If this happens, click **'Shutdown'** and try to connect again.

#### SmartFoxServer 2X

You have to install SmartFoxServer 2X. You can download if from their [website](https://www.smartfoxserver.com/download/sfs2x#p=installer). It will
ask you if you want to install it as a service. If you are just using it to test this benchmark, it's recommended you do not. If you do not install
it as a service, you will have to manually start the SFS2X server by running '<SFS2X install location>/SFS2X/sfs2x-standalone.exe' before you can
connect and run the benchmark. 

The Unity SFS2X benchmark connects to the SFS2X server on 2 different ports--one for the client that acts as a server and runs the simulation and
sends transform updates, and one for the client who receives transform updates. By default the client port is 9933 and the server port is 9934.
This is so when we monitor the bandwidth, we can exclude the traffix from SFS2X to the Unity "server" by only monitoring the bandwidth on the
client port. By default SFS2X only accepts connections on port 9933, so we have to configure it to also accept connections on port 9934.

Once the SFS2X server is running, open 'http://localhost:8080/admin/' in a browser and login to the admin panel with username and password
'sfsadmin'. Navigate to the 'Server Configurator' tab. Click the '+' button below the 'Socket addresses' table. Set the 'Port' to 9934. Leave the
'IP address' as '127.0.0.1' and the 'Protocol' as 'TCP'. Click the 'Add+' button.  Click the 'Submit' button at the bottom of the page, then click
the button with the circular arrow refresh icon to restart the server so your changes will be applied. This configuration is saved so you should
only need to do this once before you can run the benchmark.

### Modifying the Benchmark

You can find benchmark data assets in 'Assets/Benchmarks/Resources' and edit their parameters to change things like the number of objects or
their speeds. There is currently only one bencmark data asset.

The Reactor server does not run in Unity, and therefore there are two implementations of the benchmark--one for the Reactor server and one for Unity.
The Reactor server implementation is located in 'Assets/ReactorScripts/Server/sSphereRingBenchmark.cs' and the Unity implementation is in
'Assets/Scripts/SphereRingBenchmark.cs'. Any changes to one implementation should also be made in the other implementation.

## Asteroid Network Transform Benchmark

The benchmark has 250 asteroids with dynamic rigid bodies travelling in random directions with random angular velocities within a sphere. The
asteroids have a random target speed they will accelerate or decelerate to maintain. The asteroids change direction when they collide or when
they reach the bounds of the sphere. There is a small static planetoid at the origin of the sphere the asteroids can collide with. The asteroids
eventually settle into a ring formation with most asteroids travelling in the same direction around the sphere, after approximately 5 minutes.
All frameworks were configured to sync all objects 30 times per second. For frameworks that allow precision or sensitivity/thresholds to be
configured, position precision was set to .01 and rotation precision to .001.

250 objects was chosen because Photon Fusion will not sync more than 255 objects in a single update. This can be seen if you set the
**'Tools>Fusion>Network Project Config>Replication Features'** to 'None', as it will not sync more than 255 objects. Also Fusion's bandwidth
barely increases once you surpass 255 objects.

#### Reactor

- Version 1.1.0-0
- Server Frame Rate: 60
- Sync Rate: 30
- Client Send Rate: 60
- Position, Rotation, and Scale Precision: .01
- Rotation Precision: .001
- Protocol: RUDP

#### Photon Fusion

- Version: 2.0.6.1034
- Client and Server Tick Rate: 60
- Client and Server Send Rate: 30
- Protocol: UDP
- Page Shift: 128 Kb (Needed to be changed to fix an out of memory error when syncing 1000 objects).
- Page Count: 256
- Sync Scale: true (it has to be true to sync the initial scale)
- Sync Parent: false
- Photon Fusion does not have precision or sensitivity settings. It appears to sync with high precision.

#### PurrNet

- Version: 1.14.0
- Tick Rate: 30
- Transport: UDP Transport with default settings
- Visibility Rules: Always Visible
- Sync Position and Rotation: World
- Sync Scale and Parent: false (initial scale still syncs)
- PurrNet does not have precision or sensitivity settings. It appears to sync with high precision.

The following modifications were made to PurrNet to fix build errors when PurrNet and Mirror are in the same project:
- The asmdef at Assets/PurrNet/Externals/SimpleWebTransport/SimpleWebTransport.asmdef was renamed to PNSimpleWebTransport.
- Assets/PurrNet/Externals/SimpleWebTransport/Client/WebGL folder was deleted. This means PurrNet won't in WebGL builds.
- Changed SimpleWebClient.Create to return null in WebGL builds.


#### FishNet

- Version: 4.6.12R
- Tick Rate: 30
- Send Interval: 1
- Transport: Tugboat with default settings
- Synchronize Parent: false
- Synchronize Position and Rotation: true
- Synchronize Scale: false (it still syncs the initial scale)
- Position Sensitivity: .01
- Position and Rotation Packing: Packed (.01 precision for position, not sure about rotation)
- Scale Packing: Unpacked (because we disable scale syncing)

Some classes were renamed and their GUIDs were changed to fix build errors when FishNet and NGO are in the same project:
- PostProcessAssemblyResolver -> FNPostProcessAssemblyResolver
- PostProcessReflectionImporter -> FNPostProcessReflectionImporter
- PostProcessReflectionImporterProvider -> FNPostProcessReflectionImporterProvider

#### Mirror

- Version: 96.0.1
- Send Rate: 30
- Transport: KCP Transport with default settings
- Visibility: Force Shown
- Sync Position and Rotation: true
- Sync Scale: false (it still syncs the initial scale)
- Only Sync On Change: true
- Compress Rotation: true
- Use Fixed Update: true
- Rotation Sensitivity: .001
- Position and Scale Precision: .01
- Sync Interval: 0

#### SmartFoxServer 2X

SFS2X is a very low level framework compared to the other frameworks and leaves a lot of the implementation details for how and when to send and
apply updates to the developer. 

For this benchmark, we send updates for all objects 30 times per second. Each object is represented as two SFSObject room variables--one containing
the data that never changes (prefab, scale), and one containing the updating data (position, rotation). All SFSObject field names are a single
character to minimize bandwidth. We assign the synced game objects an incrementing integer id that starts at 1. The room variable names are a
single character ('s' for immutable spawn data, 'u' for updating data) prefixed to the game object's integer id.

Position and scale data is quantized to integers with .01 accuracy, and rotations are quantized with .001 accuracy and use smallest-3 encoding.

- Server Version: 2.19.0
- C# API Version: 1.8.5

#### NGO

- Version: 2.4.4
- Tick Rate: 30
- Transport: Unity Transport with default settings
- Sync Position and Rotation: X, Y, Z
- Sync Scale: None (it still syncs the initial scale)
- Position Threshold: .01
- Rot Angle Threshold: .001
- Use Unreliable Deltas: false
- Use Quaternion Synchronization: false (this was chosen because it performed better than using quaternion synchronization with compression enabled)
- Use Half Float Precision: true

### Results

```
Reactor     ~15 kB/s
PurrNet     ~100 kB/s
FishNet     ~103 kB/s
Photon      ~112 kB/s
Mirror      ~122 kB/s
SFS2X       ~160 kB/s
NGO         ~185 kB/s
```

These results were obtained from Wireshark by running each benchmark locally and capturing network traffic for at least 2 minutes and averaging
the bandwidth for those 2 minutes. Each test was run twice and the average of the two tests was rounded to the nearest kB/s.


