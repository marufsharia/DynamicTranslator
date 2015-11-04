﻿namespace Dynamic.Translator.Core.Orchestrators
{
    using System;
    using Dependency.Markers;

    public interface ITranslatorBootstrapper : ISingletonDependency, IDisposable, IEvents
    {
        bool IsInitialized { get; }

        void Initialize();
    }
}