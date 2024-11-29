using Mono.Cecil;
using Mono.Cecil.Cil;

/// <summary>
/// Demonstrates the use of Mono.Cecil with MonoCecilExtenstions to modify a .NET assembly by cloning and modifying
/// methods, altering their IL instructions, and updating method references.
/// </summary>
class Program
{
    /// <summary>
    /// Entry point of the program. Modifies the "Target.exe" assembly by cloning and modifying methods,
    /// adjusting IL instructions, and saving the updated assembly.
    /// </summary>
    static void Main()
    {
        // Load the target assembly for modification.
        // The second parameter (true) indicates the assembly will be written back.
        var targetAssembly = MonoCecilExtensions.LoadAssembly("Target.exe", true);

        // Find the type "Foo" within the loaded assembly.
        var fooClass = targetAssembly.FindType("Foo");

        // Find the "Main" method in the "Program" type.
        var mainMethod = targetAssembly.FindMethodOfType("Program", "Main");

        // Find the "Bar" method in the "Foo" type.
        var barMethod = targetAssembly.FindMethodOfType("Foo", "Bar");

        // Clone the "Bar" method to create a new method named "Baz".
        var bazMethod = barMethod.Clone();
        bazMethod.Name = "Baz";

        // Add the cloned "Baz" method to the "Foo" type.
        fooClass.Methods.Add(bazMethod);

        // Iterate through the instructions of the "Baz" method.
        foreach (var instruction in bazMethod.Body.Instructions)
        {
            // If the instruction is an Ldc_I4_S (load constant) opcode with a signed byte operand,
            // double the value of the operand.
            if (instruction.OpCode == OpCodes.Ldc_I4_S && instruction.Operand is System.SByte operand)
                instruction.Operand = (operand *= 2);
        }

        // Get the instructions of the "Main" method.
        var instructions = mainMethod.Body.Instructions;

        // Clone the instructions of the "Main" method.
        var clonedInstructions = instructions.Clone();

        // Remove the last instruction of the "Main" method's original instructions.
        instructions.RemoveAt(instructions.Count - 1);

        // Add the cloned instructions back to the "Main" method.
        foreach (var instructionClone in clonedInstructions)
        {
            mainMethod.Body.Instructions.Add(instructionClone);

            // If the instruction is a method call to "Bar", update the operand to reference "Baz" instead.
            if (instructionClone.OpCode == OpCodes.Call &&
                instructionClone.Operand is MethodReference methodRef &&
                methodRef.FullName == barMethod.FullName)
                instructionClone.Operand = bazMethod;
        }

        // Update fields, properties, and methods in the assembly after modifications.
        targetAssembly.UpdateFieldsPropertiesAndMethods(true);

        // Write the modified assembly back to disk, replacing the original file.
        targetAssembly.Write();

        // Dispose of the loaded assembly to release resources.
        targetAssembly.Dispose();
    }
}
