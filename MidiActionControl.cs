using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MidiActionControl : MonoBehaviour
{
    public MidiDeviceIdentifier identifier;
    public string controlName;
    public bool isButton;
    public int channel;
    public int number;

    public float value;
    public bool state = false;

    public event Action<string, float> ActionControl
    {
        add { (_actioncontrol = _actioncontrol ?? new List<Action<string, float>>()).Add(value); }
        remove { _actioncontrol.Remove(value); }
    }
    public event Func<float, float> ValueControl
    {
        add { (_valuecontrol = _valuecontrol ?? new List<Func<float, float>>()).Add(value);  }
        remove { _valuecontrol.Remove(value); }
    }
    public event Func<bool,bool> StateControl
    {
        add { (_statecontrol = _statecontrol ?? new List<Func<bool, bool>>()).Add(value); }
        remove { _statecontrol.Remove(value); }
    }

    private List<Action<string, float>> _actioncontrol;
    private List<Func<float, float>> _valuecontrol;
    private List<Func<bool, bool>> _statecontrol;

    private void Start()
    {
        if (identifier == null) return;
        ValueControl += (value) => value;
        StateControl += (state) => !state;
        Init();
    }

    private void Init()
    {
        InputSystem.onDeviceChange += (device, change) =>
        {
            if (change != InputDeviceChange.Added) return;

            var midiDevice = device as Minis.MidiDevice;
            if (midiDevice == null) return;

            if (identifier.midiDevices[midiDevice.channel] != midiDevice) return;

            int ch = midiDevice.channel;

            midiDevice.onWillNoteOn += (note, velocity) =>
            {
                if (isButton && ch == channel && note.noteNumber == number)
                {
                    if (_valuecontrol != null)
                    {
                        foreach (var del in _valuecontrol)
                        {
                            this.value = del(1);
                        }
                    }
                    if (_statecontrol != null)
                    {
                        foreach (var del in _statecontrol)
                        {
                            this.state = del(this.state);
                        }
                    }
                    if (_actioncontrol != null)
                    {
                        foreach (var del in _actioncontrol)
                        {
                            del(controlName, 1);
                        }
                    }
                }
            };

            midiDevice.onWillNoteOff += (note) =>
            {
                if (isButton && ch == channel && note.noteNumber == number)
                {
                    if (_valuecontrol != null)
                    {
                        foreach (var del in _valuecontrol)
                        {
                            this.value = del(0);
                        }
                    }
                    if (_actioncontrol != null)
                    {
                        foreach (var del in _actioncontrol)
                        {
                            del(controlName, 0);
                        }
                    }
                }
            };

            midiDevice.onWillControlChange += (control, velocity) =>
            {
                if (!isButton && ch == channel && control.controlNumber == number)
                {
                    if (_valuecontrol != null)
                    {
                        foreach (var del in _valuecontrol)
                        {
                            this.value = del(velocity);
                        }
                    }
                    if (_actioncontrol != null)
                    {
                        foreach (var del in _actioncontrol)
                        {
                            del(controlName, velocity);
                        }
                    }
                }
            };
        };
    }
}
