// Copyright 2026 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Client;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Genetec.Sdk.Workspace.Pages;

public partial class CustomEventsReportFilter : ReportFilter, INotifyPropertyChanged
{
    private string m_message;
    private EventChoice m_selectedEvent;
    private int? m_restoredId;
    private bool m_initialized;

    public CustomEventsReportFilter()
    {
        InitializeComponent();
        DataContext = this;
    }

    public ObservableCollection<EventChoice> Events { get; } = new();
    public event PropertyChangedEventHandler PropertyChanged;

    public string Message
    {
        get => m_message;
        set => Set(ref m_message, value);
    }

    public EventChoice SelectedEvent
    {
        get => m_selectedEvent;
        set => Set(ref m_selectedEvent, value);
    }

    protected override string FilterName => "Custom events";
    protected override bool IsStateValid => Events.Any(item => item.Id.HasValue && item.Available)
        && SelectedEvent is not null && SelectedEvent.Available;
    protected override string FilterData
    {
        get => new CustomEventFilterData { CustomEventId = SelectedEvent?.Id ?? m_restoredId, Message = Message }.Serialize();
        set
        {
            var filter = CustomEventFilterData.Deserialize(value);
            m_restoredId = filter.CustomEventId;
            Message = filter.Message;
            if (m_initialized) RestoreSelection();
        }
    }

    protected override void Initialize()
    {
        Events.Clear();
        Events.Add(new EventChoice(null, "All custom events", true));
        foreach (var definition in CustomEventReport.GetDefinitions(Workspace.Sdk))
            Events.Add(new EventChoice(definition.Id, $"{definition.Name} ({definition.SourceEntityType})", true));
        m_initialized = true;
        RestoreSelection();
    }

    private void RestoreSelection()
    {
        var choice = Events.FirstOrDefault(item => item.Id == m_restoredId);
        if (choice is null)
        {
            choice = new EventChoice(m_restoredId, $"Unavailable custom event ({m_restoredId})", false);
            Events.Add(choice);
        }
        SelectedEvent = choice;
    }

    private void Set<T>(ref T field, T value, [CallerMemberName] string name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        if (name == nameof(SelectedEvent)) m_restoredId = m_selectedEvent?.Id;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        OnModified();
    }

    public sealed class EventChoice
    {
        public EventChoice(int? id, string name, bool available) { Id = id; Name = name; Available = available; }
        public int? Id { get; }
        public string Name { get; }
        public bool Available { get; }
    }
}
