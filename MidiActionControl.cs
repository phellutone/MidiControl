using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MidiActionControl : MidiDeviceActionControl
{
    public MidiDeviceIdentifier identifier;
    public string controlName;
    public bool isButton;
    public int channel;
    public int number;

    public float value;
    public event Action<string, float> ActionControl;
    private event Action<float> ValueControl;

    void Start()
    {
        if (identifier == null) return;
        ValueControl += (value) => this.value = value;

        var scheme = new MidiDeviceActionControlScheme(isButton, controlName, channel, number);
        scheme.Init(ActionControl, ValueControl, identifier);
    }
}
