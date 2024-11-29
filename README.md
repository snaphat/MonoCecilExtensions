# MonoCecilExtensions

## Static Class
> [!IMPORTANT]
> The `MonoCecilExtensions` is a static class that provides extension methods for classes from the Mono.Cecil library. Mono.Cecil is a popular library used for reading and writing Intermediate Language (IL) code. With the added functionality of `MonoCecilExtensions`, manipulation of IL code becomes even more convenient, enabling users to easily clone, merge, and update types in collections, methods, fields, and other components of a .NET assembly.
```csharp
public static class MonoCecilExtensions
```
**Summary:**
Provides extension methods for classes from the Mono.Cecil library,  a library for reading and writing Intermediate Language (IL) code.
            These extensions facilitate manipulation of IL code,  providing functionality to clone,  merge,  and update types in collections,  methods,  fields,  and other components of a .NET assembly.

## Static Properties

### <ins>UpdateInfo</ins>
> [!NOTE]
> Nested within the `MonoCecilExtensions` class, the `UpdateInfo` class serves as an information container for updating Mono.Cecil definitions. It keeps track of all modifications made to the objects of the Mono.Cecil library during the manipulation process. The class includes the following properties:
>
> - `updatedAttributes`: A collection of `CustomAttribute` objects that have been updated.
> - `updatedInterfaces`: A collection of `InterfaceImplementation` objects that have been updated.
> - `updatedFields`: A collection of `FieldDefinition` objects that have been updated.
> - `updatedProperties`: A collection of `PropertyDefinition` objects that have been updated.
> - `updatedMethods`: A collection of `MethodDefinition` objects that have been updated.
> - `srcTypes`: A collection of source `TypeDefinition` objects that are being merged.
> - `destTypes`: A collection of destination `TypeDefinition` objects where source objects are merged into.

```csharp
public class UpdateInfo
```
**Summary:**
Represents an information container for updating Mono.Cecil definitions.
<!----->

#### <ins>UpdateInfo.updatedAttributes</ins>
```csharp
public readonly Collection<CustomAttribute> updatedAttributes
```
**Summary:**
A collection of CustomAttribute objects that have been updated.
<!----->

#### <ins>UpdateInfo.updatedInterfaces</ins>
```csharp
public readonly Collection<InterfaceImplementation> updatedInterfaces
```
**Summary:**
A collection of InterfaceImplementation objects that have been updated.
<!----->

#### <ins>UpdateInfo.updatedFields</ins>
```csharp
public readonly Collection<FieldDefinition> updatedFields
```
**Summary:**
A collection of FieldDefinition objects that have been updated.
<!----->

#### <ins>UpdateInfo.updatedProperties</ins>
```csharp
ublic readonly Collection<PropertyDefinition> updatedProperties
```
**Summary:**
A collection of PropertyDefinition objects that have been updated.
<!----->

#### <ins>UpdateInfo.updatedMethods</ins>
```csharp
public readonly Collection<MethodDefinition> updatedMethods
```
**Summary:**
A collection of MethodDefinition objects that have been updated.
<!----->

#### <ins>UpdateInfo.srcTypes</ins>
```csharp
public readonly Collection<TypeDefinition> srcTypes
```
**Summary:**
A collection of source TypeDefinition objects that are being merged.
<!----->

#### <ins>UpdateInfo.destTypes</ins>
```csharp
public readonly Collection<TypeDefinition> destTypes
```
**Summary:**
A collection of destination TypeDefinition objects where source objects are merged into.
<!----->

### <ins>assemblyUpdateInfo</ins>
> [!NOTE]
> The `assemblyUpdateInfo` is a static readonly property within the `MonoCecilExtensions` class. It's a dictionary mapping from `AssemblyDefinition` objects to their corresponding `UpdateInfo` objects, making it an essential tool for keeping track of the updates made to each assembly.
```csharp
public static readonly Dictionary<AssemblyDefinition, UpdateInfo> assemblyUpdateInfo
```
**Summary:**
A dictionary mapping from AssemblyDefinition objects to their corresponding UpdateInfo objects.
            Used to keep track of the updates made to each assembly.
<!----->

### <ins>additionalSearchDirectories</ins>
> [!NOTE]
> `MonoCecilExtensions` also includes a static readonly property `additionalSearchDirectories`, which is a collection of string values specifying extra directories to be included in the search when resolving assembly types.
>
> More details about other methods and functionalities of the `MonoCecilExtensions` class are provided in the following sections of the document.
```csharp
public static readonly Collection<string> additionalSearchDirectories
```
**Summary:**
Additional search directories for resolving assembly types.
<!----->

---

## Base Extension Methods
> [!TIP]
> The `MonoCecilExtensions` class provides a set of basic extension methods that add simplicity and convenience to various operations in the Mono.Cecil library.
> These methods streamline operations such as loading assemblies, adding elements to collections, and finding types, fields, and methods in Mono.Cecil objects.

### <ins>LoadAssembly</ins>
```csharp
public static AssemblyDefinition LoadAssembly(this string location, bool readWrite = false)
```
**Summary:**
This extension method loads an assembly from a given location.

**Parameters:**
- `location`: The location of the assembly to be loaded.
- `readWrite`: A boolean value to determine if the assembly is read-only or writable.

**Returns:** The AssemblyDefinition object of the loaded assembly if successful
<!----->

### <ins>FindMethodOfType</ins>
```csharp
public static MethodDefinition FindMethodOfType(this AssemblyDefinition assembly, string typeSignature, string methodSignature)
```
**Summary:**
This extension method finds a method of a given type in an assembly.

**Parameters:**
- `assembly`: The assembly where the type and method are located.
- `typeSignature`: The full or simple name of the type.
- `methodSignature`: The full or simple name of the method.

**Returns:** The MethodDefinition object of the found method. Null if not found.
<!----->

### <ins>FindType(..., string)</ins>
```csharp
public static TypeDefinition FindType(this AssemblyDefinition assembly, string typeSignature)
```
**Summary:**
This extension method finds a type in an assembly using its full name or simple name.

**Parameters:**
- `assembly`: The assembly where the type is located.
- `typeSignature`: The full or simple name of the type.

**Returns:** The TypeDefinition object of the found type. Null if not found.
<!----->

### <ins>FindType(..., Type)</ins>
```csharp
public static TypeDefinition FindType(this AssemblyDefinition assembly, Type type)
```
**Summary:**
This extension method finds a type in an assembly using its full name or simple name.

**Parameters:**
- `assembly`: The assembly where the type is located.
- `type`: The type to locate.

**Returns:** The TypeDefinition object of the found type. Null if not found.
<!----->

### <ins>FindField</ins>
```csharp
public static FieldDefinition FindField(TypeDefinition type, string fieldSignature)
```
**Summary:**
This extension method finds a field in a type.

**Parameters:**
- `type`: The type where the field is located.
- `fieldSignature`: The full or simple name of the field.

**Returns:** The FieldDefinition object of the found field. Null if not found.
<!----->

### <ins>FindMethod</ins>
```csharp
public static MethodDefinition FindMethod(this TypeDefinition type, string methodSignature)
```
**Summary:**
This extension method finds a method in a type.

**Parameters:**
- `type`: The type where the method is located.
- `methodSignature`: The full or simple name of the method.

**Returns:** The MethodDefinition object of the found method. Null if not found.
<!----->

### <ins>FindMethods</ins>
```csharp
public static Collection<MethodDefinition> FindMethods(this TypeDefinition type, string methodSignature)
```
**Summary:**
This extension method finds all methods in a type that match a given method signature.

**Parameters:**
- `type`: The type where the methods are located.
- `methodSignature`: The full or simple name of the methods.

**Returns:** A collection of MethodDefinition objects for the found methods. Empty collection if none found.

**Returns:** void.
<!----->

## AddType Extension Method
> [!TIP]
> The `MonoCecilExtensions` class offers a method that simplifies adding types to an assembly when using `Mono.Cecil`.

### <ins>AddType</ins>
```csharp
public static void AddType(this AssemblyDefinition assembly, TypeDefinition src, bool avoidSignatureConflicts)
```
**Summary:**
Adds a type to an assembly. This includes adding the type's fields,  properties,  and methods.
            If the source type is nested,  it will be added as a nested type within the parent type in the destination assembly.

**Parameters:**
- `assembly`: The assembly to which the type will be added.
- `src`: The source type that will be added to the assembly.
- `avoidSignatureConflicts`: Avoid name conflicts by adding a '_' suffix to the copied class name.

**Returns:** void.
<!----->

## AddFieldsPropertiesAndMethods Extension Method
> [!TIP]
> The `MonoCecilExtensions` class provides an extension method that handles the addition of fields, properties, and methods from a source type to a destination type.
> This is a key part of merging two types, ensuring the destination type includes all necessary components from the source type.
### <ins>AddFieldsPropertiesAndMethods</ins>
```csharp
public static void AddFieldsPropertiesAndMethods(this TypeDefinition dest, TypeDefinition src)
```
**Summary:**
Merges the source type into the destination type by cloning the fields,  properties,  and methods of the source,  updating their types and adding them to the destination.

**Parameters:**
- `dest`: The destination type definition where fields,  properties,  and methods from source will be added.
- `src`: The source type definition whose fields,  properties,  and methods will be cloned and added to the destination.

**Returns:** void.
<!----->

## UpdateFieldsPropertiesAndMethods Extension Method
> [!TIP]
> The `MonoCecilExtensions` class provides an extension method that handle the updating of fields, properties, and methods within a destination type after they have been cloned from a source type.
> These methods ensure that the newly added components in the destination type correctly reference the destination type, rather than the original source type.
### <ins>UpdateFieldsPropertiesAndMethods</ins>
```csharp
public static void UpdateFieldsPropertiesAndMethods(this AssemblyDefinition assembly, bool avoidSignatureConflicts)
```
**Summary:**
Updates the types of attributes,  interfaces,  fields,  properties,  and methods within a given assembly.
            This includes updating the types in interfaces,  fields,  properties,  and methods. It also updates the getter and setter methods for properties,
            updates the instruction types for methods,  imports references for attributes,  interfaces,  fields,  properties,  and methods,
            imports base types of each destination type,  and swaps any duplicate methods in the destination types.

**Parameters:**
- `assembly`: The assembly to be updated. This assembly's types are matched against the source types and replaced with the corresponding destination types,  based on previously registered update information.
- `avoidSignatureConflicts`: Avoid signature conflicts by changing original method parameters to be base object types for duplicate methods

**Returns:** void.
<!----->

---

# Additional Internal APIs

## Clone Extension Methods
> [!TIP]
> The `MonoCecilExtensions` class also provides extension methods for cloning various Mono.Cecil objects.
> This is useful when you want to create a copy of an object without modifying the original object.
> The clone methods return a new object that is identical to the original but distinct in memory.

### <ins>Clone(CustomAttribute)</ins>
```csharp
public static CustomAttribute Clone(this CustomAttribute attribute)
```
**Summary:**
Clones a CustomAttribute.

**Parameters:**
- `attribute`: The attribute to be cloned.

**Returns:** A clone of the original attribute.
<!----->

### <ins>Clone(InterfaceImplementation)</ins>
```csharp
public static InterfaceImplementation Clone(this InterfaceImplementation interface)
```
**Summary:**
Clones a InterfaceImplementation.

**Parameters:**
- `interface`: The interface to be cloned.

**Returns:** A clone of the original interface.
<!----->

### <ins>Clone(FieldDefinition)</ins>
```csharp
public static FieldDefinition Clone(this FieldDefinition field)
```
**Summary:**
Clones a FieldDefinition.

**Parameters:**
- `field`: The field to be cloned.

**Returns:** A clone of the original field.
<!----->

### <ins>Clone(PropertyDefinition)</ins>
```csharp
public static PropertyDefinition Clone(this PropertyDefinition property)
```
**Summary:**
Clones a PropertyDefinition.

**Parameters:**
- `property`: The property to be cloned.

**Returns:** A clone of the original property.
<!----->

### <ins>Clone(ParameterDefinition)</ins>
```csharp
public static ParameterDefinition Clone(this ParameterDefinition parameter)
```
**Summary:**
Clones a ParameterDefinition.

**Parameters:**
- `parameter`: The parameter to be cloned.

**Returns:** A clone of the original parameter.
<!----->

### <ins>Clone(VariableDefinition)</ins>
```csharp
public static VariableDefinition Clone(this VariableDefinition variable)
```
**Summary:**
Clones a VariableDefinition.

**Parameters:**
- `variable`: The variable to be cloned.

**Returns:** A clone of the original variable.
<!----->

### <ins>Clone(Instruction)</ins>
```csharp
public static Collection<Instruction> Clone(this Collection<Instruction> instructions)
```
**Summary:**
Clones an Instruction.

**Parameters:**
- `instruction`: The instruction to be cloned.

**Returns:** A clone of the original instruction.
<!----->

### <ins>Clone(Collection\<Instruction\>)</ins>
```csharp
public static Collection<Instruction> Clone(this Collection<Instruction> instructions)
```
**Summary:**
Clones all instructions in the collection.

**Parameters:**
- `instructions`: The collection of instructions to be cloned.

**Returns:** A new collection containing clones of the original instructions.
<!----->

### <ins>Clone(MethodDefinition)</ins>
```csharp
public static MethodDefinition Clone(this MethodDefinition method)
```
**Summary:**
Clones a MethodDefinition.

**Parameters:**
- `method`: The method to be cloned.

**Returns:** A clone of the original method.
<!----->

---

## UpdateTypes Extension Methods
> [!TIP]
> The `MonoCecilExtensions` class also provides extension methods for replacing references to a source type with references to a destination type within Mono.Cecil objects.
> These methods ensure that copied fields, properties, and methods reference the copied types instead of the originals.

### <ins>UpdateTypes(InterfaceImplementation, ..., ...)</ins>
```csharp
public static void UpdateTypes(this InterfaceImplementation @interface, TypeDefinition src, TypeDefinition dest)
```
**Summary:**
Updates the InterfaceType of the given InterfaceImplementation,  if it matches the source type,  to the destination type.

**Parameters:**
- `interface`: InterfaceImplementation that may have its InterfaceType updated.
- `src`: The source type which could be replaced.
- `dest`: The destination type which could replace the source type.

**Returns:** void.
<!----->

### <ins>UpdateTypes(FieldDefinition, ..., ...)</ins>
```csharp
public static void UpdateTypes(this FieldDefinition field, TypeDefinition src, TypeDefinition dest)
```
**Summary:**
Updates the FieldType of the given FieldDefinition,  if it matches the source type,  to the destination type.

**Parameters:**
- `field`: FieldDefinition that may have its FieldType updated.
- `src`: The source type which could be replaced.
- `dest`: The destination type which could replace the source type.

**Returns:** void.
<!----->

### <ins>UpdateTypes(FieldReference, ..., ...)</ins>
```csharp
public static FieldReference UpdateTypes(this FieldReference field, TypeDefinition src, TypeDefinition dest)
```
**Summary:**
Updates the FieldReference and DeclaringType of the given FieldReference,  if they match the source type,  to the destination type.
            If a matching field definition is found in the destination type,  a reference to it is returned.
            Otherwise,  the original field reference is returned.

**Parameters:**
- `field`: FieldReference that may have its FieldType,  and DeclaringType updated.
- `src`: The source type which could be replaced.
- `dest`: The destination type which could replace the source type.

**Returns:** A FieldReference with updated types,  or the original FieldReference if no updates were made.
<!----->

### <ins>UpdateTypes(PropertyDefinition, ..., ...)</ins>
```csharp
public static void UpdateTypes(this PropertyDefinition property, TypeDefinition src, TypeDefinition dest)
```
**Summary:**
Updates the PropertyType of the given PropertyDefinition,  if it matches the source type,  to the destination type.

**Parameters:**
- `property`: PropertyDefinition that may have its PropertyType updated.
- `src`: The source type which could be replaced.
- `dest`: The destination type which could replace the source type.

**Returns:** void.
<!----->

### <ins>UpdateTypes(ParameterDefinition, ..., ...)</ins>
```csharp
public static void UpdateTypes(this ParameterDefinition parameter, TypeDefinition src, TypeDefinition dest)
```
**Summary:**
Updates the ParameterType of the given ParameterDefinition,  if it matches the source type,  to the destination type.

**Parameters:**
- `parameter`: ParameterDefinition that may have its ParameterType updated.
- `src`: The source type which could be replaced.
- `dest`: The destination type which could replace the source type.

**Returns:** void.
<!----->

### <ins>UpdateTypes(VariableDefinition, ..., ...)</ins>
```csharp
public static void UpdateTypes(this VariableDefinition variable, TypeDefinition src, TypeDefinition dest)
```
**Summary:**
Updates the VariableType of the given VariableDefinition,  if it matches the source type,  to the destination type.

**Parameters:**
- `variable`: VariableDefinition that may have its VariableType updated.
- `src`: The source type which could be replaced.
- `dest`: The destination type which could replace the source type.

**Returns:** void.
<!----->

### <ins>UpdateTypes(MethodDefinition, ..., ...)</ins>
```csharp
public static void UpdateTypes(this MethodDefinition method, TypeDefinition src, TypeDefinition dest)
```
**Summary:**
Updates the ReturnType of the given MethodDefinition,  if it matches the source type,  to the destination type.
            Also updates ParameterTypes and VariableTypes of the MethodDefinition using the same rule.

**Parameters:**
- `method`: MethodDefinition that may have its ReturnType,  ParameterTypes,  and VariableTypes updated.
- `src`: The source type which could be replaced.
- `dest`: The destination type which could replace the source type.

**Returns:** void.
<!----->

### <ins>UpdateTypes(MethodReference, ..., ...)</ins>
```csharp
public static MethodReference UpdateTypes(this MethodReference method, TypeDefinition src, TypeDefinition dest)
```
**Summary:**
Updates the ReturnType and DeclaringType of the given MethodReference,  if they match the source type,  to the destination type.
            Also updates the ParameterTypes of the MethodReference using the same rule.
            If a matching method definition is found in the destination type,  a reference to it is returned.
            Otherwise,  the original method reference is returned.

**Parameters:**
- `method`: MethodReference that may have its ReturnType,  DeclaringType and ParameterTypes updated.
- `src`: The source type which could be replaced.
- `dest`: The destination type which could replace the source type.

**Returns:** A MethodReference with updated types,  or the original MethodReference if no updates were made.
<!----->

### <ins>UpdateTypes(CallSite, ..., ...)</ins>
```csharp
public static void UpdateTypes(this CallSite callSite, TypeDefinition src, TypeDefinition dest)
```
**Summary:**
Updates the ReturnType and Parameters of the CallSite to the destination type,  if they match the source type,  to the destination type.

**Parameters:**
- `callSite`: CallSite that needs its return type and parameters updated.
- `src`: The original type which is being replaced.
- `dest`: The new type which is replacing the original type.

**Returns:** void.
<!----->

## UpdateInstructionTypes Extension Methods
> [!TIP]
> The `MonoCecilExtensions` class also provides extension methods for replacing references to a source type with references to a destination type within Mono.Cecil.Instruction objects.
> These methods are crucial for ensuring that the instructions within methods correctly reference the fields, properties, and methods of the destination type after cloning from the source type.


### <ins>UpdateInstructionTypes(Instruction, ..., ...)</ins>
```csharp
public static void UpdateInstructionTypes(this Instruction instruction, TypeDefinition src, TypeDefinition dest)
```
**Summary:**
Updates the Operand of an instruction when merging classes.
            The update strategy depends on the type of the operand.
            If the operand is a ParameterDefinition,  VariableDefinition,  FieldReference,  MethodReference,  CallSite,  or TypeReference,  it's updated accordingly.

**Parameters:**
- `instruction`: Instruction that needs its operand updated.
- `src`: The original type which is being replaced.
- `dest`: The new type which is replacing the original type.

**Returns:** void.
<!----->

### <ins>UpdateInstructionTypes(MethodDefinition, ..., ...)</ins>
```csharp
public static void UpdateInstructionTypes(this MethodDefinition method, TypeDefinition src, TypeDefinition dest)
```
**Summary:**
Updates all instructions in the method's body.
            If the instruction's operand type matches the source type, it is replaced with the destination type.

**Parameters:**
- `method`: Method whose instructions are to be updated.
- `src`: The original type which is being replaced.
- `dest`: The new type which is replacing the original type.

**Returns:** void.
<!----->

## UpdateGettersAndSetters Extension Method
> [!TIP]
> The `MonoCecilExtensions` class provides an extension method for replacing references to a source type with references to a destination type within Mono.Cecil.Property getter and setter methods.
> This method ensures that the properties of the destination type reference copied getters and setters instead of the originals.


### <ins>UpdateGettersAndSetters</ins>
```csharp
public static void UpdateGettersAndSetters(this PropertyDefinition property, TypeDefinition src, TypeDefinition dest)
```
**Summary:**
Updates the getter and setter methods of a property to reference the destination type when merging classes.
            This method does the following:
                - Clones the existing getter/setter methods,  so that any modifications do not affect the original methods
                - Calls UpdateTypes to update all type references within the methods' bodies from src to dest
                - Updates the declaring type of the methods to be dest
                - Finds the equivalent methods in dest (if they exist),  and updates the property's getter/setter methods to reference them
            This ensures that the property correctly interacts with the destination type after merging.

**Parameters:**
- `property`: PropertyDefinition whose getter and setter need to be updated.
- `src`: The original type which is being replaced.
- `dest`: The new type which is replacing the original type.

**Returns:** void.
<!----->
---


## ImportReferences Extension Methods
> [!TIP]
> The `MonoCecilExtensions` class provides several extension methods for importing references from one module to another using `Mono.Cecil`.
> These methods are crucial when merging assembly classes as they allow the destination type to access types that may not have been referenced prior.

### <ins>ImportReferences(CustomAttribute, ...)</ins>
```csharp
public static void ImportReferences(this CustomAttribute attribute, ModuleDefinition module)
```
**Summary:**
Imports the constructor reference for a given attribute into a module.

**Parameters:**
- `attribute`: The custom attribute whose constructor reference needs to be imported.
- `module`: The module type into whose module the reference should be imported.

**Returns:** void.
<!----->

### <ins>ImportReferences(InterfaceImplementation, ...)</ins>
```csharp
public static void ImportReferences(this InterfaceImplementation interface, ModuleDefinition module)
```
**Summary:**
Imports the interface type and custom attributes references of an interface into a module.

**Parameters:**
- `interface`: The interface whose references need to be imported.
- `module`: The module type into whose module the references should be imported.

**Returns:** void.
<!----->

### <ins>ImportReferences(FieldDefinition, ...)</ins>
```csharp
public static void ImportReferences(this FieldDefinition field, ModuleDefinition module)
```
**Summary:**
Imports the field type and custom attributes references of a field into a module.

**Parameters:**
- `field`: The field whose references need to be imported.
- `module`: The module type into whose module the references should be imported.

**Returns:** void.
<!----->

### <ins>ImportReferences(PropertyDefinition, ...)</ins>
```csharp
public static void ImportReferences(this PropertyDefinition property, ModuleDefinition module)
```
**Summary:**
Imports the property type and custom attributes references of a property into a module.

**Parameters:**
- `property`: The property whose references need to be imported.
- `module`: The module type into whose module the references should be imported.

**Returns:** void.
<!----->

### <ins>ImportReferences(ParameterDefinition, ...)</ins>
```csharp
public static void ImportReferences(this ParameterDefinition parameter, ModuleDefinition module)
```
**Summary:**
Imports the parameter type and custom attributes references of a parameter into a module.

**Parameters:**
- `parameter`: The parameter whose references need to be imported.
- `module`: The module type into whose module the references should be imported.

**Returns:** void.
<!----->

### <ins>ImportReferences(VariableDefinition, ...)</ins>
```csharp
public static void ImportReferences(this VariableDefinition variable, ModuleDefinition module)
```
**Summary:**
Imports the variable type references of a variable into a module.

**Parameters:**
- `variable`: The variable whose type references need to be imported.
- `module`: The module type into whose module the references should be imported.

**Returns:** void.
<!----->

### <ins>ImportReferences(MethodDefinition, ...)</ins>
```csharp
public static void ImportReferences(this MethodDefinition method, ModuleDefinition module)
```
**Summary:**
Imports the method type references and the custom attributes of a method into a module.

**Parameters:**
- `method`: The method whose references need to be imported.
- `module`: The module type into whose module the references should be imported.

**Returns:** void.
<!----->

### <ins>ImportReferences(CallSite, ...)</ins>
```csharp
public static void ImportReferences(this CallSite callSite, ModuleDefinition module)
```
**Summary:**
Imports the return type references of a CallSite into a module.

**Parameters:**
- `callSite`: The CallSite whose return type references need to be imported.
- `module`: The module type into whose module the references should be imported.

**Returns:** void.
<!----->
### <ins>ImportReferences(Instruction, ...)</ins>
```csharp
public static void ImportReferences(this Instruction instruction, ModuleDefinition module)
```
**Summary:**
Imports the operand type references of an instruction into a module.

**Parameters:**
- `instruction`: The instruction whose operand references need to be imported.
- `module`: The module type into whose module the references should be imported.

**Returns:** void.
<!----->

---

## SwapMethods Extension Methods
> [!TIP]
> The `MonoCecilExtensions` class provides several extension methods for swapping method implementations between different types using `Mono.Cecil`.
> These methods can be used when you want to replace method functionality in the destination type with the corresponding functionality from the source type.

### <ins>SwapMethodReferences(Instruction, ..., ...)</ins>
```csharp
public static void SwapMethodReferences(this Instruction instruction, MethodDefinition leftMethod, MethodDefinition rightMethod)
```
**Summary:**
Swaps the method references within the provided instruction between two given methods.

**Parameters:**
- `instruction`: The instruction to modify.
- `leftMethod`: The first method to swap.
- `rightMethod`: The second method to swap.

**Returns:** void.
<!----->

### <ins>SwapMethodReferences(Collection\<Instruction\>, ..., ...)</ins>
```csharp
public static void SwapMethodReferences(this Collection<Instruction> instructions, MethodDefinition leftMethod, MethodDefinition rightMethod)
```
**Summary:**
Swaps the method references within the provided collection of instructions between two given methods.

**Parameters:**
- `instructions`: The collection of instructions to modify.
- `leftMethod`: The first method to swap.
- `rightMethod`: The second method to swap.

**Returns:** void.
<!----->

### <ins>SwapMethodReferences(Mono.Cecil.MethodDefinition, ..., ...)</ins>
```csharp
public static void SwapMethodReferences(this MethodDefinition method, MethodDefinition leftMethod, MethodDefinition rightMethod)
```
**Summary:**
Swaps the method references within the body of the provided method between two given methods.

**Parameters:**
- `method`: The method to modify.
- `leftMethod`: The first method to swap.
- `rightMethod`: The second method to swap.

**Returns:** void.
<!----->

### <ins>SwapMethods</ins>
```csharp
public static void SwapMethods(this MethodDefinition leftMethod, MethodDefinition rightMethod)
```
**Summary:**
Swaps the attributes,  parameters,  custom attributes,  and generic parameters between two given methods.

**Parameters:**
- `leftMethod`: The first method to swap.
- `rightMethod`: The second method to swap.

**Returns:** void.
<!----->

### <ins>SwapDuplicateMethods</ins>
```csharp
public static void SwapDuplicateMethods(this TypeDefinition type, bool avoidSignatureConflicts)
```
**Summary:**
Finds and swaps methods with the same full name within the given type.

**Parameters:**
- `type`: The type to modify.
- `avoidSignatureConflicts`: Avoid signature conflicts by changing original method parameters to be base object types

**Returns:** void.
<!----->

---

## InstructionOptimizations Extension Methods
> [!TIP]
> `MonoCecilExtensions` class also offers methods that help with instruction optimizations when using `Mono.Cecil`.

**Returns:** void.
<!----->

### <ins>CanBeOptimizedOut</ins>
```csharp
public static bool CanBeOptimizedOut(this Instruction instruction, MethodDefinition method)
```
**Summary:**
Determines if a given instruction within a method can be optimized out.
            Specifically,  this method looks for type conversion instructions (Isinst or Castclass)
            that are unnecessary because the type of the value at the top of the stack is
            already the target conversion type.

**Parameters:**
- `instruction`: The instruction to be checked for optimization.
- `method`: The method definition that contains the instruction.

**Returns:** Returns true if the instruction can be optimized out. Otherwise,  returns false.

**Remarks:** This method works by examining the instructions before the given instruction in the method,
            maintaining a conceptual "stack balance" and tracking the type of the value at the top of the stack.
            The stack balance is a measure of the net effect of the instructions on the stack,
            with a positive balance indicating more values have been pushed than popped,
            and a negative balance indicating more values have been popped than pushed.
            If the stack balance is zero and the type of the value at the top of the stack
            matches the type conversion,  the conversion is unnecessary and the method returns true.
<!----->

### <ins>OptimizeInstructions</ins>
```csharp
public static void OptimizeInstructions(this MethodDefinition method)
```
**Summary:**
Optimizes a given method by removing any instructions
            that can be optimized out.

**Parameters:**
- `method`: The MethodDefinition object to be optimized. This method contains a list
            of instructions that are to be checked and potentially removed if they can be optimized out.

**Returns:** void.
<!----->
