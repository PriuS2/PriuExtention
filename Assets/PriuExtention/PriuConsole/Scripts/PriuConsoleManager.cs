using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PriuConsole
{
    public class PriuConsoleManager : MonoBehaviour
    {
        public static PriuConsoleManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogError("GameManager is not initialized. Make sure it exists in the scene.");
                }

                return _instance;
            }
        }

        private static PriuConsoleManager _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateSingletonInstance()
        {
            if (_instance == null)
            {
                GameObject singletonObject = new GameObject($"[{nameof(PriuConsoleManager)}]");
                _instance = singletonObject.AddComponent<PriuConsoleManager>();
                DontDestroyOnLoad(singletonObject);
            }
        }

        private static Dictionary<string, Action> commandDictionary = new Dictionary<string, Action>();


        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            RegisterAllCommands();

            Debug.Log($"{GetType()} Initialized");
        }

        // 모든 명령어 등록
        private static void RegisterAllCommands()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Instance |
                                                           BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        var attribute = method.GetCustomAttribute<ConsoleCommandAttribute>();
                        if (attribute != null)
                        {
                            var commandName = attribute.CommandName;
                            Action commandAction = null;

                            if (method.IsStatic)
                            {
                                commandAction = () => method.Invoke(null, null);
                            }
                            else
                            {
                                var instance = FindObjectOfType(type);
                                if (instance != null)
                                {
                                    commandAction = () => method.Invoke(instance, null);
                                }
                                else
                                {
                                    Debug.LogWarning($"Instance of {type.Name} not found for command {commandName}");
                                    continue;
                                }
                            }

                            if (!commandDictionary.ContainsKey(commandName))
                            {
                                commandDictionary.Add(commandName, commandAction);
                                Debug.Log($"Command '{commandName}' registered successfully.");
                            }
                            else
                            {
                                Debug.LogWarning($"Command '{commandName}' is already registered.");
                            }
                        }
                    }
                }
            }
        }

        public static void ExecuteCommand(string command)
        {
            if (!Instance)
            {
                Debug.LogError($"PriuConsoleManager not initialize yet!!");
            }

            if (commandDictionary.TryGetValue(command, out var action))
            {
                action?.Invoke();
                Debug.Log($"Command '{command}' executed.");
            }
            else
            {
                Debug.LogWarning($"Command '{command}' not found.");
                RegisterAllCommands();
                if (commandDictionary.TryGetValue(command, out var action2))
                {
                    action2?.Invoke();
                    Debug.Log($"Command '{command}' executed.");
                }
            }
        }

        public static void DebugCommands()
        {
            foreach (var key in commandDictionary.Keys)
            {
                Debug.Log($"PriuConsoleCommand Key : {key}");
            }

            
        }
    }
}