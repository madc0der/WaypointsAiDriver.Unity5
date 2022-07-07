# Waypoints driver

### Build path from waypoints, ordered empty `GameObjects` with only `Transform` component.

**Note:** compatible with Unity 5.6.4+, CarController and Car from Standard Assets included w/o any modifications.


![demo](Docs/follower-demo.gif "Demo")

### Package contains scripts for:
- Building path from root `GameObject` which contains several waypoints as a children
- Pre-calculate ahead distance to calculate steering and use it to get current steering angle
- Steering angle is calculated with a fix to control how close Follower will be to path line 

## How to use
1. Create new empty `GameObject` and add several empty children `GameObjects`. 
   It'll be waypoints, order is matter
   ![Add waypoints](Docs/put-waypoints.gif "Add waypoints")  
     

2. Add `WaypointsPathProvider` script to the waypoints root
   ![Add WaypointsPathProvider](Docs/add-path-provider.png "Add WaypointsPathProvider")
     
      
3. Attach `WaypointsAiDriver` script to your "Car" `GameObject`
   ![Add WaypointsAiDriver](Docs/add-follower-script.png "Add WaypointsAiDriver to Car GameObject")
   

4. Update `WaypointsAiDriver` to apply steering to your "Car"
   ![Update WaypointsAiDriver to apply steering to your car](Docs/update-driver-script.png "Update WaypointsAiDriver to apply steering to your car")
   
## `WaypointsAiDriver` params 
Steering is calculated as an angle between following vectors: current Car `transform.forward` 
and direction to **Ahead** point 


- `MaxAheadDistance` - max distance where ahead point can be shifted from Car position along the path
- `MinAheadDistance` - min distance where ahead point can be put
- `MaxAheadPathOffset` - maximum side offset from path to limit ahead scan distance
- `SteeringAngleLimit` - limit of steering angle (i.e. +/- 45 degrees)
- `PathProvider` - reference to `WaypointsPathProvider` component
- `FakeSpeed` - speed of movement of fake Car, you'll ignore it after update the script
- `SmoothSteeringSpeed` - last argument for `Lerp` in calculation of current steering angle
- `UseOutOfPathSteeringFix` - whether use `PathOffsetSteeringFix` or not
- `PathOffsetSteeringFix` - how much to steer to be close to path line
- `PauseKey` - key used to pause the demo