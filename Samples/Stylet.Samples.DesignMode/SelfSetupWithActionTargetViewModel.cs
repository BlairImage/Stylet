﻿using System;

namespace Stylet.Samples.DesignMode;

public class SelfSetupWithActionTargetViewModel : Screen
{
#pragma warning disable IDE0052 // Remove unread private members
    private readonly IEventAggregator eventAggregator;
#pragma warning restore IDE0052 // Remove unread private members

    public string TextBoxText { get; set; }

    /// <summary>
    /// This constructor is called by the designer. Use it to set up dummy values
    /// </summary>
    public SelfSetupWithActionTargetViewModel()
    {
        this.TextBoxText = "This is some dummy text.";
    }

    /// <summary>
    /// This constructor is *not* called by the designer, but will be chosen at runtime by StyletIoC
    /// </summary>
    public SelfSetupWithActionTargetViewModel(IEventAggregator eventAggregator)
    {
        this.eventAggregator = eventAggregator;
    }

    public bool CanDoSomething => false;

    public void DoSomething()
    {
    }
}
