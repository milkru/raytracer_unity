[![MIT License][license-shield]][license-url]

<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li><a href="#about-the-project">About The Project</a></li>
    <li><a href="#built-with">Built With</a></li>
    <li><a href="#getting-started">Getting Started</a></li>
    <li><a href="#features">Features</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
  </ol>
</details>

## About The Project
![Product Name Screen Shot][product-screenshot]
A simple real-time ray-tracing project built using GPU compute in Unity engine.

### Built With
This project is built using Unity [2019.4.35f1](https://unity3d.com/get-unity/download/archive).

## Getting Started
By running the project, Unity renderer would be used by default, but by pressing the `SPACE` button, the same scene would get rendered using the ray-tracing path.
This allows easy comparison.

## Features
* Hard shadows
* Reflections with customizable number of bounces
* Specular lighting model
* Sphere and mesh tracing
* Point lights
* Integrated in the existing Unity workflow, so it allows switching between default Unity renderer and ray tracing path
* Accumulative anti aliasing implementation
* Skybox rendering
* Fake ambient occlusion

## License
Distributed under the MIT License. See `LICENSE` for more information.

## Contact
[@cg_kru](https://twitter.com/cg_kru)

[product-screenshot]: Assets/Textures/ur.png
[license-shield]: https://img.shields.io/github/license/othneildrew/Best-README-Template.svg?style=for-the-badge
[license-url]: https://github.com/othneildrew/Best-README-Template/blob/master/LICENSE.txt
