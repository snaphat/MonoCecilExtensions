csc -debug ../MonoCecilExtensions.cs Injector.cs /reference:Mono.Cecil.dll
csc -debug Target.cs
Target.exe
Injector.exe
Target.exe