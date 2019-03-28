# HoloLensCameraStream for Unity
This Unity plugin makes the HoloLens video camera frames available to a Unity app in real time. This enables Unity devs to easily use the HoloLens camera for computer vision (or anything they want).

Use this if you need access to the HoloLens camera's frame buffer in Unity, including the [locatable camera attributes](https://developer.microsoft.com/en-us/windows/mixed-reality/locatable_camera).

## With this plugin, you can
* Do computer vision and machine learning on the frames in real time. (algorithms not included)
* Show a preview of what the HoloLens camera sees.
* Obtain a 3D coordinate given an image pixel coordinate from the HoloLens video camera. For instance, you could identify a book using computer vision, then render something on top of that book.

## Getting Started
The tutorials below will show you how to **build the plugin (DLL)** or run the **Unity sample app**.

### What's in this repository
There are two separate "solutions" (if you will) in this repository:
* **The Plugin (HoloLensCameraStream/)** - This is a Video Studio solution that builds the Unity Plugin, which is just a DLL (library file). You can drop this DLL into a Unity project (in the appropriate directory) and then use its public methods, properties, and events from Unity.
* **The Unity example project (HoloLensVideoCaptureExample)/** - This shows you how to use the HoloLensCameraStream plugin in a Unity project. You can build it and run it on the HoloLens, or copy it and use it as a starter template for your own project.

### Things you need
* [The HoloLens development tools](https://developer.microsoft.com/en-us/windows/mixed-reality/install_the_tools), sans Vuforia and the Emulator.

### Building the Plugin
The plugin solution has two projects. One is the actual library project, and the other is a "dummy" or "placeholder" project that is used to keep the Unity editor from complaining ([more info here](https://docs.unity3d.com/Manual/windowsstore-plugins.html)). The placeholder project doesn't do anything, it just mimics the plugin's public API. To build the solution (including both projects), follow the steps below:
1. **Clone this repository.**
2. **Open the HoloLensCameraStream solution in Visual Studio.** It lives in `HoloLensCameraStream/HoloLensCameraStream/HoloLensCameraStream.sln`.
3. **Build the solution** In the VS menu, select **Build > Build Solution**. *Note: Make sure Visual Studio is set up to download missing NuGet packages, or you will get a spew of errors about missing namespaces.* 
4. That's all! This will produce two DLLs, one is the real DLL, and the other is the "placeholder" described above.

### Integrating the plugin into a Unity project
If you made some changes to the plugin project, and you want to use your newly-built DLLs, follow these steps. If you haven't made changes to the plugin, but just want to learn how to *use the CameraStream plugin in Unity*, skip this part and read instead about running the example project.
1. **Find the DLLs you just build:** Look in the output window after you build the plugin solution. You will see two paths to the newly-build DLLs. Navigate to them.
2. **Paste the plugin DLL into Unity:** Copy the plugin DLL from the output directory and paste it into the your Unity app. It must be pasted into the `Assets/Plugins/WSA/` directory in your Unity project because it will only compile for WSA devices.
3. **Paste the placeholder DLL into Unity:** Copy the placeholder (dummy) DLL from its output directory and paste it into your Unity app. It must be in the `Assets/Plugins/` directory in your Unity project. This is the DLL that the Unity editor compiles against uses while you're coding in Unity.
4. **Edit the plugin's settings:** In the Unity editor, select the `HoloLensCameraStream.dll` plugin file that you pasted in step 2. In the inspector, uncheck all platforms except `WSAPlayer`. Set the SDK to `UWP`, and set the Scripting Backend to `Dot Net`. In the Placeholder dropdown, select the `HoloLensCameraStream.dll` option (it will likely be the only option).
5. **Click Apply.**
6. **Edit the placeholder plugin's settings:** Select the placeholder (dummy) plugin in `Assets/Plugins/HoloLensCameraStream.dll`. Uncheck all platforms except `Editor`.
7. **Click Apply.**

You should now be able to code against the HoloLensCameraStream plugin after importing said namespace in your Unity scripts. *Note: Your Unity project needs to be [appropriately configured for HoloLens development](https://developer.microsoft.com/en-us/windows/mixed-reality/holograms_100).*

### Running the example Unity project
The example Unity project can be found in the root `HoloLensVideoCaptureExample/` directory. This Unity project is a great way to learn how to use the CameraStream plugin in Unity, or to use as a template for your own Unity project. Read on to learn how to build and run the example project on your HoloLens. You should be familiar with creating and configuring a new Unity-HoloLens project [according to Microsoft's instructions](https://developer.microsoft.com/en-us/windows/mixed-reality/holograms_100). As Microsoft and Unity update their HoloLens documentation, I'm sure this tutorial will become out of date.
1. **Open the example project:** Navigate to and open the example project directory (`HoloLensVideoCaptureExample/`) in Unity.
2. **Configure build settings:** Once the project opens, select **File > Build Settings**. In the Platform list, select `Windows Store`. Set the SDK to `Universal 10`; set Target device to `HoloLens`; set UWP Build Type to `D3D`; check the Unity C# Projects checkbox; and finally, click **Switch Platform**.
3. **Build the project:** You can now build the Unity project, which generates a Visual Studio Solution (which you will then have to also build). With the Build Settings window still open, click **Build**. In the explorer window that appears, make a new folder called `App`, which should live as a sibling next to the `Assets` folder. Then click Select Folder to generate the VS solution in that folder. Then wait for Unity to build the solution.
4. **Open the VS Solution:** When the solution is built, a Windows explorer folder will open. Open the newly-built VS solution, which lives in `App/HoloLensVideoCaptureExample.sln`. This is the solution that ultimately gets deployed to your HoloLens.
5. **Configure the deploy settings:** In the Visual Studio toolbar, change the solution platform from `ARM` to `x86`; Change the deploy target (the green play button) to `Device` (if your HoloLens is plugged into your computer), or `Remote Machine` (if your HoloLens is connected via WiFi).
6. **Run the app:** Go to **Debug > Start Debugging**. Once the app is deployed to the HoloLens, you should see some confirmation output in the Output window.

If you have questions, [check out the FAQ](https://github.com/VulcanTechnologies/HoloLensCameraStream/wiki/FAQ).

## Contributing
We would love for you to contribute to this project. Take a look at the TODO list in the Plugin's solution, or look at the Issues tab on GitHub to see how you can contribute. Thanks, enjoy!
