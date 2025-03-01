﻿using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Cirrus.Circuit.Controls;

namespace Cirrus.Circuit.UI
{
    public class HUD : BaseSingleton<HUD>
    {
        [SerializeField]
        private Player[] _playerDisplays;

        private List<Player> _availablePlayerDisplays;

        public override void Awake()
        {
            base.Awake();

            _availablePlayerDisplays = new List<Player>();
            //Game.Instance.OnLevelSelectHandler += OnLevelSelect;
        }

        public override void OnValidate()
        {

            base.OnValidate();
            //if (_characterSelect == null)
            //    _characterSelect = FindObjectOfType<CharacterSelect>();            
        }

        public void Join(Controls.Player player)
        {
            if (_availablePlayerDisplays.Count != 0)
            {
                //_availablePlayerDisplays[0].SetState(Player.State.Ready, player.ServerId);
                _availablePlayerDisplays.RemoveAt(0);
                //_playerDisplays[index]?.SetState(state);
            }            
        }
   
    }
}