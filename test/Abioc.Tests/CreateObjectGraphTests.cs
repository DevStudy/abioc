﻿// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abioc.Registration;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    public abstract class WhenCreatingAnObjectGraphOfClassesBase
    {
        protected abstract TService GetService<TService>();

        [Fact]
        public void ItShouldCreateTheHeadClassWithDependencies()
        {
            // Act
            Example.Ns1.MyClass3 actual = GetService<Example.Ns1.MyClass3>();

            // Assert
            actual.Should().NotBeNull();
            actual.MyClass1.Should().NotBeNull();
            actual.MyClass2.Should().NotBeNull();
            actual.MyOtherClass1.Should().NotBeNull();
            actual.MyOtherClass2.Should().NotBeNull();

            // Make sure new instances are always created.
            actual.MyClass2.MyClass1.Should().NotBeSameAs(actual.MyClass1);
        }

        [Fact]
        public void ItShouldCreateAnIntermediateClassWithDependencies()
        {
            // Act
            Example.Ns2.MyClass2 actual = GetService<Example.Ns2.MyClass2>();

            // Assert
            actual.Should().NotBeNull();
            actual.MyClass1.Should().NotBeNull();
            actual.MyOtherClass1.Should().NotBeNull();
        }

        [Fact]
        public void ItShouldCreateATailClass()
        {
            // Act
            Example.Ns1.MyClass1 actual = GetService<Example.Ns1.MyClass1>();

            // Assert
            actual.Should().NotBeNull();
        }
    }

    public class WhenCreatingAnObjectGraphOfClassesWithAContext : WhenCreatingAnObjectGraphOfClassesBase
    {
        private readonly IContainer<int> _container;

        public WhenCreatingAnObjectGraphOfClassesWithAContext(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup<int>()
                    .Register<Example.Ns1.MyClass1>()
                    .Register<Example.Ns1.MyClass2>()
                    .Register<Example.Ns1.MyClass3>()
                    .Register<Example.Ns2.MyClass1>()
                    .Register<Example.Ns2.MyClass2>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>(1);
    }

    public class WhenCreatingAnObjectGraphOfClassesWithoutAContext : WhenCreatingAnObjectGraphOfClassesBase
    {
        private readonly IContainer _container;

        public WhenCreatingAnObjectGraphOfClassesWithoutAContext(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup()
                    .Register<Example.Ns1.MyClass1>()
                    .Register<Example.Ns1.MyClass2>()
                    .Register<Example.Ns1.MyClass3>()
                    .Register<Example.Ns2.MyClass1>()
                    .Register<Example.Ns2.MyClass2>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>();
    }
}
