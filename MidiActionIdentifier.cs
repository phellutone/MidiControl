using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MidiActionIdentifier : MonoBehaviour
{
    public Minis.MidiNoteControl midiNote;
    public Minis.MidiValueControl midiControl;

    private List<Action<string, float>> _actioncontrol;
    public event Action<string, float> ActionControl
    {
        add { (_actioncontrol = _actioncontrol ?? new List<Action<string, float>>()).Add(value); }
        remove { _actioncontrol.Remove(value); }
    }

    private List<Func<float, float>> _valuecontrol;
    public event Func<float, float> ValueControl
    {
        add { _valuecontrol = new List<Func<float, float>> { value }; }
        remove { _valuecontrol.Remove(value); }
    }

    private List<Func<bool, bool>> _statecontrol;
    public event Func<bool, bool> StateControl
    {
        add { _statecontrol = new List<Func<bool, bool>> { value }; }
        remove { _statecontrol.Remove(value); }
    }

    public MidiChannelIndentifier channelIndentifier;

    public string product;
    public int channel = -1;
    public int number;
    public bool isButton;
    public string displayName;
    public string controlName;
    public float value;
    public bool state;

    private void Awake()
    {
        if (this.controlName == "")
            this.controlName = gameObject.name;

        try
        {
            channelIndentifier = transform.parent.GetComponent<MidiChannelIndentifier>();
        }
        catch (NullReferenceException)
        {
            //Debug.Log(e.Message);
        }
        if (channelIndentifier == null) return;

        ValueControl += value => value;
        StateControl += state => !state;

        channelIndentifier.ActionDistributor += (dev, isb, note, ctrl, val) =>
        {
            if (((isb) ? note.noteNumber : ctrl.controlNumber) != this.number) return;

            this.midiNote = note;
            this.midiControl = ctrl;
            this.channel = dev.channel;

            if (_valuecontrol != null)
                foreach (var action in _valuecontrol)
                    this.value = action(val);

            if (val == 1 && _statecontrol != null)
                foreach (var action in _statecontrol)
                    this.state = action(this.state);

            if (_actioncontrol != null)
                foreach (var action in _actioncontrol)
                    action(this.controlName, val);

            this.product = dev.description.product;
            this.displayName = (isb) ? note.displayName : ctrl.displayName;
        };
    }
}
