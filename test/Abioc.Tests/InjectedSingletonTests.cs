﻿// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abioc.InjectedSingletonTests;
    using Abioc.Registration;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    namespace InjectedSingletonTests
    {
        public class ConcreteOnlyDependency
        {
        }

        public interface IInterfaceOnlyDependency
        {
        }

        public class InterfaceOnlyDependency : IInterfaceOnlyDependency
        {
        }

        public interface IMixedDependency
        {
        }

        public class MixedDependency : IMixedDependency
        {
        }

        public struct ValueTypeDependency
        {
            public ValueTypeDependency(Guid guid)
            {
                Guid = guid;
            }

            public Guid Guid { get; }
        }

        public class DependentClass
        {
            public DependentClass(
                ConcreteOnlyDependency concreteOnlyDependency,
                IInterfaceOnlyDependency interfaceOnlyDependency,
                IMixedDependency mixedDependencyInferface,
                MixedDependency mixedDependencyClass,
                ValueTypeDependency valueTypeDependency)
            {
                ConcreteOnlyDependency = concreteOnlyDependency ??
                                         throw new ArgumentNullException(nameof(concreteOnlyDependency));
                InterfaceOnlyDependency = interfaceOnlyDependency ??
                                          throw new ArgumentNullException(nameof(interfaceOnlyDependency));
                MixedDependencyInferface = mixedDependencyInferface ??
                                           throw new ArgumentNullException(nameof(mixedDependencyInferface));
                MixedDependencyClass = mixedDependencyClass ??
                                       throw new ArgumentNullException(nameof(mixedDependencyClass));
                ValueTypeDependency = valueTypeDependency;
            }

            public ConcreteOnlyDependency ConcreteOnlyDependency { get; }
            public IInterfaceOnlyDependency InterfaceOnlyDependency { get; }
            public IMixedDependency MixedDependencyInferface { get; }
            public MixedDependency MixedDependencyClass { get; }
            public ValueTypeDependency ValueTypeDependency { get; }
        }
    }

    public abstract class InjectedSingletonTestsBase
    {
        protected ConcreteOnlyDependency ExpectedConcreteOnlyDependency;
        protected InterfaceOnlyDependency ExpectedInterfaceOnlyDependency;
        protected MixedDependency ExpectedMixedDependency;
        protected ValueTypeDependency ExpectedValueTypeDependency;

        protected abstract TService GetService<TService>();

        [Fact]
        public void ItShouldResolveAConcreteOnlyDependency()
        {
            // Act
            DependentClass actual = GetService<DependentClass>();

            // Assert
            actual.Should().NotBeNull();
            actual.ConcreteOnlyDependency
                .Should()
                .NotBeNull()
                .And.BeSameAs(ExpectedConcreteOnlyDependency);
        }

        [Fact]
        public void ItShouldResolveAnInterfaceOnlyDependency()
        {
            // Act
            DependentClass actual = GetService<DependentClass>();

            // Assert
            actual.Should().NotBeNull();
            actual.InterfaceOnlyDependency
                .Should()
                .NotBeNull()
                .And.BeSameAs(ExpectedInterfaceOnlyDependency);
        }

        [Fact]
        public void ItShouldResolveAMixedDependencyInferface()
        {
            // Act
            DependentClass actual = GetService<DependentClass>();

            // Assert
            actual.Should().NotBeNull();
            actual.MixedDependencyInferface
                .Should()
                .NotBeNull()
                .And.BeSameAs(ExpectedMixedDependency);
        }

        [Fact]
        public void ItShouldResolveAMixedDependencyClass()
        {
            // Act
            DependentClass actual = GetService<DependentClass>();

            // Assert
            actual.Should().NotBeNull();
            actual.MixedDependencyClass
                .Should()
                .NotBeNull()
                .And.BeSameAs(ExpectedMixedDependency);
        }

        [Fact]
        public void ItShouldResolveAValueTypeDependency()
        {
            // Act
            DependentClass actual = GetService<DependentClass>();

            // Assert
            actual.Should().NotBeNull();
            actual.ValueTypeDependency
                .Should()
                .Be(ExpectedValueTypeDependency);
            actual.ValueTypeDependency.Guid
                .Should()
                .Be(ExpectedValueTypeDependency.Guid);
        }
    }

    public class WhenSingletonDependenciesWithAContext : InjectedSingletonTestsBase
    {
        private readonly IContainer<int> _container;

        public WhenSingletonDependenciesWithAContext(ITestOutputHelper output)
        {
            ExpectedConcreteOnlyDependency = new ConcreteOnlyDependency();
            ExpectedInterfaceOnlyDependency = new InterfaceOnlyDependency();
            ExpectedMixedDependency = new MixedDependency();
            ExpectedValueTypeDependency = new ValueTypeDependency(Guid.NewGuid());

            _container =
                new RegistrationSetup<int>()
                    .RegisterFixed(ExpectedConcreteOnlyDependency)
                    .RegisterFixed<IInterfaceOnlyDependency>(ExpectedInterfaceOnlyDependency)
                    .RegisterFixed<IMixedDependency, MixedDependency>(ExpectedMixedDependency)
                    .RegisterFixed(ExpectedValueTypeDependency)
                    .Register<DependentClass>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>(1);
    }

    public class WhenSingletonDependenciesWithoutAContext : InjectedSingletonTestsBase
    {
        private readonly IContainer _container;

        public WhenSingletonDependenciesWithoutAContext(ITestOutputHelper output)
        {
            ExpectedConcreteOnlyDependency = new ConcreteOnlyDependency();
            ExpectedInterfaceOnlyDependency = new InterfaceOnlyDependency();
            ExpectedMixedDependency = new MixedDependency();
            ExpectedValueTypeDependency = new ValueTypeDependency(Guid.NewGuid());

            _container =
                new RegistrationSetup()
                    .RegisterFixed(ExpectedConcreteOnlyDependency)
                    .RegisterFixed<IInterfaceOnlyDependency>(ExpectedInterfaceOnlyDependency)
                    .RegisterFixed<IMixedDependency, MixedDependency>(ExpectedMixedDependency)
                    .RegisterFixed(ExpectedValueTypeDependency)
                    .Register<DependentClass>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>();
    }
}
