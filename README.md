# Swift Timer
A lightweight and flexible callback-driven timer system for Unity.  

This package provides a centralized way to schedule and manage timers without relying on individual MonoBehaviour instances, coroutines, or manually maintained update loops.  

I designed it with performance and flexibility in mind, it also provides you with simple APIs for creating and managing timers. This system is job supported, meaning it can handle many timers at once and not bog down the mainthread, however, it does not mean its free performance. The job itself still has a cost because of its overhead, unless your game uses alot of individual timers (256+ timers just to barely break even). This timer system would more likely be a liability than a benefit, so be mindful.

## Installation
1. Open you Unity project
2. Select Window at your top corner screen
3. Then go to Package Management
4. Select Package Manager
5. Select + at the top-left corner of the package manager window
6. Select Install package from git URL
7. Paste this git URL `https://github.com/Castforn/CommandAndCallback.git`

8. SwiftTimer also requires these separate packages to work
- `Unity Job System` - Exists in the Unity Registry. But its usually already built-in in URP projects
- `Unity Collections` - Exists in the Unity Registry. But its usually already built-in in URP projects
- `Command And Callback` - My own command library package. Paste this git URL `https://github.com/Castforn/CommandAndCallback.git`

10. Your done!

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
I say 'most' because the rest of the remaining API are all abstracted by `TimerHandle` which you will generally be using most of the time.

### TimerBlueprint

```csharp

```
