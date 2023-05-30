#if UNITY_EDITOR
using System;
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
    #region Base // Basic extension methods for loading assemblies, Adding elements to collections, and Finding types, fields, and methods in Mono.Cecil objects

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
    /// This extension method finds a type in an assembly.
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
        // Return the first method that matches the provided method signature.
        return type.Methods.FirstOrDefault(m => m.FullName == methodSignature || m.Name == methodSignature);
    }

    public static Collection<MethodDefinition> FindMethods(this TypeDefinition type, string methodSignature)
    {
        var collection = new Collection<MethodDefinition>();
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
        // Check that this collection isn't null
        if (collection == null) throw new ArgumentNullException(nameof(otherCollection), "The collection to add to cannot be null.");
        // Check that the other collection isn't null
        if (otherCollection == null) throw new ArgumentNullException(nameof(otherCollection), "The collection to be added cannot be null.");

        // Add items to collection
        foreach (var otherT in otherCollection)
            collection.Add(otherT);
    }

    #endregion Base // Basic extension methods for loading assemblies, Adding elements to collections, and Finding types, fields, and methods in Mono.Cecil objects

    #region Clone // Extension methods for cloning various Mono.Cecil objects

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

    #endregion Clone // Extension methods for cloning various Mono.Cecil objects

    #region UpdateReferences // Extension methods for Replacing references to a source type with references to a destination type within Mono.Cecil objects

    // The following UpdateType extension methods replace references to a source type (src)
    // with references to a destination type (dest) within different Mono.Cecil objects.
    // These methods are used to merge a class of type src into a class of type dest.

    /// <summary>
    /// Updates the constructor of the CustomAttribute to the destination module when merging classes.
    /// </summary>
    /// <param name="attribute">CustomAttribute that needs its constructor updated.</param>
    /// <param name="src">The original type which is being replaced.</param>
    /// <param name="dest">The new type which is replacing the original type.</param>
    /// <returns>Does not return a value.</returns>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void UpdateReferences(this CustomAttribute attribute, TypeDefinition src, TypeDefinition dest)
    {
        // Check that attribute isn't null
        if (attribute == null) throw new ArgumentNullException(nameof(attribute), "The parameter attribute cannot be null.");
        // Check that src isn't null
        if (src == null) throw new ArgumentNullException(nameof(src), "The parameter src cannot be null.");
        // Check that dest isn't null
        if (dest == null) throw new ArgumentNullException(nameof(dest), "The parameter dest cannot be null.");

        // Suppress unused parameter warning - parameter is needed to match other interfaces
        _ = src;

        // Update Constructor method type reference to destination module
        attribute.Constructor = dest.Module.ImportReference(attribute.Constructor);
    }

    /// <summary>
    /// Updates the CustomAttributes and FieldType of the FieldDefinition to the destination type when merging classes.
    /// </summary>
    /// <param name="field">FieldDefinition that needs its attributes and type updated.</param>
    /// <param name="src">The original type which is being replaced.</param>
    /// <param name="dest">The new type which is replacing the original type.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void UpdateReferences(this FieldDefinition field, TypeDefinition src, TypeDefinition dest)
    {
        // Check that field isn't null
        if (field == null) throw new ArgumentNullException(nameof(field), "The parameter field cannot be null.");
        // Check that src isn't null
        if (src == null) throw new ArgumentNullException(nameof(src), "The parameter src cannot be null.");
        // Check that dest isn't null
        if (dest == null) throw new ArgumentNullException(nameof(dest), "The parameter dest cannot be null.");

        // Update custom attributes to destination type
        field.CustomAttributes.UpdateReferences(src, dest);

        // If the current field type is the source type, update it to destination type
        if (field.FieldType == src) field.FieldType = dest;

        // Update field type reference to destination module
        field.FieldType = dest.Module.ImportReference(field.FieldType);
    }

    /// <summary>
    /// Updates the Constructor of the CustomAttributes and the PropertyType of the PropertyDefinition to the destination type when merging classes.
    /// </summary>
    /// <param name="property">PropertyDefinition that needs its attributes' constructors and type updated.</param>
    /// <param name="src">The original type which is being replaced.</param>
    /// <param name="dest">The new type which is replacing the original type.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void UpdateReferences(this PropertyDefinition property, TypeDefinition src, TypeDefinition dest)
    {
        // Check that property isn't null
        if (property == null) throw new ArgumentNullException(nameof(property), "The parameter property cannot be null.");
        // Check that src isn't null
        if (src == null) throw new ArgumentNullException(nameof(src), "The parameter src cannot be null.");
        // Check that dest isn't null
        if (dest == null) throw new ArgumentNullException(nameof(dest), "The parameter dest cannot be null.");

        // Update custom attributes to destination type
        property.CustomAttributes.UpdateReferences(src, dest);

        // If the current property type is the source type, update it to destination type
        if (property.PropertyType == src) property.PropertyType = dest;

        // Update property type reference to destination module
        property.PropertyType = dest.Module.ImportReference(property.PropertyType);
    }

    /// <summary>
    /// Updates the ParameterType of the ParameterDefinition to the destination type when merging classes.
    /// </summary>
    /// <param name="parameter">ParameterDefinition that needs its type updated.</param>
    /// <param name="src">The original type which is being replaced.</param>
    /// <param name="dest">The new type which is replacing the original type.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void UpdateReferences(this ParameterDefinition parameter, TypeDefinition src, TypeDefinition dest)
    {
        // Check that parameter isn't null
        if (parameter == null) throw new ArgumentNullException(nameof(parameter), "The parameter parameter cannot be null.");
        // Check that src isn't null
        if (src == null) throw new ArgumentNullException(nameof(src), "The parameter src cannot be null.");
        // Check that dest isn't null
        if (dest == null) throw new ArgumentNullException(nameof(dest), "The parameter dest cannot be null.");

        // If the current parameter type is the source type, update it to destination type
        if (parameter.ParameterType == src) parameter.ParameterType = dest;

        // Update parameter type reference to destination module
        parameter.ParameterType = dest.Module.ImportReference(parameter.ParameterType);
    }

    /// <summary>
    /// Updates the VariableType of the VariableDefinition to the destination type when merging classes.
    /// </summary>
    /// <param name="variable">VariableDefinition that needs its type updated.</param>
    /// <param name="src">The original type which is being replaced.</param>
    /// <param name="dest">The new type which is replacing the original type.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void UpdateReferences(this VariableDefinition variable, TypeDefinition src, TypeDefinition dest)
    {
        // Check that variable isn't null
        if (variable == null) throw new ArgumentNullException(nameof(variable), "The parameter variable cannot be null.");
        // Check that src isn't null
        if (src == null) throw new ArgumentNullException(nameof(src), "The parameter src cannot be null.");
        // Check that dest isn't null
        if (dest == null) throw new ArgumentNullException(nameof(dest), "The parameter dest cannot be null.");

        // If the current variable type is the source type, update it to destination type
        if (variable.VariableType == src) variable.VariableType = dest;

        // Update variable type reference to destination module
        variable.VariableType = dest.Module.ImportReference(variable.VariableType);
    }

    /// <summary>
    /// Updates the CustomAttributes, Parameters, ReturnType, Variables and Instructions of the MethodDefinition to the destination type when merging classes.
    /// </summary>
    /// <param name="method">MethodDefinition that needs its attributes, parameters, return type, variables and instructions updated.</param>
    /// <param name="src">The original type which is being replaced.</param>
    /// <param name="dest">The new type which is replacing the original type.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void UpdateReferences(this MethodDefinition method, TypeDefinition src, TypeDefinition dest)
    {
        // Check that method isn't null
        if (method == null) throw new ArgumentNullException(nameof(method), "The parameter method cannot be null.");
        // Check that src isn't null
        if (src == null) throw new ArgumentNullException(nameof(src), "The parameter src cannot be null.");
        // Check that dest isn't null
        if (dest == null) throw new ArgumentNullException(nameof(dest), "The parameter dest cannot be null.");

        // Update custom attributes to destination type
        method.CustomAttributes.UpdateReferences(src, dest);

        // Update method parameters to destination type
        method.Parameters.UpdateReferences(src, dest);

        // If the current return type is the source type, update it to destination type
        if (method.ReturnType == src) method.ReturnType = dest;

        // Update return type reference to destination module
        method.ReturnType = dest.Module.ImportReference(method.ReturnType);

        // Update variables in the method body (if exists) to destination type
        method.Body?.Variables.UpdateReferences(src, dest);
    }

    /// <summary>
    /// Updates the type of each item in the collection to the destination type when merging classes.
    /// </summary>
    /// <typeparam name="T">The type of the objects within the collection.</typeparam>
    /// <param name="collection">The collection of items that need their type updated.</param>
    /// <param name="src">The original type which is being replaced.</param>
    /// <param name="dest">The new type which is replacing the original type.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    /// <exception cref="RuntimeBinderException">Thrown when the type T does not have a Clone method.</exception>
    public static void UpdateReferences<T>(this Collection<T> collection, TypeDefinition src, TypeDefinition dest)
    {
        // Check that collection isn't null
        if (collection == null) throw new ArgumentNullException(nameof(collection), "The parameter collection cannot be null.");
        // Check that src isn't null
        if (src == null) throw new ArgumentNullException(nameof(src), "The parameter src cannot be null.");
        // Check that dest isn't null
        if (dest == null) throw new ArgumentNullException(nameof(dest), "The parameter dest cannot be null.");

        // Iterate through each item in the collection
        foreach (var item in collection)
        {
            // Cast the item to dynamic to resolve the correct UpdateType method at runtime
            // And then call UpdateType for that item
            UpdateReferences(item as dynamic, src, dest);
        }
    }

    #endregion UpdateReferences // Extension methods for Replacing references to a source type with references to a destination type within Mono.Cecil objects

    #region UpdateInstructionReferences // Extension methods for Replacing references to a source type with references to a destination type within Mono.Cecil.Instruction objects

    /// <summary>
    /// Updates the Operand of an instruction when merging classes.
    /// The update strategy depends on the type of the operand.
    /// If the operand is a ParameterDefinition, VariableDefinition, FieldReference, MethodReference, CallSite, or TypeReference, it's updated accordingly.
    /// </summary>
    /// <param name="instruction">Instruction that needs its operand updated.</param>
    /// <param name="src">The original type which is being replaced.</param>
    /// <param name="dest">The new type which is replacing the original type.</param>
    /// <exception cref="ArgumentNullException">Thrown if instruction, src, or dest are null.</exception>
    public static void UpdateInstructionReferences(this Instruction instruction, TypeDefinition src, TypeDefinition dest)
    {
        // Check that instruction isn't null
        if (instruction == null) throw new ArgumentNullException(nameof(instruction), "The parameter instruction cannot be null.");
        // Check that src isn't null
        if (src == null) throw new ArgumentNullException(nameof(src), "The parameter src cannot be null.");
        // Check that dest isn't null
        if (dest == null) throw new ArgumentNullException(nameof(dest), "The parameter dest cannot be null.");

        // Depending on the type of operand, update it accordingly
        if (instruction.Operand is ParameterDefinition param)
            param.UpdateReferences(src, dest);
        else if (instruction.Operand is VariableDefinition variable)
            variable.UpdateReferences(src, dest);
        else if (instruction.Operand is TypeReference type)
            instruction.UpdateInstructionReferences(type, src, dest);
        else if (instruction.Operand is FieldReference field)
            instruction.UpdateInstructionReferences(field, src, dest);
        else if (instruction.Operand is MethodReference method)
            instruction.UpdateInstructionReferences(method, src, dest);
        else if (instruction.Operand is CallSite callSite)
            callSite.UpdateInstructionReferences(src, dest);
    }

    /// <summary>
    /// Updates the Operand of an instruction when merging classes.
    /// If the TypeReference of the operand matches the source type, it's replaced with the destination type.
    /// If not, it imports the reference from the destination module and updates the operand to that.
    /// </summary>
    /// <param name="instruction">The instruction whose operand needs to be updated.</param>
    /// <param name="type">TypeReference that serves as a template for the updating process.</param>
    /// <param name="src">The original type which is being replaced.</param>
    /// <param name="dest">The new type which is replacing the original type.</param>
    /// <exception cref="ArgumentNullException">Thrown if instruction, type, src, or dest are null.</exception>
    public static void UpdateInstructionReferences(this Instruction instruction, TypeReference type, TypeDefinition src, TypeDefinition dest)
    {
        // Check that instruction isn't null
        if (instruction == null) throw new ArgumentNullException(nameof(instruction), "The parameter instruction cannot be null.");
        // Check that type isn't null
        if (type == null) throw new ArgumentNullException(nameof(type), "The parameter type cannot be null.");
        // Check that src isn't null
        if (src == null) throw new ArgumentNullException(nameof(src), "The parameter src cannot be null.");
        // Check that dest isn't null
        if (dest == null) throw new ArgumentNullException(nameof(dest), "The parameter dest cannot be null.");

        // If the type is the source, update it to the destination. 
        // Otherwise, import the reference from the destination module and update the operand to that.
        instruction.Operand = type == src ? dest : dest.Module.ImportReference(type);
    }

    /// <summary>
    /// Updates the Operand of an instruction when merging classes.
    /// Updates the FieldType and DeclaringType of a FieldReference operand if they match the source type.
    /// If a matching field is found in the destination type, the Operand is updated to this field.
    /// If no matching field is found, the FieldType and DeclaringType are imported from the destination module.
    /// </summary>
    /// <param name="instruction">The instruction whose operand needs to be updated.</param>
    /// <param name="field">FieldReference that serves as a template for the updating process.</param>
    /// <param name="src">The original type which is being replaced.</param>
    /// <param name="dest">The new type which is replacing the original type.</param>
    /// <exception cref="ArgumentNullException">Thrown if instruction, field, src, or dest are null.</exception>
    public static void UpdateInstructionReferences(this Instruction instruction, FieldReference field, TypeDefinition src, TypeDefinition dest)
    {
        // Check that instruction isn't null
        if (instruction == null) throw new ArgumentNullException(nameof(instruction), "The parameter instruction cannot be null.");
        // Check that field isn't null
        if (field == null) throw new ArgumentNullException(nameof(field), "The parameter field cannot be null.");
        // Check that src isn't null
        if (src == null) throw new ArgumentNullException(nameof(src), "The parameter src cannot be null.");
        // Check that dest isn't null
        if (dest == null) throw new ArgumentNullException(nameof(dest), "The parameter dest cannot be null.");

        // Check if the field's FieldType or DeclaringType matches the source type, and if so, replace them with the destination type
        if (field.FieldType == src) field.FieldType = dest;
        if (field.DeclaringType == src) field.DeclaringType = dest;

        // Attempt to find a field in the destination type that matches the field's full name
        var fieldDefinition = dest.FindField(field.FullName);
        if (fieldDefinition != null)
        {
            // If a matching field is found, update the instruction's operand to this field
            instruction.Operand = fieldDefinition;
        }
        else
        {
            // If no matching field is found, import the FieldType and DeclaringType references from the destination module
            field.FieldType = dest.Module.ImportReference(field.FieldType);
            field.DeclaringType = dest.Module.ImportReference(field.DeclaringType);
        }
    }

    /// <summary>
    /// Updates the Operand of an instruction when merging classes.
    /// Updates the ReturnType and DeclaringType of a MethodReference operand if they match the source type.
    /// If a matching method is found in the destination type, the Operand is updated to this method.
    /// If no matching method is found, the ReturnType and DeclaringType are imported from the destination module.
    /// </summary>
    /// <param name="instruction">Instruction whose MethodReference operand is to be updated.</param>
    /// <param name="method">MethodReference of the instruction that needs its parameters, return type and declaring type updated.</param>
    /// <param name="src">The original type which is being replaced.</param>
    /// <param name="dest">The new type which is replacing the original type.</param>
    /// <exception cref="ArgumentNullException">Thrown if instruction, method, src, or dest are null.</exception>
    public static void UpdateInstructionReferences(this Instruction instruction, MethodReference method, TypeDefinition src, TypeDefinition dest)
    {
        // Check that instruction isn't null
        if (instruction == null) throw new ArgumentNullException(nameof(instruction), "The parameter instruction cannot be null.");
        // Check that method isn't null
        if (method == null) throw new ArgumentNullException(nameof(method), "The parameter method cannot be null.");
        // Check that src isn't null
        if (src == null) throw new ArgumentNullException(nameof(src), "The parameter src cannot be null.");
        // Check that dest isn't null
        if (dest == null) throw new ArgumentNullException(nameof(dest), "The parameter dest cannot be null.");

        // Omitted null check for brevity
        // Check if the method's ReturnType or DeclaringType matches the source type, and if so, replace them with the destination type
        if (method.ReturnType == src) method.ReturnType = dest;
        if (method.DeclaringType == src) method.DeclaringType = dest;

        // Attempt to find a method in the destination type that matches the method's full name
        var methodDefinition = dest.FindMethod(method.FullName);
        if (methodDefinition != null)
        {
            // If a matching method is found, update the instruction's operand to this method
            instruction.Operand = methodDefinition;
        }
        else
        {
            // If no matching method is found, import the ReturnType and DeclaringType references from the destination module
            method.ReturnType = dest.Module.ImportReference(method.ReturnType);
            method.DeclaringType = dest.Module.ImportReference(method.DeclaringType);
        }
    }

    /// <summary>
    /// Updates the ReturnType and Parameters of the CallSite to the destination type when merging classes.
    /// </summary>
    /// <param name="callSite">CallSite that needs its return type and parameters updated.</param>
    /// <param name="src">The original type which is being replaced.</param>
    /// <param name="dest">The new type which is replacing the original type.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void UpdateInstructionReferences(this CallSite callSite, TypeDefinition src, TypeDefinition dest)
    {
        // Check that callSite isn't null
        if (callSite == null) throw new ArgumentNullException(nameof(callSite), "The parameter callSite cannot be null.");
        // Check that src isn't null
        if (src == null) throw new ArgumentNullException(nameof(src), "The parameter src cannot be null.");
        // Check that dest isn't null
        if (dest == null) throw new ArgumentNullException(nameof(dest), "The parameter dest cannot be null.");

        // Update callsite parameters to destination type
        callSite.Parameters.UpdateReferences(src, dest);

        // If the current return type is the source type, update it to destination type
        if (callSite.ReturnType == src) callSite.ReturnType = dest;

        // Update return type reference to destination module
        callSite.ReturnType = dest.Module.ImportReference(callSite.ReturnType);
    }

    /// <summary>
    /// Updates all instructions in the method's body.
    /// </summary>
    /// <param name="method">Method whose instructions are to be updated.</param>
    /// <param name="src">The original type which is being replaced.</param>
    /// <param name="dest">The new type which is replacing the original type.</param>
    public static void UpdateInstructionReferences(this MethodDefinition method, TypeDefinition src, TypeDefinition dest)
    {
        // Check that method isn't null
        if (method == null) throw new ArgumentNullException(nameof(method), "The parameter method cannot be null.");
        // Check that src isn't null
        if (src == null) throw new ArgumentNullException(nameof(src), "The parameter src cannot be null.");
        // Check that dest isn't null
        if (dest == null) throw new ArgumentNullException(nameof(dest), "The parameter dest cannot be null.");

        method.Body?.Instructions?.UpdateInstructionReferences(src, dest);
    }

    /// <summary>
    /// Updates all instructions in the given collection.
    /// </summary>
    /// <param name="instructions">Collection of instructions to update.</param>
    /// <param name="src">The original type which is being replaced.</param>
    /// <param name="dest">The new type which is replacing the original type.</param>
    public static void UpdateInstructionReferences(this Collection<Instruction> instructions, TypeDefinition src, TypeDefinition dest)
    {
        // Check that instructions isn't null
        if (instructions == null) throw new ArgumentNullException(nameof(instructions), "The parameter instructions cannot be null.");
        // Check that src isn't null
        if (src == null) throw new ArgumentNullException(nameof(src), "The parameter src cannot be null.");
        // Check that dest isn't null
        if (dest == null) throw new ArgumentNullException(nameof(dest), "The parameter dest cannot be null.");

        foreach (var instruction in instructions)
            instruction.UpdateInstructionReferences(src, dest);
    }

    /// <summary>
    /// Updates type references of each item in the given collection when merging classes.
    /// </summary>
    /// <typeparam name="T">The type of the objects within the collection.</typeparam>
    /// <param name="collection">Collection of items whose type references are to be updated.</param>
    /// <param name="src">The original type which is being replaced.</param>
    /// <param name="dest">The new type which is replacing the original type references.</param>
    /// <exception cref="ArgumentNullException">Thrown if collection, src, or dest are null.</exception>
    /// <exception cref="RuntimeBinderException">Thrown when the type T does not have a Clone method.</exception>
    public static void UpdateInstructionReferences<T>(this Collection<T> collection, TypeDefinition src, TypeDefinition dest)
    {
        // Check that collection isn't null
        if (collection == null) throw new ArgumentNullException(nameof(collection), "The parameter collection cannot be null.");
        // Check that src isn't null
        if (src == null) throw new ArgumentNullException(nameof(src), "The parameter src cannot be null.");
        // Check that dest isn't null
        if (dest == null) throw new ArgumentNullException(nameof(dest), "The parameter dest cannot be null.");

        // Update references for each item in the collection
        foreach (var item in collection)
        {
            // Cast the item to dynamic to resolve the correct UpdateReferences method at runtime
            // And then call UpdateReferences for that item
            UpdateInstructionReferences(item as dynamic, src, dest);
        }
    }

    #endregion UpdateInstructionReferences // Extension methods for Replacing references to a source type with references to a destination type within Mono.Cecil.Instruction objects

    #region UpdateGettersAndSetters // Extension methods for Replacing references to a source type with references to a destination type within Mono.Cecil.Property getter and setter methods

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
        // Check that property isn't null
        if (property == null) throw new ArgumentNullException(nameof(property), "The parameter property cannot be null.");
        // Check that src isn't null
        if (src == null) throw new ArgumentNullException(nameof(src), "The parameter src cannot be null.");
        // Check that dest isn't null
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
                clonedGetter.UpdateReferences(src, dest);
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
                clonedSetter.UpdateReferences(src, dest);
                // Update the declaring type of the setter to be dest
                clonedSetter.DeclaringType = dest;
                // If an equivalent method exists in dest, update the property's setter to reference it
                if (dest.FindMethod(clonedSetter.FullName) is MethodDefinition setMethod)
                    property.SetMethod = setMethod;
            }
        }
    }

    #endregion UpdateGettersAndSetters // Extension methods for Replacing references to a source type with references to a destination type within Mono.Cecil.Property getter and setter methods

    /// <summary>
    /// Updates the Get and Set methods of a collection of properties to the destination type when merging classes.
    /// </summary>
    /// <param name="properties">Collection of PropertyDefinition that needs their getters and setters updated.</param>
    /// <param name="src">The original type which is being replaced.</param>
    /// <param name="dest">The new type which is replacing the original type.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void UpdateGettersAndSetters(this Collection<PropertyDefinition> properties, TypeDefinition src, TypeDefinition dest)
    {
        // Check that properties isn't null
        if (properties == null) throw new ArgumentNullException(nameof(properties), "The parameter properties cannot be null.");

        // For each property in the collection, update its getter and setter
        foreach (var property in properties)
            property.UpdateGettersAndSetters(src, dest);
    }

    public static void SwapMethodReferences(this Instruction instruction, MethodDefinition leftMethod, MethodDefinition rightMethod)
    {
        if (instruction.Operand is MethodReference method)
        {
            if (method == leftMethod)
                instruction.Operand = rightMethod;
            else if (method == rightMethod)
                instruction.Operand = leftMethod;
        }
    }

    public static void SwapMethodReferences(this Collection<Instruction> instructions, MethodDefinition leftMethod, MethodDefinition rightMethod)
    {
        foreach (var instruction in instructions)
            instruction.SwapMethodReferences(leftMethod, rightMethod);
    }

    public static void SwapMethodReferences(this MethodDefinition method, MethodDefinition leftMethod, MethodDefinition rightMethod)
    {
        if (method.Body?.Instructions != null)
            method.Body.Instructions.SwapMethodReferences(leftMethod, rightMethod);
    }

    public static void SwapMethods(this MethodDefinition leftMethod, MethodDefinition rightMethod)
    {
        var leftBody = leftMethod.Body;
        var leftAttributes = leftMethod.Attributes;
        var leftImplAttributes = leftMethod.ImplAttributes;
        var leftSemanticsAttributes = leftMethod.SemanticsAttributes;
        var leftParameters = new Collection<ParameterDefinition>(leftMethod.Parameters);
        var leftCustomAttributes = new Collection<CustomAttribute>(leftMethod.CustomAttributes);
        var leftGenericParameters = new Collection<GenericParameter>(leftMethod.GenericParameters);

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

        leftMethod.SwapMethodReferences(leftMethod, rightMethod);
        rightMethod.SwapMethodReferences(leftMethod, rightMethod);
    }

    /// <summary>
    /// Merges the source type into the destination type by cloning the fields, properties, and methods of the source, updating their types and adding them to the destination.
    /// </summary>
    /// <param name="dest">The destination type definition where fields, properties, and methods from source will be added.</param>
    /// <param name="src">The source type definition whose fields, properties, and methods will be cloned and added to the destination.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    public static void MergeFieldsPropertiesAndMethods(this TypeDefinition dest, TypeDefinition src)
    {
        // Check that dest isn't null
        if (dest == null) throw new ArgumentNullException(nameof(dest), "The parameter dest cannot be null.");
        // Check that src isn't null
        if (src == null) throw new ArgumentNullException(nameof(src), "The parameter src cannot be null.");
        // Check that src's Fields, Properties, and Methods aren't null
        if (src.Fields == null || src.Properties == null || src.Methods == null) throw new ArgumentNullException(nameof(src), "Fields, Properties, or Methods of the source TypeDefinition cannot be null.");
        // Check that dest's Methods aren't null
        if (dest.Methods == null) throw new ArgumentNullException(nameof(dest), "Methods of the destination TypeDefinition cannot be null.");

        // Clone the fields from the source type and add the updated fields to the destination type
        var clonedFields = src.Fields.Clone() ??
            throw new Exception("Cloning of Fields failed."); // Error handling for failed cloning of Fields
        dest.Fields.Add(clonedFields);

        // Clone the properties from the source type and add the updated properties to the destination type
        var clonedProperties = src.Properties.Clone() ??
            throw new Exception("Cloning of Properties failed."); // Error handling for failed cloning of Properties
        dest.Properties.Add(clonedProperties);

        // Clone the methods from the source type
        var clonedMethods = src.Methods.Clone() ??
            throw new Exception("Cloning of Methods failed."); // Error handling for failed cloning of Methods

        // Used to store each method that needs its types updated
        var methodsToTypeUpdate = new Collection<MethodDefinition>();

        // Process each cloned method
        foreach (var clonedMethod in clonedMethods.ToList())
        {
            // Check if clonedMethod, its Body, and its Body.Instructions are not null
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

                // If the destination already contains a constructor or destructor, merge the instructions
                if (destMethod != null)
                {
                    var clonedInstructions = clonedMethod.Body.Instructions;
                    var trimmedClonedInstructions = clonedInstructions.ToList();

                    // Special handling for constructors
                    if (clonedMethod.Name is ".ctor")
                    {
                        // Find the constructor call instruction and remove the instructions before it.
                        // This is done to prevent calling the base class constructor twice when merging.
                        var callIndex = trimmedClonedInstructions.FindIndex(x => x.OpCode == OpCodes.Call);

                        // Check if callIndex is within valid range
                        if (callIndex < 0 || callIndex >= trimmedClonedInstructions.Count)
                            throw new Exception("Invalid Call Instruction Index in cloned method.");

                        // Remove starting instructions
                        trimmedClonedInstructions.RemoveRange(0, callIndex + 1);
                        // Remove the last instruction (ret)
                        trimmedClonedInstructions.RemoveAt(trimmedClonedInstructions.Count - 1);

                        // Insert the trimmed instructions to the existing constructor, just before the last instruction (ret)
                        int insertIndex = destMethod.Body.Instructions.Count - 1;
                        foreach (var clonedInstruction in trimmedClonedInstructions)
                        {
                            destMethod.Body.Instructions.Insert(insertIndex, clonedInstruction);
                            insertIndex++;
                        }
                    }
                    // Special handling for static constructors
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
                    // Special handling for destructors
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

                    // Add the method to the list of methods to update
                    methodsToTypeUpdate.Add(destMethod);
                }
                else
                {
                    // Add new constructor to desitnation type directly
                    methodsToTypeUpdate.Add(clonedMethod);
                }
            }
            else
            {
                // For non-constructor and non-destructor methods, add them directly to methodsToTypeUpdate list
                methodsToTypeUpdate.Add(clonedMethod);
            }
        }

        // Add the updated methods to the destination type
        dest.Methods.Add(clonedMethods);

        // Update the type of the fields, properties, and methods from source type to destination type
        clonedFields.UpdateReferences(src, dest);
        clonedProperties.UpdateReferences(src, dest);
        methodsToTypeUpdate.UpdateReferences(src, dest);

        clonedProperties.UpdateGettersAndSetters(src, dest);
        methodsToTypeUpdate.UpdateInstructionReferences(src, dest);
    }
}
#endif