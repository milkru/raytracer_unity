### About the project
A simple real-time ray-tracing project built using GPU compute in Unity engine.
  
![Product Name Screen Shot][product-screenshot]

### Built with
This project is built using Unity [2019.4.35f1](https://unity3d.com/get-unity/download/archive).

### How to use
By running the project, Unity renderer would be used by default, but by pressing the `SPACE` button, the same scene would get rendered using the ray-tracing path.
This allows easy comparison. Every raytraced object should have `RayTracingTag.cs` component attached to it.

### Features
* Point lights
* Hard shadows
* Reflections with customizable number of bounces
* Specular lighting model
* Sphere and triangle mesh tracing
* Integrated into existing Unity workflow, allowing Unity rendering path and ray tracing path comparisons
* Accumulative anti aliasing
* Skybox rendering
* Fake ambient occlusion

### License
Distributed under the MIT License. See `LICENSE` for more information.

[product-screenshot]: Assets/Textures/ur.png
