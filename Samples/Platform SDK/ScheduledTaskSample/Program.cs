// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.Actions;
using Genetec.Sdk.Actions.Schedules;
using Genetec.Sdk.Entities;

SdkResolver.Initialize();

await RunSample();

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();

async Task RunSample()
{
    const string server = "localhost";
    const string username = "admin";
    const string password = "";

    using var engine = new Engine();

    ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

    if (state != ConnectionStateCode.Success)
    {
        Console.WriteLine($"Logon failed: {state}");
        return;
    }

    await engine.TransactionManager.ExecuteTransactionAsync(() =>
    {
        // Create a sample macro (this macro won't do anything, but it will be executed)
        var macro = (Macro)engine.CreateEntity("SampleMacro", EntityType.Macro);

        // Create an action that executes the macro
        var action = (ExecuteMacroAction)engine.ActionManager.BuildAction(ActionType.RunMacro, macro.Guid, Schedule.AlwaysScheduleGuid);

        // Create scheduled tasks with various schedule types

        // ByMinuteSchedule: Executes the task every minute at the specified second
        CreateScheduledTask("ByMinuteTask", new ByMinuteSchedule(second: 30), action);

        // HourlySchedule: Executes the task every hour at the specified minute and second
        CreateScheduledTask("HourlyTask", new HourlySchedule(minute: 10, second: 0), action);

        // DailySchedule: Executes the task every day at the specified hour, minute, and second
        CreateScheduledTask("DailyTask", new DailySchedule(hour: 1, minute: 30, second: 0), action);

        // MonthlySchedule: Executes the task on a specific day of each month at the specified time
        CreateScheduledTask("MonthlyTask", new MonthlySchedule(dayOfTheMonth: 15, hour: 12, minute: 0, second: 0), action);

        // OneTimeSchedule: Executes the task once at a specific date and time
        CreateScheduledTask("OneTimeTask", new OneTimeSchedule(executionDate: DateTime.UtcNow.AddMinutes(5)), action);

        // OnStartupSchedule: Executes the task when the system starts up
        CreateScheduledTask("OnStartupTask", new OnStartupSchedule(), action);

        // WeeklySchedule: Executes the task on specified days of the week at the given time
        CreateScheduledTask("WeeklyTask", new WeeklySchedule([DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday], hour: 9, minute: 0, second: 0), action);

        // YearlySchedule: Executes the task once a year on a specific date and time
        CreateScheduledTask("YearlyTask", new YearlySchedule(month: 1, dayOfTheMonth: 1, hour: 0, minute: 0, second: 0), action);

        // CustomIntervalSchedule: Executes the task at a custom interval specified in days, hours, minutes, and seconds
        CreateScheduledTask("CustomIntervalTask", new CustomIntervalSchedule(days: 3, hours: 2, minutes: 4, seconds: 5), action);

        Console.WriteLine("All scheduled tasks created successfully.");
    });

    void CreateScheduledTask(string taskName, ScheduleBase schedule, Genetec.Sdk.Actions.Action action)
    {
        var scheduledTask = (ScheduledTask)engine.CreateEntity(taskName, EntityType.ScheduledTask);
        scheduledTask.SetSchedule(schedule);
        scheduledTask.SetAction(action);
        scheduledTask.IsEnabled = true; // Enable the task

        Console.WriteLine($"Created scheduled task: {taskName} with {schedule.GetType().Name}");
    }
}
