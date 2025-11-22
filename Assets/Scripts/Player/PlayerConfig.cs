using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerConfig", menuName = "Game Configs/Player Configuration")]
public class PlayerConfig : ScriptableObject
{
    public event Action<Color> OnColorChanged;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Visuals")]
    [SerializeField]
    private Color _playerColor = Color.white;

    public Color PlayerColor
    {
        get { return _playerColor; }
        set
        {
            if (_playerColor != value)
            {
                _playerColor = value;
                OnColorChanged?.Invoke(_playerColor);
            }
        }
    }
}
