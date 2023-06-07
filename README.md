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
`MonoCecilExtensions` also includes a static readonly property `additionalSearchDirectories`, which is a collection of string values specifying extra directories to be included in the search when resolving assembly types.

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



## AddType Extension Method
The `MonoCecilExtensions` class offers a method that simplifies adding types to an assembly when using `Mono.Cecil`.

### Add a Type to an Assembly
```C#
AddType(this AssemblyDefinition assembly, TypeDefinition src, bool avoidSignatureConflicts = false)
```
This method adds a type to an assembly. This includes adding the type's fields, properties, and methods. If the source type is nested, it will be added as a nested type within the parent type in the destination assembly. The `avoidSignatureConflicts` parameter determines if the method should avoid name conflicts by adding a '_' suffix to the copied class name.

## AddFieldsPropertiesAndMethods Extension Method

The `MonoCecilExtensions` class provides an extension method for adding fields, properties, and methods from a source type to a destination type.

### Merging Two Types
```C#
AddFieldsPropertiesAndMethods(this TypeDefinition dest, TypeDefinition src)
```
This method merges the source type into the destination type by cloning the fields, properties, and methods of the source, updating their types, and adding them to the destination. This is a key part of merging two types, ensuring the destination type includes all necessary components from the source type.

The `dest` parameter is the destination type definition where fields, properties, and methods from the source will be added.

The `src` parameter is the source type definition whose fields, properties, and methods will be cloned and added to the destination.



## UpdateFieldsPropertiesAndMethods Extension Method

The `MonoCecilExtensions` class provides an extension method for updating fields, properties, and methods within a destination type after they have been cloned from a source type.

### Update Types in an Assembly
```C#
UpdateFieldsPropertiesAndMethods(this AssemblyDefinition assembly, bool avoidSignatureConflicts = false)
```
This method updates the types of attributes, interfaces, fields, properties, and methods within a given assembly. This includes updating the types in interfaces, fields, properties, and methods. It also updates the getter and setter methods for properties, updates the instruction types for methods, imports references for attributes, interfaces, fields, properties, and methods, imports base types of each destination type, and swaps any duplicate methods in the destination types.

The `avoidSignatureConflicts` parameter determines if the method should avoid signature conflicts by changing original method parameters to be base object types for duplicate methods.



# Additional internally used APIs



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



## UpdateTypes Extension Methods
The `MonoCecilExtensions` class also provides extension methods for updating type references within various Mono.Cecil objects. These methods ensure that copied fields, properties, and methods reference the copied types instead of the originals.

### Update InterfaceImplementation
```C#
UpdateTypes(this InterfaceImplementation @interface, TypeDefinition src, TypeDefinition dest)
```
This method updates the `InterfaceType` of a `InterfaceImplementation`, if it matches the source type, to the destination type.

### Update FieldDefinition
```C#
UpdateTypes(this FieldDefinition field, TypeDefinition src, TypeDefinition dest)
```
This method updates the `FieldType` of a `FieldDefinition`, if it matches the source type, to the destination type.

### Update FieldReference
```C#
UpdateTypes(this FieldReference field, TypeDefinition src, TypeDefinition dest)
```
This method updates the `FieldReference` and `DeclaringType` of a `FieldReference`, if they match the source type, to the destination type. If a matching field definition is found in the destination type, a reference to it is returned. Otherwise, the original field reference is returned.

### Update PropertyDefinition
```C#
UpdateTypes(this PropertyDefinition property, TypeDefinition src, TypeDefinition dest)
```
This method updates the `PropertyType` of a `PropertyDefinition`, if it matches the source type, to the destination type.

### Update ParameterDefinition
```C#
UpdateTypes(this ParameterDefinition parameter, TypeDefinition src, TypeDefinition dest)
```
This method updates the `ParameterType` of a `ParameterDefinition`, if it matches the source type, to the destination type.

### Update VariableDefinition
```C#
UpdateTypes(this VariableDefinition variable, TypeDefinition src, TypeDefinition dest)
```
This method updates the `VariableType` of a `VariableDefinition`, if it matches the source type, to the destination type.

### Update MethodDefinition
```C#
UpdateTypes(this MethodDefinition method, TypeDefinition src, TypeDefinition dest)
```
This method updates the `ReturnType` of a `MethodDefinition`, as well as `ParameterTypes` and `VariableTypes`, if they match the source type, to the destination type.

### Update MethodReference
```C#
UpdateTypes(this MethodReference method, TypeDefinition src, TypeDefinition dest)
```
This method updates the `ReturnType` and `DeclaringType` of a `MethodReference`, as well as the `ParameterTypes`, if they match the source type, to the destination type. If a matching method definition is found in the destination type, a reference to it is returned. Otherwise, the original method reference is returned.

### Update CallSite
```C#
UpdateTypes(this CallSite callSite, TypeDefinition src, TypeDefinition dest)
```
This method updates the `ReturnType` and `Parameters` of a `CallSite`, if they match the source type, to the destination type.



## UpdateInstructionTypes Extension Methods
The `MonoCecilExtensions` class also provides extension methods for updating type references within `Mono.Cecil.Instruction` objects. These methods are crucial for ensuring that the instructions within methods correctly reference the fields, properties, and methods of the destination type after cloning from the source type.

### Update Instruction
```C#
UpdateInstructionTypes(this Instruction instruction, TypeDefinition src, TypeDefinition dest)
```
This method updates the `Operand` of an `Instruction` when merging classes. The update strategy depends on the type of the operand. If the operand is a `ParameterDefinition`, `VariableDefinition`, `FieldReference`, `MethodReference`, `CallSite`, or `TypeReference`, it's updated accordingly.

### Update MethodDefinition
```C#
UpdateInstructionTypes(this MethodDefinition method, TypeDefinition src, TypeDefinition dest)
```
This method updates all instructions in the method's body. If the instruction's operand type matches the source type, it is replaced with the destination type.



## UpdateGettersAndSetters Extension Method
The `MonoCecilExtensions` class provides an extension method for updating getter and setter references within `Mono.Cecil.PropertyDefinition` objects. This method ensures that the properties of the destination type reference the correct getters and setters after cloning from the source type.

### Update PropertyDefinition
```C#
UpdateGettersAndSetters(this PropertyDefinition property, TypeDefinition src, TypeDefinition dest)
```
This method updates the getter and setter methods of a `PropertyDefinition` to reference the destination type when merging classes. It does the following:

- Clones the existing getter/setter methods, so that any modifications do not affect the original methods
- Calls `UpdateTypes` to update all type references within the methods' bodies from src to dest
- Updates the declaring type of the methods to be dest
- Finds the equivalent methods in dest (if they exist), and updates the property's getter/setter methods to reference them

This process ensures that the property correctly interacts with the destination type after merging.



## ImportReferences Extension Methods
The `MonoCecilExtensions` class provides several extension methods for importing references from one module to another using `Mono.Cecil`. These methods are crucial when merging assembly classes as they allow the destination type to access types that may not have been referenced prior.

### Import CustomAttribute References
```C#
ImportReferences(this CustomAttribute attribute, ModuleDefinition module)
```
This method imports the constructor reference for a given attribute into a module.

### Import InterfaceImplementation References
```C#
ImportReferences(this InterfaceImplementation @interface, ModuleDefinition module)
```
This method imports the interface type and custom attributes references of an interface into a module.

### Import FieldDefinition References
```C#
ImportReferences(this FieldDefinition field, ModuleDefinition module)
```
This method imports the field type and custom attributes references of a field into a module.

### Import PropertyDefinition References
```C#
ImportReferences(this PropertyDefinition property, ModuleDefinition module)
```
This method imports the property type and custom attributes references of a property into a module.

### Import ParameterDefinition References
```C#
ImportReferences(this ParameterDefinition parameter, ModuleDefinition module)
```
This method imports the parameter type and custom attributes references of a parameter into a module.

### Import VariableDefinition References
```C#
ImportReferences(this VariableDefinition variable, ModuleDefinition module)
```
This method imports the variable type references of a variable into a module.

### Import MethodDefinition References
```C#
ImportReferences(this MethodDefinition method, ModuleDefinition module)
```
This method imports the method type references and the custom attributes of a method into a module.

### Import CallSite References
```C#
ImportReferences(this CallSite callSite, ModuleDefinition module)
```
This method imports the return type references of a CallSite into a module.

### Import Instruction References
```C#
ImportReferences(this Instruction instruction, ModuleDefinition module)
```
This method imports the operand type references of an instruction into a module.



## SwapMethods Extension Methods
The `MonoCecilExtensions` class provides several extension methods for swapping method implementations between different types using `Mono.Cecil`. These methods can be used when you want to replace method functionality in the destination type with the corresponding functionality from the source type.

### Swap Method References within an Instruction
```C#
SwapMethodReferences(this Instruction instruction, MethodDefinition leftMethod, MethodDefinition rightMethod)
```
This method swaps the method references within the provided instruction between two given methods.

### Swap Method References within a Collection of Instructions
```C#
SwapMethodReferences(this Collection<Instruction> instructions, MethodDefinition leftMethod, MethodDefinition rightMethod)
```
This method swaps the method references within the provided collection of instructions between two given methods.

### Swap Method References within a Method's Body
```C#
SwapMethodReferences(this MethodDefinition method, MethodDefinition leftMethod, MethodDefinition rightMethod)
```
This method swaps the method references within the body of the provided method between two given methods.

### Swap Attributes, Parameters, Custom Attributes, and Generic Parameters between Two Methods
```C#
SwapMethods(this MethodDefinition leftMethod, MethodDefinition rightMethod)
```
This method swaps the attributes, parameters, custom attributes, and generic parameters between two given methods.

### Find and Swap Methods with the Same Full Name within a Type
```C#
SwapDuplicateMethods(this TypeDefinition type, bool avoidSignatureConflicts = false)
```
This method finds and swaps methods with the same full name within the given type. It also provides an option to avoid signature conflicts by changing original method parameters to be base object types.



## InstructionOptimizations Extension Methods
The `MonoCecilExtensions` class also offers methods that help with instruction optimizations when using `Mono.Cecil`.

### Determine if an Instruction can be Optimized Out
```C#
CanBeOptimizedOut(this Instruction instruction, MethodDefinition method)
```
This method checks if a given instruction within a method can be optimized out. Specifically, it looks for type conversion instructions (Isinst or Castclass) that are unnecessary because the type of the value at the top of the stack is already the target conversion type. It returns true if the instruction can be optimized out, otherwise it returns false.

### Optimize Instructions in a Method
```C#
OptimizeInstructions(this MethodDefinition method)
```
This method optimizes a given method by removing any instructions that can be optimized out. It works by checking each instruction within the method and removing it if it can be optimized out, based on the criteria set in the `CanBeOptimizedOut()` method.


