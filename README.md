# JobObject.Net

This library allows .NET developers to create Job objects, add processes to them and manage their limits. The component is used by Stormancer to enforce resource consumption limits.

I built it because other available implementations didn't work if the application process was already running in a job (applications started in the Visual Studio debugger are in this case).
