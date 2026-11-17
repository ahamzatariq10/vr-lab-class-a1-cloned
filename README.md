# Assignment 1 – Unity Basics

In this assignment you will learn the core fundamentals of Unity: scripts as components, the Inspector, prefabs, instantiating objects, movement, collisions, and audio.

## Setup

- **Unity version:** 6000.3.25f1
- **Scene:** [`Assets/Assignment 1/Scenes/Town.unity`](Assets/Assignment%201/Scenes/Town.unity)
- **Working folder:** Work **exclusively** inside [`Assets/Assignment 1/`](Assets/Assignment%201/). Put your scripts in e.g. `Assets/Assignment 1/Scripts/`. Do not modify any files outside this folder.

### Scene Setup 
```
Assignments
├── Task 1: Tree Spawn   
│   ├── Base
│   └── House
└── Taks 2: Driving Bus
    ├── Garbage Truck        
    └── Trashbags
```
You can ignore everything outside of the "Assignments" game object.

### Provided assets

| Folder                              | Contents                                                                           |
|-------------------------------------|------------------------------------------------------------------------------------|
| `Assets/Assignment 1/Prefabs/`      | `Bush_01`, `Bush_02`, `FlowerPatch_02`, `Grass_Edge_02`, `Tree_Patch`, `Trash Bag`, `Fly` |
| `Assets/Assignment 1/Sounds/`       | `Collecting_OneShot.wav`                                                           |
| `Assets/Assignment 1/Scenes/`       | `Town.unity`                                                                       |


## Resources

Depending on your prior Unity experience and preferred learning media, you might benefit from the following resources to get started with Unity:

The [Unity Essentials Pathway](https://learn.unity.com/pathway/unity-essentials) offers tutorial videos and practise tasks to learn the very fundamentals of the Unity Editor.

In the tasks, we mention various Methods and Classes from the Unity [Scripting API](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/index.html).

For example, you can look up the reference page for the [Bounds](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Bounds.html) class, including the [Bounds.Contains](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Bounds.Contains.html) method, including a code example.

---

## Task 1 – Random Spawning of Environment Objects

When the scene starts, the environment should automatically be filled with trees, bushes, flowers, etc. – randomly distributed inside the **Base**, but **not inside the house**.

### Requirements

1. Create a new C# script (e.g. `RandomSpawner.cs`) and attach it to the **`Tree Spawn`** GameObject.
2. The script has a **list of prefabs** that is visible and editable in the **Inspector**.
   - Drag the prefabs from `Assets/Assignment 1/Prefabs/` (`Bush_01`, `Bush_02`, `FlowerPatch_02`, `Grass_Edge_02`, `Tree_Patch`) into the list.
3. The **number** of objects to spawn can also be set in the Inspector.
4. When the scene starts, randomly chosen prefabs from the list are instantiated at **random positions**.
5. Objects may **only** appear **inside the Base** (the area bounded by the four cubes under `Base`).
6. Objects must **not** appear **inside the house**.
7. Spawned objects are parented to the `Environment Prefabs` gameObject to keep the Hierarchy tidy. (Create this gameObject yourself)

### Hints

- Fields show up in the Inspector if they are `public` or marked with `[SerializeField]`. `List<GameObject>` or `GameObject[]` work well for the prefab list.
- Useful APIs: `Instantiate(...)`, `Random.Range(...)`, `Start()`.
- For the Base boundaries you can use, for example, the positions of the cubes, the `bounds` of their colliders/renderers, or simply min/max values exposed in the Inspector.
- There are several ways to exclude the house, e.g.:
  - determine the house's `Bounds` and reject positions inside it (`Bounds.Contains(...)`),
  - place a (trigger) collider around the house and check with `Physics.CheckBox` / `Physics.OverlapSphere`,
  - reject invalid positions and roll again (with a maximum number of attempts to avoid infinite loops).
- Optional: give the objects a random rotation around the Y axis (`Quaternion.Euler(...)`) so the result looks more natural.

### Grading Criteria (1 point per criterion)

- [ ] Prefab list is visible and editable in the Inspector
- [ ] Number of objects is configurable in the Inspector
- [ ] Objects spawn randomly on start
- [ ] No object spawns outside the Base
- [ ] No object spawns inside the house

## Task 1 - Bonus (0.5 points per criterion)

- Prevent spawned objects from overlapping each other (minimum distance).
- Weighted probabilities per prefab (e.g. more grass than trees).

---

## Task 2 – Driving Bus

The **Garbage Truck** has to collect the trash bags. The trash bags under `Driving Bus/Trashbags` form a line, i.e. the route the truck drives along.

### Requirements

1. Create a script (e.g. `TruckDriver.cs`) and attach it to the **`Garbage Truck`**.
2. The truck drives along the route defined by the **trash bags** – from `Trash Bag` through `Trash Bag (1)` up to `Trash Bag (9)`.
   - The truck must **rotate** to face its driving direction (it must not drive sideways).
   - Speed (and optionally rotation speed) can be set in the Inspector.
   - Movement and rotation must be **framerate independent**: the truck has to drive at the same speed whether the game runs at 30 or 300 FPS.
3. Neither the truck nor the trash bags have any physics components yet. Figure out which components are required for Unity to detect a collision between them and add them yourself (to the truck and to the **`Trash Bag` prefab**).
4. Create a second script (e.g. `TrashBag.cs`) and add it to the **`Trash Bag` prefab**.
5. When the truck drives into a trash bag:
   - the trash bag **disappears**, and
   - the sound **`Collecting_OneShot`** (`Assets/Assignment 1/Sounds/`) is played.
   - A new fly is spawned from the fly prefab as a child of the truck and circles the truck while it moves along.

### Hints

- **Collision detection:** Read up on how collisions work in Unity ([Introduction to collision](https://docs.unity3d.com/Manual/CollidersOverview.html)). Ask yourselves:
  - Which objects need a **Collider**? Which collider shape fits best?
  - Does at least one of the objects need a **Rigidbody**? Where should you add one?
  - Should the trash bags physically block the truck, or should the truck drive *through* them? What does the `Is Trigger` option change?
  - Depending on your setup, which method gets called: `OnCollisionEnter` or `OnTriggerEnter`?
- You can store the route as a list of `Transform`s in the Inspector, or read it at runtime from the children of the `Trashbags` GameObject. Careful: once a trash bag is destroyed, its reference becomes `null` – so store the positions beforehand.
- Useful APIs: `Vector3.MoveTowards(...)`, `Vector3.Distance(...)`, `Quaternion.LookRotation(...)`, `Quaternion.RotateTowards(...)` / `Quaternion.Slerp(...)`.
- **Framerate independence:** `Update()` is called once per frame, and the number of frames per second varies from machine to machine. If you move the truck by a fixed amount per frame, it drives faster on a fast PC than on a slow one. Think in **units per second** instead and scale your values with the time that has passed since the last frame (`Time.deltaTime`, or `Time.fixedDeltaTime` inside `FixedUpdate()`). Test it: when entering play mode you can press f8 to open a panel where you can set your framerate manually at runtime.
- Consider whether a Rigidbody should use `Is Kinematic == true` or use gravity.
- **Playing a sound while destroying:** an `AudioSource` on the trash bag is destroyed together with it as soon as you call `Destroy(gameObject)` – the sound is cut off immediately. Possible solutions:
  - `AudioSource.PlayClipAtPoint(clip, position)`,
  - play the sound through an `AudioSource` on the truck,
  - or hide the trash bag first (disable renderer/collider) and destroy it with a delay.
- **Fly circling truck** add a `Fly.cs` component to the fly prefab. Update the local position and rotation to make the fly circle the truck.
  - There are different ways to spawn the fly, so what if you need to get the reference to the truck's transform in OnTriggerEnter in TrashBag.cs? The argument `OnTriggerEnter(Collider other)` can be used to access the transform via `other.transform` or a component of type `T` via `other.GetComponent<T>()`.

### Grading Criteria (1 point per criterion)

- [ ] The truck visits all trash bags in the correct order
- [ ] The truck rotates to face its driving direction
- [ ] Speed is configurable in the Inspector
- [ ] Movement and rotation are framerate independent
- [ ] Trash bags disappear when the truck hits them
- [ ] `Collecting_OneShot` plays completely for every collected trash bag
- [ ] For each collected trash bag, a fly is added that circles the truck

---

## Task 2 - Bonus (0.5 points per criterion)

- Display a counter of collected trash bags in the UI.
- Make the truck stop at the end of the route, or loop the route.

---

## Submission

- All scripts are located in `Assets/Assignment 1/`.
- The `Town.unity` scene is saved and all scripts are correctly assigned.
- The project runs without errors in the Console.
- Everything that changes over time (movement, rotation, timers, …) is **framerate independent** – this applies to all tasks, including the bonus tasks.
