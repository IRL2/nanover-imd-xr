using Cysharp.Threading.Tasks;
using Essd;
using Nanover.Frontend.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using WebDiscovery;

namespace NanoverImd.UI.Scene
{
    public class DiscoverServers : MonoBehaviour
    {
        private Client client;
        private List<ServiceHub> essdListing = new List<ServiceHub>();
        private List<DiscoveryEntry> webListing = new List<DiscoveryEntry>();

        [SerializeField]
        private string discoveryHost = "irl-discovery.onrender.com";

        [SerializeField]
        private Sprite localServerIcon;

        [SerializeField]
        private Sprite remoteServerIcon;

        [SerializeField]
        private NanoverImdApplication application;

        [SerializeField]
        private DynamicMenu menu;

        private void OnEnable()
        {
            Refresh();
        }

        /// <summary>
        /// Returns either the <see cref="ServiceHub"/> with a localhost IP address or the first hub.
        /// </summary>
        private ServiceHub SelectBestService(IGrouping<string, ServiceHub> group)
        {
            foreach (var hub in group)
            {
                if (IsLocalhost(hub))
                    return hub;
            }

            return group.First();
        }

        private bool IsLocalhost(ServiceHub hub)
        {
            return hub.Address.Equals("127.0.0.1") || hub.Address.Equals("localhost");
        }

        private UniTask? currentSearchTask = null;

        [SerializeField]
        private UnityEvent startSearch;

        [SerializeField]
        private UnityEvent endSearch;

        public async UniTask SearchAll()
        {
            startSearch?.Invoke();
            await UniTask.WhenAll(SearchESSDAsync(), SearchWebAsync());
            endSearch?.Invoke();
        }

        public async UniTask SearchESSDAsync()
        {
            if (currentSearchTask != null)
            {
                client.StopSearch().AsUniTask().Forget();
                currentSearchTask = null;
                endSearch?.Invoke();
            }

            client = new Client();
            essdListing.Clear();
            client.ServiceFound += (obj, args) => essdListing.Add(args);

            currentSearchTask = client.StartSearch().AsUniTask();
            await UniTask.WhenAny(UniTask.Delay(500), currentSearchTask.Value);
            await client.StopSearch();
            currentSearchTask = null;
            RefreshListings();
        }

        public async UniTask SearchWebAsync()
        {
            var discovery = new Discovery { Host = discoveryHost };

            try
            {
                webListing = await discovery.GetListing();
                RefreshListings();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void Refresh()
        {
            SearchAll().Forget();
        }

        public void RefreshListings()
        {
            essdListing = essdListing.GroupBy(hub => hub.Id)
                       .Select(SelectBestService)
                       .ToList();

            menu.ClearChildren();
            foreach (var hub in essdListing)
            {
                var local = hub.Address.Equals("127.0.0.1") || hub.Address.Equals("localhost");
                menu.AddItem(hub.Name, local ? localServerIcon : remoteServerIcon,
                                () => application.Connect(hub), hub.Address); 
            }
            foreach (var entry in webListing)
            {
                menu.AddItem(entry.info.name, remoteServerIcon, () => application.Connect(entry), entry.info.ws ?? entry.info.wss);
            }
        }
    }
}