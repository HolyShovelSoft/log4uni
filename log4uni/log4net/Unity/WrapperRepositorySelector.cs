using System;
using System.Reflection;
using log4net.Core;
using log4net.Repository;
using UnityEngine;
using SystemInfo = log4net.Util.SystemInfo;


namespace log4net.Unity
{
    public class WrapperRepositorySelector : IRepositorySelector
    {
        private static void InvokeEditorInit()
        {
            if (!Application.isEditor) return;

            var type = SystemInfo.GetTypeFromString("log4net.Unity.EditorLogInitializer",false, true);

            if (type != null)
            {
                var method = type.GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Static);
                if (method != null)
                {
                    method.Invoke(null, null);
                }
            }
        }

        private bool systemInitialized;
        private readonly DefaultRepositorySelector defaultRepositorySelector = new(typeof(Repository.Hierarchy.Hierarchy));

        event LoggerRepositoryCreationEventHandler IRepositorySelector.LoggerRepositoryCreatedEvent
        {
            add => defaultRepositorySelector.LoggerRepositoryCreatedEvent += value;
            remove => defaultRepositorySelector.LoggerRepositoryCreatedEvent -= value;
        }

        public ILoggerRepository GetRepository(Assembly assembly)
        {
            EnsureSystemInitialized();
            return defaultRepositorySelector.GetRepository(assembly);
        }

        public ILoggerRepository GetRepository(string repositoryName)
        {
            EnsureSystemInitialized();
            return defaultRepositorySelector.GetRepository(repositoryName);
        }

        public ILoggerRepository CreateRepository(Assembly assembly, Type repositoryType)
        {
            EnsureSystemInitialized();
            return defaultRepositorySelector.CreateRepository(assembly, repositoryType);
        }

        public ILoggerRepository CreateRepository(string repositoryName, Type repositoryType)
        {
            EnsureSystemInitialized();
            return defaultRepositorySelector.CreateRepository(repositoryName, repositoryType);
        }

        public bool ExistsRepository(string repositoryName)
        {
            EnsureSystemInitialized();
            return defaultRepositorySelector.ExistsRepository(repositoryName);
        }

        public ILoggerRepository[] GetAllRepositories()
        {
            EnsureSystemInitialized();
            return defaultRepositorySelector.GetAllRepositories();
        }

        private void EnsureSystemInitialized()
        {
            if (systemInitialized) return;
            systemInitialized = true;

            RuntimeLogInitializer.Init();

            if (Application.isEditor)
            {
                InvokeEditorInit();
            }
        }
    }
}