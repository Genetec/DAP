// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Genetec.Sdk.Entities;
    using Sdk;
    using Sdk.UserTaskManagement;

    class Program
    {
        static Program() => SdkResolver.Initialize();

        static async Task Main()
        {
            const string server = "localhost";
            const string username = "admin";
            const string password = "";

            using var engine = new Engine();

            ConnectionStateCode state = await engine.LogOnAsync(server: server, username: username, password: password);

            if (state == ConnectionStateCode.Success)
            {
                IEnumerable<PrivateUserTask> privateTasks = engine.UserTaskManager.GetPrivateTasks().OfType<PrivateUserTask>().ToList();
                Console.WriteLine($"Number of private tasks: {privateTasks.Count()}");
                foreach (var task in privateTasks)
                {
                    Console.WriteLine($"Task Name: {task.Name}, Task Type: {GetTaskTypeName(task.TaskType)}");
                    foreach (var item in task.Content.Where(item => item.Type != EntityType.None))
                    {
                        Console.WriteLine($"   Content Item ID: {item.Id}, Type: {item.Type}");
                    }
                }

                Console.WriteLine();

                IEnumerable<UserTask> publicTasks = engine.UserTaskManager.GetPublicTasks().OfType<UserTask>().ToList();
                Console.WriteLine($"Number of public tasks: {publicTasks.Count()}");
                foreach (var task in publicTasks)
                {
                    Console.WriteLine($"Task Name: {task.Name}, Task Type: {GetTaskTypeName(task.TaskType)}");
                    foreach (var item in task.Content.Where(item => item.Type != EntityType.None))
                    {
                        Console.WriteLine($"   Content Item ID: {item.Id}, Type: {item.Type}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            string GetTaskTypeName(Guid taskTypeId)
            {
                if (taskTypeId == TaskGuid.AlarmsTask.Id)
                    return TaskGuid.AlarmsTask.Name;
                if (taskTypeId == TaskGuid.CardholderManagerTask.Id)
                    return TaskGuid.CardholderManagerTask.Name;
                if (taskTypeId == TaskGuid.MapDesignerTask.Id)
                    return TaskGuid.MapDesignerTask.Name;
                if (taskTypeId == TaskGuid.MapsTask.Id)
                    return TaskGuid.MapsTask.Name;
                if (taskTypeId == TaskGuid.MonitoringTask.Id)
                    return TaskGuid.MonitoringTask.Name;
                if (taskTypeId == TaskGuid.RemotingTask.Id)
                    return TaskGuid.RemotingTask.Name;

                return $"Unknown Task Type ({taskTypeId})";
            }
        }
    }
}
