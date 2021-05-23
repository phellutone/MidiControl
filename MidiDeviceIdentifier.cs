using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

public class MidiDeviceIdentifier : MonoBehaviour
{
    public Minis.MidiDevice[] midiDevices = new Minis.MidiDevice[16];

    private List<Action<Minis.MidiDevice, bool, Minis.MidiNoteControl, Minis.MidiValueControl, float>> _channeldistributor;
    public event Action<Minis.MidiDevice, bool, Minis.MidiNoteControl, Minis.MidiValueControl, float> ChannelDistributor
    {
        add { (_channeldistributor = _channeldistributor ?? new List<Action<Minis.MidiDevice, bool, Minis.MidiNoteControl, Minis.MidiValueControl, float>>()).Add(value); }
        remove { _channeldistributor.Remove(value); }
    }

    [SerializeField]
    public List<bool> channels = new List<bool>()
    {
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false
    };

    [SerializeField]
    public string productName;

    public string product;
    public int channel;
    public int number;
    public bool isButton;
    public string displayName;
    public float value;

    private Minis.MidiDevice DeviceSearcher(InputDevice dev)
    {
        InputDeviceMatcher match = new InputDeviceMatcher();
        match = match.WithInterface("Minis");
        match = match.WithProduct(this.productName);

        if (match.MatchPercentage(dev.description) > 0)
            return dev as Minis.MidiDevice;
        else
            return null;
    }

    private void Distributor(Minis.MidiDevice dev, bool isb, Minis.MidiNoteControl note, Minis.MidiValueControl ctrl, float val)
    {
        if (_channeldistributor != null)
            foreach (var action in _channeldistributor)
                action(dev, isb, note, ctrl, val);
    }

    private void Inspector(Minis.MidiDevice dev, bool isb, int num, string name, float val)
    {
        this.product = dev.description.product;
        this.channel = dev.channel;
        this.number = num;
        this.isButton = isb;
        this.displayName = name;
        this.value = val;
    }

    private void Awake()
    {
        if (this.productName == "")
            this.productName = gameObject.name;

        InputSystem.onDeviceChange += (device, change) =>
        {
            var midiDevice = DeviceSearcher(device);
            if (midiDevice == null) return;

            if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected || change == InputDeviceChange.Enabled)
            {
                this.channels[midiDevice.channel] = true;
                midiDevices[midiDevice.channel] = midiDevice;
            }
            else
            {
                this.channels[midiDevice.channel] = false;
                midiDevices[midiDevice.channel] = null;
                return;
            }

            midiDevice.onWillNoteOn += (note, velocity) =>
            {
                Distributor(midiDevice, true, note, null, velocity);
                Inspector(midiDevice, true, note.noteNumber, note.displayName, velocity);
            };

            midiDevice.onWillNoteOff += (note) =>
            {
                Distributor(midiDevice, true, note, null, 0);
                Inspector(midiDevice, true, note.noteNumber, note.displayName, 0);
            };

            midiDevice.onWillControlChange += (control, velocity) =>
            {
                Distributor(midiDevice, false, null, control, velocity);
                Inspector(midiDevice, false, control.controlNumber, control.displayName, velocity);
            };
        };
    }
}

[CustomEditor(typeof(MidiDeviceIdentifier))]
public class MidiDeviceIdentifierEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("productName"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("channels"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("product"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("channel"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("number"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("isButton"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("value"), true);
        serializedObject.ApplyModifiedProperties();
    }
}
