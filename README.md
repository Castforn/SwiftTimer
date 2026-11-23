# Swift Timer
A lightweight and flexible callback-driven timer system for Unity.  

This package provides a centralized way to schedule and manage timers without relying on individual MonoBehaviour instances, coroutines, or manually maintained update loops.  

I designed this system with performance and flexibility in mind, it also provides you with simple APIs for creating and managing timers. It is also burst-compiled and job supported, meaning it can handle many timers at once and not bog down Unity's mainthread. However, it does not mean its free performance. The synchronization and the job itself still has a cost because of their overhead. unless your game uses alot of individual timers (256+ timers just to barely break even). This timer system would more likely to be a liability than a benefit, so be mindful.

There's also the callbacks. Despite the timers being multithreaded, their callbacks are still singlethreaded. This is important because Unity's PlayerLoop is executed in the mainthread which also allow callbacks to safely interact with its ecosystem. This also means that if alot of timers were completed in the same frame, all their callbacks would be invoked at the same time, causing a lag spike (it also depends of how much work each callback does).

## Installation

1. Open you Unity project
2. Select `Window` at your top corner screen
3. Then go to `Package Management`
4. Select `Package Manager`
5. Select `+` at the top-left corner of the package manager window
6. Select `Install package from git URL`
7. Paste this git URL `https://github.com/Castforn/SwiftTimer.git`

8. SwiftTimer also requires these separate packages to work
- `Unity Job System` - Exists in the Unity Registry. But its usually already built-in in URP projects
- `Unity Collections` - Exists in the Unity Registry. But its usually already built-in in URP projects
- `Command And Callback` - My own command library package. Paste this git URL `https://github.com/Castforn/CommandAndCallback.git`

9. Your done!

## Features

- Handle-Based Management – Timers can be controlled using lightweight `TimerHandle` structs.
- Callback Driven - When a timer completes, it will invoke the callback you supplied with.
- Reusable Blueprints – `TimerBlueprint` can be reused to create independent timers.
- Flexible Timing – You can choose when callbacks are invoked using `TimerTiming`.
- Multiple Clocks – Supports up to 4 time sources, which are scaled, unscaled, frame-based, and fixed frame-based timing.
- Manipulable States – The state of a timer can freely be changed as long as its still active.
- Timer Dependencies – Timers can be scheduled to wait until another timer completes. Basically, its the same principle as Unity's `JobHandle` in the job system.
- 8 Generic Parameters Support - Callbacks can support up to 8 generic parameters.

## API

These are most of the APIs you can use.  
I say 'most' because the rest of the remaining API are all abstracted by `TimerHandle` which im pretty sure you'll generally be using that handle the entire time.

### TimerBlueprint

A reusable configuration that can create multiple independent timers with.

```csharp
using SwiftTimer;
using UnityEngine;

public class Test : MonoBehaviour
{

    // You can create a blueprint like this
    static TimerBlueprint blueprint1 = TimerBlueprint.Create()
        .SetDuration(3)
        .SetClock(TimerClock.Scaled)
        .SetTiming(TimerTiming.PreUpdate)
        .SetEnvironment(TimerEnvironment.Runtime);


    // Or this
    static TimerBlueprint blueprint2 = TimerBlueprint.Create(3, 0, false, TimerClock.Unscaled, TimerTiming.PostFixedUpdate, TimerEnvironment.Editor);


    // But i would not recommend creating a blueprint on a non-static field
    TimerBlueprint timerblueprint = TimerBlueprint.Create()
        .Set(5.5f, 0, true, TimerClock.Frame, TimerTiming.EndFrame, TimerEnvironment.Both);


    // This is because blueprints are meant to be used as a configuration for your timers
    // Having one blueprint per instance would just be a waste of memory

}
```

### TimerHandle

A lightweight struct that provides access to a scheduled timer. It also allows you to inspect, modify, and control the timer without directly accessing the `TimerManager`.

```csharp
using SwiftTimer;
using UnityEngine;

public class Test : MonoBehaviour
{

    static TimerBlueprint blueprint = TimerBlueprint.Create()
        .SetDuration(3)
        .SetClock(TimerClock.Scaled)
        .SetTiming(TimerTiming.PreUpdate)
        .SetEnvironment(TimerEnvironment.Runtime);
    

    void Start()
    {
        // When you call Schedule(), the blueprint will output a TimerHandle which you will use to track or manage its state.
        TimerHandle timer1 = blueprint.Schedule(MyMethod1);


        // You can also add dependencies to the timer
        // Timers with dependencies will not start until the timer they depend on has been terminated.
        // It basically has the same principles as JobHandle in the Unity Job System

        
        // This timer will only run once timer1 has finished
        TimerHandle timer2 = blueprint.Schedule(MyMethod2, timer1);
    }


    void MyMethod1()
    {
        Debug.Log("My method 1");
    }

    void MyMethod2()
    {
        Debug.Log("My method 2");
    }

}
```
