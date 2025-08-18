// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Actions;
using Genetec.Sdk.Actions.Schedules;
using Genetec.Sdk.Entities;

namespace Genetec.Dap.CodeSamples;

public class ScheduledTaskSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        await engine.TransactionManager.ExecuteTransactionAsync(() =>
        {
            // Create a sample macro (this macro won't do anything, but it will be executed)
            var macro = (Macro)engine.CreateEntity("SampleMacro", EntityType.Macro);

            // Create an action that executes the macro
            var action = (ExecuteMacroAction)engine.ActionManager.BuildAction(ActionType.RunMacro, macro.Guid, Schedule.AlwaysScheduleGuid);

            // Create scheduled tasks with various schedule types

            // ByMinuteSchedule: Executes the task every minute at the specified second
            CreateScheduledTask(engine, "ByMinuteTask", new ByMinuteSchedule(second: 30), action);

            // HourlySchedule: Executes the task every hour at the specified minute and second
            CreateScheduledTask(engine, "HourlyTask", new HourlySchedule(minute: 10, second: 0), action);

            // DailySchedule: Executes the task every day at the specified hour, minute, and second
            CreateScheduledTask(engine, "DailyTask", new DailySchedule(hour: 1, minute: 30, second: 0), action);

            // MonthlySchedule: Executes the task on a specific day of each month at the specified time
            CreateScheduledTask(engine, "MonthlyTask", new MonthlySchedule(dayOfTheMonth: 15, hour: 12, minute: 0, second: 0), action);

            // OneTimeSchedule: Executes the task once at a specific date and time
            CreateScheduledTask(engine, "OneTimeTask", new OneTimeSchedule(executionDate: DateTime.UtcNow.AddMinutes(5)), action);

            // OnStartupSchedule: Executes the task when the system starts up
            CreateScheduledTask(engine, "OnStartupTask", new OnStartupSchedule(), action);

            // WeeklySchedule: Executes the task on specified days of the week at the given time
            CreateScheduledTask(engine, "WeeklyTask", new WeeklySchedule([DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday], hour: 9, minute: 0, second: 0), action);

            // YearlySchedule: Executes the task once a year on a specific date and time
            CreateScheduledTask(engine, "YearlyTask", new YearlySchedule(month: 1, dayOfTheMonth: 1, hour: 0, minute: 0, second: 0), action);

            // CustomIntervalSchedule: Executes the task at a custom interval specified in days, hours, minutes, and seconds
            CreateScheduledTask(engine, "CustomIntervalTask", new CustomIntervalSchedule(days: 3, hours: 2, minutes: 4, seconds: 5), action);

            Console.WriteLine("All scheduled tasks created successfully.");
        });
    }

    private static void CreateScheduledTask(Engine engine, string taskName, ScheduleBase schedule, Genetec.Sdk.Actions.Action action)
    {
        var scheduledTask = (ScheduledTask)engine.CreateEntity(taskName, EntityType.ScheduledTask);
        scheduledTask.SetSchedule(schedule);
        scheduledTask.SetAction(action);
        scheduledTask.IsEnabled = true; // Enable the task

        Console.WriteLine($"Created scheduled task: {taskName} with {schedule.GetType().Name}");
    }
}