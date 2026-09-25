// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Client;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Sdk;
using Sdk.Entities;
using Sdk.Workspace.Components.CustomAction;

public partial class HttpRequestActionView : CustomActionView, INotifyPropertyChanged
{
    private string m_method = "GET";
    private string m_url = "https://";
    private string m_contentType = "application/json";
    private string m_body;
    private bool m_suppressModified;

    public HttpRequestActionView()
    {
        InitializeComponent();
        DataContext = this;

        ActionName = "Send HTTP request"; // The name of the action that will be displayed in the action list.
        ActionDescription = "Send an HTTP request to a configured endpoint"; // The description of the action that will be displayed in the action list.

        WireCollection(QueryParameters);
        WireCollection(Headers);
    }

    // The HTTP methods offered in the Method dropdown.
    public IReadOnlyList<string> Methods { get; } = new[] { "GET", "POST", "PUT", "PATCH", "DELETE", "HEAD", "OPTIONS" };

    // Common content types offered in the editable Content type dropdown.
    public IReadOnlyList<string> ContentTypes { get; } =
        new[] { "application/json", "application/xml", "text/plain", "application/x-www-form-urlencoded" };

    // The editable rows of the query-parameter and header grids.
    public ObservableCollection<EditableNameValue> QueryParameters { get; } = new();

    public ObservableCollection<EditableNameValue> Headers { get; } = new();

    public string Method
    {
        get => m_method;
        set
        {
            if (SetProperty(ref m_method, value))
            {
                OnPropertyChanged(nameof(CanSendBody));
                RaiseModified();
            }
        }
    }

    public string Url
    {
        get => m_url;
        set
        {
            if (SetProperty(ref m_url, value))
            {
                RaiseModified();
            }
        }
    }

    public string ContentType
    {
        get => m_contentType;
        set
        {
            if (SetProperty(ref m_contentType, value))
            {
                RaiseModified();
            }
        }
    }

    public string Body
    {
        get => m_body;
        set
        {
            if (SetProperty(ref m_body, value))
            {
                RaiseModified();
            }
        }
    }

    public bool CanSendBody => Method is "POST" or "PUT" or "PATCH";

    public override bool IsStateValid =>
        !string.IsNullOrWhiteSpace(Method)
        && Uri.TryCreate(Url, UriKind.Absolute, out Uri uri)
        && (string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
            || string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase));

    public event PropertyChangedEventHandler PropertyChanged;

    private void AddQueryParameter(object sender, RoutedEventArgs e)
    {
        QueryParameters.Add(new EditableNameValue());
    }

    private void RemoveQueryParameter(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is EditableNameValue row)
        {
            QueryParameters.Remove(row);
        }
    }

    private void AddHeader(object sender, RoutedEventArgs e)
    {
        Headers.Add(new EditableNameValue());
    }

    private void RemoveHeader(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is EditableNameValue row)
        {
            Headers.Remove(row);
        }
    }

    protected override void OnInternalInitializationDone()
    {
        // Populate the recipients with every role created from this plugin so the configured
        // action is delivered to the plugin's server side. This runs after the framework has
        // finished setting the view up, which is why it is done here rather than in the constructor.
        Recipients.Clear();
        foreach (Role role in Workspace.Sdk.GetEntities(EntityType.Role).OfType<Role>()
                     .Where(role => role.SubType == PluginTypes.SamplePlugin))
        {
            Recipients.Add(role.Guid);
        }
    }

    protected override string Serialize()
    {
        return new SendHttpRequestAction
        {
            Method = Method,
            Url = Url,
            QueryParameters = QueryParameters.Select(row => row.ToPair()).ToList(),
            Headers = Headers.Select(row => row.ToPair()).ToList(),
            ContentType = ContentType,
            Body = Body
        }.Serialize();
    }

    protected override void Deserialize(string payload)
    {
        SendHttpRequestAction data = SendHttpRequestAction.Deserialize(payload);
        if (data is null)
        {
            return;
        }

        m_suppressModified = true;
        try
        {
            Method = string.IsNullOrEmpty(data.Method) ? "GET" : data.Method;
            Url = data.Url;
            ContentType = data.ContentType;
            Body = data.Body;

            LoadRows(QueryParameters, data.QueryParameters);
            LoadRows(Headers, data.Headers);
        }
        finally
        {
            m_suppressModified = false;
        }
    }

    private void LoadRows(ObservableCollection<EditableNameValue> target, List<NameValuePair> source)
    {
        target.Clear();
        if (source is null)
        {
            return;
        }

        foreach (NameValuePair pair in source)
        {
            target.Add(new EditableNameValue { Name = pair.Name, Value = pair.Value });
        }
    }

    private void WireCollection(ObservableCollection<EditableNameValue> collection)
    {
        collection.CollectionChanged += OnRowsChanged;
    }

    private void OnRowsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (EditableNameValue row in e.NewItems.OfType<EditableNameValue>())
            {
                row.PropertyChanged += OnRowEdited;
            }
        }

        if (e.OldItems != null)
        {
            foreach (EditableNameValue row in e.OldItems.OfType<EditableNameValue>())
            {
                row.PropertyChanged -= OnRowEdited;
            }
        }

        RaiseModified();
    }

    private void OnRowEdited(object sender, PropertyChangedEventArgs e)
    {
        RaiseModified();
    }

    private void RaiseModified()
    {
        if (!m_suppressModified)
        {
            OnModified();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
    {
        if (!Equals(field, newValue))
        {
            field = newValue;
            OnPropertyChanged(propertyName);
            return true;
        }

        return false;
    }
}

// An editable name/value row for the query-parameter and header grids.
public class EditableNameValue : INotifyPropertyChanged
{
    private string m_name;
    private string m_value;

    public string Name
    {
        get => m_name;
        set
        {
            if (!Equals(m_name, value))
            {
                m_name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }
    }

    public string Value
    {
        get => m_value;
        set
        {
            if (!Equals(m_value, value))
            {
                m_value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public NameValuePair ToPair()
    {
        return new NameValuePair { Name = Name, Value = Value };
    }
}
