using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

public class MidiDeviceIdentifier : MonoBehaviour
{
    public Minis.MidiDevice[] midiDevices = new Minis.MidiDevice[16];

    [SerializeField]
    public List<bool> _channels = new List<bool>()
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
    public string _productName;

    public string _product;
    public int _channel;
    public int _number;
    public string _displayName;
    public float _value;

    private bool DeviceSearcher(InputDevice dev)
    {
        InputDeviceMatcher match = new InputDeviceMatcher();
        match = match.WithInterface("Minis");
        match = match.WithProduct(_productName);

        if (match.MatchPercentage(dev.description) > 0)
        {
            var midiDevice = dev as Minis.MidiDevice;
            if (midiDevice == null) return false;

            midiDevices[midiDevice.channel] = midiDevice;
            return true;
        }
        return false;
    }

    private void OnEnable()
    {
        InputSystem.onDeviceChange += (device, change) =>
        {
            if (!DeviceSearcher(device)) return;
            var midiDevice = device as Minis.MidiDevice;
            if (midiDevice == null) return;

            if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected || change == InputDeviceChange.Enabled)
                _channels[midiDevice.channel] = midiDevices[midiDevice.channel] is Minis.MidiDevice;
            else
                _channels[midiDevice.channel] = false;

            if (change != InputDeviceChange.Added) return;

            midiDevice.onWillNoteOn += (note, velocity) =>
            {
                _product = midiDevice.description.product;
                _channel = midiDevice.channel;
                _number = note.noteNumber;
                _displayName = note.displayName;
                _value = velocity;
            };

            midiDevice.onWillNoteOff += (note) =>
            {
                _product = midiDevice.description.product;
                _channel = midiDevice.channel;
                _number = note.noteNumber;
                _displayName = note.displayName;
                _value = 0;
            };

            midiDevice.onWillControlChange += (control, velocity) =>
            {
                _product = midiDevice.description.product;
                _channel = midiDevice.channel;
                _number = control.controlNumber;
                _displayName = control.displayName;
                _value = velocity;
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
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_productName"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_channels"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_product"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_channel"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_displayName"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_value"), true);
        serializedObject.ApplyModifiedProperties();
    }
}
