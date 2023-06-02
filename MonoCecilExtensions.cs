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
    private class UpdateInfo
    {
        internal readonly Collection<FieldDefinition> updatedFields = new();
        internal readonly Collection<PropertyDefinition> updatedProperties = new();
        internal readonly Collection<MethodDefinition> updatedMethods = new();

        internal readonly Collection<TypeDefinition> srcTypes = new();
        internal readonly Collection<TypeDefinition> destTypes = new();
    };

    private static readonly Dictionary<AssemblyDefinition, UpdateInfo> assemblyUpdateInfo = new();

    public static readonly Collection<string> additionalSearchDirectories = new();

    // Basic extension methods for loading assemblies, adding elements to collections, and finding types, fields, and methods in Mono.Cecil objects.
    #region Base

    /// <summary>
    /// This extension method loads an assembly from a given location.
    /// </summary>
    /// <param name="location">The location of the assembly to be loaded.</param>
    /// <param name="readWrite">A boolean value to determine if the assembly is read-only or writable.</param>
    /// <returns>The AssemblyDefinition object of the loaded assembly if successful, null otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the location is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when an assembly cannot be found at the provided location.</exception>
    /// <exception cref="BadImageFormatException">Thrown when the file at the provided location is not a valid .NET assembly.</exception>
    public static AssemblyDefinition LoadAssembly(this string location, bool readWrite = false)
    {
        // Check for empty location
        if (string.IsNullOrEmpty(location)) throw new ArgumentNullException(nameof(location), "The location of the assembly to be loaded cannot be null or empty.");
        // Check that the target assembly exists
        if (!File.Exists(location)) throw new FileNotFoundException("The assembly cannot be found at the provided location.", location);

        // Create a new instance of the DefaultAssemblyResolver.
        var resolver = new DefaultAssemblyResolver();

        // Add search directories to the resolver.
        foreach (var directory in additionalSearchDirectories)
            resolver.AddSearchDirectory(directory);
        resolver.AddSearchDirectory(Path.GetDirectoryName(typeof(int).Assembly.Location));
        resolver.AddSearchDirectory(Path.Combine(Path.GetDirectoryName(typeof(int).Assembly.Location), "Facades"));

        try
        {
            // Read and return the assembly using the provided location and reader parameters.
            return AssemblyDefinition.ReadAssembly(location, new ReaderParameters()
            {
                AssemblyResolver = resolver,
                ReadWrite = readWrite,
            });
        }
        catch (BadImageFormatException ex)
        {
            throw new BadImageFormatException("The file at the provided location is not a valid .NET assembly.", ex);
        }
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

    /// <summary>
    /// Adds elements from another collection to the current collection.
    /// </summary>
    /// <typeparam name="T">The type of the objects within the collection.</typeparam>
    /// <param name="collection">The collection to which elements will be added.</param>
    /// <param name="otherCollection">The collection from which elements will be taken. It must not be null.</param>
    /// <returns>Does not return a value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the otherCollection is null.</exception>
    public static void Add<T>(this Collection<T> collection, Collection<T> otherCollection)
    {
        // Ensure that none of the arguments are null
        if (collection == null) throw new ArgumentNullException(nameof(otherCollection), "The collection to add to cannot be null.");
        if (otherCollection == null) throw new ArgumentNullException(nameof(otherCollection), "The collection to be added cannot be null.");

        // Add items to collection
        foreach (var otherT in otherCollection)
            collection.Add(otherT);
    }

    #endregion Base

    // Extension methods for cloning various Mono.Cecil objects.
    #region Clone

    /// <summary>
    /// Clones a CustomAttribute.
    /// </summary>
    /// <param name="attribute">The attribute to be cloned.</param>
    /// <returns>A clone of the original attribute.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the attribute to be cloned is null.</exception>
    public static CustomAttribute Clone(this CustomAttribute attribute)
    {
        // Check that this attribute isn't null
        if (attribute == null) throw new ArgumentNullException(nameof(attribute), "The attribute to be cloned cannot be null.");

        // Create a new CustomAttribute with the constructor of the original attribute.
        var clonedAttribute = new CustomAttribute(attribute.Constructor);

        // Copy all constructor arguments from the original attribute to the cloned attribute.
        foreach (var argument in attribute.ConstructorArguments)
            clonedAttribute.ConstructorArguments.Add(argument);

        // Copy all properties from the original attribute to the cloned attribute.
        clonedAttribute.Properties.Add(attribute.Properties);

        // Copy all fields from the original attribute to the cloned attribute.
        clonedAttribute.Fields.Add(attribute.Fields);

        // Return the cloned attribute.
        return clonedAttribute;
    }

    /// <summary>
    /// Clones a FieldDefinition.
    /// </summary>
    /// <param name="field">The field to be cloned.</param>
    /// <returns>A clone of the original field.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the field to be cloned is null.</exception>
    public static FieldDefinition Clone(this FieldDefinition field)
    {
        // Check that this field isn't null
        if (field == null) throw new ArgumentNullException(nameof(field), "The field to be cloned cannot be null.");

        // Create a new FieldDefinition with the same properties as the original field.
        var clonedField = new FieldDefinition(field.Name, field.Attributes, field.FieldType);

        // Copy all custom attributes from the original field to the cloned field.
        clonedField.CustomAttributes.Add(field.CustomAttributes.Clone());

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
    /// <exception cref="ArgumentNullException">Thrown when the property to be cloned is null.</exception>
    public static PropertyDefinition Clone(this PropertyDefinition property)
    {
        // Check that this property isn't null
        if (property == null) throw new ArgumentNullException(nameof(property), "The property to be cloned cannot be null.");

        // Create a new PropertyDefinition with the same properties as the original property.
        var clonedProperty = new PropertyDefinition(property.Name, property.Attributes, property.PropertyType);

        // Copy all custom attributes from the original property to the cloned property.
        clonedProperty.CustomAttributes.Add(property.CustomAttributes.Clone());

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
    /// <exception cref="ArgumentNullException">Thrown when the parameter to be cloned is null.</exception>
    public static ParameterDefinition Clone(this ParameterDefinition parameter)
    {
        // Check that this parameter isn't null
        if (parameter == null) throw new ArgumentNullException(nameof(parameter), "The parameter to be cloned cannot be null.");

        // Create a new ParameterDefinition with the same properties as the original parameter.
        var clonedParameter = new ParameterDefinition(parameter.Name, parameter.Attributes, parameter.ParameterType);

        // Copy all custom attributes from the original parameter to the cloned parameter.
        clonedParameter.CustomAttributes.Add(parameter.CustomAttributes.Clone());

        // Return the cloned parameter.
        return clonedParameter;
    }

    /// <summary>
    /// Clones a VariableDefinition.
    /// </summary>
    /// <param name="variable">The variable to be cloned.</param>
    /// <returns>A clone of the original variable.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the variable to be cloned is null.</exception>
    public static VariableDefinition Clone(this VariableDefinition variable)
    {
        // Check that this variable isn't null
        if (variable == null) throw new ArgumentNullException(nameof(variable), "The variable to be cloned cannot be null.");

        // Create and return a new VariableDefinition with the same type as the original variable.
        return new VariableDefinition(variable.VariableType);
    }

    /// <summary>
    /// Clones an Instruction.
    /// </summary>
    /// <param name="instruction">The instruction to be cloned.</param>
    /// <returns>A clone of the original instruction.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the instruction to be cloned is null.</exception>
    public static Instruction Clone(this Instruction instruction)
    {
        // Check that this instruction isn't null
        if (instruction == null) throw new ArgumentNullException(nameof(instruction), "The instruction to be cloned cannot be null.");

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
    /// <exception cref="ArgumentNullException">Thrown when the method to be cloned is null.</exception>
    public static MethodDefinition Clone(this MethodDefinition method)
    {
        // Check that this method isn't null
        if (method == null) throw new ArgumentNullException(nameof(method), "The method to be cloned cannot be null.");

        // Create a new MethodDefinition with the same properties as the original method.
        var clonedMethod = new MethodDefinition(method.Name, method.Attributes, method.ReturnType)
        {
            ImplAttributes = method.ImplAttributes,
            SemanticsAttributes = method.SemanticsAttributes
        };

        // Copy all custom attributes from the original method to the cloned method.
        clonedMethod.CustomAttributes.Add(method.CustomAttributes.Clone());

        // Clone all parameters and add them to the cloned method.
        clonedMethod.Parameters.Add(method.Parameters.Clone());

        // Create a new method body for the cloned method.
        clonedMethod.Body = new MethodBody(clonedMethod);

        // If the original method has a body, copy the relevant properties to the cloned method's body.
        if (method.HasBody)
        {
            clonedMethod.Body.MaxStackSize = method.Body.MaxStackSize;
            clonedMethod.Body.InitLocals = method.Body.InitLocals;

            // Clone all instructions and variables and add them to the cloned method's body.
            clonedMethod.Body.Instructions.Add(method.Body.Instructions.Clone());
            clonedMethod.Body.Variables.Add(method.Body.Variables.Clone());
        }

        // Return the cloned method.
        return clonedMethod;
    }

    /// <summary>
    /// Creates a deep copy of a Collection<typeparamref name="T"/> where T can be any type that implements a Clone method.
    /// </summary>
    /// <typeparam name="T">The type of the objects within the collection.</typeparam>
    /// <param name="collection">The collection to clone.</param>
    /// <returns>A cloned collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the collection to be cloned is null.</exception>
    /// <exception cref="RuntimeBinderException">Thrown when the type T does not have a Clone method.</exception>
    public static Collection<T> Clone<T>(this Collection<T> collection)
    {
        // Check that this collection isn't null
        if (collection == null) throw new ArgumentNullException(nameof(collection), "The collection to be cloned cannot be null.");

        // Iterate through each item in the original collection.
        var clonedCollection = new Collection<T>();
        foreach (var item in collection)
        {
            // Create a clone of each item and add it to the cloned collection.
            clonedCollection.Add(Clone(item as dynamic));
        }

        // Return the cloned collection.
        return clonedCollection;
    }

    #endregion Clone

    // Extension methods for replacing references to a source type with references to a destination type within Mono.Cecil objects.
    // This is used to ensure that copied fields, properties, and methods reference copied types instead of the originals.
    #region UpdateTypes 

    /// <summary>
    /// Updates the FieldType of the given FieldDefinition, if it matches the source type, to the destination type.
    /// </summary>
    /// <param name="field">FieldDefinition that may have its FieldType updated.</param>
    /// <param name="src">The source type which could be replaced.</param>
    /// <param name="dest">The destination type which could replace the source type.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void UpdateTypes(this FieldDefinition field, TypeDefinition src, TypeDefinition dest)
    {
        // Ensure that none of the arguments are null
        if (field == null) throw new ArgumentNullException(nameof(field), "Field cannot be null.");
        if (src == null) throw new ArgumentNullException(nameof(src), "Source type cannot be null.");
        if (dest == null) throw new ArgumentNullException(nameof(dest), "Destination type cannot be null.");

        // If the field's type matches the source type, update it to the destination type
        if (field.FieldType == src) field.FieldType = dest;
    }

    /// <summary>
    /// Updates the PropertyType of the given PropertyDefinition, if it matches the source type, to the destination type.
    /// </summary>
    /// <param name="property">PropertyDefinition that may have its PropertyType updated.</param>
    /// <param name="src">The source type which could be replaced.</param>
    /// <param name="dest">The destination type which could replace the source type.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void UpdateTypes(this PropertyDefinition property, TypeDefinition src, TypeDefinition dest)
    {
        // Ensure that none of the arguments are null
        if (property == null) throw new ArgumentNullException(nameof(property), "Property cannot be null.");
        if (src == null) throw new ArgumentNullException(nameof(src), "Source type cannot be null.");
        if (dest == null) throw new ArgumentNullException(nameof(dest), "Destination type cannot be null.");

        // If the property's type matches the source type, update it to the destination type
        if (property.PropertyType == src) property.PropertyType = dest;
    }

    /// <summary>
    /// Updates the ParameterType of the given ParameterDefinition, if it matches the source type, to the destination type.
    /// </summary>
    /// <param name="parameter">ParameterDefinition that may have its ParameterType updated.</param>
    /// <param name="src">The source type which could be replaced.</param>
    /// <param name="dest">The destination type which could replace the source type.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void UpdateTypes(this ParameterDefinition parameter, TypeDefinition src, TypeDefinition dest)
    {
        // Ensure that none of the arguments are null
        if (parameter == null) throw new ArgumentNullException(nameof(parameter), "Parameter cannot be null.");
        if (src == null) throw new ArgumentNullException(nameof(src), "Source type cannot be null.");
        if (dest == null) throw new ArgumentNullException(nameof(dest), "Destination type cannot be null.");

        // If the parameter's type matches the source type, update it to the destination type
        if (parameter.ParameterType == src) parameter.ParameterType = dest;
    }

    /// <summary>
    /// Updates the VariableType of the given VariableDefinition, if it matches the source type, to the destination type.
    /// </summary>
    /// <param name="variable">VariableDefinition that may have its VariableType updated.</param>
    /// <param name="src">The source type which could be replaced.</param>
    /// <param name="dest">The destination type which could replace the source type.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void UpdateTypes(this VariableDefinition variable, TypeDefinition src, TypeDefinition dest)
    {
        // Ensure that none of the arguments are null
        if (variable == null) throw new ArgumentNullException(nameof(variable), "Variable cannot be null.");
        if (src == null) throw new ArgumentNullException(nameof(src), "Source type cannot be null.");
        if (dest == null) throw new ArgumentNullException(nameof(dest), "Destination type cannot be null.");

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
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void UpdateTypes(this MethodDefinition method, TypeDefinition src, TypeDefinition dest)
    {
        // Ensure that none of the arguments are null
        if (method == null) throw new ArgumentNullException(nameof(method), "Method cannot be null.");
        if (src == null) throw new ArgumentNullException(nameof(src), "Source type cannot be null.");
        if (dest == null) throw new ArgumentNullException(nameof(dest), "Destination type cannot be null.");

        // If the method's return type matches the source type, update it to the destination type
        if (method.ReturnType == src) method.ReturnType = dest;

        // Update method parameters and variables if they match the source type
        method.Parameters.UpdateTypes(src, dest);
        method.Body?.Variables.UpdateTypes(src, dest);
    }

    /// <summary>
    /// Updates the types of each item in the collection, if they match the source type, to the destination type.
    /// The exact manner of updating depends on the dynamic type of the items in the collection.
    /// </summary>
    /// <typeparam name="T">The type of the objects within the collection.</typeparam>
    /// <param name="collection">The collection of items that may have their type updated.</param>
    /// <param name="src">The source type which could be replaced.</param>
    /// <param name="dest">The destination type which could replace the source type.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    /// <exception cref="RuntimeBinderException">Thrown when the type T does not have an applicable UpdateTypes method.</exception>
    public static void UpdateTypes<T>(this Collection<T> collection, TypeDefinition src, TypeDefinition dest)
    {
        // Ensure that none of the arguments are null
        if (collection == null) throw new ArgumentNullException(nameof(collection), "Collection cannot be null.");
        if (src == null) throw new ArgumentNullException(nameof(src), "Source type cannot be null.");
        if (dest == null) throw new ArgumentNullException(nameof(dest), "Destination type cannot be null.");

        // Update each item's types, depending on its dynamic type
        foreach (var item in collection)
            UpdateTypes(item as dynamic, src, dest); // Cast the item to dynamic to resolve the appropriate UpdateTypes method at runtime
    }

    /// <summary>
    /// Updates the types within the given collection. This operation occurs for each pair of source and destination types provided.
    /// </summary>
    /// <typeparam name="T">The type of items contained within the collection.</typeparam>
    /// <param name="collection">The collection whose types need to be updated.</param>
    /// <param name="srcTypes">The collection of source types.</param>
    /// <param name="destTypes">The collection of destination types.</param>
    public static void UpdateTypes<T>(this Collection<T> collection, Collection<TypeDefinition> srcTypes, Collection<TypeDefinition> destTypes)
    {
        // Loop over each source-destination type pair
        for (int i = 0; i < destTypes.Count; ++i)
        {
            // Extract source and destination types from the provided collections
            var src = srcTypes[i];
            var dest = destTypes[i];

            // Update types within the collection
            collection.UpdateTypes(src, dest);
        }
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
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void UpdateInstructionTypes(this Instruction instruction, TypeDefinition src, TypeDefinition dest)
    {
        // Ensure that none of the arguments are null
        if (instruction == null) throw new ArgumentNullException(nameof(instruction), "The parameter instruction cannot be null.");
        if (src == null) throw new ArgumentNullException(nameof(src), "The parameter src cannot be null.");
        if (dest == null) throw new ArgumentNullException(nameof(dest), "The parameter dest cannot be null.");

        // Check operand type and update accordingly
        if (instruction.Operand is ParameterDefinition parameter)
            parameter.UpdateTypes(src, dest);  // Update types in ParameterDefinition
        else if (instruction.Operand is VariableDefinition variable)
            variable.UpdateTypes(src, dest);  // Update types in VariableDefinition
        else if (instruction.Operand is TypeReference type)
            instruction.UpdateInstructionTypes(type, src, dest);  // Update types in TypeReference
        else if (instruction.Operand is FieldReference field)
            instruction.UpdateInstructionTypes(field, src, dest);  // Update types in FieldReference
        else if (instruction.Operand is MethodReference method)
            instruction.UpdateInstructionTypes(method, src, dest);  // Update types in MethodReference
        else if (instruction.Operand is CallSite callSite)
            callSite.UpdateInstructionTypes(src, dest);  // Update types in CallSite
    }

    /// <summary>
    /// Updates the Operand of an instruction when merging classes.
    /// If the TypeReference of the operand matches the source type, it's replaced with the destination type.
    /// </summary>
    /// <param name="instruction">The instruction whose operand needs to be updated.</param>
    /// <param name="type">TypeReference that serves as a template for the updating process.</param>
    /// <param name="src">The original type which is being replaced.</param>
    /// <param name="dest">The new type which is replacing the original type.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void UpdateInstructionTypes(this Instruction instruction, TypeReference type, TypeDefinition src, TypeDefinition dest)
    {
        // Ensure that none of the arguments are null
        if (instruction == null) throw new ArgumentNullException(nameof(instruction), "The parameter instruction cannot be null.");
        if (type == null) throw new ArgumentNullException(nameof(type), "The parameter type cannot be null.");
        if (src == null) throw new ArgumentNullException(nameof(src), "The parameter src cannot be null.");
        if (dest == null) throw new ArgumentNullException(nameof(dest), "The parameter dest cannot be null.");

        // If the operand type is the source type, update it to destination type
        if (type == src) instruction.Operand = dest;
    }

    /// <summary>
    /// Updates the Operand of an instruction when merging classes.
    /// Updates the FieldType and DeclaringType of a FieldReference operand if they match the source type.
    /// If a matching field is found in the destination type, the Operand is updated to this field.
    /// If no matching field is found, the Operand remains unchanged.
    /// </summary>
    /// <param name="instruction">The instruction whose operand needs to be updated.</param>
    /// <param name="field">FieldReference that serves as a template for the updating process.</param>
    /// <param name="src">The original type which is being replaced.</param>
    /// <param name="dest">The new type which is replacing the original type.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void UpdateInstructionTypes(this Instruction instruction, FieldReference field, TypeDefinition src, TypeDefinition dest)
    {
        // Ensure that none of the arguments are null
        if (instruction == null) throw new ArgumentNullException(nameof(instruction), "The parameter instruction cannot be null.");
        if (field == null) throw new ArgumentNullException(nameof(field), "The parameter field cannot be null.");
        if (src == null) throw new ArgumentNullException(nameof(src), "The parameter src cannot be null.");
        if (dest == null) throw new ArgumentNullException(nameof(dest), "The parameter dest cannot be null.");

        // Check if the field's FieldType or DeclaringType matches the source type, and if so, replace them with the destination type
        if (field.FieldType == src) field.FieldType = dest;
        if (field.DeclaringType == src) field.DeclaringType = dest;

        // Attempt to find a field in the destination type that matches the field's full name
        var fieldDefinition = dest.FindField(field.FullName);

        // If a matching field is found, update the instruction's operand to this field
        if (fieldDefinition != null) instruction.Operand = fieldDefinition;
    }

    /// <summary>
    /// Updates the Operand of an instruction when merging classes.
    /// Updates the ReturnType and DeclaringType of a MethodReference operand if they match the source type.
    /// Updates the parameters of the MethodReference if they match the source type.
    /// If a matching method is found in the destination type, the Operand is updated to this method.
    /// If no matching method is found, the Operand remains unchanged.
    /// </summary>
    /// <param name="instruction">Instruction whose MethodReference operand is to be updated.</param>
    /// <param name="method">MethodReference of the instruction that needs its parameters, return type and declaring type updated.</param>
    /// <param name="src">The original type which is being replaced.</param>
    /// <param name="dest">The new type which is replacing the original type.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void UpdateInstructionTypes(this Instruction instruction, MethodReference method, TypeDefinition src, TypeDefinition dest)
    {
        // Ensure that none of the arguments are null
        if (instruction == null) throw new ArgumentNullException(nameof(instruction), "The parameter instruction cannot be null.");
        if (method == null) throw new ArgumentNullException(nameof(method), "The parameter method cannot be null.");
        if (src == null) throw new ArgumentNullException(nameof(src), "The parameter src cannot be null.");
        if (dest == null) throw new ArgumentNullException(nameof(dest), "The parameter dest cannot be null.");

        // Update method parameters to destination type
        method.Parameters.UpdateTypes(src, dest);

        // Check if the method's ReturnType or DeclaringType matches the source type, and if so, replace them with the destination type
        if (method.ReturnType == src) method.ReturnType = dest;
        if (method.DeclaringType == src) method.DeclaringType = dest;

        // Attempt to find a method in the destination type that matches the method's full name
        var methodDefinition = dest.FindMethod(method.FullName);

        // If a matching method is found, update the instruction's operand to this method
        if (methodDefinition != null) instruction.Operand = methodDefinition;
    }

    /// <summary>
    /// Updates the ReturnType and Parameters of the CallSite to the destination type when merging classes.
    /// </summary>
    /// <param name="callSite">CallSite that needs its return type and parameters updated.</param>
    /// <param name="src">The original type which is being replaced.</param>
    /// <param name="dest">The new type which is replacing the original type.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void UpdateInstructionTypes(this CallSite callSite, TypeDefinition src, TypeDefinition dest)
    {
        // Ensure that none of the arguments are null
        if (callSite == null) throw new ArgumentNullException(nameof(callSite), "The parameter callSite cannot be null.");
        if (src == null) throw new ArgumentNullException(nameof(src), "The parameter src cannot be null.");
        if (dest == null) throw new ArgumentNullException(nameof(dest), "The parameter dest cannot be null.");

        // Update callsite parameters to destination type
        callSite.Parameters.UpdateTypes(src, dest);

        // If the current return type is the source type, update it to destination type
        if (callSite.ReturnType == src) callSite.ReturnType = dest;
    }

    /// <summary>
    /// Updates all instructions in the method's body.
    /// </summary>
    /// <param name="method">Method whose instructions are to be updated.</param>
    /// <param name="src">The original type which is being replaced.</param>
    /// <param name="dest">The new type which is replacing the original type.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void UpdateInstructionTypes(this MethodDefinition method, TypeDefinition src, TypeDefinition dest)
    {
        // Ensure that none of the arguments are null
        if (method == null) throw new ArgumentNullException(nameof(method), "The parameter method cannot be null.");
        if (src == null) throw new ArgumentNullException(nameof(src), "The parameter src cannot be null.");
        if (dest == null) throw new ArgumentNullException(nameof(dest), "The parameter dest cannot be null.");

        // Update instructions in the method body to the destination type
        method.Body?.Instructions?.UpdateInstructionTypes(src, dest);
    }

    /// <summary>
    /// Updates all instructions in the given collection.
    /// </summary>
    /// <param name="instructions">Collection of instructions to update.</param>
    /// <param name="src">The original type which is being replaced.</param>
    /// <param name="dest">The new type which is replacing the original type.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void UpdateInstructionTypes(this Collection<Instruction> instructions, TypeDefinition src, TypeDefinition dest)
    {
        // Ensure that none of the arguments are null
        if (instructions == null) throw new ArgumentNullException(nameof(instructions), "The parameter instructions cannot be null.");
        if (src == null) throw new ArgumentNullException(nameof(src), "The parameter src cannot be null.");
        if (dest == null) throw new ArgumentNullException(nameof(dest), "The parameter dest cannot be null.");

        // Iterate through each instruction in the collection and update its types
        foreach (var instruction in instructions)
            instruction.UpdateInstructionTypes(src, dest);
    }

    /// <summary>
    /// Calls UpdateInstructionTypes method on each item in the collection, if available.
    /// Uses dynamic dispatch to resolve the appropriate UpdateInstructionTypes method at runtime based on the actual type of each item.
    /// </summary>
    /// <typeparam name="T">The type of the objects within the collection.</typeparam>
    /// <param name="collection">Collection of items on which UpdateInstructionTypes method is to be called.</param>
    /// <param name="src">The original type which is being replaced.</param>
    /// <param name="dest">The new type which is replacing the original type references.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    /// <exception cref="RuntimeBinderException">Thrown when the type T does not have a suitable UpdateInstructionTypes method.</exception>
    public static void UpdateInstructionTypes<T>(this Collection<T> collection, TypeDefinition src, TypeDefinition dest)
    {
        // Ensure that none of the arguments are null
        if (collection == null) throw new ArgumentNullException(nameof(collection), "The parameter collection cannot be null.");
        if (src == null) throw new ArgumentNullException(nameof(src), "The parameter src cannot be null.");
        if (dest == null) throw new ArgumentNullException(nameof(dest), "The parameter dest cannot be null.");

        // Attempt to call UpdateInstructionTypes method on each item in the collection
        foreach (var item in collection)
            UpdateInstructionTypes(item as dynamic, src, dest); // Use dynamic dispatch to find a suitable UpdateInstructionTypes method at runtime
    }

    /// <summary>
    /// Updates the instruction types within the given collection. This operation occurs for each pair of source and destination types provided.
    /// </summary>
    /// <typeparam name="T">The type of instructions contained within the collection.</typeparam>
    /// <param name="collection">The collection whose instruction types need to be updated.</param>
    /// <param name="srcTypes">The collection of source types.</param>
    /// <param name="destTypes">The collection of destination types.</param>
    public static void UpdateInstructionTypes<T>(this Collection<T> collection, Collection<TypeDefinition> srcTypes, Collection<TypeDefinition> destTypes)
    {
        // Loop over each source-destination type pair
        for (int i = 0; i < destTypes.Count; ++i)
        {
            // Extract source and destination types from the provided collections
            var src = srcTypes[i];
            var dest = destTypes[i];

            // Update instruction types within the collection
            collection.UpdateInstructionTypes(src, dest);
        }
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
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void UpdateGettersAndSetters(this PropertyDefinition property, TypeDefinition src, TypeDefinition dest)
    {
        // Ensure that none of the arguments are null
        if (property == null) throw new ArgumentNullException(nameof(property), "The parameter property cannot be null.");
        if (src == null) throw new ArgumentNullException(nameof(src), "The parameter src cannot be null.");
        if (dest == null) throw new ArgumentNullException(nameof(dest), "The parameter dest cannot be null.");

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

    /// <summary>
    /// Iterates over a collection of PropertyDefinition items, updating their getters and setters to reference the destination type.
    /// This method is useful when multiple properties need to be updated as part of class merging.
    /// </summary>
    /// <param name="collection">A collection of PropertyDefinition items whose getters and setters need to be updated.</param>
    /// <param name="src">The original type which is being replaced.</param>
    /// <param name="dest">The new type which is replacing the original type.</param>
    /// <exception cref="ArgumentNullException">Thrown if the collection parameter is null.</exception>
    public static void UpdateGettersAndSetters(this Collection<PropertyDefinition> collection, TypeDefinition src, TypeDefinition dest)
    {
        // Check that this collection isn't null
        if (collection == null) throw new ArgumentNullException(nameof(collection), "The parameter collection cannot be null.");

        // Iterates over each property in the collection, updating its getters and setters
        foreach (var property in collection)
            property.UpdateGettersAndSetters(src, dest);
    }

    /// <summary>
    /// Updates the getter and setter methods of properties within the given collection. This operation occurs for each pair of source and destination types provided.
    /// </summary>
    /// <param name="collection">The collection of PropertyDefinition objects whose getter and setter methods need to be updated.</param>
    /// <param name="srcTypes">The collection of source types.</param>
    /// <param name="destTypes">The collection of destination types.</param>
    public static void UpdateGettersAndSetters(this Collection<PropertyDefinition> collection, Collection<TypeDefinition> srcTypes, Collection<TypeDefinition> destTypes)
    {
        // Loop over each source-destination type pair
        for (int i = 0; i < destTypes.Count; ++i)
        {
            // Extract source and destination types from the provided collections
            var src = srcTypes[i];
            var dest = destTypes[i];

            // Update getter and setter methods for properties within the collection
            collection.UpdateGettersAndSetters(src, dest);
        }
    }

    #endregion UpdateGettersAndSetters

    // Extension methods to import references from one module to another.
    // This is important when merging assemblies classes as it allows the destination to access types that may not have been referenced prior.
    #region ImportReferences

    /// <summary>
    /// Imports the constructor reference for a given attribute into a module type module.
    /// </summary>
    /// <param name="attribute">The custom attribute whose constructor reference needs to be imported.</param>
    /// <param name="module">The module type into whose module the reference should be imported.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void ImportReferences(this CustomAttribute attribute, ModuleDefinition module)
    {
        // Ensure that none of the arguments are null
        if (attribute == null) throw new ArgumentNullException(nameof(attribute), "The parameter attribute cannot be null.");
        if (module == null) throw new ArgumentNullException(nameof(module), "The parameter module cannot be null.");

        // Import the constructor reference into the module type module
        attribute.Constructor = module.ImportReference(attribute.Constructor);
    }
    /// <summary>
    /// Imports the field type and custom attributes references of a field into a module type module.
    /// </summary>
    /// <param name="field">The field whose references need to be imported.</param>
    /// <param name="module">The module type into whose module the references should be imported.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void ImportReferences(this FieldDefinition field, ModuleDefinition module)
    {
        // Ensure that none of the arguments are null
        if (field == null) throw new ArgumentNullException(nameof(field), "The parameter field cannot be null.");
        if (module == null) throw new ArgumentNullException(nameof(module), "The parameter module cannot be null.");

        // Import the custom attributes references into the module type module
        field.CustomAttributes.ImportReferences(module);

        // Import the field type reference into the module type module
        field.FieldType = module.ImportReference(field.FieldType);
    }

    /// <summary>
    /// Imports the property type and custom attributes references of a property into a module type module.
    /// </summary>
    /// <param name="property">The property whose references need to be imported.</param>
    /// <param name="module">The module type into whose module the references should be imported.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void ImportReferences(this PropertyDefinition property, ModuleDefinition module)
    {
        // Ensure that none of the arguments are null
        if (property == null) throw new ArgumentNullException(nameof(property), "The parameter property cannot be null.");
        if (module == null) throw new ArgumentNullException(nameof(module), "The parameter module cannot be null.");

        // Import the custom attributes references into the module type module
        property.CustomAttributes.ImportReferences(module);

        // Import the property type reference into the module type module
        property.PropertyType = module.ImportReference(property.PropertyType);
    }

    /// <summary>
    /// Imports the parameter type and custom attributes references of a parameter into a module type module.
    /// </summary>
    /// <param name="parameter">The parameter whose references need to be imported.</param>
    /// <param name="module">The module type into whose module the references should be imported.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void ImportReferences(this ParameterDefinition parameter, ModuleDefinition module)
    {
        // Ensure that none of the arguments are null
        if (parameter == null) throw new ArgumentNullException(nameof(parameter), "The parameter parameter cannot be null.");
        if (module == null) throw new ArgumentNullException(nameof(module), "The parameter module cannot be null.");

        // Import the custom attributes references into the module type module
        parameter.CustomAttributes.ImportReferences(module);

        // Import the parameter type reference into the module type module
        parameter.ParameterType = module.ImportReference(parameter.ParameterType);
    }

    /// <summary>
    /// Imports the variable type references of a variable into a module type module.
    /// </summary>
    /// <param name="variable">The variable whose type references need to be imported.</param>
    /// <param name="module">The module type into whose module the references should be imported.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void ImportReferences(this VariableDefinition variable, ModuleDefinition module)
    {
        // Ensure that none of the arguments are null
        if (variable == null) throw new ArgumentNullException(nameof(variable), "The parameter variable cannot be null.");
        if (module == null) throw new ArgumentNullException(nameof(module), "The parameter module cannot be null.");

        // Import the variable type reference into the module type module
        variable.VariableType = module.ImportReference(variable.VariableType);
    }
    /// <summary>
    /// Imports the method type references and the custom attributes of a method into a module type module.
    /// </summary>
    /// <param name="method">The method whose references need to be imported.</param>
    /// <param name="module">The module type into whose module the references should be imported.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void ImportReferences(this MethodDefinition method, ModuleDefinition module)
    {
        // Ensure that none of the arguments are null
        if (method == null) throw new ArgumentNullException(nameof(method), "The parameter method cannot be null.");
        if (module == null) throw new ArgumentNullException(nameof(module), "The parameter module cannot be null.");

        // Import the custom attributes references into the module type module
        method.CustomAttributes.ImportReferences(module);

        // Import the parameter type references into the module type module
        method.Parameters.ImportReferences(module);

        // Import the return type reference into the module type module
        method.ReturnType = module.ImportReference(method.ReturnType);

        // Import the variable type references in the method body into the module type module
        method.Body?.Variables.ImportReferences(module);

        // Import the instruction type references in the method body into the module type module
        method.Body?.Instructions.ImportReferences(module);
    }

    /// <summary>
    /// Imports the operand type reference of a particular instruction into a module type module.
    /// </summary>
    /// <param name="instruction">The instruction whose operand type reference needs to be imported.</param>
    /// <param name="type">The operand type reference to be imported.</param>
    /// <param name="module">The module type into whose module the operand type reference should be imported.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void ImportReferences(this Instruction instruction, TypeReference type, ModuleDefinition module)
    {
        // Ensure that none of the arguments are null
        if (instruction == null) throw new ArgumentNullException(nameof(instruction), "The parameter instruction cannot be null.");
        if (type == null) throw new ArgumentNullException(nameof(type), "The parameter type cannot be null.");
        if (module == null) throw new ArgumentNullException(nameof(module), "The parameter module cannot be null.");

        // Import the operand type reference of the instruction into the module type module
        instruction.Operand = module.ImportReference(type);
    }

    /// <summary>
    /// Imports the field type references of a field into a module type module.
    /// </summary>
    /// <param name="field">The field whose type references need to be imported.</param>
    /// <param name="module">The module type into whose module the references should be imported.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void ImportReferences(this FieldReference field, ModuleDefinition module)
    {
        // Ensure that none of the arguments are null
        if (field == null) throw new ArgumentNullException(nameof(field), "The parameter field cannot be null.");
        if (module == null) throw new ArgumentNullException(nameof(module), "The parameter module cannot be null.");

        // Import the field type reference into the module type module
        field.FieldType = module.ImportReference(field.FieldType);

        // Import the declaring type reference of the field into the module type module
        field.DeclaringType = module.ImportReference(field.DeclaringType);
    }

    /// <summary>
    /// Imports the method type references of a method into a module type module.
    /// </summary>
    /// <param name="method">The method whose type references need to be imported.</param>
    /// <param name="module">The module type into whose module the references should be imported.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void ImportReferences(this MethodReference method, ModuleDefinition module)
    {
        // Ensure that none of the arguments are null
        if (method == null) throw new ArgumentNullException(nameof(method), "The parameter method cannot be null.");
        if (module == null) throw new ArgumentNullException(nameof(module), "The parameter module cannot be null.");

        // Import the parameter type references of the method into the module type module
        method.Parameters.ImportReferences(module);

        // Import the return type reference of the method into the module type module
        method.ReturnType = module.ImportReference(method.ReturnType);

        // Import the declaring type reference of the method into the module type module
        method.DeclaringType = module.ImportReference(method.DeclaringType);
    }

    /// <summary>
    /// Imports the return type references of a CallSite into a module type module.
    /// </summary>
    /// <param name="callSite">The CallSite whose return type references need to be imported.</param>
    /// <param name="module">The module type into whose module the references should be imported.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void ImportReferences(this CallSite callSite, ModuleDefinition module)
    {
        // Ensure that none of the arguments are null
        if (callSite == null) throw new ArgumentNullException(nameof(callSite), "The parameter callSite cannot be null.");
        if (module == null) throw new ArgumentNullException(nameof(module), "The parameter module cannot be null.");

        // Import the return type reference of the callSite into the module type module
        callSite.ReturnType = module.ImportReference(callSite.ReturnType);
    }
    /// <summary>
    /// Imports the operand type references of an instruction into a module type module.
    /// </summary>
    /// <param name="instruction">The instruction whose operand references need to be imported.</param>
    /// <param name="module">The module type into whose module the references should be imported.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void ImportReferences(this Instruction instruction, ModuleDefinition module)
    {
        // Ensure that none of the arguments are null
        if (instruction == null) throw new ArgumentNullException(nameof(instruction), "The parameter instruction cannot be null.");
        if (module == null) throw new ArgumentNullException(nameof(module), "The parameter module cannot be null.");

        // Import the operand references of the instruction into the module type module
        if (instruction.Operand is ParameterDefinition parameter)
            parameter.ImportReferences(module);
        else if (instruction.Operand is VariableDefinition variable)
            variable.ImportReferences(module);
        else if (instruction.Operand is TypeReference type)
            instruction.ImportReferences(type, module);
        else if (instruction.Operand is FieldReference field)
            field.ImportReferences(module);
        else if (instruction.Operand is MethodReference method)
            method.ImportReferences(module);
        else if (instruction.Operand is CallSite callSite)
            callSite.ImportReferences(module);
    }

    /// <summary>
    /// Imports the references of a collection into a module type module.
    /// </summary>
    /// <typeparam name="T">The type of the items in the collection.</typeparam>
    /// <param name="collection">The collection whose items' references need to be imported.</param>
    /// <param name="module">The module type into whose module the references should be imported.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void ImportReferences<T>(this Collection<T> collection, ModuleDefinition module)
    {
        // Ensure that none of the arguments are null
        if (collection == null) throw new ArgumentNullException(nameof(collection), "The parameter collection cannot be null.");
        if (module == null) throw new ArgumentNullException(nameof(module), "The parameter module cannot be null.");

        // Iterate over each item in the collection and import its references
        foreach (var item in collection)
            ImportReferences(item as dynamic, module);
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
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void SwapMethodReferences(this Instruction instruction, MethodDefinition leftMethod, MethodDefinition rightMethod)
    {
        // Ensure that none of the arguments are null
        if (instruction == null) throw new ArgumentNullException(nameof(instruction), "The parameter instruction cannot be null.");
        if (leftMethod == null) throw new ArgumentNullException(nameof(leftMethod), "The parameter leftMethod cannot be null.");
        if (rightMethod == null) throw new ArgumentNullException(nameof(rightMethod), "The parameter rightMethod cannot be null.");

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
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void SwapMethodReferences(this Collection<Instruction> instructions, MethodDefinition leftMethod, MethodDefinition rightMethod)
    {
        // Ensure that none of the arguments are null
        if (instructions == null) throw new ArgumentNullException(nameof(instructions), "The parameter instructions cannot be null.");
        if (leftMethod == null) throw new ArgumentNullException(nameof(leftMethod), "The parameter leftMethod cannot be null.");
        if (rightMethod == null) throw new ArgumentNullException(nameof(rightMethod), "The parameter rightMethod cannot be null.");

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
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void SwapMethodReferences(this MethodDefinition method, MethodDefinition leftMethod, MethodDefinition rightMethod)
    {
        // Ensure that none of the arguments are null
        if (method == null) throw new ArgumentNullException(nameof(method), "The parameter method cannot be null.");
        if (leftMethod == null) throw new ArgumentNullException(nameof(leftMethod), "The parameter leftMethod cannot be null.");
        if (rightMethod == null) throw new ArgumentNullException(nameof(rightMethod), "The parameter rightMethod cannot be null.");

        // Swap method references for each instruction in the method's body
        if (method.Body?.Instructions != null)
            method.Body.Instructions.SwapMethodReferences(leftMethod, rightMethod);
    }

    /// <summary>
    /// Swaps the attributes, parameters, custom attributes, and generic parameters between two given methods.
    /// </summary>
    /// <param name="leftMethod">The first method to swap.</param>
    /// <param name="rightMethod">The second method to swap.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void SwapMethods(this MethodDefinition leftMethod, MethodDefinition rightMethod)
    {
        // Ensure that none of the arguments are null
        if (leftMethod == null) throw new ArgumentNullException(nameof(leftMethod), "The parameter leftMethod cannot be null.");
        if (rightMethod == null) throw new ArgumentNullException(nameof(rightMethod), "The parameter rightMethod cannot be null.");

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
        leftMethod.Parameters.Add(rightMethod.Parameters);
        leftMethod.CustomAttributes.Clear();
        leftMethod.CustomAttributes.Add(rightMethod.CustomAttributes);
        leftMethod.GenericParameters.Clear();
        leftMethod.GenericParameters.Add(rightMethod.GenericParameters);

        // Swap the details from the left method (which were saved) to the right
        rightMethod.Body = leftBody;
        rightMethod.Body = leftBody;
        rightMethod.Attributes = leftAttributes;
        rightMethod.ImplAttributes = leftImplAttributes;
        rightMethod.SemanticsAttributes = leftSemanticsAttributes;
        rightMethod.Parameters.Clear();
        rightMethod.Parameters.Add(leftParameters);
        rightMethod.CustomAttributes.Clear();
        rightMethod.CustomAttributes.Add(leftCustomAttributes);
        rightMethod.GenericParameters.Clear();
        rightMethod.GenericParameters.Add(leftGenericParameters);

        // Swap method references within each method body
        leftMethod.SwapMethodReferences(leftMethod, rightMethod);
        rightMethod.SwapMethodReferences(leftMethod, rightMethod);
    }

    /// <summary>
    /// Finds and swaps methods with the same full name within the given type.
    /// </summary>
    /// <param name="type">The type to modify.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void SwapDuplicateMethods(this TypeDefinition type)
    {
        // Check that this type isn't null
        if (type == null) throw new ArgumentNullException(nameof(type), "The parameter type cannot be null.");

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
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void SwapDuplicateMethods(this Collection<TypeDefinition> types)
    {
        // Check that this types isn't null
        if (types == null) throw new ArgumentNullException(nameof(types), "The parameter types cannot be null.");

        // Swap duplicate methods for each type in the collection
        foreach (var type in types)
            type.SwapDuplicateMethods();
    }

    #endregion SwapMethods

    // Extension method that handles the addition of fields, properties, and methods from a source type to a destination type.
    // This is a key part of merging two types, ensuring the destination type includes all necessary components from the source type.
    #region AddFieldsPropertiesAndMethods

    /// <summary>
    /// Merges the source type into the destination type by cloning the fields, properties, and methods of the source, updating their types and adding them to the destination.
    /// </summary>
    /// <param name="dest">The destination type definition where fields, properties, and methods from source will be added.</param>
    /// <param name="src">The source type definition whose fields, properties, and methods will be cloned and added to the destination.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void AddFieldsPropertiesAndMethods(this TypeDefinition dest, TypeDefinition src)
    {
        // Ensure that none of the arguments are null
        if (dest == null) throw new ArgumentNullException(nameof(dest), "The parameter dest cannot be null.");
        if (src == null) throw new ArgumentNullException(nameof(src), "The parameter src cannot be null.");

        // Check that src's Fields, Properties, and Methods aren't null
        if (src.Fields == null || src.Properties == null || src.Methods == null) throw new ArgumentNullException(nameof(src), "Fields, Properties, or Methods of the source TypeDefinition cannot be null.");
        // Check that dest's Methods aren't null
        if (dest.Methods == null) throw new ArgumentNullException(nameof(dest), "Methods of the destination TypeDefinition cannot be null.");

        // Clone fields from the source and add to the destination
        var clonedFields = src.Fields.Clone() ?? throw new Exception("Cloning of Fields failed.");
        foreach (var field in clonedFields)
            dest.Fields.Insert(0, field);

        // Clone properties from the source and add to the destination
        var clonedProperties = src.Properties.Clone() ?? throw new Exception("Cloning of Properties failed.");
        dest.Properties.Add(clonedProperties);

        // Clone methods from the source
        var clonedMethods = src.Methods.Clone() ?? throw new Exception("Cloning of Methods failed.");

        // List for keeping track of methods that need further processing
        var methodsToUpdate = new Collection<MethodDefinition>();

        // Process each method
        foreach (var clonedMethod in clonedMethods.ToList())
        {
            if (clonedMethod == null || clonedMethod.Body == null || clonedMethod.Body.Instructions == null)
                throw new ArgumentNullException(nameof(clonedMethod), "clonedMethod, its Body, or its Body.Instructions cannot be null.");

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
                    methodsToUpdate.Add(destMethod);
                }
                else
                {
                    // Add the cloned constructor to the destination type
                    methodsToUpdate.Add(clonedMethod);
                }
            }
            else
            {
                // For non-constructor/non-destructor methods
                methodsToUpdate.Add(clonedMethod);
            }
        }

        // Add updated methods to the destination type
        dest.Methods.Add(clonedMethods);

        // Add updated fields, properties and methods to the update info
        if (!assemblyUpdateInfo.TryGetValue(dest.Module.Assembly, out var updateInfo))
            updateInfo = assemblyUpdateInfo[dest.Module.Assembly] = new();
        updateInfo.updatedFields.Add(clonedFields);
        updateInfo.updatedProperties.Add(clonedProperties);
        updateInfo.updatedMethods.Add(methodsToUpdate);

        // Add source and destination types to the update info
        updateInfo.srcTypes.Add(src);
        updateInfo.destTypes.Add(dest);
    }

    #endregion AddFieldsPropertiesAndMethods

    // Extension methods that handle the updating of fields, properties, and methods within a destination type after they have been cloned from a source type.
    // These methods ensure that the newly added components in the destination type correctly reference the destination type, rather than the original source type.
    #region UpdateFieldsPropertiesAndMethods

    /// <summary>
    /// Updates the fields, properties, and methods within a given assembly.
    /// This includes updating the types, getters and setters, and instruction types, as well as importing references and swapping duplicate methods.
    /// </summary>
    /// <param name="assembly">The assembly to be updated.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void UpdateFieldsPropertiesAndMethods(this AssemblyDefinition assembly)
    {
        // Check that this assembly isn't null
        if (assembly == null) throw new ArgumentNullException(nameof(assembly), "The parameter assembly cannot be null.");

        // Check if update information exists for the assembly
        if (assemblyUpdateInfo.TryGetValue(assembly, out var updateInfo))
        {
            // Update types in fields, properties, and methods
            updateInfo.updatedFields.UpdateTypes(updateInfo.srcTypes, updateInfo.destTypes);
            updateInfo.updatedProperties.UpdateTypes(updateInfo.srcTypes, updateInfo.destTypes);
            updateInfo.updatedMethods.UpdateTypes(updateInfo.srcTypes, updateInfo.destTypes);

            // Update getter and setter methods for properties
            updateInfo.updatedProperties.UpdateGettersAndSetters(updateInfo.srcTypes, updateInfo.destTypes);

            // Update instruction types for methods
            updateInfo.updatedMethods.UpdateInstructionTypes(updateInfo.srcTypes, updateInfo.destTypes);

            // Import references into the fields, properties, and methods from the main module of the assembly
            updateInfo.updatedFields.ImportReferences(assembly.MainModule);
            updateInfo.updatedProperties.ImportReferences(assembly.MainModule);
            updateInfo.updatedMethods.ImportReferences(assembly.MainModule);

            // Swap any duplicate methods in the destination types
            updateInfo.destTypes.SwapDuplicateMethods();

            // Remove the assembly from the update information collection
            _ = assemblyUpdateInfo.Remove(assembly);
        }
    }

    #endregion UpdateFieldsPropertiesAndMethods

}
#endif
