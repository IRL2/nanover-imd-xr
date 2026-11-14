using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

namespace NanoverImd
{
    /// <summary>
    /// Voice chat through Unity Vivox for everyone connected to the same
    /// NanoVer server.
    /// </summary>
    /// <remarks>
    /// The voice channel is named after the server address the client
    /// connected to, so everyone who typed, or looked up, the same address
    /// shares one channel. The client signs in to Vivox only while it is
    /// connected to a server, so only people in a session count towards
    /// Vivox usage.
    /// </remarks>
    public sealed class NanoverImdVoiceChat : MonoBehaviour
    {
        private const string ChannelPrefix = "nanover-";
        private const float CheckInterval = 5f;
        private const float RetryDelay = 30f;

        [SerializeField]
        private NanoverImdApplication application;

        [Tooltip("Join voice chat automatically when connecting to a server.")]
        [SerializeField]
        private bool voiceEnabled = true;

        [Tooltip("Join voice chat with the microphone muted.")]
        [SerializeField]
        private bool startMuted;

        [Tooltip("How much Vivox itself writes to the log (the Console, or logcat on Quest). Applies from the next app start.")]
        [SerializeField]
        private VivoxLogLevel vivoxLogLevel = VivoxLogLevel.Info;

        private NanoverImdSimulation simulation;
        private string targetChannel;
        private bool updating;
        private bool vivoxReady;
        private float nextCheck;
        private float retryAt;

        /// <summary>
        /// The Vivox channel this client is in, or null.
        /// </summary>
        public string CurrentChannel { get; private set; }

        /// <summary>
        /// The server address the target channel is named after, or null.
        /// </summary>
        public string TargetAddress { get; private set; }

        /// <summary>
        /// What voice chat is doing, or the step it was on when it failed.
        /// </summary>
        public string Stage { get; private set; } = "Not connected to a server";

        /// <summary>
        /// The state of the Android microphone permission, as last seen.
        /// </summary>
        public string MicrophonePermission { get; private set; } = "not asked yet";

        /// <summary>
        /// Whether this client's microphone is muted in voice chat.
        /// </summary>
        public bool IsMuted => vivoxReady && VivoxService.Instance.IsInputDeviceMuted;

        /// <summary>
        /// Whether to be in voice chat while connected to a server.
        /// </summary>
        public bool VoiceEnabled
        {
            get => voiceEnabled;
            set
            {
                voiceEnabled = value;
                UpdateTargetChannel();
            }
        }

        /// <summary>
        /// The Vivox channel name for a server address. Addresses that differ
        /// only in case, a trailing slash, an omitted default port, or
        /// surrounding whitespace give the same channel.
        /// </summary>
        public static string ChannelNameForAddress(string address)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(NormalizeAddress(address)));

            // Vivox channel names allow only letters, digits and a few symbols,
            // so use the first 128 bits of the hash, in hex.
            var name = new StringBuilder(ChannelPrefix);
            for (var i = 0; i < 16; i++)
                name.Append(hash[i].ToString("x2"));

            return name.ToString();
        }

        /// <summary>
        /// Reduce a server address to host, port and path, so that different
        /// spellings of the same address match.
        /// </summary>
        public static string NormalizeAddress(string address)
        {
            // Text from a TextMeshPro input ends with a zero-width space.
            var text = address.Replace(((char) 0x200B).ToString(), "").Trim();

            if (!Uri.TryCreate(text, UriKind.Absolute, out var uri))
                return text.ToLowerInvariant();

            var port = uri.Port >= 0 ? uri.Port : uri.Scheme == "wss" ? 443 : 80;
            var path = uri.AbsolutePath.TrimEnd('/');

            return $"{uri.Host.ToLowerInvariant()}:{port}{path}";
        }

        /// <summary>
        /// Mute or unmute this client's microphone in voice chat.
        /// </summary>
        public void SetMuted(bool muted)
        {
            if (!vivoxReady)
                return;

            if (muted)
                VivoxService.Instance.MuteInputDevice();
            else
                VivoxService.Instance.UnmuteInputDevice();
        }

        [ContextMenu("Toggle Mute")]
        public void ToggleMute()
        {
            SetMuted(!IsMuted);
            Debug.Log($"Voice chat: microphone {(IsMuted ? "muted" : "on")}");
        }

        /// <summary>
        /// Try joining again after a failure, without reconnecting to the server.
        /// </summary>
        [ContextMenu("Retry")]
        public void Retry()
        {
            UpdateTargetChannel();
        }

        private void Awake()
        {
            if (application == null)
                application = FindAnyObjectByType<NanoverImdApplication>();

            simulation = application.Simulation;

            Debug.Log("Voice chat: ready, waiting for a server connection");
        }

        private void OnEnable()
        {
            simulation.SessionOpened += UpdateTargetChannel;
            simulation.SessionClosed += UpdateTargetChannel;
            UpdateTargetChannel();
        }

        private void OnDisable()
        {
            simulation.SessionOpened -= UpdateTargetChannel;
            simulation.SessionClosed -= UpdateTargetChannel;

            targetChannel = null;
            UpdateVoiceAsync();
        }

        private void OnValidate()
        {
            // Apply Inspector changes to "Voice Enabled" while playing.
            if (Application.isPlaying && isActiveAndEnabled && simulation != null)
                UpdateTargetChannel();
        }

        private void Update()
        {
            if (updating || Time.unscaledTime < nextCheck)
                return;

            nextCheck = Time.unscaledTime + CheckInterval;

            // Vivox can drop a channel without being asked to, for example
            // while the headset is asleep, so check and join again.
            if (CurrentChannel != null && vivoxReady && !VivoxService.Instance.ActiveChannels.ContainsKey(CurrentChannel))
            {
                Debug.LogWarning($"Voice chat: dropped from {CurrentChannel}, joining again");
                CurrentChannel = null;
            }

            // Also retry a while after a failure.
            if (CurrentChannel != targetChannel && Time.unscaledTime >= retryAt)
                UpdateVoiceAsync();
        }

        private void OnDestroy()
        {
            if (!vivoxReady)
                return;

            var vivox = VivoxService.Instance;
            vivox.ParticipantAddedToChannel -= OnParticipantAdded;
            vivox.ParticipantRemovedFromChannel -= OnParticipantRemoved;
            vivox.ConnectionRecovering -= OnConnectionRecovering;
            vivox.ConnectionRecovered -= OnConnectionRecovered;
            vivox.ConnectionFailedToRecover -= OnConnectionFailedToRecover;
        }

        private void UpdateTargetChannel()
        {
            TargetAddress = voiceEnabled && simulation.Connected ? simulation.ServerAddress : null;
            targetChannel = string.IsNullOrEmpty(TargetAddress) ? null : ChannelNameForAddress(TargetAddress);

            if (targetChannel == null && CurrentChannel == null)
                Stage = voiceEnabled ? "Not connected to a server" : "Voice chat turned off";

            UpdateVoiceAsync();
        }

        /// <summary>
        /// Join or leave Vivox channels until this client is in the target
        /// channel. Only one update runs at a time; a change of target during
        /// an update is picked up by the same loop.
        /// </summary>
        private async void UpdateVoiceAsync()
        {
            if (updating)
                return;

            updating = true;

            try
            {
                while (CurrentChannel != targetChannel)
                {
                    if (CurrentChannel != null)
                        await LeaveAsync();
                    else
                        await JoinAsync(targetChannel);
                }
            }
            catch (Exception e)
            {
                // Voice chat must never break the NanoVer session itself.
                var code = e is RequestFailedException failure ? $" (error code {failure.ErrorCode})" : "";
                Debug.LogWarning($"Voice chat failed while: {Stage}{code}\n{e}");
                Stage = $"Failed: {Stage}";
                retryAt = Time.unscaledTime + RetryDelay;

                await LogOutIfNotInChannelAsync();
            }
            finally
            {
                updating = false;
            }
        }

        private async Task JoinAsync(string channel)
        {
            // Ask first, so the prompt appears even if a later step fails.
            SetStage("Asking for microphone permission");
            if (!await RequestMicrophonePermissionAsync())
                Debug.LogWarning("Voice chat: no microphone permission, so others can't hear you.");

            await InitializeVivoxAsync();

            if (!VivoxService.Instance.IsLoggedIn)
            {
                SetStage("Logging in to Vivox");

                // The display name links this voice participant to this
                // client's avatar, whose ID is the session's access token.
                await VivoxService.Instance.LoginAsync(new LoginOptions
                {
                    DisplayName = simulation.Multiplayer.AccessToken,
                });
            }

            SetStage($"Joining channel {channel} for {TargetAddress}");
            await VivoxService.Instance.JoinGroupChannelAsync(channel, ChatCapability.AudioOnly);
            CurrentChannel = channel;
            SetMuted(startMuted);

            SetStage("In voice channel");
            Debug.Log($"Voice chat: microphone {VivoxService.Instance.EffectiveInputDevice?.DeviceName}, " +
                      $"speaker {VivoxService.Instance.EffectiveOutputDevice?.DeviceName}");
        }

        private async Task LeaveAsync()
        {
            var channel = CurrentChannel;
            SetStage($"Leaving channel {channel}");

            try
            {
                // Vivox may have dropped the channel already, and leaving a
                // channel it isn't in throws.
                if (VivoxService.Instance.ActiveChannels.ContainsKey(channel))
                    await VivoxService.Instance.LeaveChannelAsync(channel);
            }
            finally
            {
                CurrentChannel = null;

                // Log out between sessions so that only people in a session
                // count towards Vivox usage.
                if (VivoxService.Instance.IsLoggedIn)
                    await VivoxService.Instance.LogoutAsync();

                SetStage(targetChannel == null ? "Not connected to a server" : "Left channel");
            }
        }

        private async Task LogOutIfNotInChannelAsync()
        {
            try
            {
                if (CurrentChannel == null && vivoxReady && VivoxService.Instance.IsLoggedIn)
                    await VivoxService.Instance.LogoutAsync();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Voice chat: logout after failure also failed: {e.Message}");
            }
        }

        private async Task InitializeVivoxAsync()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                SetStage("Starting Unity Services");
                await UnityServices.InitializeAsync();
            }

            // An anonymous sign-in needs no account; Unity Authentication then
            // issues this client's Vivox tokens, so no Vivox key ships in the app.
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                SetStage("Signing in to Unity (anonymous)");
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"Voice chat: signed in to Unity as player {AuthenticationService.Instance.PlayerId}");
            }

            if (vivoxReady)
                return;

            SetStage("Starting Vivox");
            await VivoxService.Instance.InitializeAsync(new VivoxConfigurationOptions
            {
                LogLevel = vivoxLogLevel,
            });

            vivoxReady = true;

            var vivox = VivoxService.Instance;
            vivox.ParticipantAddedToChannel += OnParticipantAdded;
            vivox.ParticipantRemovedFromChannel += OnParticipantRemoved;
            vivox.ConnectionRecovering += OnConnectionRecovering;
            vivox.ConnectionRecovered += OnConnectionRecovered;
            vivox.ConnectionFailedToRecover += OnConnectionFailedToRecover;
        }

        private void SetStage(string stage)
        {
            Stage = stage;
            Debug.Log($"Voice chat: {stage}");
        }

        private static void OnParticipantAdded(VivoxParticipant participant)
        {
            Debug.Log($"Voice chat: {Describe(participant)} joined");
        }

        private static void OnParticipantRemoved(VivoxParticipant participant)
        {
            Debug.Log($"Voice chat: {Describe(participant)} left");
        }

        private void OnConnectionRecovering() => SetStage("Connection to Vivox lost, reconnecting");

        private void OnConnectionRecovered() => SetStage("In voice channel (reconnected)");

        private void OnConnectionFailedToRecover()
        {
            SetStage("Connection to Vivox lost; reconnect to the server to retry");
            CurrentChannel = null;
        }

        private static string Describe(VivoxParticipant participant)
        {
            return participant.IsSelf ? "you" : participant.DisplayName ?? participant.PlayerId;
        }

        /// <summary>
        /// On Android (Quest), ask for the microphone permission if it hasn't
        /// been granted. Returns whether the app may use the microphone.
        /// </summary>
        private Task<bool> RequestMicrophonePermissionAsync()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                MicrophonePermission = "granted";
                return Task.FromResult(true);
            }

            MicrophonePermission = "asking";

            var result = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += _ => Answer(true, "granted");
            callbacks.PermissionDenied += _ => Answer(false, "denied");
            callbacks.PermissionRequestDismissed += _ => Answer(false, "dismissed");
#pragma warning disable 618 // obsolete in newer Unity versions, but may still be the only callback raised
            callbacks.PermissionDeniedAndDontAskAgain += _ => Answer(false, "denied (don't ask again)");
#pragma warning restore 618

            Permission.RequestUserPermission(Permission.Microphone, callbacks);
            return result.Task;

            void Answer(bool granted, string state)
            {
                MicrophonePermission = state;
                Debug.Log($"Voice chat: microphone permission {state}");
                result.TrySetResult(granted);
            }
#else
            MicrophonePermission = "not needed on this platform";
            return Task.FromResult(true);
#endif
        }
    }
}
