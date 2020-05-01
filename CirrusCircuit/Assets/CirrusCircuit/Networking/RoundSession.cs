﻿using UnityEngine;
using System.Collections;
using Cirrus.Circuit.Controls;
using Cirrus.Circuit.UI;
using Mirror;
//using UnityEngine;
using Cirrus.MirrorExt;

namespace Cirrus.Circuit.Networking
{
    public delegate void OnIntermission(int count);

    public delegate void OnCountdown(int count);

    public delegate void OnRoundBegin(int roundNumber);

    public delegate void OnRoundEnd();

    public class RoundSession : NetworkBehaviour
    {
        public OnIntermission OnIntermissionHandler;

        public OnCountdown OnCountdownHandler;

        public OnRoundBegin OnRoundBeginHandler;

        public OnRoundEnd OnRoundEndHandler;

        [SerializeField]        
        private Timer _timer;

        [SerializeField]
        private Timer _countDownTimer;
        
        [SerializeField]
        private Timer _intermissionTimer;

        public float Time => _roundTime - _timer.Time;

        [SyncVar]
        [SerializeField]        
        private float _countDownTime = 1f;

        [SyncVar]
        [SerializeField]        
        private float _intermissionTime = 0; // Where we show the round number

        [SyncVar]
        [SerializeField]        
        private int _countDown;

        [SyncVar]
        [SerializeField]
        private float _roundTime;

        [SyncVar]
        [SerializeField]
        private int _id = 0;

        public int Id => _id;

        private static RoundSession _instance;

        public override void OnStartServer()
        {
            base.OnStartServer();

            _countDownTimer.OnTimeLimitHandler += Cmd_OnTimeout;
            _intermissionTimer.OnTimeLimitHandler += Cmd_OnIntermissionTimeoutBeginCountdown;

        }

        public override void OnStartClient()
        {
            base.OnStartClient();            

            Game.Instance._SetState(Game.State.Round);
        }        

        public static RoundSession Instance
        {
            get
            {
                if (_instance == null) _instance = FindObjectOfType<RoundSession>();
                return _instance;
            }
        }

        public static RoundSession Create(
            int countDown, 
            float time, 
            float countDownTime, 
            float intermissionTime, 
            int id)
        {
            RoundSession session = null;

            if (ServerUtils.TryCreateNetworkObject(                
                NetworkingLibrary.Instance.RoundSession.gameObject,
                out GameObject obj,                
                true))
             {
                session = obj.GetComponent<RoundSession>();
                if (session != null)
                {
                    session._intermissionTime = intermissionTime;
                    session._id = id;
                    session._countDown = countDown;
                    session._roundTime = time;
                    session._countDownTime = countDownTime;
                    session._countDownTimer = new Timer(countDownTime, start: false, repeat: true);
                    session._timer = new Timer(session._roundTime, start: false);
                    session._intermissionTimer = new Timer(session._intermissionTime, start: false, repeat: false);
                }
            }

            return session;
        }

        public void BeginIntermission()
        {
            OnIntermissionHandler?.Invoke(_id);
            if(CustomNetworkManager.IsServer) _intermissionTimer.Start();
        }


        [ClientRpc]
        public void Rpc_OnIntermissionTimeoutBeginCountdown()
        {
            _OnIntermissionTimeoutBeginCountdown();
        }

        public void _OnIntermissionTimeoutBeginCountdown()
        {
            OnCountdownHandler?.Invoke(_countDown);
            if(CustomNetworkManager.IsServer) _countDownTimer.Start();
        }


        public void Terminate()
        {
            _timer.Stop();
            _countDownTimer.Stop();
            _intermissionTimer.Stop();
            Cmd_OnRoundEnd();
        }


        private void Cmd_OnTimeout()
        {
            ClientPlayer.Instance.Cmd_RoundSession_OnTimeout(gameObject);
        }


        public void Cmd_OnIntermissionTimeoutBeginCountdown()
        {
            ClientPlayer.Instance.Cmd_OnIntermissionTimeoutBeginCountdown(gameObject);
        }

        [ClientRpc]
        public void Rpc_OnTimeout()
        {
            _OnTimeOut();
        }

        private void _OnTimeOut()
        {
            _countDown--;

            if (_countDown < -1)
            {
                OnCountdownHandler?.Invoke(_countDown);

                if(CustomNetworkManager.IsServer) _countDownTimer.Stop();
            }
            else if (_countDown < 0)
            {
                OnCountdownHandler?.Invoke(_countDown);
                OnRoundBeginHandler.Invoke(_id);

                if (CustomNetworkManager.IsServer)
                {
                    _timer.Start();
                    _timer.OnTimeLimitHandler += Cmd_OnRoundEnd;
                }

                return;
            }
            else OnCountdownHandler?.Invoke(_countDown);
        }

        public void Cmd_OnRoundEnd()
        {
            ClientPlayer.Instance.Cmd_RoundSession_OnRoundEnd(gameObject);
        }

        [ClientRpc]
        public void Rpc_OnRoundEnd()
        {
            _OnRoundEnd();
        }

        public void _OnRoundEnd()
        {
            OnRoundEndHandler.Invoke();
        }

    }
}
