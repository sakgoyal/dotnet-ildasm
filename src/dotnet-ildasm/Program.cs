﻿using System;
using DotNet.Ildasm.Adapters;
using DotNet.Ildasm.Configuration;

namespace DotNet.Ildasm
{
    internal class Program(IOutputWriter writer)
    {
        private readonly IOutputWriter _writer = writer;
        private readonly IDisassemblerFactory _factory = new DisassemblerFactory(new AssemblyDefinitionResolver());

        public Program() : this(new ConsoleOutputWriter())
        {
        }

        private static int Main(string[] args)
        {
            return new Program().Execute(args);
        }

        internal int Execute(string[] args)
        {
            var handler = new CommandHandler(ExecuteDisassembler, text =>
            {
                _writer.WriteLine(text);
                return -1;
            });

            return handler.Handle(args);
        }

        private int ExecuteDisassembler(CommandArgument argument)
        {
            ExecutionResult executionResult;

            try
            {
                using var disassembler = _factory.Create(argument);
                var itemFilter = new ItemFilter(argument.Item);

                executionResult = disassembler.Execute(argument, itemFilter);
            }
            catch (Exception e)
            {
                executionResult = new ExecutionResult(false, e.Message);
            }

            if (executionResult.Message?.Length > 0)
                _writer.WriteLine(executionResult.Message);

            return executionResult.Succeeded ? 0 : -1;
        }
    }
}
