# MonoCecilExtensions

The `MonoCecilExtensions` is a static class that provides extension methods for classes from the Mono.Cecil library. Mono.Cecil is a popular library used for reading and writing Intermediate Language (IL) code. With the added functionality of `MonoCecilExtensions`, manipulation of IL code becomes even more convenient, enabling users to easily clone, merge, and update types in collections, methods, fields, and other components of a .NET assembly.

## Static Properties

### UpdateInfo

Nested within the `MonoCecilExtensions` class, the `UpdateInfo` class serves as an information container for updating Mono.Cecil definitions. It keeps track of all modifications made to the objects of the Mono.Cecil library during the manipulation process. The class includes the following properties:

- `updatedAttributes`: A collection of `CustomAttribute` objects that have been updated.
- `updatedInterfaces`: A collection of `InterfaceImplementation` objects that have been updated.
- `updatedFields`: A collection of `FieldDefinition` objects that have been updated.
- `updatedProperties`: A collection of `PropertyDefinition` objects that have been updated.
- `updatedMethods`: A collection of `MethodDefinition` objects that have been updated.
- `srcTypes`: A collection of source `TypeDefinition` objects that are being merged.
- `destTypes`: A collection of destination `TypeDefinition` objects where source objects are merged into.

### Assembly Update Information

The `assemblyUpdateInfo` is a static readonly property within the `MonoCecilExtensions` class. It's a dictionary mapping from `AssemblyDefinition` objects to their corresponding `UpdateInfo` objects, making it an essential tool for keeping track of the updates made to each assembly.

### Additional Search Directories

`MonoCecilExtensions` also includes a static readonly property `additionalSearchDirectories`, which is a collection of string values specifying extra directories to be included in the search during the IL code manipulation process.

More details about other methods and functionalities of the `MonoCecilExtensions` class are provided in the following sections of the document.

## Base Extension Methods

The `MonoCecilExtensions` class provides a set of basic extension methods that add simplicity and convenience to various operations in the Mono.Cecil library. These methods streamline operations such as loading assemblies, finding elements in Mono.Cecil objects, and adding elements to collections.

### LoadAssembly

```C#
LoadAssembly(this string location, bool readWrite = false)
```

This extension method loads an assembly from a given location, specified as a string. A boolean parameter `readWrite` can be set to determine whether the assembly is read-only or writable. The default value is `false`, indicating read-only. This method returns the `AssemblyDefinition` object of the loaded assembly if successful.

### FindMethodOfType

```C#
FindMethodOfType(this AssemblyDefinition assembly, string typeSignature, string methodSignature)
```

This method finds a method of a given type in an assembly. The `typeSignature` parameter is the full or simple name of the type, and the `methodSignature` parameter is the full or simple name of the method. The method returns the `MethodDefinition` object of the found method, or `null` if not found.

### FindType

```C#
FindType(this AssemblyDefinition assembly, string typeSignature)
```
```C#
FindType(this AssemblyDefinition assembly, Type type)
```

These two methods find a type in an assembly using its full name or simple name. The first method accepts a `typeSignature` parameter as a string, and the second method accepts a `type` parameter as a `Type` object. Both methods return the `TypeDefinition` object of the found type, or `null` if not found.

### FindField

```C#
FindField(this TypeDefinition type, string fieldSignature)
```

This method finds a field in a type, based on its full name or simple name. It returns the `FieldDefinition` object of the found field, or `null` if not found.

### FindMethod and FindMethods

```C#
FindMethod(this TypeDefinition type, string methodSignature)
```
```C#
FindMethods(this TypeDefinition type, string methodSignature)
```

These two methods find a method or methods in a type. The `FindMethod` method returns the `MethodDefinition` object of the found method or `null` if not found, while the `FindMethods` method returns a collection of `MethodDefinition` objects for all found methods that match the given `methodSignature`, or an empty collection if none are found.

## Clone Extension Methods

The `MonoCecilExtensions` class also provides extension methods for cloning various Mono.Cecil objects. This is useful when you want to create a copy of an object without modifying the original object. The clone methods return a new object that is identical to the original but distinct in memory.

### Clone CustomAttribute

```C#
Clone(this CustomAttribute attribute)
```

This method clones a `CustomAttribute` object. The parameter to be cloned is a `CustomAttribute` and it returns a clone of the original attribute.

### Clone InterfaceImplementation

```C#
Clone(this InterfaceImplementation @interface)
```

This method clones an `InterfaceImplementation` object. The parameter to be cloned is an `InterfaceImplementation` and it returns a clone of the original interface.

### Clone FieldDefinition

```C#
Clone(this FieldDefinition field)
```

This method clones a `FieldDefinition` object. The parameter to be cloned is a `FieldDefinition` and it returns a clone of the original field.

### Clone PropertyDefinition

```C#
Clone(this PropertyDefinition property)
```

This method clones a `PropertyDefinition` object. The parameter to be cloned is a `PropertyDefinition` and it returns a clone of the original property.

### Clone ParameterDefinition

```C#
Clone(this ParameterDefinition parameter)
```

This method clones a `ParameterDefinition` object. The parameter to be cloned is a `ParameterDefinition` and it returns a clone of the original parameter.

### Clone VariableDefinition

```C#
Clone(this VariableDefinition variable)
```

This method clones a `VariableDefinition` object. The parameter to be cloned is a `VariableDefinition` and it returns a clone of the original variable.

### Clone Instruction

```C#
Clone(this Instruction instruction)
```

This method clones an `Instruction` object. The parameter to be cloned is an `Instruction` and it returns a clone of the original instruction.

### Clone MethodDefinition
```C#
Clone(this MethodDefinition method)
```

This method clones a `MethodDefinition` object. The parameter to be cloned is a `MethodDefinition` and it returns a clone of the original method.
