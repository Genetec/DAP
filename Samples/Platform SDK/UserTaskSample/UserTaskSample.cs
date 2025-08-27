// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.UserTaskManagement;

namespace Genetec.Dap.CodeSamples;

public class UserTaskSample : SampleBase
{
    protected override Task RunAsync(Engine engine, CancellationToken token)
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

        return Task.CompletedTask;
    }

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