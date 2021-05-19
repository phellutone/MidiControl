using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MidiDeviceActionControl : MonoBehaviour { }

public class MidiDeviceActionControlScheme
{
    public bool isButton;
    public string controlName;
    public int channel;
    public int number;

    public MidiDeviceActionControlScheme(bool isButton, string controlName, int channel, int number)
    {
        this.isButton = isButton;
        this.controlName = controlName;
        this.channel = channel;
        this.number = number;
    }

    public void Init(Action<string, float> _ActionControl, Action<float> _ValueControl, MidiDeviceIdentifier _identifier)
    {
        InputSystem.onDeviceChange += (device, change) =>
        {
            if (change != InputDeviceChange.Added) return;

            var midiDevice = device as Minis.MidiDevice;
            if (midiDevice == null) return;

            if (_identifier.midiDevices[midiDevice.channel] != midiDevice) return;

            int ch = midiDevice.channel;

            midiDevice.onWillNoteOn += (note, velocity) =>
            {
                if (isButton && ch == channel && note.noteNumber == number)
                {
                    if (_ActionControl != null) _ActionControl(controlName, 1);
                    if (_ValueControl != null) _ValueControl(1);
                }
            };

            midiDevice.onWillNoteOff += (note) =>
            {
                if (isButton && ch == channel && note.noteNumber == number)
                {
                    if (_ActionControl != null) _ActionControl(controlName, 0);
                    if (_ValueControl != null) _ValueControl(0);
                }
            };

            midiDevice.onWillControlChange += (control, velocity) =>
            {
                if (!isButton && ch == channel && control.controlNumber == number)
                {
                    if (_ActionControl != null) _ActionControl(controlName, velocity);
                    if (_ValueControl != null) _ValueControl(velocity);
                }
            };
        };
    }
}