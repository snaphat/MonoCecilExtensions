#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using MethodBody = Mono.Cecil.Cil.MethodBody;

/// <summary>
/// Provides extension methods for classes from the Mono.Cecil library, a library for reading and writing Intermediate Language (IL) code.
/// These extensions facilitate manipulation of IL code, providing functionality to clone, merge, and update types in collections, methods, fields, and other components of a .NET assembly.
/// </summary>
public static class MonoCecilExtensions
{
    /// <summary>
    /// Represents an information container for updating Mono.Cecil definitions.
    /// </summary>
    private class UpdateInfo
    {
        /// <summary>
        /// A collection of CustomAttribute objects that have been updated.
        /// </summary>
        internal readonly Collection<CustomAttribute> updatedAttributes = new();

        /// <summary>
        /// A collection of InterfaceImplementation objects that have been updated.
        /// </summary>
        internal readonly Collection<InterfaceImplementation> updatedInterfaces = new();

        /// <summary>
        /// A collection of FieldDefinition objects that have been updated.
        /// </summary>
        internal readonly Collection<FieldDefinition> updatedFields = new();

        /// <summary>
        /// A collection of PropertyDefinition objects that have been updated.
        /// </summary>
        internal readonly Collection<PropertyDefinition> updatedProperties = new();

        /// <summary>
        /// A collection of MethodDefinition objects that have been updated.
        /// </summary>
        internal readonly Collection<MethodDefinition> updatedMethods = new();

        /// <summary>
        /// A collection of source TypeDefinition objects that are being merged.
        /// </summary>
        internal readonly Collection<TypeDefinition> srcTypes = new();

        /// <summary>
        /// A collection of destination TypeDefinition objects where source objects are merged into.
        /// </summary>
        internal readonly Collection<TypeDefinition> destTypes = new();
    };

    /// <summary>
    /// A dictionary mapping from AssemblyDefinition objects to their corresponding UpdateInfo objects.
    /// Used to keep track of the updates made to each assembly.
    /// </summary>
    private static readonly Dictionary<AssemblyDefinition, UpdateInfo> assemblyUpdateInfo = new();

    public static readonly Collection<string> additionalSearchDirectories = new();

    // Basic extension methods for loading assemblies, adding elements to collections, and finding types, fields, and methods in Mono.Cecil objects.
    #region Base

    /// <summary>
    /// This extension method loads an assembly from a given location.
    /// </summary>
    /// <param name="location">The location of the assembly to be loaded.</param>
    /// <param name="readWrite">A boolean value to determine if the assembly is read-only or writable.</param>
    /// <returns>The AssemblyDefinition object of the loaded assembly if successful</returns>
    public static AssemblyDefinition LoadAssembly(this string location, bool readWrite = false)
    {
        // Create a new instance of the DefaultAssemblyResolver.
        var resolver = new DefaultAssemblyResolver();

        // Add search directories to the resolver.
        foreach (var directory in additionalSearchDirectories)
            resolver.AddSearchDirectory(directory);
        resolver.AddSearchDirectory(Path.GetDirectoryName(typeof(int).Assembly.Location));
        resolver.AddSearchDirectory(Path.Combine(Path.GetDirectoryName(typeof(int).Assembly.Location), "Facades"));

        // Read and return the assembly using the provided location and reader parameters.
        return AssemblyDefinition.ReadAssembly(location, new ReaderParameters()
        {
            AssemblyResolver = resolver,
            ReadWrite = readWrite,
        });
    }

    /// <summary>
    /// This extension method finds a method of a given type in an assembly.
    /// </summary>
    /// <param name="assembly">The assembly where the type and method are located.</param>
    /// <param name="typeSignature">The full or simple name of the type.</param>
    /// <param name="methodSignature">The full or simple name of the method.</param>
    /// <returns>The MethodDefinition object of the found method. Null if not found.</returns>
    public static MethodDefinition FindMethodOfType(this AssemblyDefinition assembly, string typeSignature, string methodSignature)
    {
        // Find and return the method of the given type in the assembly.
        return assembly.FindType(typeSignature)?.FindMethod(methodSignature);
    }

    /// <summary>
    /// This extension method finds a type in an assembly using its full name or simple name.
    /// </summary>
    /// <param name="assembly">The assembly where the type is located.</param>
    /// <param name="typeSignature">The full or simple name of the type.</param>
    /// <returns>The TypeDefinition object of the found type. Null if not found.</returns>
    public static TypeDefinition FindType(this AssemblyDefinition assembly, string typeSignature)
    {
        // Return the first type that matches the provided type signature.
        return assembly.MainModule.Types.FirstOrDefault(type => type.FullName == typeSignature || type.Name == typeSignature);
    }

    /// <summary>
    /// This extension method finds a type in an assembly using its full name or simple name.
    /// </summary>
    /// <param name="assembly">The assembly where the type is located.</param>
    /// <param name="type">The type to locate.</param>
    /// <returns>The TypeDefinition object of the found type. Null if not found.</returns>
    public static TypeDefinition FindType(this AssemblyDefinition assembly, Type type)
    {
        // Return the first type that matches the provided type signature.
        return assembly.MainModule.Types.FirstOrDefault(_type => _type.FullName == type.FullName || _type.Name == type.Name);
    }

    /// <summary>
    /// This extension method finds a field in a type.
    /// </summary>
    /// <param name="type">The type where the field is located.</param>
    /// <param name="fieldSignature">The full or simple name of the field.</param>
    /// <returns>The FieldDefinition object of the found field. Null if not found.</returns>
    public static FieldDefinition FindField(this TypeDefinition type, string fieldSignature)
    {
        // Return the first field that matches the provided field signature.
        return type.Fields.FirstOrDefault(m => m.FullName == fieldSignature || m.Name == fieldSignature);
    }

    /// <summary>
    /// This extension method finds a method in a type.
    /// </summary>
    /// <param name="type">The type where the method is located.</param>
    /// <param name="methodSignature">The full or simple name of the method.</param>
    /// <returns>The MethodDefinition object of the found method. Null if not found.</returns>
    public static MethodDefinition FindMethod(this TypeDefinition type, string methodSignature)
    {
        // The function checks each method in the type's Methods collection,
        // and returns the first method whose full name or simple name matches the provided method signature.
        return type.Methods.FirstOrDefault(m => m.FullName == methodSignature || m.Name == methodSignature);
    }

    /// <summary>
    /// This extension method finds all methods in a type that match a given method signature.
    /// </summary>
    /// <param name="type">The type where the methods are located.</param>
    /// <param name="methodSignature">The full or simple name of the methods.</param>
    /// <returns>A collection of MethodDefinition objects for the found methods. Empty collection if none found.</returns>
    public static Collection<MethodDefinition> FindMethods(this TypeDefinition type, string methodSignature)
    {
        var collection = new Collection<MethodDefinition>();

        // This function checks each method in the type's Methods collection, 
        // and adds those methods to the collection whose full name or simple name matches the provided method signature.
        foreach (var item in type.Methods.Where(m => m.FullName == methodSignature || m.Name == methodSignature))
            collection.Add(item);
        return collection;
    }

    #endregion Base

    // Extension methods for cloning various Mono.Cecil objects.
    #region Clone

    /// <summary>
    /// Clones a CustomAttribute.
    /// </summary>
    /// <param name="attribute">The attribute to be cloned.</param>
    /// <returns>A clone of the original attribute.</returns>
    public static CustomAttribute Clone(this CustomAttribute attribute)
    {
        // Create a new CustomAttribute with the constructor of the original attribute.
        var clonedAttribute = new CustomAttribute(attribute.Constructor);

        // Add all constructor arguments from the original attribute to the cloned attribute.
        foreach (var argument in attribute.ConstructorArguments) clonedAttribute.ConstructorArguments.Add(argument);

        // Add all properties from the original attribute to the cloned attribute.
        foreach (var property in attribute.Properties) clonedAttribute.Properties.Add(property);

        // Add all fields from the original attribute to the cloned attribute.
        foreach (var field in attribute.Fields) clonedAttribute.Fields.Add(field);

        // Return the cloned attribute.
        return clonedAttribute;
    }

    /// <summary>
    /// Clones a InterfaceImplementation.
    /// </summary>
    /// <param name="interface">The interface to be cloned.</param>
    /// <returns>A clone of the original interface.</returns>
    public static InterfaceImplementation Clone(this InterfaceImplementation @interface)
    {
        // Create a new InterfaceImplementation with the type the original interface.
        var clonedInterface = new InterfaceImplementation(@interface.InterfaceType);

        // Copy all custom attributes from the original interface to the cloned interface.
        foreach (var attribute in @interface.CustomAttributes) clonedInterface.CustomAttributes.Add(attribute.Clone());

        // Return the cloned interface.
        return clonedInterface;
    }

    /// <summary>
    /// Clones a FieldDefinition.
    /// </summary>
    /// <param name="field">The field to be cloned.</param>
    /// <returns>A clone of the original field.</returns>
    public static FieldDefinition Clone(this FieldDefinition field)
    {
        // Create a new FieldDefinition with the same properties as the original field.
        var clonedField = new FieldDefinition(field.Name, field.Attributes, field.FieldType);

        // Copy all custom attributes from the original field to the cloned field.
        foreach (var attribute in field.CustomAttributes) clonedField.CustomAttributes.Add(attribute.Clone());

        // Copy the MarshalInfo if it exists.
        clonedField.MarshalInfo = field.MarshalInfo != null ? new MarshalInfo(field.MarshalInfo.NativeType) : null;

        // Copy the initial value of the field.
        clonedField.InitialValue = field.InitialValue;

        // Return the cloned field.
        return clonedField;
    }

    /// <summary>
    /// Clones a PropertyDefinition.
    /// </summary>
    /// <param name="property">The property to be cloned.</param>
    /// <returns>A clone of the original property.</returns>
    public static PropertyDefinition Clone(this PropertyDefinition property)
    {
        // Create a new PropertyDefinition with the same properties as the original property.
        var clonedProperty = new PropertyDefinition(property.Name, property.Attributes, property.PropertyType);

        // Copy all custom attributes from the original property to the cloned property.
        foreach (var attribute in property.CustomAttributes) clonedProperty.CustomAttributes.Add(attribute.Clone());

        // Clone the get and set methods if they exist.
        clonedProperty.GetMethod = property.GetMethod?.Clone();
        clonedProperty.SetMethod = property.SetMethod?.Clone();

        // Return the cloned property.
        return clonedProperty;
    }

    /// <summary>
    /// Clones a ParameterDefinition.
    /// </summary>
    /// <param name="parameter">The parameter to be cloned.</param>
    /// <returns>A clone of the original parameter.</returns>
    public static ParameterDefinition Clone(this ParameterDefinition parameter)
    {
        // Create a new ParameterDefinition with the same properties as the original parameter.
        var clonedParameter = new ParameterDefinition(parameter.Name, parameter.Attributes, parameter.ParameterType);

        // Copy all custom attributes from the original parameter to the cloned parameter.
        foreach (var attribute in parameter.CustomAttributes) clonedParameter.CustomAttributes.Add(attribute.Clone());

        // Return the cloned parameter.
        return clonedParameter;
    }

    /// <summary>
    /// Clones a VariableDefinition.
    /// </summary>
    /// <param name="variable">The variable to be cloned.</param>
    /// <returns>A clone of the original variable.</returns>
    public static VariableDefinition Clone(this VariableDefinition variable)
    {
        // Create and return a new VariableDefinition with the same type as the original variable.
        return new VariableDefinition(variable.VariableType);
    }

    /// <summary>
    /// Clones an Instruction.
    /// </summary>
    /// <param name="instruction">The instruction to be cloned.</param>
    /// <returns>A clone of the original instruction.</returns>
    public static Instruction Clone(this Instruction instruction)
    {
        // Create a new Instruction with a default opcode.
        var clonedInstruction = Instruction.Create(OpCodes.Nop);

        // Copy the opcode and operand from the original instruction to the cloned instruction.
        clonedInstruction.OpCode = instruction.OpCode;
        clonedInstruction.Operand = instruction.Operand;

        // Return the cloned instruction.
        return clonedInstruction;
    }

    /// <summary>
    /// Clones a MethodDefinition.
    /// </summary>
    /// <param name="method">The method to be cloned.</param>
    /// <returns>A clone of the original method.</returns>
    public static MethodDefinition Clone(this MethodDefinition method)
    {
        // Create a new MethodDefinition with the same properties as the original method.
        var clonedMethod = new MethodDefinition(method.Name, method.Attributes, method.ReturnType)
        {
            ImplAttributes = method.ImplAttributes,
            SemanticsAttributes = method.SemanticsAttributes
        };

        // Add all overides from the original method to the cloned method (references).
        foreach (var @override in method.Overrides) clonedMethod.Overrides.Add(@override);

        // Copy all custom attributes from the original method to the cloned method.
        foreach (var attribute in method.CustomAttributes) clonedMethod.CustomAttributes.Add(attribute.Clone());

        // Clone all parameters and add them to the cloned method.
        foreach (var parameter in method.Parameters) clonedMethod.Parameters.Add(parameter.Clone());

        // Create a new method body for the cloned method.
        clonedMethod.Body = new MethodBody(clonedMethod);

        // If the original method has a body, copy the relevant properties to the cloned method's body.
        if (method.HasBody)
        {
            clonedMethod.Body.MaxStackSize = method.Body.MaxStackSize;
            clonedMethod.Body.InitLocals = method.Body.InitLocals;

            // Clone all instructions and variables and add them to the cloned method's body.
            foreach (var instruction in method.Body.Instructions) clonedMethod.Body.Instructions.Add(instruction.Clone());
            foreach (var variable in method.Body.Variables) clonedMethod.Body.Variables.Add(variable.Clone());
        }

        // Return the cloned method.
        return clonedMethod;
    }

    #endregion Clone

    // Extension methods for replacing references to a source type with references to a destination type within Mono.Cecil objects.
    // This is used to ensure that copied fields, properties, and methods reference copied types instead of the originals.
    #region UpdateTypes

    /// <summary>
    /// Updates the InterfaceType of the given InterfaceImplementation, if it matches the source type, to the destination type.
    /// </summary>
    /// <param name="interface">InterfaceImplementation that may have its InterfaceType updated.</param>
    /// <param name="src">The source type which could be replaced.</param>
    /// <param name="dest">The destination type which could replace the source type.</param>
    public static void UpdateTypes(this InterfaceImplementation @interface, TypeDefinition src, TypeDefinition dest)
    {
        // If the interface's type matches the source type, update it to the destination type
        if (@interface.InterfaceType == src) @interface.InterfaceType = dest;
    }

    /// <summary>
    /// Updates the FieldType of the given FieldDefinition, if it matches the source type, to the destination type.
    /// </summary>
    /// <param name="field">FieldDefinition that may have its FieldType updated.</param>
    /// <param name="src">The source type which could be replaced.</param>
    /// <param name="dest">The destination type which could replace the source type.</param>
    public static void UpdateTypes(this FieldDefinition field, TypeDefinition src, TypeDefinition dest)
    {
        // If the field's type matches the source type, update it to the destination type
        if (field.FieldType == src) field.FieldType = dest;
    }

    /// <summary>
    /// Updates the FieldReference and DeclaringType of the given FieldReference, if they match the source type, to the destination type.
    /// If a matching field definition is found in the destination type, a reference to it is returned.
    /// Otherwise, the original field reference is returned.
    /// </summary>
    /// <param name="field">FieldReference that may have its FieldType, and DeclaringType updated.</param>
    /// <param name="src">The source type which could be replaced.</param>
    /// <param name="dest">The destination type which could replace the source type.</param>
    /// <returns>A FieldReference with updated types, or the original FieldReference if no updates were made.</returns>
    public static FieldReference UpdateTypes(this FieldReference field, TypeDefinition src, TypeDefinition dest)
    {
        // Check if the field's FieldType or DeclaringType matches the source type, and if so, replace them with the destination type
        if (field.FieldType == src) field.FieldType = dest;
        if (field.DeclaringType == src) field.DeclaringType = dest;

        // Attempt to find a field in the destination type that matches the field's full name
        // If a matching definition is found, return a reference to it otherwise return original reference
        return dest.FindField(field.FullName) ?? field;
    }

    /// <summary>
    /// Updates the PropertyType of the given PropertyDefinition, if it matches the source type, to the destination type.
    /// </summary>
    /// <param name="property">PropertyDefinition that may have its PropertyType updated.</param>
    /// <param name="src">The source type which could be replaced.</param>
    /// <param name="dest">The destination type which could replace the source type.</param>
    public static void UpdateTypes(this PropertyDefinition property, TypeDefinition src, TypeDefinition dest)
    {
        // If the property's type matches the source type, update it to the destination type
        if (property.PropertyType == src) property.PropertyType = dest;
    }

    /// <summary>
    /// Updates the ParameterType of the given ParameterDefinition, if it matches the source type, to the destination type.
    /// </summary>
    /// <param name="parameter">ParameterDefinition that may have its ParameterType updated.</param>
    /// <param name="src">The source type which could be replaced.</param>
    /// <param name="dest">The destination type which could replace the source type.</param>
    public static void UpdateTypes(this ParameterDefinition parameter, TypeDefinition src, TypeDefinition dest)
    {

        // If the parameter's type matches the source type, update it to the destination type
        if (parameter.ParameterType == src) parameter.ParameterType = dest;
    }

    /// <summary>
    /// Updates the VariableType of the given VariableDefinition, if it matches the source type, to the destination type.
    /// </summary>
    /// <param name="variable">VariableDefinition that may have its VariableType updated.</param>
    /// <param name="src">The source type which could be replaced.</param>
    /// <param name="dest">The destination type which could replace the source type.</param>
    public static void UpdateTypes(this VariableDefinition variable, TypeDefinition src, TypeDefinition dest)
    {
        // If the variable's type matches the source type, update it to the destination type
        if (variable.VariableType == src) variable.VariableType = dest;
    }

    /// <summary>
    /// Updates the ReturnType of the given MethodDefinition, if it matches the source type, to the destination type.
    /// Also updates ParameterTypes and VariableTypes of the MethodDefinition using the same rule.
    /// </summary>
    /// <param name="method">MethodDefinition that may have its ReturnType, ParameterTypes, and VariableTypes updated.</param>
    /// <param name="src">The source type which could be replaced.</param>
    /// <param name="dest">The destination type which could replace the source type.</param>
    public static void UpdateTypes(this MethodDefinition method, TypeDefinition src, TypeDefinition dest)
    {
        // Update method overrides if they match the source type
        for (int i = 0; i < method.Overrides.Count; i++) method.Overrides[i] = method.Overrides[i].UpdateTypes(src, dest);

        // If the method's return type matches the source type, update it to the destination type
        if (method.ReturnType == src) method.ReturnType = dest;

        // Update method parameters and variables if they match the source type
        foreach (var parameter in method.Parameters) parameter.UpdateTypes(src, dest);
        if (method.HasBody) foreach (var variable in method.Body.Variables) variable.UpdateTypes(src, dest);
    }

    /// <summary>
    /// Updates the ReturnType and DeclaringType of the given MethodReference, if they match the source type, to the destination type.
    /// Also updates the ParameterTypes of the MethodReference using the same rule.
    /// If a matching method definition is found in the destination type, a reference to it is returned.
    /// Otherwise, the original method reference is returned.
    /// </summary>
    /// <param name="method">MethodReference that may have its ReturnType, DeclaringType and ParameterTypes updated.</param>
    /// <param name="src">The source type which could be replaced.</param>
    /// <param name="dest">The destination type which could replace the source type.</param>
    /// <returns>A MethodReference with updated types, or the original MethodReference if no updates were made.</returns>
    public static MethodReference UpdateTypes(this MethodReference method, TypeDefinition src, TypeDefinition dest)
    {
        // Update method parameters to destination type
        foreach (var parameter in method.Parameters) parameter.UpdateTypes(src, dest);

        // Check if the method's ReturnType or DeclaringType matches the source type, and if so, replace them with the destination type
        if (method.ReturnType == src) method.ReturnType = dest;
        if (method.DeclaringType == src) method.DeclaringType = dest;

        // Attempt to find a method in the destination type that matches the method's full name
        // If a matching definition is found, return a reference to it otherwise return original reference
        return dest.FindMethod(method.FullName) ?? method;
    }

    /// <summary>
    /// Updates the ReturnType and Parameters of the CallSite to the destination type, if they match the source type, to the destination type.
    /// </summary>
    /// <param name="callSite">CallSite that needs its return type and parameters updated.</param>
    /// <param name="src">The original type which is being replaced.</param>
    /// <param name="dest">The new type which is replacing the original type.</param>
    public static void UpdateTypes(this CallSite callSite, TypeDefinition src, TypeDefinition dest)
    {
        // Update callsite parameters to destination type
        foreach (var parameter in callSite.Parameters) parameter.UpdateTypes(src, dest);

        // If the current return type is the source type, update it to destination type
        if (callSite.ReturnType == src) callSite.ReturnType = dest;
    }

    #endregion UpdateTypes

    // Extension methods for replacing references to a source type with references to a destination type within Mono.Cecil.Instruction objects.
    // This is crucial for ensuring that the instructions within methods correctly reference the fields, properties, and methods of the destination type after cloning from the source type.
    #region UpdateInstructionTypes

    /// <summary>
    /// Updates the Operand of an instruction when merging classes.
    /// The update strategy depends on the type of the operand.
    /// If the operand is a ParameterDefinition, VariableDefinition, FieldReference, MethodReference, CallSite, or TypeReference, it's updated accordingly.
    /// </summary>
    /// <param name="instruction">Instruction that needs its operand updated.</param>
    /// <param name="src">The original type which is being replaced.</param>
    /// <param name="dest">The new type which is replacing the original type.</param>
    public static void UpdateInstructionTypes(this Instruction instruction, TypeDefinition src, TypeDefinition dest)
    {
        // Check operand type and update accordingly
        if (instruction.Operand is ParameterDefinition parameter)
            parameter.UpdateTypes(src, dest);  // Update types in ParameterDefinition
        else if (instruction.Operand is VariableDefinition variable)
            variable.UpdateTypes(src, dest);  // Update types in VariableDefinition
        else if (instruction.Operand is TypeReference type && type == src)
            instruction.Operand = dest;  // Update type in TypeReference
        else if (instruction.Operand is FieldReference field)
            instruction.Operand = field.UpdateTypes(src, dest);  // Update types in FieldReference
        else if (instruction.Operand is MethodReference method)
            instruction.Operand = method.UpdateTypes(src, dest);  // Update types in MethodReference
        else if (instruction.Operand is CallSite callSite)
            callSite.UpdateTypes(src, dest);  // Update types in CallSite
    }

    /// <summary>
    /// Updates all instructions in the method's body.
    /// </summary>
    /// <param name="method">Method whose instructions are to be updated.</param>
    /// <param name="src">The original type which is being replaced.</param>
    /// <param name="dest">The new type which is replacing the original type.</param>
    public static void UpdateInstructionTypes(this MethodDefinition method, TypeDefinition src, TypeDefinition dest)
    {
        // Update instructions in the method body to the destination type
        if (method.HasBody) foreach (var instruction in method.Body.Instructions) UpdateInstructionTypes(instruction, src, dest);
    }

    #endregion UpdateInstructionTypes

    // Extension methods for replacing references to a source type with references to a destination type within Mono.Cecil.Property getter and setter methods.
    // This ensures that the properties of the destination type reference copied getters and setters instead of the originals.
    #region UpdateGettersAndSetters

    /// <summary>
    /// Updates the getter and setter methods of a property to reference the destination type when merging classes.
    /// This method does the following:
    ///     - Clones the existing getter/setter methods, so that any modifications do not affect the original methods
    ///     - Calls UpdateTypes to update all type references within the methods' bodies from src to dest
    ///     - Updates the declaring type of the methods to be dest
    ///     - Finds the equivalent methods in dest (if they exist), and updates the property's getter/setter methods to reference them
    /// This ensures that the property correctly interacts with the destination type after merging.
    /// </summary>
    /// <param name="property">PropertyDefinition whose getter and setter need to be updated.</param>
    /// <param name="src">The original type which is being replaced.</param>
    /// <param name="dest">The new type which is replacing the original type.</param>
    public static void UpdateGettersAndSetters(this PropertyDefinition property, TypeDefinition src, TypeDefinition dest)
    {
        // If the declaring type of the property is the destination type
        if (property.DeclaringType == dest)
        {
            // If the property has a getter, clone and update it
            if (property.GetMethod != null)
            {
                // Clone the getter
                var clonedGetter = property.GetMethod.Clone();
                // Update all type references within the getter from src to dest
                clonedGetter.UpdateTypes(src, dest);
                // Update the declaring type of the getter to be dest
                clonedGetter.DeclaringType = dest;
                // If an equivalent method exists in dest, update the property's getter to reference it
                if (dest.FindMethod(clonedGetter.FullName) is MethodDefinition getMethod)
                    property.GetMethod = getMethod;
            }
            // If the property has a setter, clone and update it
            if (property.SetMethod != null)
            {
                // Clone the setter
                var clonedSetter = property.SetMethod.Clone();
                // Update all type references within the setter from src to dest
                clonedSetter.UpdateTypes(src, dest);
                // Update the declaring type of the setter to be dest
                clonedSetter.DeclaringType = dest;
                // If an equivalent method exists in dest, update the property's setter to reference it
                if (dest.FindMethod(clonedSetter.FullName) is MethodDefinition setMethod)
                    property.SetMethod = setMethod;
            }
        }
    }

    #endregion UpdateGettersAndSetters

    // Extension methods to import references from one module to another.
    // This is important when merging assemblies classes as it allows the destination to access types that may not have been referenced prior.
    #region ImportReferences

    /// <summary>
    /// Imports the constructor reference for a given attribute into a module.
    /// </summary>
    /// <param name="attribute">The custom attribute whose constructor reference needs to be imported.</param>
    /// <param name="module">The module type into whose module the reference should be imported.</param>
    public static void ImportReferences(this CustomAttribute attribute, ModuleDefinition module)
    {
        // Import the constructor reference into the module
        attribute.Constructor = module.ImportReference(attribute.Constructor);
    }

    /// <summary>
    /// Imports the interface type and custom attributes references of an interface into a module.
    /// </summary>
    /// <param name="interface">The interface whose references need to be imported.</param>
    /// <param name="module">The module type into whose module the references should be imported.</param>
    public static void ImportReferences(this InterfaceImplementation @interface, ModuleDefinition module)
    {
        // Import the custom attributes references into the module
        foreach (var attribute in @interface.CustomAttributes) attribute.ImportReferences(module);

        // Import the interface type reference into the module
        @interface.InterfaceType = module.ImportReference(@interface.InterfaceType);
    }

    /// <summary>
    /// Imports the field type and custom attributes references of a field into a module.
    /// </summary>
    /// <param name="field">The field whose references need to be imported.</param>
    /// <param name="module">The module type into whose module the references should be imported.</param>
    public static void ImportReferences(this FieldDefinition field, ModuleDefinition module)
    {
        // Import the custom attributes references into the module
        foreach (var attribute in field.CustomAttributes) attribute.ImportReferences(module);

        // Import the field type reference into the module
        field.FieldType = module.ImportReference(field.FieldType);

        // Import the declaring type definition into the module
        field.DeclaringType = module.ImportReference(field.DeclaringType).Resolve();
    }

    /// <summary>
    /// Imports the property type and custom attributes references of a property into a module.
    /// </summary>
    /// <param name="property">The property whose references need to be imported.</param>
    /// <param name="module">The module type into whose module the references should be imported.</param>
    public static void ImportReferences(this PropertyDefinition property, ModuleDefinition module)
    {
        // Import the custom attributes references into the module
        foreach (var attribute in property.CustomAttributes) attribute.ImportReferences(module);

        // Import the property type reference into the module
        property.PropertyType = module.ImportReference(property.PropertyType);

        // Import the declaring type definition into the module
        property.DeclaringType = module.ImportReference(property.DeclaringType).Resolve();
    }

    /// <summary>
    /// Imports the parameter type and custom attributes references of a parameter into a module.
    /// </summary>
    /// <param name="parameter">The parameter whose references need to be imported.</param>
    /// <param name="module">The module type into whose module the references should be imported.</param>
    public static void ImportReferences(this ParameterDefinition parameter, ModuleDefinition module)
    {
        // Import the custom attributes references into the module
        foreach (var attribute in parameter.CustomAttributes) attribute.ImportReferences(module);

        // Import the parameter type reference into the module
        parameter.ParameterType = module.ImportReference(parameter.ParameterType);
    }

    /// <summary>
    /// Imports the variable type references of a variable into a module.
    /// </summary>
    /// <param name="variable">The variable whose type references need to be imported.</param>
    /// <param name="module">The module type into whose module the references should be imported.</param>
    public static void ImportReferences(this VariableDefinition variable, ModuleDefinition module)
    {
        // Import the variable type reference into the module
        variable.VariableType = module.ImportReference(variable.VariableType);
    }
    /// <summary>
    /// Imports the method type references and the custom attributes of a method into a module.
    /// </summary>
    /// <param name="method">The method whose references need to be imported.</param>
    /// <param name="module">The module type into whose module the references should be imported.</param>
    public static void ImportReferences(this MethodDefinition method, ModuleDefinition module)
    {
        // Import method overrides into the module
        for (int i = 0; i < method.Overrides.Count; ++i) method.Overrides[i] = module.ImportReference(method.Overrides[i]);

        // Import the custom attributes references into the module
        foreach (var attribute in method.CustomAttributes) attribute.ImportReferences(module);

        // Import the parameter type references into the module
        foreach (var parameter in method.Parameters) parameter.ImportReferences(module);

        // Import the return type reference into the module
        method.ReturnType = module.ImportReference(method.ReturnType);

        // Import the declaring type definition into the module
        method.DeclaringType = module.ImportReference(method.DeclaringType).Resolve();

        // If the method has a body, import references for each variable and instruction
        if (method.HasBody)
        {
            // Import the variable type references in the method body into the module
            foreach (var variable in method.Body.Variables) variable.ImportReferences(module);

            // Import the instruction type references in the method body into the module
            foreach (var instruction in method.Body.Instructions) instruction.ImportReferences(module);
        }
    }

    /// <summary>
    /// Imports the return type references of a CallSite into a module.
    /// </summary>
    /// <param name="callSite">The CallSite whose return type references need to be imported.</param>
    /// <param name="module">The module type into whose module the references should be imported.</param>
    public static void ImportReferences(this CallSite callSite, ModuleDefinition module)
    {
        // Import the return type reference of the callSite into the module
        callSite.ReturnType = module.ImportReference(callSite.ReturnType);
    }
    /// <summary>
    /// Imports the operand type references of an instruction into a module.
    /// </summary>
    /// <param name="instruction">The instruction whose operand references need to be imported.</param>
    /// <param name="module">The module type into whose module the references should be imported.</param>
    public static void ImportReferences(this Instruction instruction, ModuleDefinition module)
    {
        // Import the operand references of the instruction into the module
        if (instruction.Operand is ParameterDefinition parameter)
            parameter.ImportReferences(module);
        else if (instruction.Operand is VariableDefinition variable)
            variable.ImportReferences(module);
        else if (instruction.Operand is TypeReference type)
            instruction.Operand = module.ImportReference(type);
        else if (instruction.Operand is FieldReference field)
            instruction.Operand = module.ImportReference(field);
        else if (instruction.Operand is MethodReference method)
            instruction.Operand = module.ImportReference(method);
        else if (instruction.Operand is CallSite callSite)
            callSite.ImportReferences(module);
    }

    #endregion ImportReferences

    // Extension methods for swapping method implementations between different types.
    // This can be used when wanting to replace method functionality in the destination type with the corresponding functionality from the source type.
    #region SwapMethods

    /// <summary>
    /// Swaps the method references within the provided instruction between two given methods.
    /// </summary>
    /// <param name="instruction">The instruction to modify.</param>
    /// <param name="leftMethod">The first method to swap.</param>
    /// <param name="rightMethod">The second method to swap.</param>
    public static void SwapMethodReferences(this Instruction instruction, MethodDefinition leftMethod, MethodDefinition rightMethod)
    {
        // If the instruction's operand is a method reference
        if (instruction.Operand is MethodReference method)
        {
            // If the operand matches the left method, replace it with the right method
            if (method == leftMethod)
                instruction.Operand = rightMethod;
            // If the operand matches the right method, replace it with the left method
            else if (method == rightMethod)
                instruction.Operand = leftMethod;
        }
    }

    /// <summary>
    /// Swaps the method references within the provided collection of instructions between two given methods.
    /// </summary>
    /// <param name="instructions">The collection of instructions to modify.</param>
    /// <param name="leftMethod">The first method to swap.</param>
    /// <param name="rightMethod">The second method to swap.</param>
    public static void SwapMethodReferences(this Collection<Instruction> instructions, MethodDefinition leftMethod, MethodDefinition rightMethod)
    {
        // Swap method references for each instruction in the collection
        foreach (var instruction in instructions)
            instruction.SwapMethodReferences(leftMethod, rightMethod);
    }

    /// <summary>
    /// Swaps the method references within the body of the provided method between two given methods.
    /// </summary>
    /// <param name="method">The method to modify.</param>
    /// <param name="leftMethod">The first method to swap.</param>
    /// <param name="rightMethod">The second method to swap.</param>
    public static void SwapMethodReferences(this MethodDefinition method, MethodDefinition leftMethod, MethodDefinition rightMethod)
    {
        // Swap method references for each instruction in the method's body
        if (method.HasBody) method.Body.Instructions.SwapMethodReferences(leftMethod, rightMethod);
    }

    /// <summary>
    /// Swaps the attributes, parameters, custom attributes, and generic parameters between two given methods.
    /// </summary>
    /// <param name="leftMethod">The first method to swap.</param>
    /// <param name="rightMethod">The second method to swap.</param>
    public static void SwapMethods(this MethodDefinition leftMethod, MethodDefinition rightMethod)
    {
        // Save the left method's original details
        var leftBody = leftMethod.Body;
        var leftAttributes = leftMethod.Attributes;
        var leftImplAttributes = leftMethod.ImplAttributes;
        var leftSemanticsAttributes = leftMethod.SemanticsAttributes;
        var leftParameters = new Collection<ParameterDefinition>(leftMethod.Parameters);
        var leftCustomAttributes = new Collection<CustomAttribute>(leftMethod.CustomAttributes);
        var leftGenericParameters = new Collection<GenericParameter>(leftMethod.GenericParameters);

        // Swap the details from the right method to the left
        leftMethod.Body = rightMethod.Body;
        leftMethod.Body = rightMethod.Body;
        leftMethod.Attributes = rightMethod.Attributes;
        leftMethod.ImplAttributes = rightMethod.ImplAttributes;
        leftMethod.SemanticsAttributes = rightMethod.SemanticsAttributes;
        leftMethod.Parameters.Clear();
        leftMethod.CustomAttributes.Clear();
        leftMethod.GenericParameters.Clear();
        foreach (var parameter in rightMethod.Parameters) leftMethod.Parameters.Add(parameter);
        foreach (var attribute in rightMethod.CustomAttributes) leftMethod.CustomAttributes.Add(attribute);
        foreach (var parameter in rightMethod.GenericParameters) leftMethod.GenericParameters.Add(parameter);

        // Swap the details from the left method (which were saved) to the right
        rightMethod.Body = leftBody;
        rightMethod.Body = leftBody;
        rightMethod.Attributes = leftAttributes;
        rightMethod.ImplAttributes = leftImplAttributes;
        rightMethod.SemanticsAttributes = leftSemanticsAttributes;
        rightMethod.Parameters.Clear();
        rightMethod.CustomAttributes.Clear();
        rightMethod.GenericParameters.Clear();
        foreach (var parameter in leftParameters) rightMethod.Parameters.Add(parameter);
        foreach (var attribute in leftCustomAttributes) rightMethod.CustomAttributes.Add(attribute);
        foreach (var parameter in leftGenericParameters) rightMethod.GenericParameters.Add(parameter);

        // Swap method references within each method body
        leftMethod.SwapMethodReferences(leftMethod, rightMethod);
        rightMethod.SwapMethodReferences(rightMethod, leftMethod);
    }

    /// <summary>
    /// Finds and swaps methods with the same full name within the given type.
    /// </summary>
    /// <param name="type">The type to modify.</param>
    public static void SwapDuplicateMethods(this TypeDefinition type)
    {
        // This HashSet is used for tracking the methods that have already been swapped.
        var alreadySwapped = new HashSet<string>();

        // Convert the method collection to list for efficient index-based access.
        var methods = type.Methods.ToList();

        // Iterate over each pair of methods in the type
        for (int i = 0; i < methods.Count; i++)
        {
            for (int j = i + 1; j < methods.Count; j++)
            {
                var methodLeft = methods[i];
                var methodRight = methods[j];

                // If two methods have the same full name and haven't been swapped yet
                if (methodLeft.FullName == methodRight.FullName && !alreadySwapped.Contains(methodLeft.FullName))
                {
                    // Add the method full name to the set of already swapped methods
                    _ = alreadySwapped.Add(methodLeft.FullName);
                    // Swap the two methods
                    methodLeft.SwapMethods(methodRight);
                }
            }
        }
    }

    /// <summary>
    /// Finds and swaps methods with the same full name within each type in the given collection.
    /// </summary>
    /// <param name="types">The collection of types to modify.</param>
    public static void SwapDuplicateMethods(this Collection<TypeDefinition> types)
    {
        // Swap duplicate methods for each type in the collection
        foreach (var type in types)
            type.SwapDuplicateMethods();
    }

    #endregion SwapMethods

    // Extension method that handles adding types to an assembly.
    #region AddType

    /// <summary>
    /// Adds a type to an assembly. This includes adding the type's fields, properties, and methods.
    /// If the source type is nested, it will be added as a nested type within the parent type in the destination assembly.
    /// </summary>
    /// <param name="assembly">The assembly to which the type will be added.</param>
    /// <param name="src">The source type that will be added to the assembly.</param>
    public static void AddType(this AssemblyDefinition assembly, TypeDefinition src)
    {
        // Create a new TypeDefinition with the same properties as the source type
        var dest = new TypeDefinition(src.Namespace, src.Name, src.Attributes);

        // If the source type isn't nested, add the new type directly to the assembly's types
        // Otherwise, find the declaring type in the assembly and add the new type as a nested type
        if (!src.IsNested)
            assembly.MainModule.Types.Add(dest);
        else
            assembly.FindType(src.DeclaringType.FullName).NestedTypes.Add(dest);

        // Set the base type of the new type to match the base type of the source type
        dest.BaseType = src.BaseType;

        // Add the fields, properties, and methods from the source type to the new type
        dest.AddFieldsPropertiesAndMethods(src);
    }

    #endregion AddType

    // Extension method that handles the addition of fields, properties, and methods from a source type to a destination type.
    // This is a key part of merging two types, ensuring the destination type includes all necessary components from the source type.
    #region AddFieldsPropertiesAndMethods

    /// <summary>
    /// Merges the source type into the destination type by cloning the fields, properties, and methods of the source, updating their types and adding them to the destination.
    /// </summary>
    /// <param name="dest">The destination type definition where fields, properties, and methods from source will be added.</param>
    /// <param name="src">The source type definition whose fields, properties, and methods will be cloned and added to the destination.</param>
    public static void AddFieldsPropertiesAndMethods(this TypeDefinition dest, TypeDefinition src)
    {
        // Add nested types to the module
        foreach (var subtype in src.NestedTypes)
            dest.Module.Assembly.AddType(subtype);

        // Clone attributes from the source and add to the destination
        var clonedAttributes = new Collection<CustomAttribute>();
        foreach (var attribute in src.CustomAttributes)
        {
            var clonedAttribute = attribute.Clone();
            dest.CustomAttributes.Add(clonedAttribute);
            clonedAttributes.Add(clonedAttribute);
        }

        // Clone interfaces from the source and add to the destination
        var clonedInterfaces = new Collection<InterfaceImplementation>();
        foreach (var @interface in src.Interfaces)
        {
            var clonedInterface = @interface.Clone();
            dest.Interfaces.Add(clonedInterface);
            clonedInterfaces.Add(clonedInterface);
        }

        // Clone fields from the source and add to the destination
        var clonedFields = new Collection<FieldDefinition>();
        foreach (var field in src.Fields)
        {
            var clonedField = field.Clone();
            clonedFields.Add(clonedField);
            dest.Fields.Add(clonedField);
        }

        // Clone properties from the source and add to the destination
        var clonedProperties = new Collection<PropertyDefinition>();
        foreach (var property in src.Properties)
        {
            var clonedProperty = property.Clone();
            clonedProperties.Add(clonedProperty);
            dest.Properties.Add(clonedProperty);
        }

        // Clone methods from the source (don't add to the destination yet)
        var clonedMethods = new Collection<MethodDefinition>();
        foreach (var method in src.Methods)
        {
            var clonedMethod = method.Clone();
            clonedMethods.Add(clonedMethod);
        }

        // List for keeping track of methods that need further processing
        var updatedMethods = new Collection<MethodDefinition>();

        // Process each method
        foreach (var clonedMethod in clonedMethods.ToList())
        {
            // Special handling for constructors
            if (clonedMethod.Name is ".ctor" or ".cctor" or "Finalize")
            {
                // Temporarily set the declaring type of the cloned method to the destination type
                // This is required to get the correct full name of the method for the FindMethod call
                clonedMethod.DeclaringType = dest;

                // Find an existing method in the destination type that matches the full name of the cloned method
                // Note that the full name of a method includes the name of its declaring type
                var destMethod = dest.FindMethod(clonedMethod.FullName);

                // Reset the declaring type of the cloned method to null
                // This is done because the cloned method hasn't been added to the destination type yet,
                // and leaving the declaring type set will cause failures to add the method to the destination type
                clonedMethod.DeclaringType = null;

                // If destination already contains a constructor/destructor, merge the instructions
                if (destMethod != null)
                {
                    var clonedInstructions = clonedMethod.Body.Instructions;
                    var trimmedClonedInstructions = clonedInstructions.ToList();

                    // For constructors
                    if (clonedMethod.Name is ".ctor")
                    {
                        // Find the constructor call instruction and remove the instructions before it
                        // This is done to prevent calling the base class constructor twice when merging
                        var callIndex = trimmedClonedInstructions.FindIndex(x => x.OpCode == OpCodes.Call);

                        // Check if callIndex is within valid range
                        if (callIndex < 0 || callIndex >= trimmedClonedInstructions.Count)
                            throw new Exception("Invalid Call Instruction Index in cloned method.");

                        // Remove starting instructions
                        trimmedClonedInstructions.RemoveRange(0, callIndex + 1);
                        trimmedClonedInstructions.RemoveAt(trimmedClonedInstructions.Count - 1);

                        // Insert the trimmed instructions to the existing constructor, just before the last instruction (ret)
                        int insertIndex = destMethod.Body.Instructions.Count - 1;
                        foreach (var clonedInstruction in trimmedClonedInstructions)
                        {
                            destMethod.Body.Instructions.Insert(insertIndex, clonedInstruction);
                            insertIndex++;
                        }
                    }
                    // For static constructors
                    else if (clonedMethod.Name is ".cctor")
                    {
                        // Remove the last instruction (ret)
                        trimmedClonedInstructions.RemoveAt(trimmedClonedInstructions.Count - 1);

                        // Insert the trimmed instructions to the existing static constructor, just before the last instruction (ret)
                        int insertIndex = destMethod.Body.Instructions.Count - 1;
                        foreach (var clonedInstruction in trimmedClonedInstructions)
                        {
                            destMethod.Body.Instructions.Insert(insertIndex, clonedInstruction);
                            insertIndex++;
                        }
                    }
                    // For destructors
                    else if (clonedMethod.Name is "Finalize")
                    {
                        // Find the leave.s instruction and remove the instructions after it.
                        // This is done to prevent calling the base class destructor twice when merging.
                        var trimIndex = trimmedClonedInstructions.FindIndex(x => x.OpCode == OpCodes.Leave_S);

                        // Check if trimIndex is within valid range
                        if (trimIndex < 0 || trimIndex >= trimmedClonedInstructions.Count)
                            throw new Exception("Invalid trim index in cloned method.");

                        // Remove instructions after leave.s (inclusive)
                        trimmedClonedInstructions.RemoveRange(trimIndex, trimmedClonedInstructions.Count - trimIndex);

                        // Insert the trimmed instructions to the existing destructor, at the beginning
                        int insertionIndex = 0;
                        foreach (var clonedInstruction in trimmedClonedInstructions)
                        {
                            destMethod.Body.Instructions.Insert(insertionIndex, clonedInstruction);
                            insertionIndex++;
                        }
                    }

                    // Remove the cloned constructor or destructor from the list of methods to add to the destination type
                    _ = clonedMethods.Remove(clonedMethod);

                    // Add the method to the list of methods to update since it has been modified
                    updatedMethods.Add(destMethod);
                }
                else
                {
                    // Add the cloned constructor to the destination type
                    updatedMethods.Add(clonedMethod);
                }
            }
            else
            {
                // For non-constructor/non-destructor methods
                updatedMethods.Add(clonedMethod);
            }
        }

        // Add updated methods to the destination type
        foreach (var method in clonedMethods) dest.Methods.Add(method);

        // Add updated attributes, interfaces, fields, properties and methods to the update info
        if (!assemblyUpdateInfo.TryGetValue(dest.Module.Assembly, out var updateInfo))
            updateInfo = assemblyUpdateInfo[dest.Module.Assembly] = new();
        foreach (var attribute in clonedAttributes) updateInfo.updatedAttributes.Add(attribute);
        foreach (var @interface in clonedInterfaces) updateInfo.updatedInterfaces.Add(@interface);
        foreach (var field in clonedFields) updateInfo.updatedFields.Add(field);
        foreach (var property in clonedProperties) updateInfo.updatedProperties.Add(property);
        foreach (var method in updatedMethods) updateInfo.updatedMethods.Add(method);

        // Add source and destination types to the update info
        updateInfo.srcTypes.Add(src);
        updateInfo.destTypes.Add(dest);
    }

    #endregion AddFieldsPropertiesAndMethods

    // Extension methods that handle the updating of fields, properties, and methods within a destination type after they have been cloned from a source type.
    // These methods ensure that the newly added components in the destination type correctly reference the destination type, rather than the original source type.
    #region UpdateFieldsPropertiesAndMethods

    /// <summary>
    /// Updates the types of attributes, interfaces, fields, properties, and methods within a given assembly.
    /// This includes updating the types in interfaces, fields, properties, and methods. It also updates the getter and setter methods for properties, 
    /// updates the instruction types for methods, imports references for attributes, interfaces, fields, properties, and methods, 
    /// imports base types of each destination type, and swaps any duplicate methods in the destination types.
    /// </summary>
    /// <param name="assembly">The assembly to be updated. This assembly's types are matched against the source types and replaced with the corresponding destination types, based on previously registered update information.</param>
    public static void UpdateFieldsPropertiesAndMethods(this AssemblyDefinition assembly)
    {
        // Check if update information exists for the assembly
        if (assemblyUpdateInfo.TryGetValue(assembly, out var updateInfo))
        {
            // Update types in interfaces, fields, properties, and methods
            for (int i = 0; i < updateInfo.destTypes.Count; ++i)
                foreach (var @interface in updateInfo.updatedInterfaces) @interface.UpdateTypes(updateInfo.srcTypes[i], updateInfo.destTypes[i]);
            for (int i = 0; i < updateInfo.destTypes.Count; ++i)
                foreach (var field in updateInfo.updatedFields) field.UpdateTypes(updateInfo.srcTypes[i], updateInfo.destTypes[i]);
            for (int i = 0; i < updateInfo.destTypes.Count; ++i)
                foreach (var property in updateInfo.updatedProperties) property.UpdateTypes(updateInfo.srcTypes[i], updateInfo.destTypes[i]);
            for (int i = 0; i < updateInfo.destTypes.Count; ++i)
                foreach (var method in updateInfo.updatedMethods) method.UpdateTypes(updateInfo.srcTypes[i], updateInfo.destTypes[i]);

            // Update getter and setter methods for properties
            for (int i = 0; i < updateInfo.destTypes.Count; ++i)
                foreach (var property in updateInfo.updatedProperties) property.UpdateGettersAndSetters(updateInfo.srcTypes[i], updateInfo.destTypes[i]);

            // Update instruction types for methods
            for (int i = 0; i < updateInfo.destTypes.Count; ++i)
                foreach (var method in updateInfo.updatedMethods) method.UpdateInstructionTypes(updateInfo.srcTypes[i], updateInfo.destTypes[i]);

            // Import references for attributes, interfaces, fields, properties, and methods
            foreach (var attribute in updateInfo.updatedAttributes) attribute.ImportReferences(assembly.MainModule);
            foreach (var @interface in updateInfo.updatedInterfaces) @interface.ImportReferences(assembly.MainModule);
            foreach (var field in updateInfo.updatedFields) field.ImportReferences(assembly.MainModule);
            foreach (var property in updateInfo.updatedProperties) property.ImportReferences(assembly.MainModule);
            foreach (var method in updateInfo.updatedMethods) method.ImportReferences(assembly.MainModule);

            // Import base type of each dest type
            foreach (var type in updateInfo.destTypes) type.BaseType = assembly.MainModule.ImportReference(type.BaseType);

            // Swap any duplicate methods in the destination types
            updateInfo.destTypes.SwapDuplicateMethods();

            // Remove the assembly from the update information collection
            _ = assemblyUpdateInfo.Remove(assembly);
        }
    }

    #endregion UpdateFieldsPropertiesAndMethods

}
#endif
