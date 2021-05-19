using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEditor;
using System;

public class DeviceInspector : MonoBehaviour
{
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

    public string _product;
    public int _channel;
    public int _number;
    public string _displayName;
    public string _shortDisplayName;
    public float _value;

    void Start()
    {
        InputSystem.onDeviceChange += (device, change) =>
        {
            var midiDevice = device as Minis.MidiDevice;
            if (midiDevice == null) return;

            if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected || change == InputDeviceChange.Enabled)
            {
                _channels[midiDevice.channel] = true;
            }
            else
            {
                _channels[midiDevice.channel] = false;
            }

            if (change != InputDeviceChange.Added) return;
            midiDevice.onWillNoteOn += (note, velocity) =>
            {
                _product = note.device.description.product;
                _channel = midiDevice.channel;
                _number = note.noteNumber;
                _displayName = note.displayName;
                _shortDisplayName = note.shortDisplayName;
                _value = velocity;
            };

            midiDevice.onWillNoteOff += (note) =>
            {
                _product = note.device.description.product;
                _channel = midiDevice.channel;
                _number = note.noteNumber;
                _displayName = note.displayName;
                _shortDisplayName = note.shortDisplayName;
                _value = 0;
            };

            midiDevice.onWillControlChange += (control, velocity) =>
            {
                _product = control.device.description.product;
                _channel = midiDevice.channel;
                _number = control.controlNumber;
                _displayName = control.displayName;
                _shortDisplayName = control.shortDisplayName;
                _value = velocity;
            };
        };
    }

    
}

[CustomEditor(typeof(DeviceInspector))]
public class DeviceInspectorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_channels"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_product"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_channel"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_number"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_displayName"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_shortDisplayName"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_value"), true);
        serializedObject.ApplyModifiedProperties();
    }
}
