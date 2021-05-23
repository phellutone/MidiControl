using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MidiChannelIndentifier : MonoBehaviour
{
    public Minis.MidiDevice midiDevice;

    private List<Action<Minis.MidiDevice, bool, Minis.MidiNoteControl, Minis.MidiValueControl, float>> _actiondistributor;
    public event Action<Minis.MidiDevice, bool, Minis.MidiNoteControl, Minis.MidiValueControl, float> ActionDistributor
    {
        add { (_actiondistributor = _actiondistributor ?? new List<Action<Minis.MidiDevice, bool, Minis.MidiNoteControl, Minis.MidiValueControl, float>>()).Add(value); }
        remove { _actiondistributor.Remove(value); }
    }

    public MidiDeviceIdentifier deviceIdentifier;

    public string product;
    public int channel = -1;
    public int number;
    public bool isButton;
    public string displayName;
    public float value;

    private void Awake()
    {
        if (this.channel < 0 && Int32.TryParse(gameObject.name, out int _channel))
            this.channel = _channel;

        try
        {
            deviceIdentifier = transform.parent.GetComponent<MidiDeviceIdentifier>();
        }
        catch (NullReferenceException)
        {
            //Debug.Log(e.Message);
        }
        if (deviceIdentifier == null) return;

        deviceIdentifier.ChannelDistributor += (dev, isb, note, ctrl, val) =>
        {
            if (dev.channel != this.channel) return;

            this.midiDevice = dev;

            if (_actiondistributor != null)
                foreach (var action in _actiondistributor)
                    action(dev, isb, note, ctrl, val);

            this.product = dev.description.product;
            this.number = (isb) ? note.noteNumber : ctrl.controlNumber;
            this.isButton = isb;
            this.displayName = (isb) ? note.displayName : ctrl.displayName;
            this.value = val;
        };
    }
}
