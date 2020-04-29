﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cirrus.Circuit.Networking;

using Mirror;

namespace Cirrus.Circuit
{
    public delegate  void OnPodiumFinished();


    public class Podium : NetworkBehaviour
    {

        protected static Podium _instance;

        public static Podium Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<Podium>();
                }

                return _instance;
            }
        }    

        public OnPodiumFinished OnPodiumFinishedHandler;

        [SerializeField]
        private UI.Announcement _announcement;

        [SerializeField]
        private Platform _platformTemplate;

        [SerializeField]
        private float _platformOffset = 2f;

        [SerializeField]
        private GameObject _platformsParent;

        [SerializeField]
        public Vector3 TargetPosition = Vector3.zero;

        [SerializeField]
        private List<Platform> _platforms;

        [SerializeField]
        private List<World.Objects.Characters.Character> _characters;

        private Timer _timer;

        [SerializeField]
        private float _timeTransition = 2f;

        private Timer _finalTimer;

        [SerializeField]
        private float _timeFinal = 3f;

        [SerializeField]
        private float _timeTransitionFrom = 2f;

        [SerializeField]
        public float _positionSpeed = 0.4f;


        private bool _isFinal = false;

        private int _platformFinishedCount = 0;

        public virtual void OnValidate()
        {
            //base.OnValidate();

            if (_announcement == null)
                _announcement = FindObjectOfType<UI.Announcement>();
        }

        public virtual void Awake()
        {
            //base.Awake();

            _timer = new Timer(_timeTransition, start: false, repeat: false);
            _finalTimer = new Timer(_timeFinal, start: false, repeat: false);

            _finalTimer.OnTimeLimitHandler += OnFinalTimeout;

            GameSession.OnStartClientStaticHandler += OnClientStarted;
            Game.Instance.OnPodiumHandler += OnPodium;
            Game.Instance.OnFinalPodiumHandler += OnFinalPodium;
        }

        public void OnClientStarted(bool enable)
        {

        }

        public void FixedUpdate()
        {
            transform.position = Vector3.Lerp(transform.position, TargetPosition, _positionSpeed);

            for(int i = 0; i < _characters.Count; i++) {
                _characters[i].Transform.transform.position =
                Vector3.Lerp(
                    _characters[i].Transform.transform.position,
                    _platforms[i]._characterAnchor.transform.position,
                    _timer.Time/_timeTransitionFrom);

                _characters[i].Transform.transform.rotation = Quaternion.Lerp(
                    _characters[i].Transform.transform.rotation, 
                    _platforms[i]._visual.Parent.transform.rotation,
                    _timer.Time / _timeTransitionFrom);
            }
        }

        public bool IsEmpty => _platforms.Count == 0;


        public void OnRound(RoundSession round)
        {
            //round.OnRoundEndHandler += OnRoundEnd;
        }

        public void OnPodium()
        {
            _isFinal = false;
            _platformFinishedCount = 0;
            _timer.Start();
        }

        public void OnFinalPodium()
        {
            _isFinal = true;
            _platformFinishedCount = 0;
            _timer.Start();
        }

        public void Clear()
        {
            foreach (var p in _platforms)
            {
                if (p == null)
                    continue;

                _timer.OnTimeLimitHandler -= p.OnTransitionToTimeOut;
                Destroy(p.gameObject);
            }

            _platforms.Clear();
            _characters.Clear();
        }

        public void Add(PlayerSession player, World.Objects.Characters.CharacterAsset characterResource)
        {
            Platform platform = _platformTemplate.Create(
                _platformsParent.transform.position + Vector3.right * _platforms.Count * _platformOffset,
                _platformsParent.transform,
                player);

            _timer.OnTimeLimitHandler += platform.OnTransitionToTimeOut;
            _platforms.Add(platform);
            platform.OnPlatformFinishedHandler += OnPlatformFinished;

            World.Objects.Characters.Character character = 
                characterResource.Create(platform._characterAnchor.transform.position, 
                platform._characterAnchor.transform);
            _characters.Add(character);
            character.transform.rotation = platform._visual.Parent.transform.rotation;
            platform.Character = character;

            character.ColorId = player.ServerId;
            character.Color = player.Color;

        }

        public void OnFinalTimeout()
        {
            OnPodiumFinishedHandler?.Invoke();
        }

        public void OnPlatformFinished()
        {
            _platformFinishedCount++;
            if (_platformFinishedCount >= _platforms.Count)
            {
                if (_isFinal)
                {
                    PlayerSession second = null;
                    float secondMax = -99999999f;
                    PlayerSession winner = null;
                    float max = -99999999f;
                    foreach (PlayerSession player in GameSession.Instance.Players)
                    {
                        if (player.Score > max)
                        {
                            if (second == null)
                            {
                                second = winner;
                                secondMax = max;
                            }

                            winner = player;
                            max = player.Score;

                        }
                        else if (player.Score > secondMax)
                        {
                            second = player;
                            secondMax = player.Score;
                        }
                    }

                    if (winner != null)
                    {
                        if (Mathf.Approximately(max, secondMax))
                        {
                            _announcement.Message = "Tie.";
                        }
                        else
                        {
                            _announcement.Message = winner.Name + " wins!";
                        }
                    }

                    _finalTimer.Start();
                }
                else
                {

                    OnPodiumFinishedHandler?.Invoke();
                }
            }
        }


    }
}